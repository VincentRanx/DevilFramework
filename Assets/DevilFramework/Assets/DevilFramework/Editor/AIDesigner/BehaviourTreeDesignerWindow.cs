using Devil;
using Devil.AI;
using Devil.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    using BTNode = BehaviourTreeAsset.BTData;

    public class BehaviourTreeDesignerWindow : EditorCanvasWindow
    {
        public enum ENodeEditMode
        {
            none,
            modify_parent,
            modify_child,
        }

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

        public int GenerateId { get { return ++mIdCounter; } }


        bool mBuildIndex;
        int mIdCounter;

        [MenuItem("Devil Framework/Behaviour Design")]
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

        public BehaviourTreeDesignerWindow() : base()
        {
            GraphCanvas.GridSize = 200;
            SelectionRect = new SelectionGUI();
            RootCanvas.AddElement(SelectionRect);
            mScaledCanvas.Pivot = new Vector2(0.5f, 0.25f);

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

            mClipCanvas.Resort(true);

            GraphCanvas.OnDragBegin = OnGraphDragBegin;
            GraphCanvas.OnDrag = OnGraphDrag;
            GraphCanvas.OnDragEnd = OnGraphDragEnd;
            GraphCanvas.OnMouseClick = OnGraphClick;
            GraphCanvas.OnKeyUp = OnGraphKeyUp;
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

        BehaviourNodeGUI InitNode(BTNode node)
        {
            BehaviourMeta meta = Installizer.FindBTMeta(node.m_Type, node.m_Name);
            if (meta == null)
                return null;
            BehaviourNodeGUI bnode = new BehaviourNodeGUI(this);
            bnode.Self = new BehaviourNodeGUI.Decorator(node.m_Id, meta);
            bnode.Self.ParseData(node.m_JsonData);
            if (node.m_Services != null)
            {
                for (int i = 0; i < node.m_Services.Length; i++)
                {
                    BTNode serv = BehaviourAsset.GetDataById(node.m_Services[i]);
                    if (serv == null)
                        continue;
                    BehaviourMeta bm = Installizer.FindBTMeta(EBTNodeType.service, serv.m_Name);
                    if (bm != null)
                    {
                        BehaviourNodeGUI.Decorator decor = new BehaviourNodeGUI.Decorator(serv.m_Id, bm);
                        decor.ParseData(serv.m_JsonData);
                        bnode.services.Add(decor);
                    }
                }
            }
            //bnode.services.AddRange(node.m_Services);
            if (node.m_Conditions != null)
            {
                for (int i = 0; i < node.m_Conditions.Length; i++)
                {
                    BTNode cond = BehaviourAsset.GetDataById(node.m_Conditions[i]);
                    if (cond == null)
                        continue;
                    BehaviourMeta bm = Installizer.FindBTMeta(EBTNodeType.condition, cond.m_Name);
                    if (bm != null)
                    {
                        BehaviourNodeGUI.Decorator decor = new BehaviourNodeGUI.Decorator(cond.m_Id, bm);
                        decor.ParseData(cond.m_JsonData);
                        bnode.conditions.Add(decor);
                    }
                }
            }
            //bnode.decorators.AddRange(node.m_Decorators);
            Rect r = new Rect();
            r.size = bnode.CalculateLocalSize();
            r.position = node.m_Pos - Vector2.right * r.size.x * 0.5f;
            bnode.LocalRect = r;
            TreeGraph.AddNode(bnode);
            TreeCanvas.AddElement(bnode);
            int children = node.m_Children == null ? 0 : node.m_Children.Length;
            for (int i = 0; i < children; i++)
            {
                BTNode child = BehaviourAsset.GetDataById(node.m_Children[i]);
                if (child == null)
                    continue;
                BehaviourNodeGUI bchild = InitNode(child);
                if(bchild != null)
                   TreeGraph.AddPath(0, bnode, bchild);
            }
            return bnode;
        }

        public void InitWith(BehaviourTreeRunner runner)
        {
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
                BTNode broot = BehaviourAsset.GetDataById(BehaviourAsset.m_RootNodeId);
                if (broot != null)
                {
                    BehaviourNodeGUI bnode = InitNode(broot);
                    TreeGraph.AddPath(0, mRootGUI, bnode);
                }
                for(int i = 0; i < BehaviourAsset.m_Comments.Length; i++)
                {
                    BehaviourCommentGUI comment = new BehaviourCommentGUI(this);
                    comment.Comment = BehaviourAsset.m_Comments[i].m_Comment;
                    comment.LocalRect = BehaviourAsset.m_Comments[i].m_Rect;
                    CommentCanvas.AddElement(comment);
                }
            }
            ResetIdCounter();
            RebuildExecutionOrder();
            UpdateStateInfo();
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
            List<PaintElement> children = new List<PaintElement>();
            TreeGraph.GetAllChildren(0, TreeGraph.Root, children);
            List<BTNode> nodes = new List<BTNode>();
            for (int i = 0; i < children.Count; i++)
            {
                BTNode node = SaveAssetAtRoot(children[i], nodes);
                if (node != null)
                {
                    BehaviourAsset.m_RootNodeId = node.m_Id;
                    GlobalUtil.Sort(nodes, (x, y) => x.m_Id - y.m_Id);
                    BehaviourAsset.m_Datas = nodes.ToArray();
                    BehaviourAsset.m_Sorted = true;
                    break;
                }
            }
            BehaviourTreeAsset.Comment[] comments = new BehaviourTreeAsset.Comment[CommentCanvas.ElementCount];
            for(int i = 0; i < comments.Length; i++)
            {
                comments[i] = new BehaviourTreeAsset.Comment();
                BehaviourCommentGUI com = CommentCanvas.GetElement<BehaviourCommentGUI>(i);
                if(com!= null)
                {
                    comments[i].m_Rect = com.LocalRect;
                    comments[i].m_Comment = com.Comment ?? "";
                }
            }
            BehaviourAsset.m_Comments = comments;
            EditorUtility.SetDirty(BehaviourAsset);
            if(refresh)
                AssetDatabase.Refresh();
        }

        BTNode SaveAssetAtRoot(PaintElement root, ICollection<BTNode> collection)
        {
            BehaviourNodeGUI node = root as BehaviourNodeGUI;
            if (node == null)
                return null;
            BTNode bnode = node.ExportNodeData(collection);
            List<PaintElement> children = new List<PaintElement>();
            TreeGraph.GetAllChildren(0, root, children);
            GlobalUtil.Sort(children, (x, y) => x.LocalRect.center.x <= y.LocalRect.center.x ? -1 : 1);
            bnode.m_Children = new int[children.Count];
            for (int i = 0; i < children.Count; i++)
            {
                BTNode child = SaveAssetAtRoot(children[i], collection);
                bnode.m_Children[i] = child == null ? -1 : child.m_Id;
            }
            return bnode;
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectedObjectChanged;
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
            OnSelectedObjectChanged();
            
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectedObjectChanged;
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
        }

        void OnSelectedObjectChanged()
        {
            GameObject obj = Selection.activeGameObject;
            BehaviourTreeRunner r = obj == null ? null : obj.GetComponent<BehaviourTreeRunner>();
#if DEBUG_AI
            if (EditorApplication.isPlaying)
            {
                BindBehaviourTreeEvent(false);
            }
#endif
            InitWith(r);
#if DEBUG_AI
            if (EditorApplication.isPlaying)
            {
                BindBehaviourTreeEvent(true);
            }
#endif
            Repaint();
        }

#if DEBUG_AI
        void BindBehaviourTreeEvent(bool bind)
        {
            if (Runner == null)
                return;
            if (bind)
            {
                Runner.OnBehaviourTreeBegin += OnBehaviourTreeBegin;
                Runner.OnBehaviourTreeFrame += OnBehaviourTreeFrame;
            }
            else
            {
                Runner.OnBehaviourTreeBegin -= OnBehaviourTreeBegin;
                Runner.OnBehaviourTreeFrame -= OnBehaviourTreeFrame;
            }
        }
        private void OnBehaviourTreeFrame(BehaviourTreeRunner btree)
        {
            if (btree == Runner)
            {
                ResetRuntimeTreeState(btree.RootNode);
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
                    node.BTRuntimeState = state;
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

        void ResetRuntimeTreeState(BTNodeBase node)
        {
            if (node == null)
                return;
            SetNodeRuntimeState(node.NodeId, node.State);
            for (int i = 0; i < node.ChildLength; i++)
            {
                BTNodeBase child = node.ChildAt(i);
                ResetRuntimeTreeState(child);
            }
        }

#endif

        protected override void OnTitleGUI()
        {
            GUILayout.Label(BehaviourAsset == null ? "<i>NO TREE</i>" : BehaviourAsset.name);
            if (GUILayout.Button("REVERT", "TE toolbarbutton", GUILayout.Width(70)))
            {
                OnSelectedObjectChanged();
            }
            if (GUILayout.Button("APPLY", "TE toolbarbutton", GUILayout.Width(70)))
            {
                SaveAsset();
            }
        }

        protected override void OnPostGUI()
        {
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
                SelectComment(null);
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
                    SelectNodes((x) => x.Selected || SelectionRect.GlobalRect.Overlaps(x.GlobalRect, true));
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
            SelectComment(null);
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
#if DEBUG_AI
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                OnSelectedObjectChanged();
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                OnSelectedObjectChanged();
            }
#endif
        }

        bool OnGraphKeyUp(KeyCode key)
        {
            if (key == KeyCode.Delete)
            {
                List<BehaviourNodeGUI> nodes = new List<BehaviourNodeGUI>();
                EditNodes((x) => { if (x.Selected) nodes.Add(x); });
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
                    node.Selected = selector(node);
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

        public void SelectComment(BehaviourCommentGUI target)
        {
            for (int i = 0; i < CommentCanvas.ElementCount; i++)
            {
                BehaviourCommentGUI com = CommentCanvas.GetElement<BehaviourCommentGUI>(i);
                if (com == null)
                    continue;
                com.IsSelected = com == target;
            }
        }
    }
}