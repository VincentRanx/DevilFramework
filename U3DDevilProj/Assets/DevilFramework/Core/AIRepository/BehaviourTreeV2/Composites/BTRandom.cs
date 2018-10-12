using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "随机", Detail = "随机 {times} 次子节点", IconPath = "Assets/DevilFramework/Editor/Icons/random.png")]
    public class BTRandom : BTNodeBase
    {
        [BTVariable(Name = "times", DefaultVallue = "1")]
        int mTimes = 1;
        int mVisitIndex;
        bool mAbort;
        int[] mRandomIndex;
        EBTTaskState mState;

        public BTRandom(int id) : base(id)
        {
        }

        public override void InitData(BehaviourTreeRunner btree, string jsonData)
        {
            JObject dt = JsonConvert.DeserializeObject<JObject>(jsonData);
            mTimes = dt.Value<int>("times");
        }

        public override void InitDecoratorSize(int conditionLen, int childLen, int serviceLen)
        {
            base.InitDecoratorSize(conditionLen, childLen, serviceLen);
            mRandomIndex = new int[childLen];
            for(int i = 0; i < childLen; i++)
            {
                mRandomIndex[i] = i;
            }
        }

        public override BTNodeBase ChildForVisit
        {
            get
            {
                if (!mAbort && State == EBTTaskState.running && mVisitIndex < mTimes && ChildLength > 0)
                {
                    return ChildAt(mRandomIndex[mVisitIndex % mTimes]);
                }
                else
                {
                    return null;
                }
            }
        }

        protected override void OnReturnWithState(BehaviourTreeRunner btree, EBTTaskState state)
        {
            mState = state == EBTTaskState.faild ? EBTTaskState.faild : EBTTaskState.success;
            mVisitIndex++;
            if (mVisitIndex >= mTimes)
                this.State = mState;
            else if (!IsOnCondition(btree))
                this.State = EBTTaskState.faild;
            if (mVisitIndex > ChildLength && ChildLength > 0)
                RandomIndex();
        }

        void RandomIndex()
        {
            for (int i = mRandomIndex.Length >> 1; i >= 0; i--)
            {
                int p = Mathf.Min(mRandomIndex.Length - 1, (int)(Random.value * mRandomIndex.Length));
                int a = mRandomIndex[i];
                mRandomIndex[i] = mRandomIndex[p];
                mRandomIndex[p] = a;
            }
        }

        protected override void OnStartLoop(BehaviourTreeRunner behaviourTree)
        {
            mAbort = false;
            mVisitIndex = 0;
            mState = EBTTaskState.success;
            RandomIndex();
            this.State = mVisitIndex < mTimes && ChildLength > 0 ? EBTTaskState.running : mState;
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
                if (mVisitIndex >= mTimes)
                    this.State = mState;
            }
        }

        protected override void OnAbort(BehaviourTreeRunner btree)
        {
            mAbort = true;
        }
    }
}