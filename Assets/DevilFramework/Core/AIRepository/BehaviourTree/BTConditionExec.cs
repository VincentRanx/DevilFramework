using UnityEngine;

namespace DevilTeam.AI
{
    /// <summary>
    /// 基本条件节点
    /// </summary>
    public class BTConditionExec : IBTNode
    {
        private BehaviourGraph.BehaviourNode m_Node;
        public ConditionTick IsOnCondition { get; set; }

        public BTConditionExec(BehaviourGraph.BehaviourNode node)
        {
            m_Node = node;
        }

        public IBTControlNode ParentNode { get; set; }

        public EBTNode NodeType { get { return EBTNode.condition; } }

        public IBTNode LeafNode { get { return this; } }

        public object Output { get { return m_Node.UserData.m_OutputData; } }
        
        public int Priority { get { return m_Node.UserData.m_Priority; } set { m_Node.UserData.m_Priority = value; } }

        public EBTState OnTick()
        {
            EBTState stat = OnCondition() ? EBTState.success : EBTState.failed;
            return stat;
        }

        public void OnVisit()
        {
        }

        protected bool OnCondition()
        {
            return IsOnCondition == null ? true : IsOnCondition(m_Node.UserData);
        }
    }

}
