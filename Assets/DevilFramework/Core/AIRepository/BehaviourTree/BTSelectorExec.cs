using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{
    /// <summary>
    /// 选择控制节点
    /// </summary>
    public class BTSelectorExec : BTCtrlNodeBase
    {
        public BTSelectorExec(BehaviourGraph.BehaviourNode node):base(node)
        {
        }

        public override EBTNode NodeType { get { return EBTNode.selector; } }

        public override void ReturnState(EBTState state)
        {
            if (state != EBTState.running)
            {
                m_Current++;
                if(state == EBTState.success)
                {
                    m_State = EBTState.success;
                }
                else
                {
                    m_State = m_Current < ChildrenCount ? EBTState.running : EBTState.failed;
                }
            }
            else
            {
                m_State = EBTState.running;
            }
        }
    }
}