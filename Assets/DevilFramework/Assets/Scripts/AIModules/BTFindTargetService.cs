using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BTComposite(Title = "检测目标", Detail = "通过碰撞检查目标，并将目标设置为黑板的 target")]
public class BTFindTargetService : BTServiceBase
{
    [BTVariable(Name = "distance")]
    float mViewDistance;
    [BTVariable(Name = "radiaus")]
    float mViewRadiaus;

    BTBlackboardSetter<Transform> mTarget;

    public BTFindTargetService(int id) : base(id) { }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mTarget = btree.Blackboard.Setter<Transform>("target");
        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mViewDistance = obj.Value<float>("distance");
        mViewRadiaus = obj.Value<float>("radiaus");
    }

    public override void OnServiceTick(BehaviourTreeRunner behaviourTree, float deltaTime)
    {
        Ray ray = new Ray(behaviourTree.transform.position, behaviourTree.transform.forward);
        RaycastHit hit;
        bool ret = Physics.SphereCast(ray, mViewRadiaus, out hit, mViewDistance);
        Debug.DrawRay(ray.origin, ray.direction.normalized * mViewDistance, Color.red, deltaTime);
        if (ret)
        {
            mTarget.SetValue(hit.transform);
            //behaviourTree.GetComponent<TestMono>().m_Target = hit.collider.gameObject;
        }
        else
        {
            mTarget.UnsetValue();
            //behaviourTree.GetComponent<TestMono>().m_Target = null;
        }
    }

    public override void OnServiceStart(BehaviourTreeRunner blackboard)
    {
        Debug.Log(string.Format("Serv[{0}] started", Id));
    }

    public override void OnServiceStop(BehaviourTreeRunner btree)
    {
        Debug.Log(string.Format("Serv[{0}] stoped", Id));
    }
}
