using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "等待", Detail = "等待{timeForWait}~{maxTime}秒钟\n始终返回成功：{alwaysSuccess}", HideProperty = true)]
    public class BTWaitTask : BTTaskBase
    {
        [BTVariable(Name = "timeForWait", DefaultVallue = "1")]
        float mWaitTime = 1;
        [BTVariable(Name = "maxTime", DefaultVallue = "0")]
        float mMaxWaitTime = 1;
        [BTVariable(Name = "alwaysSuccess", DefaultVallue = "false")]
        bool mAlwaysSucces;

        bool mAbort;
        float mTime;

        public BTWaitTask(int id) : base(id) { }

        public override void OnAbort(BehaviourTreeRunner btree)
        {
            mAbort = true;
        }

        public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
        {
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            mWaitTime = obj.Value<float>("timeForWait");
            mMaxWaitTime = obj.Value<float>("maxTime");
            mAlwaysSucces = obj.Value<bool>("alwaysSuccess");
        }

        public override void OnClearData(BehaviourTreeRunner btree)
        {
        }

        public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
        {
            mAbort = false;
            if(mMaxWaitTime > mWaitTime)
                mTime = Random.Range(mWaitTime, mMaxWaitTime);
            else
                mTime = mWaitTime;
            return EBTTaskState.running;
        }

        public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
        {
            if (mAbort)
                return mAlwaysSucces ? EBTTaskState.success : EBTTaskState.faild;
            return btree.TaskTime >= mTime ? EBTTaskState.success : EBTTaskState.running;
        }
    }
}