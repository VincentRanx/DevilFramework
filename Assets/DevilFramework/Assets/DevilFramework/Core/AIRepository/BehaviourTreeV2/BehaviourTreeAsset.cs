using Devil.Utility;
using UnityEngine;

namespace Devil.AI
{
    public enum EBTNodeType
    {
        task = 4,
        controller = 5,
        condition = 6,
        service = 7,
    }

    [CreateAssetMenu(fileName ="New Behaviour Asset", menuName = "AI/Behaviour Asset")]
    public class BehaviourTreeAsset : ScriptableObject
    {

        [System.Serializable]
        public class BTNodeInfo
        {
            public int m_Id;
            public Vector2 m_Pos;
            public string m_Name;
            public EBTNodeType m_Type;
            public int[] m_Children;
            public string[] m_Conditions;
            public string[] m_Services;

            public BTNodeBase CreateNodeInstance(BehaviourTreeAsset asset)
            {
                BTNodeBase node = null;
                switch (m_Type)
                {
                    case EBTNodeType.task:
                        node = new BTTask(m_Id, BehaviourLibrary.NewTask(m_Name));
                        break;
                    case EBTNodeType.controller:
                        node = BehaviourLibrary.NewPlugin(m_Name, m_Id);
                        break;
                    default:
                        break;
                }
                if(node != null)
                {
                    node.InitDecoratorSize(m_Conditions == null ? 0 : m_Conditions.Length, m_Children == null ? 0 : m_Children.Length, m_Services == null ? 0 : m_Services.Length);
                    for (int i = 0; i < node.DecoratorLength; i++)
                    {
                        node.SetCondition(i, BehaviourLibrary.NewCondition(m_Conditions[i]));
                    }
                    for(int i = 0; i < node.ServiceLength; i++)
                    {
                        node.SetService(i, BehaviourLibrary.NewService(m_Services[i]));
                    }
                }
                return node;
            }
        }

        // 使用该行为树的对象共享实例
        public bool m_SharedInstance;

        //[HideInInspector]
        public BTNodeInfo[] m_Nodes = new BTNodeInfo[0];
        //[HideInInspector]
        public int m_RootNodeId;
        [HideInInspector]
        public bool m_Sorted;

        public BehaviourTreeAsset GetNewOrSharedInstance()
        {
            if (m_SharedInstance)
                return this;
            else
                return Object.Instantiate(this);
        }

        public BTNodeInfo GetNodeById(int id)
        {
            if (m_Sorted)
            {
                return GlobalUtil.Binsearch(m_Nodes, (x) => m_Nodes[x].m_Id - id, 0, m_Nodes.Length);
            }
            else
            {
                return GlobalUtil.Find(m_Nodes, (x) => x.m_Id == id);
            }
        }

        /// <summary>
        /// 构造行为树
        /// </summary>
        /// <param name="runner"></param>
        /// <returns></returns>
        public BTNodeBase CreateBehaviourTree(BehaviourTreeRunner runner)
        {
            BTNodeInfo root = GetNodeById(m_RootNodeId);
            if (root == null)
                return null;
            BTNodeBase rootNode = InstBTNode(root);
            return rootNode;
        }

        BTNodeBase InstBTNode(BTNodeInfo info)
        {
            BTNodeBase node = info.CreateNodeInstance(this);
            if (node != null)
            {
                for (int i = 0; i < info.m_Children.Length; i++)
                {
                    BTNodeInfo child = GetNodeById(info.m_Children[i]);
                    if (child != null)
                    {
                        BTNodeBase cnode = InstBTNode(child);
                        node.SetChild(i, cnode);
                    }
                }
            }
            return node;
        }

    }
}