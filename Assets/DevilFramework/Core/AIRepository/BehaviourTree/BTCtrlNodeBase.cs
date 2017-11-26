using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{
    /// <summary>
    /// 选择控制节点
    /// </summary>
    public abstract class BTCtrlNodeBase : IBTControlNode
    {
        protected int m_Current;
        protected List<IBTNode> m_Children;
        protected EBTState m_State;
        protected BehaviourGraph.BehaviourNode m_Node;
        protected float m_VisitTime;

        public BTCtrlNodeBase(BehaviourGraph.BehaviourNode node)
        {
            m_Children = new List<IBTNode>();
            m_Node = node;
        }

        public abstract EBTNode NodeType { get; }

        public IBTNode LeafNode
        {
            get
            {
                if (m_State != EBTState.success && m_Current < ChildrenCount)
                {
                    IBTNode node = m_Children[m_Current];
                    return node.LeafNode ?? node;
                }
                else
                {
                    return this;
                }
            }
        }

        public int Priority
        {
            get
            {
                if (m_Current < m_Children.Count)
                    return Mathf.Max(m_Node.UserData.m_Priority, m_Children[m_Current].Priority);
                else
                    return m_Node.UserData.m_Priority;
            }
            set
            {
                m_Node.UserData.m_Priority = value;
            }
        }

        public bool OverrideState { get { return false; } }

        public IBTControlNode ParentNode { get; set; }

        public void AddChild(IBTNode node)
        {
            if(node != null)
            {
                m_Children.Add(node);
            }
        }

        public int ChildrenCount
        {
            get
            {
                return m_Children.Count;
            }
        }

        public IBTNode GetChildAt(int index)
        {
            return m_Children[index];
        }

        public EBTState OnTick()
        {
            return m_State;
        }

        public object Output { get { return m_Node.UserData.m_OutputData; } }

        public void OnVisit()
        {
            m_Current = 0;
            m_State = ChildrenCount > 0 ? EBTState.running : EBTState.failed;
            for (int i = 0; i < m_Children.Count; i++)
            {
                m_Children[i].OnVisit();
            }
            if (Time.time - m_VisitTime > m_Node.UserData.m_FloatData)
            {
                m_VisitTime = Time.time;
                if (m_Node.UserData.m_SortType == ESortType.desc)
                {
                    m_Children.Sort((x, y) => y.Priority - x.Priority);
                }
                else if (m_Node.UserData.m_SortType == ESortType.asc)
                {
                    m_Children.Sort((x, y) => x.Priority - y.Priority);
                }
                else if (m_Node.UserData.m_SortType == ESortType.random)
                {
                    m_Children.Sort((x, y) => Random.Range(-2, 2));
                }
            }
        }

        public abstract void ReturnState(EBTState state);
    }
}