namespace Devil.AI
{
    public interface IBTTask
    {
        EBTState OnAbort();
        EBTState OnStart();
        EBTState OnUpdate(float deltaTime);
        void OnStop();
    }

    public abstract class BTTaskAsset : BTNodeAsset , IBTTask
	{
        EBTState mState;
        public override EBTState State { get { return mState; } }

        public override bool IsController { get { return false; } }

        public override bool EnableChild { get { return false; } }

        public override void Abort()
        {
            mState = OnAbort();
            if (mState <= EBTState.running)
                mState = EBTState.failed;
        }

        public override void Start()
        {
            mState = OnStart();
        }

        public override IBTNode GetNextChildTask() { return null; }

        public override void Stop()
        {
            OnStop();
        }

        public override void ReturnState(EBTState state) { }

        public override void OnTick(float deltaTime)
        {
            mState = OnUpdate(deltaTime);
        }

        public abstract EBTState OnStart();
        public abstract EBTState OnUpdate(float deltaTime);
        public abstract EBTState OnAbort();
        public virtual void OnStop() { }
    }
}