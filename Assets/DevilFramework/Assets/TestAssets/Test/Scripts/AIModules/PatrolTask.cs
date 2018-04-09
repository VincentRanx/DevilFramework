using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[BTComposite(Title = "巡逻", Detail = "寻找下一个检查点")]
public class PatrolTask : BTTaskBase
{

    bool mAbort;
    int mPtr;
    Transform[] mCheckPoints;

    BTBlackboardSetter<Vector3> mTarget;

    public PatrolTask(int id) : base(id) { }

    public override void OnAbort(BehaviourTreeRunner btree)
    {
        mAbort = true;
        Debug.Log("On Patrol Task Abort: " + Id);
    }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mTarget = btree.Blackboard.Setter<Vector3>("targetPos");
        GameObject obj = GameObject.Find("CheckPoints");
        if (obj)
        {
            Transform trans = obj.transform;
            mCheckPoints = new Transform[trans.childCount];
            for(int i = 0; i < mCheckPoints.Length; i++)
            {
                mCheckPoints[i] = trans.GetChild(i);
            }
            mPtr = Mathf.FloorToInt(Random.value * (mCheckPoints.Length - 0.1f));
        }
        else
        {
            mCheckPoints = new Transform[0];
        }
    }

    public override EBTTaskState OnTaskStart(BehaviourTreeRunner btree)
    {
        if (mCheckPoints.Length < 2)
            return EBTTaskState.faild;
        mPtr = (mPtr + 1) % mCheckPoints.Length;
        mTarget.SetValue(mCheckPoints[mPtr].position);
        return EBTTaskState.success;
    }

    public override EBTTaskState OnTaskTick(BehaviourTreeRunner btree, float deltaTime)
    {
        return EBTTaskState.success;
    }
}
