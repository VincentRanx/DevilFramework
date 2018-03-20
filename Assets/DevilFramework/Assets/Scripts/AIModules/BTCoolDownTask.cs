using Devil.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameExt
{
    [BehaviourTree(DisplayName = "冷却")]
    public class BTCoolDownTask : IBTTask
    {
        float time;

        public EBTTaskState OnStartTask(BehaviourTreeRunner behaviourTree)
        {
            time = 3;
            return EBTTaskState.running;
        }

        public EBTTaskState OnTaskTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            time -= deltaTime;
            return time <= 0 ? EBTTaskState.success : EBTTaskState.running;
        }

    }
}