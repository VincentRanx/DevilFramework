using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExt
{
    [BehaviourTree(DisplayName = "等待一段时间",SubTitle = "等待 {time} 秒后继续其他任务\n中断返回 {abort}", InputDatas = "time:row,abort:text")]
    public class BTCoolDownTask : IBTTask
    {
        float time;
        float mWaitTime;
        bool mAbortSucees;

        public bool AbortWithSuccess()
        {
            return mAbortSucees;
        }

        public void OnInitData(string jsonData)
        {
            JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
            if (obj != null)
            {
                mWaitTime = obj.Value<float>("time");
                mAbortSucees = obj.Value<string>("abort") == "success";
            }
        }

        public EBTTaskState OnStartTask(BehaviourTreeRunner behaviourTree)
        {
            time = mWaitTime;
            return EBTTaskState.running;
        }

        public EBTTaskState OnTaskTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            time -= deltaTime;
            return time <= 0 ? EBTTaskState.success : EBTTaskState.running;
        }

    }
}