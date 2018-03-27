using Devil.AI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[BTComposite(Title = "寻找目标", Detail = "通过自身的旋转发现目标(黑板：target)")]
public class BTFindTargetTask : BTTaskBase
{
    [BTVariable]
    float mRotSpeed;

    BTBlackboardGetter<Transform> mTarget; 

    public BTFindTargetTask(int id) : base(id) { }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mTarget = btree.Blackboard.Getter<Transform>("target");

        JObject obj = JsonConvert.DeserializeObject<JObject>(jsonData);
        mRotSpeed = obj.Value<float>("mRotSpeed");
    }

    public override bool OnTaskAbortAndReturnSuccess(BehaviourTreeRunner behaviourTree)
    {
        return mTarget.GetValue() != null;
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner behaviourTree)
    {
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner behaviourTree, float deltaTime)
    {
        if(mTarget.GetValue() == null)
        {
            behaviourTree.transform.Rotate(Vector3.up, mRotSpeed * deltaTime);
            return EBTTaskState.running;
        }
        else
        {
            return EBTTaskState.success;
        }
        
    }
}
