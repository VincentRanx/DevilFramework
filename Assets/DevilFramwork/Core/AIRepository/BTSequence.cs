using System.Collections.Generic;

namespace DevilTeam.AI
{
    public class BTSequence : IBTControlNode
    {
        private IBTControlNode m_Parent;
        private int m_Current;
        private List<IBTNode> m_Children;
        private BTState m_State;

        public int ChildrenCount
        {
            get
            {
                return m_Children == null ? 0 : m_Children.Count;
            }
        }

        public IBTNode LeafNode
        {
            get
            {
                if (m_Current < ChildrenCount)
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

        public IBTControlNode ParentNode
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        public void AddChild(IBTNode node)
        {
            if(node != null)
            {
                if (m_Children == null)
                    m_Children = new List<IBTNode>();
                m_Children.Add(node);
            }
        }

        public IBTNode GetChildAt(int index)
        {
            return m_Children[index];
        }

        public BTState OnTick()
        {
            return m_State;
        }

        public void OnVisit()
        {
            m_State = ChildrenCount > 0 ? BTState.running : BTState.failed;
        }

        public void ReturnState(BTState state)
        {
            if (state != BTState.running)
            {
                m_Current++;
                if(state == BTState.failed)
                {
                    m_State = BTState.failed;
                }
                else
                {
                    m_State = m_Current < ChildrenCount ? BTState.running : BTState.success;
                }
            }
            else
            {
                m_State = BTState.running;
            }
        }
    }
}