using Devil.Utility;
using UnityEngine;

namespace Devil.AI
{
    public enum EBTNodeType
    {
        invalid = 0,
        task = 4,
        controller = 5,
        condition = 6,
        service = 7,
    }

    [CreateAssetMenu(fileName = "New Behaviour Asset", menuName = "AI/Behaviour Tree")]
    public class BehaviourTreeAsset : ScriptableObject
    {

        [System.Serializable]
        public class Comment
        {
            public string m_Comment;
            public Rect m_Rect;
        }

        [System.Serializable]
        public class BTData : IIdentified
        {
            public int m_Id;
            public Vector2 m_Pos;
            public bool m_NotFlag;
            public string m_Name;
            public string m_JsonData;
            public EBTNodeType m_Type;
            public int[] m_Children;
            public int[] m_Conditions;
            public int[] m_Services;

            public int Identify { get { return m_Id; } }

            public BTNodeBase Instantiate(BehaviourTreeRunner btree, BehaviourTreeAsset asset)
            {
                BTNodeBase node = null;
                if (m_Type == EBTNodeType.task)
                    node = new BTTask(m_Id, BehaviourLibrary.NewTask(m_Name, m_Id));
                else if (m_Type == EBTNodeType.controller)
                    node = BehaviourLibrary.NewController(m_Name, m_Id);
                if (node != null)
                {
                    node.InitData(btree, m_JsonData);
                    node.InitDecoratorSize(m_Conditions == null ? 0 : m_Conditions.Length, m_Children == null ? 0 : m_Children.Length, m_Services == null ? 0 : m_Services.Length);
                    for (int i = 0; i < node.ConditionLength; i++)
                    {
                        BTData data = asset.GetDataById(m_Conditions[i]);
                        if (data == null)
                            continue;
                        node.SetNotFlag(i, data.m_NotFlag);
                        BTConditionBase cond = BehaviourLibrary.NewCondition(data.m_Name, data.m_Id);
                        if (cond != null)
                        {
                            cond.OnInitData(btree, data.m_JsonData);
                            node.SetCondition(i, cond);
                        }
                    }
                    for (int i = 0; i < node.ServiceLength; i++)
                    {
                        BTData data = asset.GetDataById(m_Services[i]);
                        if (data == null)
                            continue;
                        BTServiceBase serv = BehaviourLibrary.NewService(data.m_Name, data.m_Id);
                        if (serv != null)
                        {
                            serv.OnInitData(btree, data.m_JsonData);
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
                return GlobalUtil.Binsearch(m_Datas, id, 0, m_Datas.Length);
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
            try
            {
                BTNodeBase rootNode = InstBTNode(runner, root);
                return rootNode;
            }
            catch (System.Exception e)
            {
                RTLog.LogError(LogCat.AI, e.ToString());
                return null;
            }
        }
        
        BTNodeBase InstBTNode(BehaviourTreeRunner btree, BTData info)
        {
            BTNodeBase node = info.Instantiate(btree, this);
            if (node != null)
            {
                for (int i = 0; i < info.m_Children.Length; i++)
                {
                    BTData child = GetDataById(info.m_Children[i]);
                    if (child != null)
                    {
                        BTNodeBase cnode = InstBTNode(btree, child);
                        node.SetChild(i, cnode);
                    }
                }
            }
            return node;
        }

        /// <summary>
        /// 清理行为树
        /// </summary>
        /// <param name="btree"></param>
        /// <param name="root"></param>
        public void ClearBehaviourTree(BehaviourTreeRunner btree, BTNodeBase root)
        {
            if (root == null)
                return;
            root.ClearData(btree);
            for (int i = 0; i < root.ConditionLength; i++)
            {
                var cond = root.GetCondition(i);
                if (cond != null)
                    cond.OnClearData(btree);
            }
            for (int i = 0; i < root.ServiceLength; i++)
            {
                var serv = root.GetService(i);
                if (serv != null)
                    serv.OnClearData(btree);
            }
            for (int i = 0; i < root.ChildLength; i++)
            {
                var child = root.ChildAt(i);
                ClearBehaviourTree(btree, child);
            }
        }

    }
}