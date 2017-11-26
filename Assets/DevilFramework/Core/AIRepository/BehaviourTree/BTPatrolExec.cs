using UnityEngine;

namespace DevilTeam.AI
{
    /// <summary>
    /// 基本行为节点
    /// </summary>
    public class BTPatrolExec : IBTControlNode
    {
        private BehaviourGraph.BehaviourNode m_Node;
        private IBTNode m_PatrolNode;
        private EBTState m_PatrolState;

        public BTPatrolExec(BehaviourGraph.BehaviourNode node)
        {
            m_Node = node;
        }

        public BehaviourTick OnBehaviourTick { get; set; }

        public IBTControlNode ParentNode { get; set; }

        public EBTNode NodeType { get { return EBTNode.patrol; } }

        public object Output { get { return m_Node.UserData.m_OutputData; } }

        public int Priority
        {
            get
            {
                if (m_PatrolNode == null)
                    return m_Node.UserData.m_Priority;
                else
                    return m_Node.UserData.m_Priority + m_PatrolNode.Priority;
            }
            set { m_Node.UserData.m_Priority = value; }
        }

        public IBTNode LeafNode
        {
            get
            {
                return m_PatrolNode == null ? this : m_PatrolNode.LeafNode;
            }
        }

        public bool OverrideState { get { return true; } }

        public int ChildrenCount { get { return m_PatrolNode == null ? 0 : 1; } }

        public IBTNode GetChildAt(int index)
        {
            return index == 0 ? m_PatrolNode : null;
        }

        public void AddChild(IBTNode node)
        {
            m_PatrolNode = node;
        }

        public void ReturnState(EBTState state)
        {
            m_PatrolState = state;
        }

        public void OnVisit()
        {
            m_PatrolState = EBTState.success;
        }

        public EBTState OnTick()
        {
            if (m_PatrolState == EBTState.success)
            {
                EBTState stat = OnBehaviourTick == null ? EBTState.success : OnBehaviourTick(m_Node.UserData);
                return stat;
            }
            else
            {
                return EBTState.failed;
            }
        }
    }

}
