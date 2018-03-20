using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BehaviourTree(DisplayName = "随机")]
public class BTRandom : BTNodeBase
{
    public BTRandom(int id) : base(id)
    {

    }

    public override BTNodeBase ChildForVisit
    {
        get { return null; }
    }

    public override void ReturnWithState(EBTTaskState state)
    {
    }

    protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
    {
    }

    protected override void OnVisit(BehaviourTreeRunner behaviourTree)
    {
    }
}
