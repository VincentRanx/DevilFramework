using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CheerupTask : BTTaskBase
{
    public CheerupTask(int id) : base(id) { }
    ThirdPersonCharacter mCharactor;

    public override void OnAbort(BehaviourTreeRunner btree)
    {

    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mCharactor = btree.GetComponent<ThirdPersonCharacter>();
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
    {
        btree.GetComponent<NavMeshAgent>().updatePosition = false;
        mCharactor.Move(Vector3.zero, false, false);
        return EBTTaskState.running;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
    {
        if (mCharactor.IsGrounded && btree.TaskTime > 0.2f)
            mCharactor.Move(Vector3.zero, false, true);
        return btree.TaskTime > 3 ? EBTTaskState.success : EBTTaskState.running;
    }
}
