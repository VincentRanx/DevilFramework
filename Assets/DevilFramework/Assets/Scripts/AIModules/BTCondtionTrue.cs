using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[BehaviourTree(InputDatas = "time:raw", DisplayName = "持续一段时间", SubTitle = "{time} 秒之后终止任务")]
public class BTCondtionTrue : IBTCondition
{

    float time;

    public void OnInitData(string jsonData)
    {
        if(string.IsNullOrEmpty(jsonData))
        {
            time = 2;
            return;
        }
        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        time = obj.Value<float>("time");
        Debug.Log("init: " + time);
    }

    public bool IsTaskOnCondition(BehaviourTreeRunner behaviourTree)
    {
        Debug.Log("test: " + behaviourTree.TaskTime);
        return behaviourTree.TaskTime < time;
    }

    public bool IsTaskRunnable(BehaviourTreeRunner behaviourTree)
    {
        return true;
    }
}
