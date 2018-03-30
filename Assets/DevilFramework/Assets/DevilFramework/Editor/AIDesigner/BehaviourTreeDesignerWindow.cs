using Devil;
using Devil.AI;
using Devil.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    using BTData = BehaviourTreeAsset.BTData;


    public class BehaviourTreeDesignerWindow : EditorCanvasWindow
    {
        public class DelayTask
        {
            public int Id { get; private set; }
            public System.Action Act { get; set; }
            public DelayTask(int id, System.Action act)
            {
                this.Id = id;
                this.Act = act;
            }
        }

        public enum ENodeEditMode
        {
            none,
            modify_parent,
            modify_child,
        }

        public const int TASK_SAVE = 1;
        public const int TASK_RESELECT = 2;

        public BehaviourTreeRunner Runner { get; private set; }
        public BehaviourTreeAsset BehaviourAsset { get; private set; }

        public SelectionGUI SelectionRect { get; private set; }
        public Graph<PaintElement> TreeGraph { get; private set; }
        List<PaintElement> mCache = new List<PaintElement>();
        public BehaviourTreeContextMenuGUI ContextMenu { get; private set; }
        public EditorGUICanvas TreeCanvas { get; private set; }
        public EditorGUICanvas CommentCanvas { get; private set; }
        public ENodeEditMode EditMode { get; private set; }
        public PaintElement EditTarget { get; private set; }
        BehaviourRootGUI mRootGUI;
        bool mLockTarget;
        public bool LockTarget
        {
            get { return mLockTarget; }
            set
            {
                bool loc = mLockTarget;
                mLockTarget = value;
                if(loc && !value)
                {
                    LoadSelectedAsset();
                }
            }
        }

        public int GenerateId { get { return ++mIdCounter; } }
        List<DelayTask> mPostTasks = new List<DelayTask>();

        bool mBuildIndex;
        int mIdCounter;

        public bool IsPlaying { get { return EditorApplication.isPlaying && Runner != null; } }

        [MenuItem("Devil Framework/AI/Behaviour Tree Design")]
        public static void OpenEditor()
        {
            GameObject obj = Selection.activeGameObject;
            BehaviourTreeRunner r = obj == null ? null : obj.GetComponent<BehaviourTreeRunner>();
            OpenEditor(r);
        }

        public static void OpenEditor(BehaviourTreeRunner runner)
        {
            BehaviourTreeDesignerWindow window = GetWindow<BehaviourTreeDesignerWindow>();
            window.InitWith(runner);
            window.Show();
        }

        public static void OpenEditor(BehaviourTreeAsset asset)
        {
            BehaviourTreeDesignerWindow window = GetWindow<BehaviourTreeDesignerWindow>();
            window.InitWith(asset);
            window.Show();
        }

        public BehaviourTreeDesignerWindow() : base()
        {
            mMaxScale = 2;
            GraphCanvas.GridSize = 200;
            SelectionRect = new SelectionGUI();
            RootCanvas.AddElement(SelectionRect);
            ScaledCanvas.Pivot = new Vector2(0.5f, 0.25f);
            GraphCanvas.Pivot = new Vector2(0.5f, 0.1f);
            Rect r = GraphCanvas.LocalRect;
            r.position = new Vector2(-0.5f * r.width, -0.1f * r.height);
            GraphCanvas.LocalRect = r;

            TreeGraph = new Graph<PaintElement>(1);

            Rect grect = new Rect(-GraphCanvas.LocalRect.size * 0.5f, GraphCanvas.LocalRect.size);
            BehaviourTreeWireGUI wires = new BehaviourTreeWireGUI(this);
            wires.LocalRect = grect;
            GraphCanvas.AddElement(wires);

            TreeCanvas = new EditorGUICanvas();
            TreeCanvas.LocalRect = grect;
            GraphCanvas.AddElement(TreeCanvas);

            CommentCanvas = new EditorGUICanvas();
            CommentCanvas.SortOrder = -2;
            CommentCanvas.LocalRect = grect;
            GraphCanvas.AddElement(CommentCanvas);

            ContextMenu = new BehaviourTreeContextMenuGUI(this);
            GraphCanvas.AddElement(ContextMenu);

            BlackboardMonitorGUI blackboard = new BlackboardMonitorGUI(this);
            blackboard.LocalRect = new Rect(0, 0, 200, 180);
            RootCanvas.AddElement(blackboard);

            RootCanvas.Resort(true);

            GraphCanvas.OnDragBegin = OnGraphDragBegin;
            GraphCanvas.OnDrag = OnGraphDrag;
            GraphCanvas.OnDragEnd = OnGraphDragEnd;
            GraphCanvas.OnMouseClick = OnGraphClick;
            GraphCanvas.OnKeyUp = OnGraphKeyUp;
        }

        public void AddDelayTask(int id, System.Action act)
        {
            if (act == null)
                return;
            for (int i = 0; i < mPostTasks.Count; i++)
            {
                if (mPostTasks[i].Id == id)
                {
                    mPostTasks[i].Act = act;
                    return;
                }
            }
            mPostTasks.Add(new DelayTask(id, act));
        }

        void ResetIdCounter()
        {
            mIdCounter = 0;
            for (int i = 0; i < TreeGraph.NodeLength; i++)
            {
                BehaviourNodeGUI node = TreeGraph[i] as BehaviourNodeGUI;
                if (node == null || node.Self == null)
                    continue;
                if (node.Self.BTId > mIdCounter)
                    mIdCounter = node.Self.BTId;
                for(int j = 0; j < node.conditions.Count; j++)
                {
                    BehaviourNodeGUI.Decorator deco = node.conditions[j];
                    if (deco != null && deco.BTId > mIdCounter)
                        mIdCounter = deco.BTId;
                }
                for (int j = 0; j < node.services.Count; j++)
                {
                    BehaviourNodeGUI.Decorator deco = node.services[j];
                    if (deco != null && deco.BTId > mIdCounter)
                        mIdCounter = deco.BTId;
                }
            }
        }

        BehaviourNodeGUI NewNode(BTData data)
        {
            BehaviourMeta meta = BehaviourModuleManager.GetOrNewInstance().FindBTMeta(data.m_Type, data.m_Name);
            if (meta == null || (meta.NodeType != EBTNodeType.task && meta.NodeType != EBTNodeType.controller))
                return null;
            BehaviourNodeGUI bnode = new BehaviourNodeGUI(this);
            bnode.Self = new BehaviourNodeGUI.Decorator(data.m_Id, meta);
            bnode.Self.ParseData(data.m_JsonData);
            if (data.m_Services != null)
            {
                for (int i = 0; i < data.m_Services.Length; i++)
                {
                    BTData serv = BehaviourAsset.GetDataById(data.m_Services[i]);
                    if (serv == null)
                        continue;
                    BehaviourMeta bm = BehaviourModuleManager.GetOrNewInstance().FindBTMeta(EBTNodeType.service, serv.m_Name);
                    if (bm != null)
                    {
                        BehaviourNodeGUI.Decorator decor = new BehaviourNodeGUI.Decorator(serv.m_Id, bm);
                        decor.ParseData(serv.m_JsonData);
                        bnode.services.Add(decor);
                    }
                }
            }
            if (data.m_Conditions != null)
            {
                for (int i = 0; i < data.m_Conditions.Length; i++)
                {
                    BTData cond = BehaviourAsset.GetDataById(data.m_Conditions[i]);
                    if (cond == null)
                        continue;
                    BehaviourMeta bm = BehaviourModuleManager.GetOrNewInstance().FindBTMeta(EBTNodeType.condition, cond.m_Name);
                    if (bm != null)
                    {
                        BehaviourNodeGUI.Decorator decor = new BehaviourNodeGUI.Decorator(cond.m_Id, bm);
                        decor.NotFlag = cond.m_NotFlag;
                        decor.ParseData(cond.m_JsonData);
                        bnode.conditions.Add(decor);
                    }
                }
            }
            Rect r = new Rect();
            r.size = bnode.CalculateLocalSize();
            r.position = data.m_Pos - Vector2.right * r.size.x * 0.5f;
            bnode.LocalRect = r;
            return bnode;
        }

        public void InitWith(BehaviourTreeAsset asset)
        {
            if (EditorApplication.isPlaying)
            {
                BindBehaviourTreeEvent(false);
            }
            ContextMenu.Hide();
            Runner = null;
            BehaviourAsset = asset;
            TreeCanvas.ClearElements();
            CommentCanvas.ClearElements();
            TreeGraph.Clear();

            mRootGUI = new BehaviourRootGUI(this);
            mRootGUI.UpdateLocalData();
            TreeCanvas.AddElement(mRootGUI);
            TreeGraph.AddNode(mRootGUI);

            if (BehaviourAsset != null)
            {
                ImportTreeData();
            }
            ResetIdCounter();
            RebuildExecutionOrder();
            UpdateStateInfo();
        }

        public void InitWith(BehaviourTreeRunner runner)
        {
            if (EditorApplication.isPlaying)
            {
                BindBehaviourTreeEvent(false);
            }
            ContextMenu.Hide();
            Runner = runner;
            BehaviourAsset = Runner == null ? (Selection.activeObject as BehaviourTreeAsset) : Runner.SourceAsset;
            TreeCanvas.ClearElements();
            CommentCanvas.ClearElements();
            TreeGraph.Clear();

            mRootGUI = new BehaviourRootGUI(this);
            mRootGUI.UpdateLocalData();
            TreeCanvas.AddElement(mRootGUI);
            TreeGraph.AddNode(mRootGUI);

            if (BehaviourAsset != null)
            {
                ImportTreeData();
            }
            ResetIdCounter();
            RebuildExecutionOrder();
            UpdateStateInfo();
            if (EditorApplication.isPlaying)
            {
                BindBehaviourTreeEvent(true);
            }
        }

        void ImportTreeData()
        {
            for (int i = 0; i < BehaviourAsset.m_Datas.Length; i++)
            {
                BehaviourNodeGUI node = NewNode(BehaviourAsset.m_Datas[i]);
                if(node != null)
                {
                    TreeGraph.AddNode(node);
                    TreeCanvas.AddElement(node);
                }
            }
            int id = 0;
            FilterDelegate<PaintElement> filter = (x) =>
            {
                BehaviourNodeGUI bx = x as BehaviourNodeGUI;
                return bx != null && bx.Self.BTId == id;
            };
            for (int i = 0; i < BehaviourAsset.m_Datas.Length; i++)
            {
                BTData data = BehaviourAsset.m_Datas[i];
                id = data.m_Id;
                BehaviourNodeGUI gnode = TreeGraph.FindNode(filter) as BehaviourNodeGUI;
                if (gnode == null)
                    continue;
                int len = data.m_Children == null ? 0 : data.m_Children.Length;
                for(int j = 0; j < len; j++)
                {
                    id = data.m_Children[j];
                    BehaviourNodeGUI gchild = TreeGraph.FindNode(filter) as BehaviourNodeGUI;
                    if (gchild != null)
                        TreeGraph.AddPath(0, gnode, gchild);
                }
            }
            id = BehaviourAsset.m_RootNodeId;
            BehaviourNodeGUI root = TreeGraph.FindNode(filter) as BehaviourNodeGUI;
            if (root != null)
                TreeGraph.AddPath(0, mRootGUI, root);
            
            for (int i = 0; i < BehaviourAsset.m_Comments.Length; i++)
            {
                BehaviourCommentGUI comment = new BehaviourCommentGUI(this);
                comment.Comment = BehaviourAsset.m_Comments[i].m_Comment;
                comment.LocalRect = BehaviourAsset.m_Comments[i].m_Rect;
                CommentCanvas.AddElement(comment);
            }
        }

        void SaveAsset()
        {
            bool refresh = false;
            if (BehaviourAsset == null)
            {
                BehaviourAsset = DevilEditorUtility.CreateAsset<BehaviourTreeAsset>(null, true);
                mRootGUI.UpdateLocalData();
                refresh = true;
            }
            if (BehaviourAsset == null)
            {
                return;
            }
            ExportTreeData();
            if (refresh)
                AssetDatabase.Refresh();
        }

        void ExportTreeData()
        {
            List<BTData> nodes = new List<BTData>();
            List<PaintElement> children = new List<PaintElement>();
            int rootId = 0;
            for (int i = 0; i < TreeCanvas.ElementCount; i++)
            {
                BehaviourNodeGUI bnode = TreeCanvas.GetElement<BehaviourNodeGUI>(i);
                if (bnode == null)
                    continue;
                if (TreeGraph.GetParent(0, bnode) == mRootGUI)
                    rootId = bnode.Self.BTId;
                BTData data = bnode.ExportNodeData(nodes);

                children.Clear();
                TreeGraph.GetAllChildren(0, bnode, children);
                GlobalUtil.Sort(children, (x, y) => x.LocalRect.center.x <= y.LocalRect.center.x ? -1 : 1);
                data.m_Children = new int[children.Count];
                for (int j = 0; j < children.Count; j++)
                {
                    BehaviourNodeGUI child = children[j] as BehaviourNodeGUI;
                    data.m_Children[j] = child == null ? 0 : child.Self.BTId;
                }
            }
            BehaviourAsset.m_RootNodeId = rootId;
            GlobalUtil.Sort(nodes, (x, y) => x.m_Id - y.m_Id);
            BehaviourAsset.m_Datas = nodes.ToArray();
            BehaviourAsset.m_Sorted = true;

            BehaviourTreeAsset.Comment[] comments = new BehaviourTreeAsset.Comment[CommentCanvas.ElementCount];
            for (int i = 0; i < comments.Length; i++)
            {
                comments[i] = new BehaviourTreeAsset.Comment();
                BehaviourCommentGUI com = CommentCanvas.GetElement<BehaviourCommentGUI>(i);
                if (com != null)
                {
                    comments[i].m_Rect = com.LocalRect;
                    comments[i].m_Comment = com.Comment ?? "";
                }
            }
            BehaviourAsset.m_Comments = comments;
            EditorUtility.SetDirty(BehaviourAsset);
        }

        void DoBuildExecutionOrder()
        {
            EditNodes((x) => x.BTExecutionOrder = 0);
            List<PaintElement> children = new List<PaintElement>();
            TreeGraph.GetAllChildren(0, TreeGraph.Root, children);
            List<BehaviourNodeGUI> nodes = new List<BehaviourNodeGUI>();
            for (int i = 0; i < children.Count; i++)
            {
                VisitChildren(nodes, children[i] as BehaviourNodeGUI);
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].BTExecutionOrder = i + 1;
                nodes[i].CheckError();
            }
        }

        void VisitChildren(List<BehaviourNodeGUI> nodes, BehaviourNodeGUI root)
        {
            nodes.Add(root);
            List<PaintElement> children = new List<PaintElement>();
            TreeGraph.GetAllChildren(0, root, children);
            GlobalUtil.Sort(children, (x, y) => x.LocalRect.center.x <= y.LocalRect.center.x ? -1 : 1);
            for (int i = 0; i < children.Count; i++)
            {
                BehaviourNodeGUI node = children[i] as BehaviourNodeGUI;
                if (node != null)
                    VisitChildren(nodes, node);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Selection.selectionChanged += OnSelectedObjectChanged;
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
            Installizer.OnReloaded += ReloadGraph;
            OnSelectedObjectChanged();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Selection.selectionChanged -= OnSelectedObjectChanged;
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            Installizer.OnReloaded -= ReloadGraph;
            BindBehaviourTreeEvent(false);
            Runner = null;
        }

        void ReloadGraph()
        {
            if (Runner != null)
                InitWith(Runner);
            else
                InitWith(BehaviourAsset);
        }
        
        void LoadSelectedAsset()
        {
            BehaviourTreeRunner r;
            GameObject obj = Selection.activeGameObject;
            r = obj == null ? null : obj.GetComponent<BehaviourTreeRunner>();
            InitWith(r);
        }

        void OnSelectedObjectChanged()
        {
            if (LockTarget && (Runner != null || BehaviourAsset != null))
                return;
            LoadSelectedAsset();
        }

        void BindBehaviourTreeEvent(bool bind)
        {
            if (Runner == null)
                return;
            if (bind)
            {
                Runner.OnBehaviourTreeBegin += OnBehaviourTreeBegin;
                Runner.OnBehaviourTreeFrame += OnBehaviourTreeFrame;
                for(int i = 0; i < TreeCanvas.ElementCount; i++)
                {
                    BehaviourNodeGUI node = TreeCanvas.GetElement<BehaviourNodeGUI>(i);
                    if (node == null)
                        continue;
                    node.RuntimeNode = Runner.FindRuntimeNode(node.Self.BTId);
                }
            }
            else
            {
                Runner.OnBehaviourTreeBegin -= OnBehaviourTreeBegin;
                Runner.OnBehaviourTreeFrame -= OnBehaviourTreeFrame;
                for (int i = 0; i < TreeCanvas.ElementCount; i++)
                {
                    BehaviourNodeGUI node = TreeCanvas.GetElement<BehaviourNodeGUI>(i);
                    if (node == null)
                        continue;
                    node.RuntimeNode = null;
                }
            }
        }
        private void OnBehaviourTreeFrame(BehaviourTreeRunner btree)
        {
            if (btree == Runner)
            {
                for(int i = 0; i < TreeCanvas.ElementCount; i++)
                {
                    BehaviourNodeGUI node = TreeCanvas.GetElement<BehaviourNodeGUI>(i);
                    if (node != null)
                        node.SyncRuntimeState(btree);
                }
            }
        }

        private void OnBehaviourTreeBegin(BehaviourTreeRunner btree)
        {
            if (btree == Runner)
                SetNodeRuntimeState(btree.RootNode);
        }

        void SetNodeRuntimeState(int nodeId, EBTTaskState state)
        {
            for (int i = 0; i < TreeGraph.NodeLength; i++)
            {
                BehaviourNodeGUI node = TreeGraph[i] as BehaviourNodeGUI;
                if (node != null && node.Self.BTId == nodeId)
                {
                    node.Self.BTRuntimeState = state;
                    return;
                }
            }
        }

        void SetNodeRuntimeState(BTNodeBase runtimeNode)
        {
            if (runtimeNode == null)
                return;
            SetNodeRuntimeState(runtimeNode.NodeId, runtimeNode.State);
            for (int i = 0; i < runtimeNode.ChildLength; i++)
            {
                BTNodeBase child = runtimeNode.ChildAt(i);
                SetNodeRuntimeState(child);
            }
        }

        protected override void OnTitleGUI()
        {
            if (GUILayout.Button(Runner == null ? "<i>[NO RUNNER]</i>" : Runner.name, "GUIEditor.BreadcrumbLeft") && Runner != null)
            {
                EditorGUIUtility.PingObject(Runner);
            }
            if (GUILayout.Button(BehaviourAsset == null ? "<i>[NO TREE]</i>" : BehaviourAsset.name, "GUIEditor.BreadcrumbMid") && BehaviourAsset != null)
            {
                EditorGUIUtility.PingObject(BehaviourAsset);
            }
            GUILayout.Label("");
            if (GUILayout.Button("重置", "TE toolbarbutton", GUILayout.Width(70)))
            {
                AddDelayTask(TASK_RESELECT, new System.Action(ReloadGraph));
            }
            if (GUILayout.Button("更新", "TE toolbarbutton", GUILayout.Width(70)))
            {
                AddDelayTask(TASK_SAVE, new System.Action(SaveAsset));
            }
        }

        protected override void OnPostGUI()
        {
            for(int i = 0; i < mPostTasks.Count; i++)
            {
                mPostTasks[i].Act();
            }
            mPostTasks.Clear();
            if (mBuildIndex)
            {
                mBuildIndex = false;
                DoBuildExecutionOrder();
            }
        }

        bool OnGraphDragBegin(EMouseButton button, Vector2 mousePosition)
        {
            if (button == EMouseButton.left)
            {
                Rect rect = SelectionRect.LocalRect;
                rect.size = Vector2.zero;
                rect.position = RootCanvas.CalculateLocalPosition(mousePosition);
                SelectionRect.LocalRect = rect;
                SelectionRect.Visible = true;
                SelectComment(null, false);
                return true;
            }
            return false;
        }

        bool OnGraphDrag(EMouseButton button, Vector2 mousePosition, Vector2 delta)
        {
            if (button == EMouseButton.left)
            {
                Rect rect = SelectionRect.LocalRect;
                rect.size = RootCanvas.CalculateLocalPosition(mousePosition) - rect.position;
                SelectionRect.LocalRect = rect;
                return true;
            }
            return false;
        }

        bool OnGraphDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            if (button == EMouseButton.left)
            {
                SelectionRect.Visible = false;
                if (Event.current.control)
                {
                    SelectNodes((x) => x.IsSelected || SelectionRect.GlobalRect.Overlaps(x.GlobalRect, true));
                }
                else
                {
                    SelectNodes((x) => SelectionRect.GlobalRect.Overlaps(x.GlobalRect, true));
                }
                ContextMenu.Hide();
                return true;
            }
            return false;
        }

        bool OnGraphClick(EMouseButton button, Vector2 mousePositoin)
        {
            SelectNodes((x) => false);
            SelectComment(null, false);
            ContextMenu.Hide();
            if (button == EMouseButton.right)
            {
                if (EditMode == ENodeEditMode.none)
                    ContextMenu.NewNode(null);
                else
                    BeginEditNode(null, ENodeEditMode.none);
            }
            else if (button == EMouseButton.left && EditMode == ENodeEditMode.modify_child)
            {
                ContextMenu.NewNode(EditTarget);
                BeginEditNode(null, ENodeEditMode.none);
            }
            return true;
        }

        private void OnPlayStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                LoadSelectedAsset();
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                LoadSelectedAsset();
            }
        }
        
        bool OnGraphKeyUp(KeyCode key)
        {
            if (IsPlaying)
                return false;
            if (key == KeyCode.Delete)
            {
                List<BehaviourNodeGUI> nodes = new List<BehaviourNodeGUI>();
                EditNodes((x) => { if (x.IsSelected) nodes.Add(x); });
                for (int i = 0; i < nodes.Count; i++)
                {
                    TreeCanvas.RemoveElement(nodes[i]);
                    TreeGraph.RemoveNode(nodes[i]);
                }
                return true;
            }
            if (key == KeyCode.C)
            {
                float w = Mathf.Abs(SelectionRect.GlobalRect.width);
                float dx = SelectionRect.GlobalRect.width < 0 ? -w : 0;
                float h = Mathf.Abs(SelectionRect.GlobalRect.height);
                float dy = SelectionRect.GlobalRect.height < 0 ? -h : 0;
                if (w > 40 && h > 30)
                {
                    Rect rect = new Rect(SelectionRect.GlobalRect.xMin + dx, SelectionRect.GlobalRect.yMin + dy, w, h);
                    BehaviourCommentGUI comment = new BehaviourCommentGUI(this);
                    comment.LocalRect = CommentCanvas.CalculateLocalRect(rect);
                    CommentCanvas.AddElement(comment);
                    SelectNodes((x) => false);
                }
            }
            return false;
        }

        public void RebuildExecutionOrder()
        {
            mBuildIndex = true;
        }

        public void SelectNodes(FilterDelegate<BehaviourNodeGUI> selector)
        {
            for (int i = 0; i < TreeCanvas.ElementCount; i++)
            {
                BehaviourNodeGUI node = TreeCanvas.GetElement<BehaviourNodeGUI>(i);
                if (node != null)
                    node.IsSelected = selector(node);
            }
        }

        public void EditNodes(EditDelegate<BehaviourNodeGUI> editor)
        {
            for (int i = 0; i < TreeCanvas.ElementCount; i++)
            {
                BehaviourNodeGUI node = TreeCanvas.GetElement<BehaviourNodeGUI>(i);
                if (node != null)
                    editor(node);
            }
        }

        public PaintElement GetNodeBTParent(PaintElement node)
        {
            TreeGraph.GetAllParent(0, node, mCache);
            PaintElement parent = mCache.Count > 0 ? mCache[0] : null;
            mCache.Clear();
            return parent;
        }

        public void BeginEditNode(PaintElement target, ENodeEditMode mode)
        {
            if (target == null && mode != ENodeEditMode.none || EditorApplication.isPlaying)
                return;
            EditMode = mode;
            EditTarget = target;
        }

        public BehaviourNodeGUI AddChild(PaintElement parent, BehaviourMeta meta, Vector2 localPos)
        {
            if (parent != null && parent.Parent != TreeCanvas)
                return null;
            BehaviourNodeGUI node = new BehaviourNodeGUI(this);
            node.Self = new BehaviourNodeGUI.Decorator(GenerateId, meta);
            node.Self.UpdatePropertiesInfo();
            Rect r = new Rect();
            r.size = node.CalculateLocalSize();
            r.position = localPos - Vector2.right * r.size.x * 0.5f;
            node.LocalRect = r;
            TreeCanvas.AddElement(node);
            TreeGraph.AddNode(node);
            if (parent != null)
                TreeGraph.AddPath(0, parent, node);
            RebuildExecutionOrder();

            return node;
        }

        public void SelectComment(BehaviourCommentGUI target, bool multiselect)
        {
            for (int i = 0; i < CommentCanvas.ElementCount; i++)
            {
                BehaviourCommentGUI com = CommentCanvas.GetElement<BehaviourCommentGUI>(i);
                if (com == null)
                    continue;
                if(com == target)
                {
                    com.IsSelected = true;
                }
                else if (!multiselect)
                {
                    com.IsSelected = false;
                }
            }
        }
    }
}