using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTCoolDownCondition : BTConditionBase
{

    float mCoolTime;
    float mTime;
    bool mFirst;

    public BTCoolDownCondition(int id) : base(id) { mFirst = true; }

    public override bool IsTaskOnCondition(BehaviourTreeRunner behaviourTree)
    {
        mTime = behaviourTree.BehaviourTime;
        return true;
    }

    public override bool IsTaskRunnable(BehaviourTreeRunner behaviourTree)
    {
        if (mFirst)
        {
            mFirst = false;
            return true;
        }
        return behaviourTree.BehaviourTime - mTime > mCoolTime;
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mCoolTime = obj.Value<float>("interval");
    }
}
