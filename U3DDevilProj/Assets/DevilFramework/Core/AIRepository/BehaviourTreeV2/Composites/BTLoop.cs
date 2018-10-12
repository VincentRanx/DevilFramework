namespace Devil.AI
{
    [BTComposite(Title = "循环", Detail = "LOOP", IconPath = "Assets/DevilFramework/Editor/Icons/loop.png")]
    public class BTLoop : BTNodeBase
    {
        int mVisitIndex;
        bool mAbort;

        public BTLoop(int id) : base(id)
        {
        }

        public override BTNodeBase ChildForVisit
        {
            get
            {
                if (!mAbort && State == EBTTaskState.running && mVisitIndex < ChildLength)
                {
                    return ChildAt(mVisitIndex);
                }
                else
                {
                    return null;
                }
            }
        }

        protected override void OnReturnWithState(BehaviourTreeRunner btree, EBTTaskState state)
        {
            if (!IsOnCondition(btree))
            {
                State = EBTTaskState.faild;
            }
            else
            {
                mVisitIndex++;
                if (mVisitIndex >= ChildLength)
                    mVisitIndex = 0;
            }
        }

        protected override void OnStartLoop(BehaviourTreeRunner behaviourTree)
        {
            mAbort = false;
            mVisitIndex = 0;
            this.State = mVisitIndex < ChildLength ? EBTTaskState.running : EBTTaskState.faild;
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            if (mAbort)
            {
                State = EBTTaskState.faild;
            }
            else
            {
                mVisitIndex++;
                if (mVisitIndex >= ChildLength)
                    mVisitIndex = 0;
            }
        }

        protected override void OnAbort(BehaviourTreeRunner btree)
        {
            mAbort = true;
        }
    }
}