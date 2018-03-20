using Devil;
using Devil.AI;
using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    using System;
    using BTNode = BehaviourTreeAsset.BTNodeInfo;

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

        SelectionGUI mSelectionRect;
        public Graph<PaintElement> TreeGraph { get; private set; }
        List<PaintElement> mCache = new List<PaintElement>();
        public BehaviourTreeContextMenuGUI ContextMenu { get; private set; }
        public EditorGUICanvas TreeCanvas { get; private set; }
        public ENodeEditMode EditMode { get; private set; }
        public PaintElement EditTarget { get; private set; }

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
            mSelectionRect = new SelectionGUI();
            RootCanvas.AddElement(mSelectionRect);
            mScaledCanvas.Pivot = new Vector2(0.5f, 0.25f);

            TreeGraph = new Graph<PaintElement>(1);

            Rect grect = new Rect(-GraphCanvas.LocalRect.size * 0.5f, GraphCanvas.LocalRect.size);
            BehaviourTreeWireGUI wires = new BehaviourTreeWireGUI(this);
            wires.LocalRect = grect;
            GraphCanvas.AddElement(wires);

            TreeCanvas = new EditorGUICanvas();
            TreeCanvas.LocalRect = grect;
            GraphCanvas.AddElement(TreeCanvas);

            ContextMenu = new BehaviourTreeContextMenuGUI(this);
            GraphCanvas.AddElement(ContextMenu);

            mClipCanvas.Resort();

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
                if (node != null && node.BTNodeId > mIdCounter)
                    mIdCounter = node.BTNodeId;
            }
        }

        BehaviourNodeGUI InitNode(BTNode node)
        {
            BehaviourMeta meta = Installizer.FindBTMeta(node.m_Type, node.m_Name);
            if (meta == null)
                return null;
            BehaviourNodeGUI bnode = new BehaviourNodeGUI(this);
            bnode.BTNodeId = node.m_Id;
            bnode.BTNodeName = meta;// string.IsNullOrEmpty(node.m_Name) ? node.m_Type.ToString().ToUpper() : node.m_Name;
            if (node.m_Services != null)
            {
                for (int i = 0; i < node.m_Services.Length; i++)
                {
                    BehaviourMeta bm = Installizer.FindBTMeta(EBTNodeType.service, node.m_Services[i]);
                    if (bm != null)
                        bnode.services.Add(bm);
                }
            }
            //bnode.services.AddRange(node.m_Services);
            if (node.m_Conditions != null)
            {
                for (int i = 0; i < node.m_Conditions.Length; i++)
                {
                    BehaviourMeta bm = Installizer.FindBTMeta(EBTNodeType.condition, node.m_Conditions[i]);
                    if (bm != null)
                        bnode.conditions.Add(bm);
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
                BTNode child = BehaviourAsset.GetNodeById(node.m_Children[i]);
                if (child == null)
                    continue;
                BehaviourNodeGUI bchild = InitNode(child);
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
            TreeGraph.Clear();

            BehaviourRootGUI root = new BehaviourRootGUI(this);
            root.UpdateLocalData();
            TreeCanvas.AddElement(root);
            TreeGraph.AddNode(root);

            if (BehaviourAsset != null)
            {
                BTNode broot = BehaviourAsset.GetNodeById(BehaviourAsset.m_RootNodeId);
                if (broot != null)
                {
                    BehaviourNodeGUI bnode = InitNode(broot);
                    TreeGraph.AddPath(0, root, bnode);
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
                    BehaviourAsset.m_Nodes = nodes.ToArray();
                    BehaviourAsset.m_Sorted = true;
                    EditorUtility.SetDirty(BehaviourAsset);
                    break;
                }
            }
        }

        BTNode SaveAssetAtRoot(PaintElement root, ICollection<BTNode> collection)
        {
            BehaviourNodeGUI node = root as BehaviourNodeGUI;
            if (node == null)
                return null;
            BTNode bnode = node.InstantNodeInfo();
            collection.Add(bnode);
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
                if (node != null && node.BTNodeId == nodeId)
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
                Rect rect = mSelectionRect.LocalRect;
                rect.size = Vector2.zero;
                rect.position = RootCanvas.CalculateLocalPosition(mousePosition);
                mSelectionRect.LocalRect = rect;
                mSelectionRect.Visible = true;
                return true;
            }
            return false;
        }

        bool OnGraphDrag(EMouseButton button, Vector2 mousePosition, Vector2 delta)
        {
            if (button == EMouseButton.left)
            {
                Rect rect = mSelectionRect.LocalRect;
                rect.size = RootCanvas.CalculateLocalPosition(mousePosition) - rect.position;
                mSelectionRect.LocalRect = rect;
                return true;
            }
            return false;
        }

        bool OnGraphDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            if (button == EMouseButton.left)
            {
                mSelectionRect.Visible = false;
                if (Event.current.control)
                {
                    SelectNodes((x) => x.Selected || mSelectionRect.GlobalRect.Overlaps(x.GlobalRect, true));
                }
                else
                {
                    SelectNodes((x) => mSelectionRect.GlobalRect.Overlaps(x.GlobalRect, true));
                }
                ContextMenu.Hide();
                return true;
            }
            return false;
        }

        bool OnGraphClick(EMouseButton button, Vector2 mousePositoin)
        {
            SelectNodes((x) => false);
            ContextMenu.Hide();
            if (button == EMouseButton.right)
            {
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

        public BehaviourNodeGUI AddChild(PaintElement parent, BehaviourMeta meta, Vector2 position)
        {
            if (parent == null || parent.Parent != TreeCanvas)
                return null;
            BehaviourNodeGUI node = new BehaviourNodeGUI(this);
            node.BTNodeId = ++mIdCounter;
            node.BTNodeName = meta;
            Rect r = new Rect();
            r.size = node.CalculateLocalSize();
            r.position = position - Vector2.right * r.size.x * 0.5f;
            node.LocalRect = r;
            TreeCanvas.AddElement(node);
            TreeGraph.AddNode(node);
            TreeGraph.AddPath(0, parent, node);
            RebuildExecutionOrder();
            return node;
        }
    }
}