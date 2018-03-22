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

    [CreateAssetMenu(fileName = "New Behaviour Asset", menuName = "AI/Behaviour Asset")]
    public class BehaviourTreeAsset : ScriptableObject
    {

        [System.Serializable]
        public class Comment
        {
            public string m_Comment;
            public Rect m_Rect;
        }

        [System.Serializable]
        public class BTData
        {
            public int m_Id;
            public Vector2 m_Pos;
            public string m_Name;
            public string m_JsonData;
            public EBTNodeType m_Type;
            public int[] m_Children;
            public int[] m_Conditions;
            public int[] m_Services;

            public BTNodeBase Instantiate(BehaviourTreeAsset asset)
            {
                BTNodeBase node = null;
                switch (m_Type)
                {
                    case EBTNodeType.task:
                        node = new BTTask(m_Id, BehaviourLibrary.NewTask(m_Name));
                        break;
                    case EBTNodeType.controller:
                        node = BehaviourLibrary.NewController(m_Name, m_Id);
                        break;
                    default:
                        break;
                }
                if (node != null)
                {
                    node.InitData(m_JsonData);
                    node.InitDecoratorSize(m_Conditions == null ? 0 : m_Conditions.Length, m_Children == null ? 0 : m_Children.Length, m_Services == null ? 0 : m_Services.Length);
                    for (int i = 0; i < node.DecoratorLength; i++)
                    {
                        BTData data = asset.GetDataById(m_Conditions[i]);
                        if (data == null)
                            continue;
                        IBTCondition cond = BehaviourLibrary.NewCondition(data.m_Name);
                        if (cond != null)
                        {
                            cond.OnInitData(data.m_JsonData);
                            node.SetCondition(i, cond);
                        }
                    }
                    for (int i = 0; i < node.ServiceLength; i++)
                    {
                        BTData data = asset.GetDataById(m_Services[i]);
                        if (data == null)
                            continue;
                        IBTService serv = BehaviourLibrary.NewService(data.m_Name);
                        if (serv != null)
                        {
                            serv.OnInitData(data.m_JsonData);
                            node.SetService(i, serv);
                        }
                    }
                }
                return node;
            }
        }

        [HideInInspector]
        public BTData[] m_Datas = new BTData[0];
        [HideInInspector]
        public int m_RootNodeId;
        [HideInInspector]
        public bool m_Sorted;
        [HideInInspector]
        public Comment[] m_Comments = new Comment[0];

        public BTData GetDataById(int id)
        {
            if (m_Sorted)
            {
                return GlobalUtil.Binsearch(m_Datas, (x) => m_Datas[x].m_Id - id, 0, m_Datas.Length);
            }
            else
            {
                return GlobalUtil.Find(m_Datas, (x) => x.m_Id == id);
            }
        }

        /// <summary>
        /// 构造行为树
        /// </summary>
        /// <param name="runner"></param>
        /// <returns></returns>
        public BTNodeBase CreateBehaviourTree(BehaviourTreeRunner runner)
        {
            BTData root = GetDataById(m_RootNodeId);
            if (root == null)
                return null;
            BTNodeBase rootNode = InstBTNode(root);
            return rootNode;
        }

        BTNodeBase InstBTNode(BTData info)
        {
            BTNodeBase node = info.Instantiate(this);
            if (node != null)
            {
                for (int i = 0; i < info.m_Children.Length; i++)
                {
                    BTData child = GetDataById(info.m_Children[i]);
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