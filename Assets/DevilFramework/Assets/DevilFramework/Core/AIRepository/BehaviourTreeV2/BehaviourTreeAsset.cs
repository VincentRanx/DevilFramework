using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Devil.AI
{
    public enum EBTNodeType
    {
        selector = 1,
        sequence = 2,
        parallel = 3,
        task = 4,
        plugin = 5,
    }

    public abstract class BehaviourTreeAsset : ScriptableObject
    {

        [System.Serializable]
        public class BTNodeInfo
        {
            public int m_Id;
            public Vector2 m_Pos;
            public string m_Name;
            public EBTNodeType m_Type;
            public int[] m_Children;
            public string[] m_Decorators;
            public string[] m_Services;

            public BTNodeBase CreateNodeInstance(BehaviourTreeAsset asset)
            {
                BTNodeBase node = null;
                //TODO
                switch (m_Type)
                {
                    case EBTNodeType.selector:
                        node = new BTSelector(m_Id, m_Decorators == null ? 0 : m_Decorators.Length, m_Children == null ? 0 : m_Children.Length, m_Services == null ? 0 : m_Services.Length);
                        break;
                    case EBTNodeType.sequence:
                        node = new BTSequence(m_Id, m_Decorators == null ? 0 : m_Decorators.Length, m_Children == null ? 0 : m_Children.Length, m_Services == null ? 0 : m_Services.Length);
                        break;
                    case EBTNodeType.parallel:
                        //node = new BTSelector(m_Decorators == null ? 0 : m_Decorators.Length, m_Children == null ? 0 : m_Children.Length, m_Services == null ? 0 : m_Services.Length);
                        break;
                    case EBTNodeType.task:
                        node = new BTTask(m_Id, asset.GetTaskByName(m_Name), m_Decorators == null ? 0 : m_Decorators.Length, m_Children == null ? 0 : m_Children.Length, m_Services == null ? 0 : m_Services.Length);
                        break;
                    case EBTNodeType.plugin:
                        //node = new BTSelector(m_Decorators == null ? 0 : m_Decorators.Length, m_Children == null ? 0 : m_Children.Length, m_Services == null ? 0 : m_Services.Length);
                        break;
                    default:
                        break;
                }
                if(node != null)
                {
                    for (int i = 0; i < node.DecoratorLength; i++)
                    {
                        node.SetDecorator(i, asset.GetDecoratorByName(m_Decorators[i]));
                    }
                    for(int i = 0; i < node.ServiceLength; i++)
                    {
                        node.SetService(i, asset.GetServiceByName(m_Services[i]));
                    }
                }
                return node;
            }
        }

        // 使用该行为树的对象共享实例
        public bool m_SharedInstance;

        [HideInInspector]
        public BTNodeInfo[] m_Nodes = new BTNodeInfo[0];
        [HideInInspector]
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
            for(int i = 0; i < info.m_Children.Length; i++)
            {
                BTNodeInfo child = GetNodeById(info.m_Children[i]);
                if(child != null)
                {
                    BTNodeBase cnode = InstBTNode(child);
                    node.SetChild(i, cnode);
                }
            }
            return node;
        }

        public abstract void GetDecoratorNames(ICollection<string> decorators);

        public abstract IBTDecorator GetDecoratorByName(string decorator);

        public abstract void GetServiceNames(ICollection<string> services);

        public abstract IBTService GetServiceByName(string service);

        public abstract void GetTaskNames(ICollection<string> tasks);

        public abstract IBTTask GetTaskByName(string taskName);
        
    }
}