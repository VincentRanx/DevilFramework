using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace DevilTeam.AI
{
    public class BehaviourTree
    {

#if UNITY_EDITOR
        public BehaviourGraph __graph;
#endif
        private string m_Name;
        // 根节点
        private IBTControlNode m_Root;
        public IBTControlNode RootNode { get { return m_Root; } }

        private IBTControlNode m_CurrentNode;
        public string Name { get { return m_Name; } }

        public BehaviourTree(string name = "Default Behaviour Tree")
        {
            m_Name = name;
        }

        // 初始化根节点
        public void InitWith(IBTControlNode rootNode)
        {
            m_Root = rootNode;
            m_CurrentNode = null;
        }

        // 访问所有节点，可以作为初始化节点使用
        public void Visit()
        {
            m_CurrentNode = m_Root;
            if (m_CurrentNode != null)
                m_CurrentNode.OnVisit();
        }

        // 在 Update 中更新
        public void OnTick()
        {
            if (m_CurrentNode == null)
            {
                Visit();
            }
            if (m_CurrentNode == null)
            {
                return;
            }
            EBTState state;
            EBTState ctrlState;
            IBTControlNode ctrl;
            do
            {
                IBTNode leaf = m_CurrentNode.LeafNode;
                ctrl = leaf.ParentNode;
                state = leaf.OnTick();
#if UNITY_EDITOR
                TickNode(leaf, state, 1);
#endif
                ctrlState = state;
                while (ctrlState != EBTState.running && ctrl != null)
                {
                    ctrl.ReturnState(ctrlState);
                    ctrlState = ctrl.OnTick();
#if UNITY_EDITOR
                    if (ctrlState != EBTState.running || ctrl.OverrideState)
                        TickNode(ctrl, ctrlState, 1);
#endif
                    if (ctrlState != EBTState.running)
                    {
                        ctrl = ctrl.ParentNode;
                    }
                }
                if (ctrl != null && ctrl.OverrideState)
                    state = ctrlState;
                m_CurrentNode = ctrl;
            } while (state != EBTState.running && m_CurrentNode != null);
        }

#if UNITY_EDITOR
        void TickNode(IBTNode node, EBTState state, int ticks)
        {
            BehaviourGraph.BehaviourNode nd = Utility.Ref.GetField(node, "m_Node") as BehaviourGraph.BehaviourNode;
            if (nd != null)
            {
                nd.__ticks += ticks;
                nd.__resultStat = state;
            }
            IBTControlNode pnode = node.ParentNode;
            if (pnode != null)
            {
                TickNode(pnode, EBTState.running, 0);
            }
        }
#endif
    }

}
