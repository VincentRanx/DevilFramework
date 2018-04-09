using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTask : BTTaskBase
{
    public LookAtTask(int id) : base(id) { }

    bool mAbort;
    BTBlackboardGetter<Transform> mTarget;
    PlayerController mPlayer;

    public override void OnAbort(BehaviourTreeRunner btree)
    {
        mAbort = true;
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mTarget = btree.Blackboard.Getter<Transform>("target");
        mPlayer = btree.GetComponent<PlayerController>();
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
    {
        mAbort = false;
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
    {
        if (!mTarget.IsSet || mTarget.Value == null)
            return EBTTaskState.faild;
        Vector3 p = mTarget.Value.position + Vector3.up * 1.6f;
        mPlayer.LookAt(p);
        return EBTTaskState.running;
    }
}
