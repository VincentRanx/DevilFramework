using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTFindTargetService : IBTService
{
    public int LiveCounter { get; set; }

    public void OnServiceTick(BehaviourTreeRunner blackboard, float deltaTime)
    {
    }

    public void OnStartService(BehaviourTreeRunner blackboard)
    {
    }
}
