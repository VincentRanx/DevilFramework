namespace Devil.AI
{
    public interface IBTService : IIdentified
    {
        void OnStart();
        void OnUpdate(float deltaTime);
        void OnStop();
    }

    public abstract class BTServiceAsset : BTAsset, IBTService
    {
        public int Identify { get; private set; }
        public virtual string DisplayName { get { return null; } }

        public abstract void OnStart();
        public abstract void OnStop();
        public abstract void OnUpdate(float deltaTime);

        public override bool EnableChild { get { return false; } }
        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            Identify = node.Identify;
        }
    }
}