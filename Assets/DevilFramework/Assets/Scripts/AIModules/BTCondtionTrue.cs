using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTCondtionTrue : IBTCondition
{
    public bool IsSuccess(BehaviourTreeRunner behaviourTree)
    {
        Debug.Log("test: " + behaviourTree.TaskTime);
        return behaviourTree.TaskTime < 2;
    }
}
