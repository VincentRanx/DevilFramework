using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public interface IBTDecorator
    {
        bool IsSuccess(BehaviourTreeRunner behaviourTree);
    }
}