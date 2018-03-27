namespace Devil.AI
{
    [BTComposite(Title = "并行", Detail = "PARRALEL", IconPath = "Assets/DevilFramework/Editor/Icons/parralel.png")]
    public class BTParralel: BTNodeBase
    {
        BehaviourLooper[] mLoopers;

        public BTParralel(int id) : base(id)
        {
        }

        public override BTNodeBase ChildForVisit
        {
            get
            {
                return null;
            }
        }

        public override void InitDecoratorSize(int conditionLen, int childLen, int serviceLen)
        {
            base.InitDecoratorSize(conditionLen, childLen, serviceLen);
            mLoopers = new BehaviourLooper[childLen];
        }

        public override void SetChild(int index, BTNodeBase node)
        {
            base.SetChild(index, node);
            if(node != null)
                mLoopers[index] = new BehaviourLooper(node);
        }

        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < mLoopers.Length; i++)
            {
                mLoopers[i].ResetTreeState();
            }
        }

        protected override void OnReturnWithState(EBTTaskState state)
        {

        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            State = ChildLength > 0 ? EBTTaskState.running : EBTTaskState.success;
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (mLoopers[i] != null)
                    mLoopers[i].Reset();
            }
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            int num = 0;
            for(int i = 0; i < mLoopers.Length; i++)
            {
                if (mLoopers[i] == null || mLoopers[i].IsComplate)
                    continue;
                mLoopers[i].Update(behaviourTree, deltaTime);
                num++;
            }
            if (num == 0)
                State = EBTTaskState.success;
        }

        public override bool AbortAndReturnSuccess(BehaviourTreeRunner behaviourTree)
        {
            return true;
        }
    }
}