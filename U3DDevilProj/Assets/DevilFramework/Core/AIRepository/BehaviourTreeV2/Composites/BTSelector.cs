namespace Devil.AI
{
    [BTComposite(Title = "选择", Detail = "SELECTOR", IconPath = "Assets/DevilFramework/Editor/Icons/selector.png")]
    public class BTSelector : BTNodeBase
    {
        int mVisitIndex;
        bool mAbort;

        public BTSelector(int id) : base(id)
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
            if (state == EBTTaskState.success)
            {
                this.State = EBTTaskState.success;
            }
            else
            {
                mVisitIndex++;
                if (mVisitIndex >= ChildLength || !IsOnCondition(btree))
                    this.State = EBTTaskState.faild;
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
                    this.State = EBTTaskState.faild;
            }
        }

        protected override void OnAbort(BehaviourTreeRunner btree)
        {
            mAbort = true;
        }
    }
}