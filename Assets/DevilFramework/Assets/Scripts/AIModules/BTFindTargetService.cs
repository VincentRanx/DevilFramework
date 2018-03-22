using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BehaviourTree(DisplayName = "搜索己方目标", InputDatas = "distance:raw,layer:text,tag:text")]
public class BTFindTargetService : IBTService
{
    public int LiveCounter { get; set; }

    public void OnInitData(string jsonData)
    {
    }

    public void OnServiceTick(BehaviourTreeRunner blackboard, float deltaTime)
    {
    }

    public void OnStartService(BehaviourTreeRunner blackboard)
    {
    }
}
