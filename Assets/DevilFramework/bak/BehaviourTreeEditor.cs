using Devil.AI;
using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    using BTNode = Devil.AI.BehaviourTreeAsset.BTNodeInfo;

    public class BehaviourTreeEditor : GraphViewEditorWindow
    {

        [MenuItem("Devil Framework/Behaviour Tree Editor")]
        public static void OpenThisWindow()
        {
            BehaviourTreeEditor window = GetWindow<BehaviourTreeEditor>();
            window.Show();
        }

        public enum EComposite
        {
            service,
            decorator,
            task,
            unknown,
        }

        public class BehaviourTreeNode : GraphNode
        {
            public BehaviourTreeNode() : base(true) { }

            public override void OnNodeGUI(GraphViewEditorWindow window, bool selected)
            {
                Rect rect = new Rect();
                rect.size = mBouds.size - new Vector2(20, 28);
                rect.center = mBouds.center;
                GUI.Label(rect, "", "flow node 6");
                GUI.Label(rect, Name, Installizer.titleStyle);
            }

            protected override void OnSocketGUI(GraphViewEditorWindow window, NodeSocket sock)
            {
                if (window.graph.GetChildCount(0, this) > 0)
                {
                    return;
                }
                Rect rect = new Rect();
                rect.size = new Vector2(PixelSize.x - 40, 15);
                rect.center = sock.GetSockPos(ClipRect) + Vector2.up * 8 * (sock.uvCoord.y == 0 ? 1 : -1);
                if (window.IsContextMenuShown)
                {
                    GUI.Label(rect, "", "Icon.ClipSelected");
                }
                else if (GUI.Button(rect, "", "Icon.ClipSelected"))
                {
                    window.CreateConnection(this, sock.layer, sock.sockPort, sock.toPort);
                }
            }
        }

        public class BehaviourNode : GraphNode
        {
            public int indexInParent;
            public int nodeId;
            public EBTNodeType nodeType;
            public List<string> decorators = new List<string>();
            public List<string> services = new List<string>();

            public EBTTaskState RuntimeState { get; set; } // 运行时状态

            public int DSLength { get { return  decorators.Count +  services.Count; } }
            public int DLength { get { return decorators.Count; } }
            public int SLength { get { return services.Count; } }

            public BTNode InstantNodeInfo()
            {
                BTNode node = new BTNode();
                node.m_Name = Name;
                node.m_Id = nodeId;
                node.m_Pos = Position;
                node.m_Type = nodeType;
                node.m_Services = services.ToArray();
                node.m_Decorators = decorators.ToArray();
                return node;
            }

            public float DSWidth
            {
                get
                {
                    int n = 0;
                    for (int i = 0; i < decorators.Count; i++)
                    {
                        n = Mathf.Max(n, decorators[i].Length);
                    }
                    for (int i = 0; i < services.Count; i++)
                    {
                        n = Mathf.Max(n, services[i].Length);
                    }
                    return n * 10;
                }
            }

            public BehaviourNode(int id, EBTNodeType type) : base(false)
            {
                nodeId = id;
                nodeType = type;
                mSockets[0].layerMask = nodeType == EBTNodeType.task ? 0 : 0xffffffffu;
            }

            public void AddDecorator(EComposite composite, string comName)
            {
                if(composite == EComposite.decorator)
                {
                    if (decorators.Contains(comName))
                    {
                        return;
                    }
                    decorators.Add(comName);
                }
                if(composite == EComposite.service)
                {
                    if (services.Contains(comName))
                        return;
                    services.Add(comName);
                }
            }

            protected override void InitDefaultData()
            {
                Position = new Vector2(0, 40);
                LayerMask = 0xffffffffu;
                PixelSize = new Vector2(200, 100);
                mSockets = new NodeSocket[2 + mExtSocks];

                mSockets[0].sockPort = PORT_PARENT;
                mSockets[0].toPort = PORT_CHILD;
                mSockets[0].layer = 0;
                mSockets[0].layerMask = 0xffffffffu;
                mSockets[0].uvCoord = new Vector2(0.5f, 1f);

                mSockets[1].sockPort = PORT_CHILD;
                mSockets[1].toPort = PORT_PARENT;
                mSockets[1].layer = -1;
                mSockets[1].layerMask = 0xffffffff;
                mSockets[1].uvCoord = new Vector2(0.5f, 0f);
            }

            public override void CalculateBounds(Vector2 viewOffset)
            {
                if(string.IsNullOrEmpty(Name))
                    Name = nodeType.ToString().ToUpper();
                Vector2 size = Installizer.SizeOfTitle(Name) + new Vector2(20, (nodeType == EBTNodeType.task ? 20 : 40) + 20 * DSLength);
                size.x = Mathf.Max(size.x, DSWidth + 20);
                PixelSize = size;
                mBouds.size = PixelSize;
                mBouds.position = new Vector2((Position.x - PixelSize.x * 0.5f), Position.y) + viewOffset;
            }

            public override void OnNodeGUI(GraphViewEditorWindow window, bool selected)
            {
                Rect rect = new Rect();
                float h = mBouds.height - (nodeType == EBTNodeType.task ? 20 : 35) - 20 * DSLength;
                rect.size = new Vector2(mBouds.width - 20, 20);
                rect.position = mBouds.position + new Vector2(10, 20);
                Rect btn = new Rect();
                btn.size = new Vector2(18, 18);
                for (int i = 0; i < decorators.Count; i++)
                {
                    GUI.Label(rect, decorators[i] + " ?", "dockareaStandalone");
                    if (!window.IsNodeInEditting && rect.Contains(Event.current.mousePosition))
                    {
                        btn.center = new Vector2(rect.xMax - 10, rect.yMin + 8);
                        if(GUI.Button(btn, "", "WinBtnCloseActiveMac"))
                        {
                            decorators.RemoveAt(i);
                            break;
                        }
                    }
                    rect.position += Vector2.up * 20;
                }

                rect.size = new Vector2(mBouds.width - 20, 20);
                rect.position = mBouds.position + new Vector2(10, mBouds.height - 12 - 20 * SLength);
                for (int i = 0; i < services.Count; i++)
                {
                    GUI.Label(rect, services[i], "dockareaStandalone");
                    if (!window.IsNodeInEditting && rect.Contains(Event.current.mousePosition))
                    {
                        btn.center = new Vector2(rect.xMax - 10, rect.yMin + 8);
                        if (GUI.Button(btn, "", "WinBtnCloseActiveMac"))
                        {
                            services.RemoveAt(i);
                            break;
                        }
                    }
                    rect.position += Vector2.up * 20;
                }

                rect.size = new Vector2(mBouds.width - 20, h);
                rect.position = mBouds.position + new Vector2(10, 18 + 20 * DLength);
                switch (nodeType)
                {
                    case EBTNodeType.parallel:
                        GUI.Label(rect, "", "flow node 5");
                        GUI.Label(rect, "", "Icon.OutlineBorder");
                        break;
                    case EBTNodeType.task:
                        GUI.Label(rect, "", "flow node 3");
                        GUI.Label(rect, "", "Icon.OutlineBorder");
                        break;
                    case EBTNodeType.sequence:
                        GUI.Label(rect, "", "flow node 2");
                        GUI.Label(rect, "", "Icon.OutlineBorder");
                        break;
                    case EBTNodeType.selector:
                        GUI.Label(rect, "", "flow node 1");
                        GUI.Label(rect, "", "Icon.OutlineBorder");
                        break;
                    default:
                        GUI.Label(rect, "", "flow node 0");
                        GUI.Label(rect, "", "Icon.OutlineBorder");
                        break;
                }
                GUI.Label(rect, Name, Installizer.titleStyle);
                GUI.Label(rect, string.Format("({0})", nodeId));
            }
        }

        BehaviourTreeAsset mAsset;

        NodeSocket tmpSock;
        List<GraphNode> nodeCache = new List<GraphNode>();
        List<string> mDecorators = new List<string>();
        List<string> mServices = new List<string>();
        List<string> mTasks = new List<string>();
        List<string> mPlugins = new List<string>();
        bool mIsDirty;
        Vector2 mDSScroll;
        Vector2 mTScroll;
        int mIdCounter;

        string[] mCompositeTypeNames = new string[] { "Decorator", "Service", "Task" };
        EComposite[] composites = new EComposite[] { EComposite.decorator, EComposite.service,  EComposite.task};
        int mSelectedComposite = -1;
        BehaviourTreeRunner mRuntimeBehaviourTree;

        List<string> CompositeList(EComposite composite)
        {
            if (composite == EComposite.task)
                return mTasks;
            if (composite == EComposite.service)
                return mServices;
            if (composite == EComposite.decorator)
                return mDecorators;
            return null;
        }

        EComposite SelectedComposite { get { return mSelectedComposite >= 0 && mSelectedComposite < composites.Length ? composites[mSelectedComposite] : EComposite.unknown; } }

        void ResetIdCounter()
        {
            mIdCounter = 0;
            for (int i = 0; i < graph.NodeLength; i++)
            {
                BehaviourNode node = graph[i] as BehaviourNode;
                if (node != null && node.nodeId > mIdCounter)
                    mIdCounter = node.nodeId;
            }
        }

        void ResetAsset(BehaviourTreeAsset asset, bool forceReset = false)
        {
            if (!forceReset && asset == mAsset)
                return;
            ResetGraph();
            mPlugins.Clear();
            BehaviourTreePlugin.GetOrNewInstance().GetPluginNames(mPlugins);
            mAsset = asset;
            mDecorators.Clear();
            mAsset.GetDecoratorNames(mDecorators);
            mServices.Clear();
            mAsset.GetServiceNames(mServices);
            mTasks.Clear();
            mAsset.GetTaskNames(mTasks);
            BTNode root = mAsset.GetNodeById(mAsset.m_RootNodeId);
            if (root == null)
                return;
            BehaviourNode bnode = InitNode(root);
            graph.AddPath(0, graph.Root, bnode);
            mViewportTitle = string.Format(titleFormat, mAsset == null ? "BEHAVIOUR DESIGN" : mAsset.name);
            ResetIdCounter();
        }

        void SaveAsset()
        {
            mIsDirty = false;
            List<GraphNode> children = new List<GraphNode>();
            graph.GetAllChildren(0, graph.Root, children);
            List<BTNode> nodes = new List<BTNode>();
            for(int i = 0; i < children.Count; i++)
            {
                BTNode node = SaveAssetAtRoot(children[i], nodes);
                if(node != null)
                {
                    mAsset.m_RootNodeId = node.m_Id;
                    GlobalUtil.Sort(nodes, (x, y) => x.m_Id - y.m_Id);
                    mAsset.m_Nodes = nodes.ToArray() ;
                    mAsset.m_Sorted = true;
                    EditorUtility.SetDirty(mAsset);
                    break;
                }
            }
        }

        BTNode SaveAssetAtRoot(GraphNode root, ICollection<BTNode> collection)
        {
            BehaviourNode node = root as BehaviourNode;
            if (node == null)
                return null;
            int id = graph.IndexOf(root);
            BTNode bnode = node.InstantNodeInfo();
            collection.Add(bnode);
            List<GraphNode> children = new List<GraphNode>();
            graph.GetAllChildren(0, root, children);
            GlobalUtil.Sort(children, (x, y) => x.Position.x - y.Position.x <= 0 ? -1 : 1);
            bnode.m_Children = new int[children.Count];
            for(int i = 0; i < children.Count; i++)
            {
                BTNode child = SaveAssetAtRoot(children[i], collection);
                bnode.m_Children[i] = child == null ? -1 : child.m_Id;
            }
            return bnode;
        }

        BehaviourNode InitNode(BTNode node)
        {
            BehaviourNode bnode = new BehaviourNode(node.m_Id, node.m_Type);
            bnode.Name = node.m_Name;
            bnode.Position = node.m_Pos;
            bnode.services.AddRange(node.m_Services);
            bnode.decorators.AddRange(node.m_Decorators);
            graph.AddNode(bnode);
            int children = node.m_Children == null ? 0 : node.m_Children.Length;
            for(int i = 0; i < children; i++)
            {
                BTNode child = mAsset.GetNodeById(node.m_Children[i]);
                if (child == null)
                    continue;
                BehaviourNode bchild = InitNode(child);
                graph.AddPath(0, bnode, bchild);
            }
            return bnode;
        }

        protected override void OnEnable()
        {
            OnSelectedGameObjectChanged();
            Selection.selectionChanged += OnSelectedGameObjectChanged;
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
            base.OnEnable();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectedGameObjectChanged;
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            if (mIsDirty)
                SaveAsset();
        }

        private void OnFocus()
        {
            OnSelectedGameObjectChanged();
        }

        protected override void OnTitleGUI()
        {
            if(mAsset == null)
            {
                GUILayout.Label("<color=red>选择一个 BehaviourTreeAsset </color>");
                return;
            }
            if (GUILayout.Button(mAsset.name, "TE toolbarbutton",GUILayout.MinWidth(100), GUILayout.MaxWidth(200)))
            {
                EditorGUIUtility.PingObject(mAsset);
            }
            if (GUILayout.Button("重置", "TE toolbarbutton", GUILayout.Width(70)))
            {
                ResetAsset(mAsset, true);
            }
            if (GUILayout.Button("保存", "TE toolbarbutton", GUILayout.Width(70)))
            {
                SaveAsset();
            }
        }

        string titleFormat = @"<size=30><b>{0}</b></size><size=23> (ver.2.0.0)</size>
<size=18>  NOTE: 
  按住【滚轮】平移视图
  按住【左键】拖动节点或者框选节点
  点击【右键】取消当前操作
  点击【DELETE】删除选中节点
  按住【SHIFT】多选
  点击【CTRL+F】居中视图</size>";

        protected override void InitParameters()
        {
            base.InitParameters();
            mViewportTitle = string.Format(titleFormat, "BEHAVIOUR DESIGN");
            mDecorators.Clear();
            mServices.Clear();
            mTasks.Clear();
            mDefaultRoot = new BehaviourTreeNode();
        }

        protected override void OnPrepareGraphGUI()
        {
            mDefaultRoot.Name = mAsset == null ? "[NO BehaviourTree]" : mAsset.name;
            Editable = !Application.isPlaying;
        }

        protected override bool TryConnectNode(int layer, NodeSocket sockA, NodeSocket sockB, GraphNode startNode, GraphNode endNode)
        {
            if (!endNode.GetSocket(sockA.toPort, ref sockB))
                return false;
            if ((sockB.layerMask & mPaintLayers) == 0)
                return false;
            switch (sockA.toPort)
            {
                case PORT_CHILD:
                    if (graph.GetParentCount(0, endNode) > 0)
                        return false;
                    if(startNode != endNode)
                       graph.AddPath(0, startNode, endNode);
                    return true;
                case PORT_PARENT:
                    if (endNode == graph.Root && graph.GetChildCount(0, endNode) > 0)
                        return false;
                    if (graph.FindPath(0, startNode, endNode))
                        return false;
                    nodeCache.Clear();
                    graph.GetAllParent(0, startNode, nodeCache);
                    GraphNode p = nodeCache.Count > 0 ? nodeCache[0] : null;
                    if (p == endNode)
                        return false;
                    if (p != null)
                        graph.RemovePath(0, p, startNode);
                    if(endNode != startNode)
                        graph.AddPath(0, endNode, startNode);
                    return true;
                default:
                    return false;
            }
        }

        protected override bool OnNewNodePanelGUI(Vector2 position, ref Rect rect)
        {
            GUI.Label(ClipRect, "", "TL Range Overlay");
            float h = Installizer.TitleHeight + 5;
            rect.size = new Vector2(200, Mathf.Min(400, h * 3 + 60 + mPlugins.Count * 25));
            rect.position = position - Vector2.right * 100;
            GUI.Label(rect, "", "flow node 0 on");
            Rect r = new Rect();
            r.size = new Vector2(180, h);
            r.position = rect.position + new Vector2(10, 15);
            GUI.Label(r, "", "flow node 1");
            GUI.Label(r, "", "Icon.OutlineBorder");
            BehaviourNode node;
            if (GUI.Button(r, "SELECTOR", Installizer.titleStyle))
            {
                node = new BehaviourNode(++mIdCounter, EBTNodeType.selector);
                node.Position = position - ViewOffset;
                node.Name = "Selector";
                graph.AddNode(node);
                graph.AddPath(0, mWireStart, node);
                return false;
            }

            r.position += Vector2.up * (h + 5);
            GUI.Label(r, "", "flow node 2");
            GUI.Label(r, "", "Icon.OutlineBorder");
            if (GUI.Button(r, "SEQUENCE", Installizer.titleStyle))
            {
                node = new BehaviourNode(++mIdCounter, EBTNodeType.sequence);
                node.Position = position - ViewOffset;
                node.Name = "Sequence";
                graph.AddNode(node);
                graph.AddPath(0, mWireStart, node);
                return false;
            }

            r.position += Vector2.up * (h + 5);
            GUI.Label(r, "", "flow node 5");
            GUI.Label(r, "", "Icon.OutlineBorder");
            if (GUI.Button(r, "PARALLEL", Installizer.titleStyle))
            {
                node = new BehaviourNode(++mIdCounter, EBTNodeType.parallel);
                node.Position = position - ViewOffset;
                node.Name = "Parallel";
                graph.AddNode(node);
                graph.AddPath(0, mWireStart, node);
                return false;
            }

            // plugins list
            r.size = new Vector2(180, rect.height - h * 3 - 40);
            r.position = new Vector2(10, rect.height - r.height - 10) + rect.position;
            GUILayout.BeginArea(r, "PLUGINS", "window");
            mDSScroll = GUILayout.BeginScrollView(mDSScroll, GUILayout.Width(180));
            bool retvalue = true;
            for(int i = 0; i < mPlugins.Count; i++)
            {
                if(GUILayout.Button(mPlugins[i], "TL RightLine", GUILayout.Height(20)))
                {
                    AddChildNode(mWireStart, mPlugins[i], EBTNodeType.plugin, position - ViewOffset);
                    retvalue = false;
                    break;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            // tasks list
            r.size = rect.size;
            r.position = new Vector2(rect.xMax, rect.yMin);
            GUILayout.BeginArea(r, "TASKS", "window");
            mTScroll = GUILayout.BeginScrollView(mTScroll, GUILayout.Width(190));
            for (int i = 0; i < mTasks.Count; i++)
            {
                if (GUILayout.Button(mTasks[i], "TL RightLine", GUILayout.Height(20)))
                {
                    AddChildNode(mWireStart, mTasks[i], EBTNodeType.task, position - ViewOffset);
                    retvalue = false;
                    break;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            return retvalue;
        }

        protected override bool OnContextMenuGUI(Vector2 position, ref Rect rect)
        {
            BehaviourNode node = ContextNode as BehaviourNode;
            if (node == null)
                return false;
            GUI.Label(ClipRect, "", "TL Range Overlay");
            float height = 30 * mCompositeTypeNames.Length + 35;
            rect.size = new Vector2(120, height);
            rect.position = position;
            bool alignTop = position.y < ClipRect.center.y;
            bool alignLeft = position.x < ClipRect.center.x;
            rect.position = new Vector2(alignLeft ? position.x : position.x - 110, alignTop ? position.y : position.y - height);
            GUI.Label(rect, "COMPOSITES", "window");

            Rect r = new Rect();
            r.size = new Vector2(110, 30);
            r.position = rect.position + new Vector2(5, 30);
            for (int i = 0; i < mCompositeTypeNames.Length; i++)
            {
                bool sel = (composites[i] == EComposite.decorator || node.nodeType != EBTNodeType.task);
                if (r.Contains(Event.current.mousePosition) && sel)
                    mSelectedComposite = i;
                //GUI.Label(r, mCompositeTypeNames[i], "TL SelectionButtonNew");
                if(sel)
                   GUI.Label(r, string.Format("<b><color=green><size=15>{0}</size></color></b>", mCompositeTypeNames[i]));
                else
                    GUI.Label(r, string.Format("<b><color=#404040><size=15>{0}</size></color></b>", mCompositeTypeNames[i]));
                if (sel && i == mSelectedComposite)
                {
                    GUI.Label(r, "", "Icon.OutlineBorder");
                }
                r.position += Vector2.up * 30;
            }

            List<string> sublist = CompositeList(SelectedComposite);
            if (sublist != null && sublist.Count > 0)
            {
                Rect subr = new Rect();
                float subHeight = Mathf.Min(ClipRect.height * 0.5f, 600, 20 + sublist.Count * 27);
                subr.size = new Vector2(150, subHeight);
                subr.position = new Vector2(alignLeft ? rect.xMax : rect.xMin - 150, alignTop ? rect.yMin : rect.yMax - subHeight);
                GUILayout.BeginArea(subr,SelectedComposite.ToString().ToUpper(), (GUIStyle)"window");
                mDSScroll = GUILayout.BeginScrollView(mDSScroll, GUILayout.Width(140));
                bool retvalue = true;
                for (int i = 0; i < sublist.Count; i++)
                {
                    if (GUILayout.Button(sublist[i], "TL RightLine", GUILayout.Height(25)))
                    {
                        if (SelectedComposite == EComposite.task)
                        {
                            AddChildNode(node, sublist[i], EBTNodeType.task, node.Position + Vector2.up * (node.PixelSize.y + 30));
                        }
                        else
                        {
                            node.AddDecorator(SelectedComposite, sublist[i]);
                        }
                        retvalue = false;
                        break;
                    }
                }
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                return retvalue;
            }
            return true;
        }

        void AddChildNode(GraphNode root, string taskName, EBTNodeType nodeType, Vector2 pos)
        {
            if (root == null)
                return;
            BehaviourNode task = new BehaviourNode(++mIdCounter, nodeType);
            task.Name = taskName;
            task.Position = pos;
            graph.AddNode(task);
            graph.AddPath(0, root, task);
        }

        protected override void OnDeleteSelections()
        {
            base.OnDeleteSelections();
            ResetIdCounter();
        }

        protected override void OnContextMenuGUIEnd()
        {
            base.OnContextMenuGUIEnd();
            mSelectedComposite = -1;
        }

        protected override void GetWireColorAndWidth(GraphNode nodeA, NodeSocket sockA, GraphNode nodeB, NodeSocket sockB, out Color color, out float width)
        {

            if (Application.isPlaying && mRuntimeBehaviourTree)
            {
                BehaviourNode bnode = nodeB as BehaviourNode;
                if (bnode == null)
                {
                    base.GetWireColorAndWidth(nodeA, sockA, nodeB, sockB, out color, out width);
                    return;
                }
                if (bnode.RuntimeState == EBTTaskState.success)
                {
                    width = 6;
                    color = Color.green;
                }
                else if (bnode.RuntimeState == EBTTaskState.faild)
                {
                    width = 6;
                    color = Color.red;
                }
                else if (bnode.RuntimeState == EBTTaskState.running)
                {
                    width = 5;
                    color = Color.blue;
                }
                else
                {
                    width = 3;
                    color = Color.gray;
                }
                return;
            }
            base.GetWireColorAndWidth(nodeA, sockA, nodeB, sockB, out color, out width);
        }

        protected override void OnNodeFrameGUI(GraphNode node, bool highlight)
        {
            if (Application.isPlaying && mRuntimeBehaviourTree)
            {
                GUI.Label(node.ClipRect, "", highlight ? "flow node 0 on" : "flow node 0");
                BehaviourNode bnode = node as BehaviourNode;
                if(bnode == null)
                {
                    base.OnNodeFrameGUI(node, highlight);
                    return;
                }
                string style;
                if(bnode.RuntimeState == EBTTaskState.success)
                {
                    style = "flow node 3 on";
                }
                else if(bnode.RuntimeState == EBTTaskState.faild)
                {
                    style = "flow node 6 on";
                }
                else if(bnode.RuntimeState == EBTTaskState.running)
                {
                    style = "flow node 1 on";
                }
                else
                {
                    style = "flow node 0 on";
                }
                GUI.Label(node.ClipRect, "", style);
                return;
            }
            base.OnNodeFrameGUI(node, highlight);
        }

        #region events

        void BindBehaviourTreeEvent(bool bind)
        {
            if (mRuntimeBehaviourTree == null)
                return;
            if (bind)
            {
                mRuntimeBehaviourTree.OnBehaviourTreeBegin += OnBehaviourTreeBegin;
                mRuntimeBehaviourTree.OnBehaviourTreeFrame += OnBehaviourTreeFrame;
            }
            else
            {
                mRuntimeBehaviourTree.OnBehaviourTreeBegin -= OnBehaviourTreeBegin;
                mRuntimeBehaviourTree.OnBehaviourTreeFrame -= OnBehaviourTreeFrame;
            }
        }

        void OnSelectedGameObjectChanged()
        {
            if (Selection.activeObject is BehaviourTreeAsset && !Application.isPlaying)
            {
                ResetAsset(Selection.activeObject as BehaviourTreeAsset);
                return;
            }
            GameObject obj = Selection.activeGameObject;
            BehaviourTreeRunner runner = obj ? obj.GetComponent<BehaviourTreeRunner>() : null;
            if (mRuntimeBehaviourTree != runner)
            {
                if (mRuntimeBehaviourTree != null && Application.isPlaying)
                {
                    BindBehaviourTreeEvent(false);
                }
                mRuntimeBehaviourTree = runner;
                if (mRuntimeBehaviourTree != null && Application.isPlaying)
                {
                    BindBehaviourTreeEvent(true);
                }
            }
            mRuntimeBehaviourTree = runner;
            if (runner && runner.BehaviourAsset != null)
            {
                ResetAsset(runner.BehaviourAsset);
            }
        }

        private void OnPlayStateChanged(PlayModeStateChange obj)
        {
#if DEBUG_AI
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                ResetAsset(mRuntimeBehaviourTree == null ? mAsset : mRuntimeBehaviourTree.BehaviourAsset, true);
                BindBehaviourTreeEvent(true);
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                BindBehaviourTreeEvent(false);
                mRuntimeBehaviourTree = null;
            }
#endif
        }

        void SetNodeRuntimeState(int nodeId, EBTTaskState state)
        {
            for(int i = 0; i < graph.NodeLength; i++)
            {
                BehaviourNode node = graph[i] as BehaviourNode;
                if(node != null && node.nodeId == nodeId)
                {
                    node.RuntimeState = state;
                    return;
                }
            }
        }

        void SetNodeRuntimeState(BTNodeBase runtimeNode)
        {
            if (runtimeNode == null)
                return;
            SetNodeRuntimeState(runtimeNode.NodeId, runtimeNode.State);
            for(int i = 0; i < runtimeNode.ChildLength; i++)
            {
                BTNodeBase child = runtimeNode.ChildAt(i);
                SetNodeRuntimeState(child);
            }
        }

        void ResetRuntimeTreeState(BTNodeBase node)
        {
            if (node == null)
                return;
            node.Reset();
            SetNodeRuntimeState(node.NodeId, node.State);
            for (int i = 0; i < node.ChildLength; i++)
            {
                BTNodeBase child = node.ChildAt(i);
                ResetRuntimeTreeState(child);
            }
        }

        void OnBehaviourTreeBegin(BehaviourTreeRunner btree)
        {
            if(btree == mRuntimeBehaviourTree)
            {
                ResetRuntimeTreeState(btree.RootNode);
            }
        }

        void OnBehaviourTreeFrame(BehaviourTreeRunner btree)
        {
            if(btree == mRuntimeBehaviourTree)
                SetNodeRuntimeState(btree.RootNode);
        }


        #endregion
    }
}