using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{
    public class BTComparableModule : BehaviourModule
    {
        public override IBTNode GetBehaviourNode(BehaviourGraph.BehaviourNode node)
        {
            return base.GetBehaviourNode(node);
        }

#if UNITY_EDITOR

        protected override IBTNode GetCustomBehaviourNode(BehaviourGraph.BehaviourNode node)
        {
            return base.GetCustomBehaviourNode(node);
        }
#endif
    }
}