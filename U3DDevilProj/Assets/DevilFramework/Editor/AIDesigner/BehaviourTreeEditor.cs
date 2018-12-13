using System.Collections.Generic;
using System.IO;
using Devil;
using Devil.AI;
using Devil.Utility;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class BehaviourTreeEditor : EditorCanvasWindow
    {
        public const int ACT_OPEN_ASSET = 1;
        public const int ACT_LOAD_GRAPH = 2;
        public const int ACT_UPDATE_WIRES = 3;
        public const int ACT_EDIT_DO = 4;

        public static BehaviourTreeEditor ActiveBTEditor { get; private set; }

        public static bool IsActive { get { return ActiveBTEditor != null; } }

        public static BehaviourTreeAsset EditingAsset
        {
            get
            {
                if (ActiveBTEditor != null && ActiveBTEditor.Binder != null)
                    return ActiveBTEditor.Binder.source;
                else
                    return null;
            }
        }

        [MenuItem("Devil Framework/AI/Behaviour Tree Editor")]
        public static void OpenBTEditor()
        {
            var window = ActiveBTEditor;
            if (window == null)
            {
                window = GetWindow<BehaviourTreeEditor>(typeof(SceneView));
                window.Show();
            }
            else
            {
                window.AddDelayTask(ACT_OPEN_ASSET, () => window.mAssetBinder.SetSelectedAsset());
            }
        }

        public static void OpenBTEditor(BehaviourTreeRunner runner)
        {
            var window = ActiveBTEditor;
            if (window == null)
            {
                window = GetWindow<BehaviourTreeEditor>(typeof(SceneView));
                window.Show();
            }
            else
            {
                window.AddDelayTask(ACT_OPEN_ASSET, () => window.mAssetBinder.SetBehaviourTreeRunner(runner));
            }
        }

        public static void OpenBTEditor(BehaviourTreeAsset asset)
        {
            var window = ActiveBTEditor;
            if (window == null)
            {
                window = GetWindow<BehaviourTreeEditor>(typeof(SceneView));
                window.Show();
            }
            window.AddDelayTask(ACT_OPEN_ASSET, () =>
            {
                window.mAssetBinder.SetBehaviourTreeAsset(asset);
            });
        }

        public static BlackboardAsset UsingBlackboard
        {
            get
            {
                var r = ActiveBTEditor == null ? null : ActiveBTEditor.TargetRunner;
                if (r != null && r.BlackboardAsset != null)
                    return r.BlackboardAsset;
                return sUsedBlackboard;
            }
        }

        static BlackboardAsset sUsedBlackboard; // 使用中的黑板

        static BTEditorMenu sNewTreeMenu; // 创建树
        static BTEditorMenu sTreeMenu; // 树编辑
        static BTEditorMenu sContextMenu; // 右键菜单
        static BTEditorMenu sNodeMenu; // 新节点
        static BTHotkeyMenu sHotkeyMenu;
        static BTEditorMenu sConditionMenu; // 
        public static BTEditorMenu sDebugMenu = new BTEditorMenu();

        public static void InitModules()
        {
            sHotkeyMenu = new BTHotkeyMenu();
            sNewTreeMenu = new BTEditorMenu();
            sTreeMenu = new BTEditorMenu();
            sContextMenu = new BTEditorMenu();
            sNodeMenu = new BTEditorMenu();
            sConditionMenu = new BTEditorMenu();
            sDebugMenu = new BTEditorMenu();

            var newtree = BTEditorMenu.NewItem("创建行为树", (menu, index, data) =>
            {
                var tree = ScriptableObject.CreateInstance<BehaviourTreeAsset>();
                tree.name = "INNER_AI";
                menu.editor.Binder.SetBehaviourTreeAsset(tree);
            });

            var resetItem = BTEditorMenu.NewItem("重置", (menu, index, data) =>
            {
                menu.editor.Binder.Reset();
            });

            var saveitem = BTEditorMenu.NewItem("保存", (menu, index, data) => {
                menu.editor.Binder.SaveAsset();
            });

            var delitem = BTEditorMenu.NewItem("删除", (menu, index, data) => {
                menu.editor.DeleteContext(menu.editor.mContextNode);
            });

            var moveup = BTEditorMenu.NewItem("向上移动", (menu, index, data) =>
            {
                menu.editor.MoveContextCondition(-1);
            });
            var movedown = BTEditorMenu.NewItem("向下移动", (menu, index, data) =>
            {
                menu.editor.MoveContextCondition(1);
            });
            var copy = BTEditorMenu.NewItem("复制", (menu, index, data) =>
            {
                menu.editor.CopySelection();
            });
            var paste = BTEditorMenu.NewItem("粘贴", (menu, index, data) => {
                menu.editor.PasteSelection();
            });

            sNewTreeMenu.AddItem(newtree);
            sTreeMenu.AddItem(newtree);
            sTreeMenu.AddItem(resetItem);
            sContextMenu.AddItem(resetItem);
            sDebugMenu.AddItem(resetItem);
            sTreeMenu.AddItem(saveitem);
            sContextMenu.AddItem(saveitem);
            sContextMenu.AddItem(copy);
            sTreeMenu.AddItem(paste);
            sContextMenu.AddItem(delitem);
            sConditionMenu.AddItem(delitem);
            sConditionMenu.AddItem(moveup);
            sConditionMenu.AddItem(movedown);

            BTEditorMenu.OnItemSelected newnode = (menu, index, data) =>
            {
                var mod = data as AIModules.Module;
                if (mod != null)
                {
                    menu.editor.AddNewNode(mod);
                }
            };

            var composites = AIModules.GetModules(AIModules.CATE_COMPOSITE);
            var tasks = AIModules.GetModules(AIModules.CATE_TASK);
            foreach (var t in composites)
            {
                var item = BTEditorMenu.NewItem(t.Path, newnode, t);
                sTreeMenu.AddItem(item);
                //sContextMenu.AddItem(item);
                sNodeMenu.AddItem(item);
            }
            foreach (var t in tasks)
            {
                var item = BTEditorMenu.NewItem(t.Path, newnode, t);
                sTreeMenu.AddItem(item);
                //sContextMenu.AddItem(item);
                sNodeMenu.AddItem(item);
            }
            var conds = AIModules.GetModules(AIModules.CATE_CONDITION);
            foreach (var t in conds)
            {
                var item = BTEditorMenu.NewItem(t.Path, newnode, t);
                sContextMenu.AddItem(item);
                sConditionMenu.AddItem(item);
            }
            var servs = AIModules.GetModules(AIModules.CATE_SERVICE);
            foreach (var t in servs)
            {
                var item = BTEditorMenu.NewItem(t.Path, newnode, t);
                sTreeMenu.AddItem(item);
                //sContextMenu.AddItem(item);
                sNodeMenu.AddItem(item);
            }

            sDebugMenu.AddItem("添加断点", (menu, index, data) =>
            {
                var item = menu.editor.mContextNode;
                if (item != null)
                    item.BreakToggle = true;
            }, null);
            sDebugMenu.AddItem("删除断点", (menu, index, data) =>
            {
                var item = menu.editor.mContextNode;
                if (item != null)
                    item.BreakToggle = false;
            }, null);
        }

        BehaviourTreeAssetBinder mAssetBinder;
        public BehaviourTreeAssetBinder Binder { get { return mAssetBinder; } }
        public BehaviourTreeAsset SourceTree { get { return mAssetBinder == null ? null : mAssetBinder.source; } }
        public BehaviourTreeAsset TargetTree { get { return mAssetBinder == null ? null : mAssetBinder.targetTree; } }
        public BehaviourTreeRunner TargetRunner { get { return mAssetBinder == null ? null : mAssetBinder.targetRunner; } }
        public BlackboardMonitorGUI BlackboardMonitor { get; private set; }
        EditorSelection mSelectionRect;
        public EditorGUICanvas AIGraph { get; private set; }
        public TipBox Tip { get; private set; }
        public BTRootNodeGUI RootNode { get; private set; }
        BTWireGUI mWires;
        public BTWireGUI Wires { get { return mWires; } }

        const int undo_size = 100;

        List<BehaviourNode> mSelection = new List<BehaviourNode>();
        public List<BehaviourNode> SelectionNodes { get { return mSelection; } }
        List<Object> mSelectionAssets = new List<Object>();
        void UpdateSelectionAssets()
        {
            bool isRuntime = Application.isPlaying && mAssetBinder.IsRunning();
            mSelectionAssets.Clear();
            foreach (var t in mSelection)
            {
                if (t == RootNode)
                {
                    BehaviourTreeAsset tree;
                    if (isRuntime)
                        tree = mAssetBinder.runtime;
                    else
                        tree = mAssetBinder.targetTree;
                    if (tree != null)
                        mSelectionAssets.Add(tree);
                }
                else
                {
                    var node = t.GetNode();
                    if (node == null)
                        node = t.GetContext();
                    if (isRuntime && node != null)
                        node = mAssetBinder.runtime.GetNodeById(node.Identify);
                    if (node != null && node.Asset != null)
                    {
                        mSelectionAssets.Add(node.Asset);
                    }
                }
            }
        }

        // 当前设置父子节点的目标
        public BehaviourNode RaycastNode { get; private set; }
        public BehaviourNode PresentParentRequest { get; private set; }
        public BehaviourNode PresentChildRequest { get; private set; }
        public bool IsRequestParentOrChild { get { return PresentParentRequest != null || PresentChildRequest != null; } }
        public BehaviourHelpGUI HelpBox { get; private set; }
        
        const int HISTORY_SIZE = 20;
        // 资源打开记录
        List<string> mHistory = new List<string>();
        GUIContent[] mHistoryContent;

        BTCopy mCopyDo;

        void AllocHistoryContent()
        {
            if (mHistoryContent == null || mHistoryContent.Length != mHistory.Count)
            {
                mHistoryContent = new GUIContent[mHistory.Count];
                for (int i = 0; i < mHistory.Count; i++)
                {
                    mHistoryContent[i] = new GUIContent(Path.GetFileName(mHistory[i]));
                }
            }
            else
            {
                for (int i = 0; i < mHistory.Count; i++)
                {
                    mHistoryContent[i].text = Path.GetFileName(mHistory[i]);
                }
            }
        }

        void AddHistory(BehaviourTreeAsset asset)
        {
            if (asset == null)
                return;
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return;
            mHistory.Remove(path);
            while (mHistory.Count >= HISTORY_SIZE)
            {
                mHistory.RemoveAt(mHistory.Count - 1);
            }
            mHistory.Insert(0, path);
        }

        // 上下文对象
        BehaviourNode mContextNode;
        Vector2 mContextPos;
        List<BTEditableDO> mUndoStack = new List<BTEditableDO>(100);
        public void DoEdit(BTEditableDO todo)
        {
            if (Application.isPlaying || todo == null)
                return;
            AddDelayTask(ACT_EDIT_DO, () => {
                if (todo.DoEditWithUndo())
                {
                    mUndoStack.Add(todo);
                    if (mUndoStack.Count > 100)
                        mUndoStack.RemoveAt(0);
                }
            });
        }
        List<BehaviourTreeRunner.AssetBinder> mBindStack = new List<BehaviourTreeRunner.AssetBinder>();

        public void MoveContextCondition(int offset)
        {
            var node = mContextNode as BTNodeGUI;
            if (node == null)
                return;
            var n = node.GetNode();
            var con = node.GetContext();
            if (n == null || con == null || n == con)
                return;
            var asset = n.Asset as BTNodeAsset;
            if (asset != null)
            {
                asset.EditorSwitchCondition(con, offset);
                node.Resize();
            }
        }

        public void RequestParent(BehaviourNode node)
        {
            PresentParentRequest = Application.isPlaying || TargetTree == null ? null : node;
            PresentChildRequest = null;
        }

        public void RequestChild(BehaviourNode node)
        {
            PresentParentRequest = null;
            PresentChildRequest = Application.isPlaying || TargetTree == null ? null : node;
        }

        protected override void InitCanvas()
        {
            titleContent = new GUIContent("AI Designer");
            base.InitCanvas();
            GraphCanvas.LocalRect = new Rect(-50000, -50000, 100000, 100000);

            mWires = new BTWireGUI(this);
            GraphCanvas.AddElement(mWires);

            AIGraph = new EditorGUICanvas();
            AIGraph.LocalRect = new Rect(-50000, -50000, 100000, 100000);
            GraphCanvas.AddElement(AIGraph);

            RootNode = new BTRootNodeGUI(this);
            AIGraph.AddElement(RootNode);

            BlackboardMonitor = new BlackboardMonitorGUI(this);
            BlackboardMonitor.LocalRect = new Rect(0, 0, 200, 180);
            RootCanvas.AddElement(BlackboardMonitor);

            HelpBox = new BehaviourHelpGUI();
            HelpBox.SortOrder = -10;
            RootCanvas.AddElement(HelpBox);

            mSelectionRect = new EditorSelection();
            mSelectionRect.Visible = false;
            GraphCanvas.AddElement(mSelectionRect);

            Tip = new TipBox();
            RootCanvas.AddElement(Tip);

            RootCanvas.Resort(true);

        }

        protected override Vector2 GetFocusDeltaPosition()
        {
            if (AIGraph.ElementCount == 0)
                return base.GetFocusDeltaPosition();
            var p = new Vector2();
            for (int i = 0; i < AIGraph.ElementCount; i++)
            {
                p += AIGraph.ElementAt(i).GlobalRect.center;
            }
            p /= (float)AIGraph.ElementCount;
            return ScaledCanvas.GlobalCentroid - p;// GraphCanvas.GlobalCentroid;
        }

        protected override void UpdateStateInfo()
        {
            base.UpdateStateInfo();
        }

        protected override void OnCanvasStart()
        {
            BehaviourNode.InitGUIStyle();
            base.OnCanvasStart();
        }

        bool EnableDropAssets()
        {
            foreach (var asset in DragAndDrop.objectReferences)
            {
                if (asset is GameObject || asset is BehaviourTreeAsset || asset is BTAsset || asset is BlackboardAsset)
                    return true;
            }
            return false;
        }

        protected override void OnResized()
        {
            base.OnResized();
            RaycastNode = GetRaycastNode(GlobalMousePosition);
            for (int i = 0; i < AIGraph.ElementCount; i++)
            {
                var bt = AIGraph.GetElement<BehaviourNode>(i);
                if (bt != null && !bt.cliped && bt != RaycastNode)
                    bt.DrawComment(false);
            }
        }

        protected override void OnPainted()
        {
            base.OnPainted();
            if (RaycastNode != null)
                RaycastNode.DrawComment(true);
        }

        protected override void OnCanvasGUI()
        {
            base.OnCanvasGUI();
            if (InterceptMouse)
                DragAndDrop.visualMode = EnableDropAssets() ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.None;
            BehaviourNode.SetFontScale(GraphCanvas.GlobalScale);
        }

        protected override void OnPostGUI()
        {
            base.OnPostGUI();

            if (InterceptMouse && Event.current.type == EventType.DragPerform && mAssetBinder != null)
            {
                bool act = false;
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject)
                    {
                        var tree = ((GameObject)obj).GetComponent<BehaviourTreeRunner>();
                        if (tree != null)
                        {
                            mAssetBinder.SetBehaviourTreeRunner(tree);
                            act = true;
                            break;
                        }
                    }
                    else if (obj is BehaviourTreeAsset)
                    {
                        mAssetBinder.SetBehaviourTreeAsset((BehaviourTreeAsset)obj);
                        act = true;
                        break;
                    }
                    else if (obj is BTAsset)
                    {
                        var owner = ((BTAsset)obj).TreeAsset;
                        if (owner != null)
                        {
                            mAssetBinder.SetBehaviourTreeAsset(owner);
                            act = true;
                            break;
                        }
                    }
                    else if(obj is BlackboardAsset)
                    {
                        sUsedBlackboard = obj as BlackboardAsset;
                        act = true;
                        break;
                    }
                }
                DragAndDrop.AcceptDrag();
                if (!act)
                {
                    EditorUtility.DisplayDialog("Error", "没有可用的行为树资源.", "OK");
                }
            }
        }

        protected override void OnEnable()
        {
            ActiveBTEditor = this;
            base.OnEnable();
            GraphCanvas.OnMouseClick = InteractCraphClick;
            GraphCanvas.OnDragBegin = InteractGraphDragBegin;
            GraphCanvas.OnDrag = InteractGraphDrag;
            GraphCanvas.OnDragEnd = InteractGraphDragEnd;
            GraphCanvas.OnKeyDown = InteractGraphKeyDown;
            if (mAssetBinder == null)
            {
                mAssetBinder = new BehaviourTreeAssetBinder();
            }
            mAssetBinder.updateTreeAsset += ReloadGraph;
            if (mAssetBinder.targetTree == null)
                mAssetBinder.SetSelectedAsset();
            else
                ReloadGraph();
        }

        protected override void OnDisable()
        {
            if (ActiveBTEditor == this)
            {
                ActiveBTEditor = null;
            }
            base.OnDisable();
            GraphCanvas.OnMouseClick = null;
            GraphCanvas.OnDragBegin = null;
            GraphCanvas.OnDrag = null;
            GraphCanvas.OnDragEnd = null;
            if (mAssetBinder != null)
            {
                mAssetBinder.updateTreeAsset -= ReloadGraph;
                mAssetBinder.Dispose();
                mAssetBinder = null;
            }
        }

        protected override void OnReadCustomData(JObject data)
        {
            base.OnReadCustomData(data);
            HelpBox.Visible = data.Value<bool>("help");
            var his = data.Value<string>("history");
            if (!string.IsNullOrEmpty(his))
            {
                mHistory.Clear();
                StringUtil.ParseArray(his, mHistory, '\n');
            }
            var black = data.Value<string>("black");
            if (sUsedBlackboard == null && !string.IsNullOrEmpty(black))
                sUsedBlackboard = AssetDatabase.LoadAssetAtPath<BlackboardAsset>(black);
        }

        protected override void OnSaveCustomData(JObject data)
        {
            base.OnSaveCustomData(data);
            data["help"] = HelpBox.Visible;
            data["history"] = StringUtil.Gather(mHistory, -1, "\n");
            if (sUsedBlackboard != null)
                data["black"] = AssetDatabase.GetAssetPath(sUsedBlackboard);
        }

        protected override void OnTitleGUI()
        {
            HelpBox.Visible = GUILayout.Toggle(HelpBox.Visible, "<b>?</b>", "TE toolbarbutton", GUILayout.Width(20));
            if (mHistory.Count > 0 && GUILayout.Button("历史", "TE toolbarbutton", GUILayout.Width(45)))
            {
                AllocHistoryContent();
                var p = mAssetBinder.source == null ? null : AssetDatabase.GetAssetPath(mAssetBinder.source);
                var index = string.IsNullOrEmpty(p) ? -1 : mHistory.IndexOf(p);
                EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition, Vector2.zero), mHistoryContent, index, (x, y, z) =>
                {
                    var asset = AssetDatabase.LoadAssetAtPath<BehaviourTreeAsset>(mHistory[z]);
                    if (asset == null)
                    {
                        mHistory.RemoveAt(z);
                        EditorUtility.DisplayDialog("Warning", "资源已丢失！！！", "OK");
                    }
                    else
                    {
                        mAssetBinder.SetBehaviourTreeAsset(asset);
                    }
                }, null);
            }
            //GUILayout.Label(" ", "TE toolbar", GUILayout.Width(15));
            bool ping = false;
            if (mAssetBinder.targetRunner == null)
                GUILayout.Button("[NO Runner]", "GUIEditor.BreadcrumbLeft");
            else
                ping = GUILayout.Button(mAssetBinder.targetRunner.name, "GUIEditor.BreadcrumbLeft");
            if (ping)
                EditorGUIUtility.PingObject(mAssetBinder.targetRunner);
            ping = false;
            if (mAssetBinder.source == null)
            {
                GUILayout.Button("[NO Tree]", "GUIEditor.BreadcrumbMid");
            }
            else
            {
                int sel = -1;
                for (int i = 0; i < mBindStack.Count; i++)
                {
                    if (GUILayout.Button(mBindStack[i].Name, "GUIEditor.BreadcrumbMid"))
                        sel = i;
                }
                if (mBindStack.Count == 0 || mBindStack[mBindStack.Count - 1].Source != mAssetBinder.source)
                    ping = GUILayout.Button(mAssetBinder.AssetName, "GUIEditor.BreadcrumbMid");
                if (sel >= 0 && sel < mBindStack.Count - 1)
                {
                    mAssetBinder.SetBehaviourBinder(mBindStack[sel]);
                }
                else if (sel != -1)
                    ping = true;
            }
            if (ping)
                EditorGUIUtility.PingObject(mAssetBinder.source);

            GUILayout.Label(" ");


            if (GUILayout.Button("重置", "TE toolbarbutton", GUILayout.Width(60)))
            {
                mAssetBinder.Reset();
            }
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            if (GUILayout.Button("保存", "TE toolbarbutton", GUILayout.Width(60)))
            {
                mAssetBinder.SaveAsset();
                AddHistory(mAssetBinder.source);
            }
            EditorGUI.EndDisabledGroup();
        }

        void ReloadGraph()
        {
            AddDelayTask(ACT_LOAD_GRAPH, UpdateTreeGraph);
        }

        void UpdateTreeGraph()
        {
            mCopyDo = null;
            AddHistory(mAssetBinder.source);
            mBindStack.Clear();
            mUndoStack.Clear();
            AIGraph.ClearElements();
            mContextNode = null;
            RaycastNode = null;
            RequestChild(null);

            if (Application.isPlaying)
            {
                var bind = mAssetBinder.RuntimeBinder;
                while (bind != null)
                {
                    mBindStack.Insert(0, bind);
                    bind = bind.Parent;
                }
            }
            AIGraph.AddElement(RootNode);
            if (mAssetBinder.targetTree != null)
            {
                List<BTNode> datas = new List<BTNode>();
                mAssetBinder.targetTree.GetAllNodes(datas);
                foreach (var t in datas)
                {
                    if(t.Asset == null)
                    {
                        mAssetBinder.targetTree.EditorDeleteNode(t);
                        continue;
                    }
                    if (t.Asset is BTNodeAsset)
                    {
                        var node = new BTNodeGUI(this, t);
                        AIGraph.AddElement(node);
                    }
                }
            }
            RootNode.Resize();
            mWires.UpdateWires();
            var runner = TargetRunner;
            BlackboardMonitor.Visible = runner != null && Application.isPlaying;
            BlackboardMonitor.Blackboard = runner == null ? null : runner.Blackboard;
        }

        public void EditNodes(System.Action<BehaviourNode> callback)
        {
            if (callback == null)
                return;
            for (int i = 0; i < AIGraph.ElementCount; i++)
            {
                var node = AIGraph.GetElement<BehaviourNode>(i);
                if (node != null)
                    callback(node);
            }
        }

        public BehaviourNode GetRaycastNode(Vector2 globalPos)
        {
            for (int i = AIGraph.ElementCount - 1; i >= 0; i--)
            {
                var node = AIGraph.GetElement<BehaviourNode>(i);
                if (node != null && node.Visible && node.GlobalRect.Contains(globalPos))
                    return node;
            }
            return null;
        }

        public void ClearSelection()
        {
            foreach (var t in mSelection)
            {
                t.selected = false;
            }
            mSelection.Clear();
            mSelectionAssets.Clear();
            mAssetBinder.SelectTarget();
        }

        public void SetSelections(FilterDelegate<BehaviourNode> select)
        {
            mSelection.Clear();
            for (int i = 0; i < AIGraph.ElementCount; i++)
            {
                var node = AIGraph.GetElement<BehaviourNode>(i);
                if (node != null)
                {
                    node.selected = select(node);
                    if (node.selected)
                    {
                        mSelection.Add(node);
                    }
                }
            }
            UpdateSelectionAssets();
            if (mSelectionAssets.Count > 0)
            {
                Selection.objects = mSelectionAssets.ToArray();
            }
            else if (mAssetBinder != null)
            {
                mAssetBinder.SelectTarget();
            }
        }

        bool InteractCraphClick(EMouseButton btn, Vector2 mousePos)
        {
            if (btn == EMouseButton.left)
            {
                if (IsRequestParentOrChild && mAssetBinder.targetTree != null)
                {
                    mContextNode = RaycastNode;
                    mContextPos = AIGraph.CalculateLocalPosition(mousePos);
                    sNodeMenu.Display(this, new Rect(mousePos, Vector2.zero));
                }
                else
                {
                    ClearSelection();
                }
                return true;
            }
            else if (btn == EMouseButton.right)
            {
                if (RaycastNode != null && !mSelection.Contains(RaycastNode))
                    ClearSelection();
                mContextNode = RaycastNode;
                mContextPos = AIGraph.CalculateLocalPosition(mousePos);
                if (Application.isPlaying)
                {
                    DisplayDebugMenu(new Rect(mousePos, Vector2.zero));
                    //EditorUtility.DisplayCustomMenu(new Rect(mousePos, Vector2.zero), mRuntimeMenu, -1, OnRuntimeMenuSelected, null);
                }
                else if (IsRequestParentOrChild)
                {
                    RequestChild(null);
                }
                else
                {
                    DisplayEditMenu(new Rect(mousePos, Vector2.zero));
                }
                return true;
            }
            return false;
        }

        void DisplayDebugMenu(Rect pos)
        {
            var node = mContextNode == null ? null : mContextNode.GetRuntimeNode();
            if (node == null)
                return;
            BehaviourTreeRunner.AssetBinder subtree = null;
            if (node != null && node.Asset != null && TargetRunner != null)
                subtree = TargetRunner.GetBinder((x) => x.Name == node.Asset.GetInstanceID().ToString("x"));
            if (subtree != null && subtree.RuntimeTree != null)
            {
                sDebugMenu.AddItem("打开子行为树", (menu, index, data) =>
                {
                    mAssetBinder.SetBehaviourBinder(data as BehaviourTreeRunner.AssetBinder);
                    mContextNode = null;
                }, subtree);
            }
            else
            {
                sDebugMenu.RemoveItem("打开子行为树");
            }
            sDebugMenu.Display(this, pos);
        }

        void DisplayEditMenu(Rect pos)
        {
            BTEditorMenu menu;
            if (mAssetBinder.targetTree == null)
                menu = sNewTreeMenu;
            else if (mContextNode == null || mContextNode.GetContext() == null)
                menu = sTreeMenu;
            else if (mContextNode.GetContext().isCondition)
                menu = sConditionMenu;
            else
                menu = sContextMenu;
            menu.Display(this, pos);
        }
        
        public void AddNewNode(AIModules.Module mod)
        {
            AddNewNode(mod, mContextNode == null ? null : mContextNode.GetNode(), mContextPos);
        }

        public void AddNewNode(AIModules.Module mod, BTNode context, Vector2 contextPos)
        {
            var todo = BTEditableDO.New<BTAddNode>(this);
            todo.context = context;
            todo.position = contextPos;
            todo.mod = mod;
            DoEdit(todo);
        }

        bool InteractGraphDragBegin(EMouseButton btn, Vector2 mousePos)
        {
            if (btn == EMouseButton.left)
            {
                mSelectionRect.Visible = true;
                mSelectionRect.LocalRect = new Rect(GraphCanvas.CalculateLocalPosition(mousePos), Vector2.zero);
                return true;
            }
            return false;
        }

        bool InteractGraphDrag(EMouseButton btn, Vector2 mousePos, Vector2 mouseDelta)
        {
            if (btn == EMouseButton.left)
            {
                var rect = mSelectionRect.LocalRect;
                rect.max = GraphCanvas.CalculateLocalPosition(mousePos);
                mSelectionRect.LocalRect = rect;
                return true;
            }
            return false;
        }

        bool InteractGraphDragEnd(EMouseButton btn, Vector2 mousePos)
        {
            if (btn == EMouseButton.left)
            {
                mSelectionRect.Visible = false;
                SetSelections((x) =>
                {
                    if (Event.current.shift)
                        return x.selected || mSelectionRect.GlobalRect.Overlaps(x.GlobalRect, true);
                    else if (Event.current.control)
                        return mSelectionRect.GlobalRect.Overlaps(x.GlobalRect, true) ? !x.selected : x.selected;
                    else
                        return mSelectionRect.GlobalRect.Overlaps(x.GlobalRect, true);
                });
                return true;
            }
            return false;
        }

        public void CopySelection()
        {
            if (mSelection.Count == 0 && RaycastNode != null)
                mSelection.Add(RaycastNode);
            if (mSelection.Count == 0)
                return;
            if (mCopyDo == null)
                mCopyDo = BTEditableDO.New<BTCopy>(this);
            mCopyDo.SetSelection(mSelection);
        }

        public void PasteSelection()
        {
            if (mCopyDo == null)
                return;
            var pos = mCopyDo.GetSelectionPositin();
            mCopyDo.deltaPosition = AIGraph.CalculateLocalPosition(GlobalMousePosition) - pos;
            DoEdit(mCopyDo);
        }

        public void DeleteSelections()
        {
            var del = BTEditableDO.New<BTDeleteNode>(this);
            del.SetSelection(SelectionNodes);
            mSelection.Clear();
            mSelectionAssets.Clear();
            DoEdit(del);
        }

        public void DeleteContext(BehaviourNode node)
        {
            if (node == null)
                return;
            var t = node.GetContext();
            if (t != null)
            {
                mAssetBinder.targetTree.EditorDeleteNode(t);
                if (t.Asset is BTNodeAsset)
                    AddDelayTask(ACT_UPDATE_WIRES, mWires.UpdateWires);
                if (node != RootNode && node.GetNode() == null)
                    AIGraph.RemoveElement(node);
                else
                    node.Resize();
            }
        }

        private bool InteractGraphKeyDown(KeyCode key)
        {
            if (key == KeyCode.Delete)
            {
                DeleteSelections();
                return true;
            }
            else if(key == KeyCode.C && Event.current.control)
            {
                CopySelection();
                return true;
            }
            else if(!Application.isPlaying && !Event.current.alt && !Event.current.control && !Event.current.shift 
                && mAssetBinder != null && mAssetBinder.targetTree != null)
            {
                mContextNode = RaycastNode;
                mContextPos = AIGraph.CalculateLocalPosition(GlobalMousePosition);
                sHotkeyMenu.Display(this, new Rect(Event.current.mousePosition, Vector2.zero), key);
            }
            return false;
        }

    }
}
