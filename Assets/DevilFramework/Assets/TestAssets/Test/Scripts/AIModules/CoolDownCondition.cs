using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolDownCondition : BTConditionBase
{
    [BTVariable]
    float mTime;

    public CoolDownCondition(int id) : base(id) { }

    public override bool IsTaskOnCondition(BehaviourTreeRunner btree)
    {
        return mTime >= btree.TaskTime;
    }

    public override bool IsTaskRunnable(BehaviourTreeRunner btree)
    {
        return true;
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mTime = obj.Value<float>("mTime");
    }
}
