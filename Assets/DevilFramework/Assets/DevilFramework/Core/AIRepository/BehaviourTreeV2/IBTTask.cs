using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public interface IBTTask
    {
        EBTTaskState OnStartTask(BehaviourTreeRunner behaviourTree);

        EBTTaskState OnTaskTick(BehaviourTreeRunner behaviourTree, float deltaTime);
    }
}