using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BehaviourTree(DisplayName = "搜索附近敌人")]
public class BTFindTargetArroundPlayerService : IBTService
{
    public int LiveCounter { get; set; }

    public void OnServiceTick(BehaviourTreeRunner blackboard, float deltaTime)
    {
        
    }

    public void OnStartService(BehaviourTreeRunner blackboard)
    {
        
    }
}
