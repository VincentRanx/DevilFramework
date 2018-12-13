namespace Devil.AI
{
    public interface IBTCondition: ICondition, IIdentified { }

    public abstract class BTConditionAsset : BTAsset, IBTCondition
    {
        public int Identify { get; private set; }
        public virtual string DisplayName { get { return null; } }
        public abstract bool IsSuccess { get; }
        public override bool EnableChild { get { return false; } }
        
        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            Identify = node.Identify;
        }
    }
}