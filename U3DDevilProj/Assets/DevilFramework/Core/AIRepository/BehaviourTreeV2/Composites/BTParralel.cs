using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Devil.AI
{
    [BTComposite(Title = "并行", Detail = "PARRALEL", IconPath = "Assets/DevilFramework/Editor/Icons/parralel.png")]
    public class BTParralel: BTNodeBase
    {
        [BTVariable(Name = "mainTaskIndex", DefaultVallue = "0")]
        int mMainTaskIndex;
        BehaviourLooper[] mLoopers;
        BehaviourLooper mMainLooper;

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

        public override void InitData(BehaviourTreeRunner btree, string jsonData)
        {
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            mMainTaskIndex = obj.Value<int>("mainTaskIndex");
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

        //public override void Reset()
        //{
        //    base.Reset();
        //    for (int i = 0; i < mLoopers.Length; i++)
        //    {
        //        mLoopers[i].Reset();
        //    }
        //}

        protected override void OnReturnWithState(BehaviourTreeRunner btree, EBTTaskState state)
        {

        }

        protected override void OnStartLoop(BehaviourTreeRunner behaviourTree)
        {
            State = ChildLength > 0 ? EBTTaskState.running : EBTTaskState.success;
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (mLoopers[i] != null)
                    mLoopers[i].Reset();
            }
            if (mMainTaskIndex < mLoopers.Length && mMainTaskIndex >= 0)
                mMainLooper = mLoopers[mMainTaskIndex];
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
            if (mMainLooper != null)
            {
                State = mMainLooper.State;
            }
            else
            {
                State = (num == 0) ? EBTTaskState.success : EBTTaskState.running;
            }
        }

        protected override void OnAbort(BehaviourTreeRunner btree)
        {
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (mLoopers[i] == null || mLoopers[i].IsComplate)
                    continue;
                mLoopers[i].Abort(btree);
            }
        }
    }
}