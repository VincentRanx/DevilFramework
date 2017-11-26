using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{
    /// <summary>
    /// 序列控制节点
    /// </summary>
    public class BTSequenceExec : BTCtrlNodeBase
    {
        public BTSequenceExec(BehaviourGraph.BehaviourNode node) : base(node)
        {
        }

        public override EBTNode NodeType { get { return EBTNode.sequence; } }

        public override void ReturnState(EBTState state)
        {
            if (state != EBTState.running)
            {
                m_Current++;
                if (state == EBTState.failed)
                {
                    m_State = EBTState.failed;
                }
                else
                {
                    m_State = m_Current < ChildrenCount ? EBTState.running : EBTState.success;
                }
            }
            else
            {
                m_State = EBTState.running;
            }
        }
    }
}