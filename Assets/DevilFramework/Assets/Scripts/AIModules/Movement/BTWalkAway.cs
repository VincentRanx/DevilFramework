using Devil.AI;
using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTWalkAway : BTTaskBase
{
    Vector3 mTargetPos;

    public BTWalkAway(int id) : base(id) { }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        
    }

    public override bool OnTaskAbortAndReturnSuccess(BehaviourTreeRunner behaviourTree)
    {
        return false;
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner behaviourTree)
    {
        Vector2 v = Random.insideUnitCircle;
        Vector3 p = Quaternion.AngleAxis(v.x * 360, Vector3.up) * behaviourTree.transform.forward * (10 + 20 * v.y);
        mTargetPos = p + behaviourTree.transform.position;
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner behaviourTree, float deltaTime)
    {
        float ang = GlobalUtil.RotateAngleFromTo(behaviourTree.transform.forward, mTargetPos - behaviourTree.transform.position, Vector3.up);

        if (Mathf.Abs(ang) > 1)
        {
            behaviourTree.transform.Rotate(Vector3.up, Mathf.MoveTowards(0, ang, 20 * deltaTime));
            return EBTTaskState.running;
        }
        else
        {
            Vector3 p = Vector3.MoveTowards(behaviourTree.transform.position, mTargetPos, 3 * deltaTime);
            behaviourTree.transform.position = p;
            return Vector3.Distance(p, mTargetPos) < 0.2f ? EBTTaskState.success : EBTTaskState.running;
        }
    }
}
