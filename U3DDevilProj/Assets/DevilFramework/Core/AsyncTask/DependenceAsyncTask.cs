namespace Devil.AsyncTask
{
    public abstract class DependenceAsyncTask : IAsyncTask
    {
        public IAsyncTask PresetTask { get; set; }

        public float Progress { get; private set; }

        public bool IsDone { get; private set; }

        bool mStart;

        public virtual void Abort()
        {
            IsDone = true;
        }

        public virtual void Start()
        {
            IsDone = false;
            Progress = 0;
        }

        public void OnTick(float deltaTime)
        {
            if (!IsDone && (PresetTask == null || PresetTask.IsDone))
            {
                if (!mStart)
                {
                    mStart = true;
                    OnTaskStart();
                }
                else
                {
                    Progress = TickAndGetTaskProgress(deltaTime);
                    if (Progress >= 1)
                        IsDone = true;
                }
            }
        }

        protected abstract void OnTaskStart();
        // return progress
        protected abstract float TickAndGetTaskProgress(float deltaTime);
    }
}