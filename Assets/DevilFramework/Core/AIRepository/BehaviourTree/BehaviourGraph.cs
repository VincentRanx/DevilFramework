using DevilTeam.Utility;
using UnityEngine;

namespace DevilTeam.AI
{
    public class BehaviourGraph : BehaviourModule
    {
        [System.Serializable]
        public class BehaviourNode
        {
            [SerializeField]
            BehaviourModule m_Target;
            public BehaviourModule Target { get { return m_Target; } }
            [SerializeField]
            int m_Id;
            public int Id { get { return m_Id; } }

            [SerializeField]
            string m_DisplayName;
            public string DisplayName { get { return m_DisplayName ?? ""; } }

            [SerializeField]
            string m_ModuleName;
            public string ModuleName { get { return m_ModuleName ?? ""; } }

            [SerializeField]
            string m_Comment;
            public string Comment { get { return m_Comment ?? ""; } }

            [SerializeField]
            EBTNode m_BehaviourType;
            public EBTNode BehaviourType { get { return m_BehaviourType; } }

            [SerializeField]
            int m_ParentId;
            public int ParentId { get { return m_ParentId; } }

            [SerializeField]
            int[] m_ChildrenId;
            public int ChildrenCount { get { return m_ChildrenId == null ? 0 : m_ChildrenId.Length; } }
            public int GetChildId(int index) { return m_ChildrenId[index]; }

            [SerializeField]
            BTCustomData m_UseData;
            public BTCustomData UserData
            {
                get
                {
                    if (m_UseData == null)
                        m_UseData = new BTCustomData();
                    return m_UseData;
                }
            }

            #region editor
#if UNITY_EDITOR
            public void __setTarget(BehaviourModule module)
            {
                m_Target = module;
            }
            long _timeStamp;
            EBTState _resultStat;
            public float __deltaTime
            {
                get { return (float)((System.DateTime.Now.Ticks - _timeStamp) * 0.0000001d); }
            }
            public int __ticks { get; set; }
            public EBTState __resultStat
            {
                get { return _resultStat; }
                set
                {
                    _timeStamp = System.DateTime.Now.Ticks;
                    _resultStat = value;
                }
            }
            public void __setId(int id)
            {
                m_Id = id;
            }
            public void __setDisplayName(string name)
            {
                m_DisplayName = name;
            }
            public void __setBehaviourType(EBTNode type)
            {
                m_BehaviourType = type;
            }
            public int[] __children { get { return m_ChildrenId; } set { m_ChildrenId = value; } }
            public void __setComment(string comment)
            {
                m_Comment = comment;
            }
            public void __setParentId(int id)
            {
                m_ParentId = id;
            }
            public void __setModule(string moduleName)
            {
                m_ModuleName = moduleName;
            }
            public bool __sameAs(BehaviourNode node)
            {
                if (m_BehaviourType != node.m_BehaviourType)
                    return false;
                if (m_ParentId != node.m_ParentId)
                    return false;
                if (ChildrenCount != node.ChildrenCount)
                    return false;
                if (m_DisplayName != node.m_DisplayName)
                    return false;
                for (int i = ChildrenCount; i >= 0; i--)
                {
                    if (m_ChildrenId[i] != node.m_ChildrenId[i])
                        return false;
                }
                return true;
            }
            public void __setUserData(BTCustomData userData)
            {
                m_UseData = userData;
            }
#endif
            #endregion
        }
        
        [HideInInspector]
        [SerializeField]
        string m_Uid = "";

        [HideInInspector]
        [SerializeField]
        BehaviourNode[] m_Nodes;

        [HideInInspector]
        [SerializeField]
        int[] m_GraphRoots;

        float m_WaitTime;

        BehaviourTree[] m_Trees;

        public BehaviourNode GetNodeById(int id)
        {
            return GlobalUtil.Binsearch(m_Nodes, (node) => m_Nodes[node].Id - id, 0, m_Nodes.Length);
        }

        private void Awake()
        {
            InitBehaviourTree();
        }

        private void Update()
        {
            for (int i = 0; i < m_Trees.Length; i++)
            {
                if (m_Trees[i] != null)
                    m_Trees[i].OnTick();
            }
        }

        void InitBehaviourTree()
        {
            m_Trees = new BehaviourTree[m_GraphRoots.Length];
            for(int i = 0; i < m_Trees.Length; i++)
            {
                BehaviourNode bnode = GetNodeById(m_GraphRoots[i]);
                IBTNode node = InitTreeWith(bnode);
                IBTControlNode ctrl = node as IBTControlNode;
                if(node != null && ctrl == null)
                {
                    BehaviourNode tmp = new BehaviourNode();
                    tmp.UserData.m_FloatData = 1;
                    ctrl = new BTSequenceExec(tmp);
                    node.ParentNode = ctrl;
                    ctrl.AddChild(node);
                }
                if (ctrl != null)
                {
                    BehaviourTree tree = new BehaviourTree(bnode.DisplayName);
#if UNITY_EDITOR
                    tree.__graph = this;
#endif
                    m_Trees[i] = tree;
                    tree.InitWith(ctrl);
                }
            }
        }

        IBTNode InitTreeWith(BehaviourNode node)
        {
            BehaviourNode root = node;
            while (root != null && root.BehaviourType == EBTNode.empty)
            {
                root = root.ChildrenCount > 0 ? GetNodeById(root.GetChildId(0)) : null;
            }
            if (root == null)
            {
                return null;
            }
            BehaviourModule module = root.Target ?? this;
            IBTNode tnode = module.GetBehaviourNode(root);
            IBTControlNode ctrl = tnode as IBTControlNode;
            if (ctrl != null)
            {
                for(int i = 0; i < root.ChildrenCount; i++)
                {
                    IBTNode child = InitTreeWith(GetNodeById(root.GetChildId(i)));
                    if (child != null)
                    {
                        child.ParentNode = ctrl;
                        ctrl.AddChild(child);
                    }
                }
            }
#if UNITY_EDITOR
            else if(root.ChildrenCount > 0)
            {
                Debug.LogError(string.Format("行为树节点(id:{0} name:{1} type:{2} module:{3}) 设定的对象子节点数量大于它所需的子节点数量。", 
                    root.Id, root.DisplayName, root.BehaviourType, root.ModuleName));
            }
#endif
            return tnode;
        }

        #region Modules

        public EBTState AddPriority(BTCustomData userData)
        {
            userData.m_Priority += userData.m_IntData;
            return EBTState.success;
        }

        public EBTState WaitForSeconds(BTCustomData userData)
        {
            m_WaitTime += Time.deltaTime;
            if (m_WaitTime > userData.m_FloatData)
            {
                m_WaitTime = 0;
                return EBTState.success;
            }
            else
            {
                return EBTState.running;
            }
        }

        //public EBTState Greater(BTCustomData userData)
        //{

        //}

        #endregion

        #region editor
#if UNITY_EDITOR

        public void __setNodes (BehaviourNode[] nodes)
        {
            m_Nodes = nodes;
        }

        public BehaviourNode[] __getNodes()
        {
            return m_Nodes;
        }

        public void __setGraphRoots(int[] graphs)
        {
            m_GraphRoots = graphs;
        }

        public int[] __getGrapRoots()
        {
            return m_GraphRoots;
        }

        public void __resetTree()
        {
            InitBehaviourTree();
        }
#endif
        #endregion
    }
}