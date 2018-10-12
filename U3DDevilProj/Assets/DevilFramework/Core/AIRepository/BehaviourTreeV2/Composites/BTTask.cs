namespace Devil.AI
{
    public class BTTask : BTNodeBase
    {
        BTTaskBase mTask;

        public BTTask(int id, BTTaskBase task) : base(id)
        {
            mTask = task;
        }

        public override BTNodeBase ChildForVisit
        {
            get { return null; }
        }

        protected override void OnReturnWithState(BehaviourTreeRunner btree, EBTTaskState state)
        {
        }

        protected override void OnStartLoop(BehaviourTreeRunner behaviourTree)
        {
            if (mTask != null)
            {
                State = mTask.OnTaskStart(behaviourTree);
            }
            else
            {
                State = EBTTaskState.faild;
            }
        }

        public override void InitData(BehaviourTreeRunner btree, string jsonData)
        {
            if (mTask != null)
                mTask.OnInitData(btree, jsonData);
        }

        public override void ClearData(BehaviourTreeRunner btree)
        {
            if (mTask != null)
                mTask.OnClearData(btree);
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            if (mTask != null)
                State = mTask.OnTaskTick(behaviourTree, deltaTime);
        }
        
        protected override void OnAbort(BehaviourTreeRunner btree)
        {
            if (mTask != null)
                mTask.OnAbort(btree);
        }
    }
    
}