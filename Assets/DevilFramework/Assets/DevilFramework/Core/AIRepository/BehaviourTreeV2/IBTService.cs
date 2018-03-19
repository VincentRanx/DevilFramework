using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public interface IBTService
    {
        int LiveCounter { get; set; }

        void OnStartService(BehaviourTreeRunner blackboard);

        void OnServiceTick(BehaviourTreeRunner blackboard, float deltaTime);
    }
}