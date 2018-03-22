using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public interface IBTCondition
    {
        void OnInitData(string jsonData);

        bool IsTaskRunnable(BehaviourTreeRunner behaviourTree);

        bool IsTaskOnCondition(BehaviourTreeRunner behaviourTree);
    }
}