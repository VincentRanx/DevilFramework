namespace DevilTeam.AsyncTask
{
    public class DelegateTask : IAsyncTask
    {
        private ValueDelegate<bool> mIsDone;
        private ValueDelegate<float> mProgress;

        public DelegateTask(ValueDelegate<bool> isDone, ValueDelegate<float> progress)
        {
            mIsDone = isDone;
            mProgress = progress;
        }

        public float Progress
        {
            get
            {
                return mProgress == null ? 1 : mProgress();
            }
        }

        public bool IsDone
        {
            get
            {
                return mIsDone == null ? true : mIsDone();
            }
        }
    }
}