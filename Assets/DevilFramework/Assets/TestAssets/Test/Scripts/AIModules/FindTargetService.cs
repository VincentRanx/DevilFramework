using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BTComposite(Title = "发现敌人", Detail = "搜索AI看到的敌人")]
public class FindTargetService : BTServiceBase
{
    PlayerController mPlayer;
    BTBlackboardSetter<Transform> mTargetSetter;
    BTBlackboardSetter<Vector3> mTargetPos;
    Transform mTarget;

    public FindTargetService(int id) : base(id) { }

    public override void OnInitData(BehaviourTreeRunner btree, string jsonData)
    {
        mPlayer = btree.GetComponent<PlayerController>();
        mTargetSetter = btree.Blackboard.Setter<Transform>("target");
        mTargetPos = btree.Blackboard.Setter<Vector3>("targetPos");
    }

    public override void OnServiceStart(BehaviourTreeRunner btree)
    {
        DoSearch();
    }

    public override void OnServiceStop(BehaviourTreeRunner btree)
    {
    }

    public override void OnServiceTick(BehaviourTreeRunner btree, float deltaTime)
    {
        DoSearch();
    }

    void DoSearch()
    {
        mTargetSetter.UnsetValue();
        mPlayer.FoundTarget = false;
        for (int i = 0; i < PlayerController.AllPlayers.Count; i++)
        {
            PlayerController player = PlayerController.AllPlayers[i];
            if (player == mPlayer)
                continue;
            Transform trans = player.transform;
            if (mPlayer.IsTargetInSight(trans))
            {
                mTarget = trans;
                mTargetSetter.SetValue(trans);
                mPlayer.FoundTarget = true;
                //mTargetPos.SetValue(trans.position);
                break;
            }
        }
    }
}
