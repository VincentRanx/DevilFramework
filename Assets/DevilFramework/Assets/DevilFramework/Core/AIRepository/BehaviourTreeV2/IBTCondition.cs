using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public interface IBTCondition
    {
        bool IsSuccess(BehaviourTreeRunner behaviourTree);
    }
}