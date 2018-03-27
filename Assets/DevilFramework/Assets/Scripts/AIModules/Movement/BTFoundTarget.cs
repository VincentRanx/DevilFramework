using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTFoundTarget : BTConditionBase
{
    float mViewDistance;
    float mRadiaus;

    public BTFoundTarget(int id) : base(id) { }

    public override bool IsTaskOnCondition(BehaviourTreeRunner behaviourTree)
    {
        return behaviourTree.GetComponent<TestMono>().m_Target != null;
        //Ray ray = new Ray(behaviourTree.transform.position, behaviourTree.transform.forward);
        //bool ret = Physics.SphereCast(ray, mRadiaus, mViewDistance);
        //Debug.DrawRay(ray.origin, ray.direction.normalized * mViewDistance, ret ? Color.green : Color.red);
        //return ret;
    }

    public override bool IsTaskRunnable(BehaviourTreeRunner behaviourTree)
    {
        return behaviourTree.GetComponent<TestMono>().m_Target != null;
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mViewDistance = obj.Value<float>("distance");
        mRadiaus = obj.Value<float>("radiaus");
    }
}
