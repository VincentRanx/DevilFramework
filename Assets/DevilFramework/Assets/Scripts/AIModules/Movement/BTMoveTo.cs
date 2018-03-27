using Devil.AI;
using Devil.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BTMoveTo : BTTaskBase
{
    [BTVariable]
    float mSpeed = 1;
    [BTVariable]
    float mRadiaus = 2;
    BTBlackboardGetter<Transform> mTarget;

    public BTMoveTo(int id) : base(id) { }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mTarget = btree.Blackboard.Getter<Transform>("target");
        JObject jobj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mSpeed = jobj.Value<float>("mSpeed");
        mRadiaus = jobj.Value<float>("mRadiaus");
    }

    public override bool OnTaskAbortAndReturnSuccess(BehaviourTreeRunner behaviourTree)
    {
        return false;
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner behaviourTree)
    {
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner behaviourTree, float deltaTime)
    {
        Transform tar = mTarget.GetValue();
        if (tar == null)
            return EBTTaskState.faild;
        float dis = Vector3.Distance(tar.transform.position, behaviourTree.transform.position);
        if (dis < mRadiaus)
            return EBTTaskState.success;
        Vector3 p = Vector3.MoveTowards(behaviourTree.transform.position, tar.transform.position, mSpeed * deltaTime);
        behaviourTree.transform.position = p;
        return EBTTaskState.running;
    }
}
