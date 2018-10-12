namespace Devil.AI
{
    [BTComposite(Title = "队列", Detail = "QUEUE", IconPath = "Assets/DevilFramework/Editor/Icons/parralel.png")]
    public class BTQueue : BTNodeBase
    {
        int mVisitIndex;
        bool mAbort;
        EBTTaskState mRetStat;

        public BTQueue(int id) : base(id)
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
            mRetStat = state;
            mVisitIndex++;
            if (mVisitIndex >= ChildLength)
                this.State = mRetStat;
            else if (!IsOnCondition(btree))
                State = EBTTaskState.faild;
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
                    this.State = mRetStat;
            }
        }

        protected override void OnAbort(BehaviourTreeRunner btree)
        {
            mAbort = true;
        }
    }
}