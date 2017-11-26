using UnityEngine;

namespace DevilTeam.AI
{
    /// <summary>
    /// 基本行为节点
    /// </summary>
    public class BTBehaviourExec : IBTNode
    {
        private BehaviourGraph.BehaviourNode m_Node;

        public BTBehaviourExec(BehaviourGraph.BehaviourNode node)
        {
            m_Node = node;
        }

        public BehaviourTick OnBehaviourTick { get; set; }

        public IBTControlNode ParentNode { get; set; }

        public EBTNode NodeType { get { return EBTNode.behaviour; } }

        public int Priority
        {
            get { return m_Node.UserData.m_Priority; }
            set { m_Node.UserData.m_Priority = value; }
        }

        public IBTNode LeafNode { get { return this; } }

        public bool OverrideState { get { return true; } }

        public object Output { get { return m_Node.UserData.m_OutputData; } }

        public void OnVisit()
        {
        }

        public EBTState OnTick()
        {
            EBTState stat = OnBehaviourTick == null ? EBTState.success : OnBehaviourTick(m_Node.UserData);
            return stat;
        }
    }

}
