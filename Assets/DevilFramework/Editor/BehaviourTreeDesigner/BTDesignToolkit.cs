using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DevilTeam.AI;
using System.IO;
using System.Xml;
using DevilTeam.Utility;
using System;

namespace DevilTeam.Editor
{
    using NODE = BehaviourGraph.BehaviourNode;

    public delegate uint DrawCallback(BTNode node, Vector2 viewOffset);

    public class BTModule
    {
        public string id { get; private set; }
        public string name { get; private set; }
        public BehaviourModule module { get; private set; }
        public Rect bounds;

        public BTModule(string name, BehaviourModule module)
        {
            this.name = name;
            this.module = module;
            id = ModuleId(module, name);
        }

        public static string ModuleId(BehaviourModule module, string name)
        {
            return (module == null ? "?" : module.GetType().FullName) + "-" + name;
        }

        public static string ModuleName(string moduleId)
        {
            if (moduleId == null)
                return BTDesignToolkit.EMPTY;
            int n = moduleId.IndexOf('-');
            return n >= 0 ? moduleId.Substring(n + 1) : BTDesignToolkit.EMPTY;
        }
    }

    public class BTNode
    {
        public int Id { get; set; }
        public BTNode Parent { get; private set; }
        public List<BTNode> Children = new List<BTNode>();
        int nodeType;
        public bool editable;
        public int IndexInParent { get; private set; }
        public Vector2 size;
        public Rect bounds;
        public Rect position;
        public bool collapse;
        public string icon = "";
        public string name = "";
        public string comment = "";

        public int priority;
        public int intInput;
        public float floatInput;
        public string text = "";
        string _moduleId = "";
        public string moduleName { get; private set; }
        public string moduleId
        {
            get { return _moduleId; }
            set
            {
                if(_moduleId != value)
                {
                    _moduleId = value;
                    moduleName = BTModule.ModuleName(value);
                }
            }
        }

        public bool collapsedByParent;

        public static BTNode ReadXml(string path)
        {
            if (!File.Exists(path))
                return null;
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlElement ele = xml.FirstChild as XmlElement;
            if (ele == null)
                return null;
            return ReadXmlElement(ele);
        }


        public static void ExportToXML(BTNode node, string path)
        {
            XmlDocument xml = new XmlDocument();
            XmlElement ele = GetElement(xml, node);
            xml.AppendChild(ele);
            xml.Save(path);
        }

        static BTNode ReadXmlElement(XmlElement root)
        {
            BTNode node = new BTNode();
            node.name = root.GetAttribute("name");
            node.NodeType = int.Parse(root.GetAttribute("type"));
            node.comment = root.GetAttribute("comment");
            node.editable = bool.Parse(root.GetAttribute("editable"));
            node.size = StringUtil.ParseVector2(root.GetAttribute("size"));
            node.collapse = bool.Parse(root.GetAttribute("collapse"));
            node.priority = int.Parse(root.GetAttribute("priority"));
            node.intInput = int.Parse(root.GetAttribute("intInput"));
            node.floatInput = float.Parse(root.GetAttribute("floatInput"));
            node.moduleId = root.GetAttribute("module");
            node.text = root.GetAttribute("text");
            foreach (XmlNode xml in root.ChildNodes)
            {
                XmlElement ele = xml as XmlElement;
                if (ele != null)
                {
                    BTNode nd = ReadXmlElement(ele);
                    node.AddChild(nd);
                }
            }
            return node;
        }

        static XmlElement GetElement(XmlDocument xml, BTNode node)
        {
            XmlElement root = xml.CreateElement("BTNode");
            root.SetAttribute("name", node.name);
            root.SetAttribute("type", node.NodeType.ToString());
            root.SetAttribute("comment", node.comment);
            root.SetAttribute("editable", node.editable.ToString());
            root.SetAttribute("size", string.Format("({0},{1})", node.size.x, node.size.y));
            root.SetAttribute("collapse", node.collapse.ToString());
            root.SetAttribute("priority", node.priority.ToString());
            root.SetAttribute("intInput", node.intInput.ToString());
            root.SetAttribute("floatInput", node.floatInput.ToString());
            root.SetAttribute("module", node.moduleId);
            root.SetAttribute("text", node.text);
            for (int i = 0; i < node.Length; i++)
            {
                XmlElement ele = GetElement(xml, node[i]);
                root.AppendChild(ele);
            }
            return root;
        }

        public BTNode CloneThis()
        {
            BTNode copy = new BTNode();
            copy.name = name;
            copy.NodeType = NodeType;
            copy.comment = comment;
            copy.editable = editable;
            copy.collapse = collapse;
            copy.priority = priority;
            copy.intInput = intInput;
            copy.floatInput = floatInput;
            copy.text = text;
            copy.IndexInParent = IndexInParent;
            copy.moduleId = moduleId;
            for (int i = 0; i < Children.Count; i++)
            {
                BTNode child = Children[i].CloneThis();
                copy.AddChild(child);
            }
            return copy;
        }

        public BTNode()
        {
            nodeType = BTDesignToolkit.NODE_NULL;
            size = BTDesignToolkit.defaultNodeSize;
            string iconName = BTDesignToolkit.IconNameForType(this);
            if (File.Exists(iconName))
            {
                icon = iconName;
            }
            editable = true;
        }

        public int IndexOfChild(BTNode child)
        {
            return Children.IndexOf(child);
        }

        public int NodeType
        {
            get { return nodeType; }
            set
            {
                if (value != nodeType)
                {
                    nodeType = value;
                    string iconName = BTDesignToolkit.IconNameForType(this);
                    if (File.Exists(iconName))
                    {
                        icon = iconName;
                    }
                }
            }
        }

        public int MoreChildCount
        {
            get
            {
                switch ((EBTNode)nodeType)
                {
                    case EBTNode.condition:
                    case EBTNode.behaviour:
                        return 0;
                    case 0:
                        return Length == 0 ? 1 : 0;
                    case EBTNode.patrol:
                        return Length == 0 ? 1 : 0;
                    default:
                        return -1;
                }
            }
        }

        public bool IsCollectedByParent
        {
            get
            {
                BTNode child = this;
                BTNode p = child.Parent;
                while (p != null)
                {
                    if (p.collapse)
                    {
                        collapsedByParent = true;
                        return true;
                    }
                    child = p;
                    p = child.Parent;
                }
                collapsedByParent = false;
                return false;
            }
        }

        public void AddChild(BTNode node)
        {
            node.Parent = this;
            node.IndexInParent = Children.Count;
            Children.Add(node);
        }

        public bool MoveChildPosition(int index, int delta)
        {
            int newindex = Mathf.Clamp(index + delta, 0, Children.Count - 1);
            if (newindex != index)
            {
                int off = Mathf.Min(newindex, index);
                BTNode child = Children[index];
                Children.RemoveAt(index);
                Children.Insert(newindex, child);
                for (int i = off; i < Children.Count; i++)
                {
                    Children[i].IndexInParent = i;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool InsertNewChild(int index, BTNode node)
        {
            //BTNode node = new BTNode();
            Children.Insert(index, node);
            node.Parent = this;
            for (int i = index; i < Children.Count; i++)
            {
                Children[i].IndexInParent = i;
            }
            return true;
        }

        public BTNode InsertParent()
        {
            if (Parent == null)
                return null;
            BTNode node = new BTNode();
            node.Parent = Parent;
            node.IndexInParent = IndexInParent;
            Parent.Children[IndexInParent] = node;
            node.AddChild(this);
            return node;
        }

        public bool IsChildOf(BTNode node)
        {
            BTNode root = Parent;
            while (root != null)
            {
                if (root == node)
                    return true;
                root = root.Parent;
            }
            return false;
        }

        public void RemoveChild(BTNode node)
        {
            int index = Children.IndexOf(node);
            if (index >= 0)
            {
                node.Parent = null;
                Children.RemoveAt(index);
                if (node.NodeType == 0 && node.Length == 1)
                {
                    BTNode child = node[0];
                    Children.Insert(index, child);
                    child.Parent = this;
                    child.IndexInParent = index;
                }
                else
                {
                    for (int i = index; i < Children.Count; i++)
                    {
                        Children[i].IndexInParent = i;
                    }
                }
            }
        }

        public int Length { get { return Children.Count; } }

        public BTNode this[int index]
        {
            get { return Children[index]; }
        }

    }

    public class BTDesignToolkit
    {

        public const uint DIRTY_RELOAD = 1u << 0;
        public const uint DIRTY_SAVE_PREFAB = 1u << 1;
        public const uint DIRTY_SAVE_GRAPH = 1u << 2;
        public const uint DIRTY_ADD_OR_DELETE_NODE = 1u << 3;
        public const uint DIRTY_MODIFY_NODE = 1u << 4;
        public const uint DIRTY_RESIZE = 1u << 5;
        public const uint DIRTY_COLLAPSE_GRAPH = 1u << 6;

        public const uint DIRTY_MASK_FOR_RELOAD = DIRTY_RELOAD;
        public const uint DIRTY_MASK_APPLY_UPDATE = DIRTY_ADD_OR_DELETE_NODE | DIRTY_RESIZE;

        public const uint DIRTY_MASK_CLEAR_VIR_FRAME = DIRTY_RELOAD | DIRTY_SAVE_PREFAB | DIRTY_SAVE_GRAPH | DIRTY_ADD_OR_DELETE_NODE
            | DIRTY_RESIZE | DIRTY_MODIFY_NODE | DIRTY_COLLAPSE_GRAPH;

        public const uint CTRL_NODES = 0
            | (1u << (int)EBTNode.sequence)
            | (1u << (int)EBTNode.selector)
            | (1u << (int)EBTNode.queue)
            | (1u << (int)EBTNode.custom_control);

        public static Vector2 defaultNodeSize = new Vector2(75, 50);
        static Vector2 nodeSpace = new Vector2(100, 35);
        static Vector2 nodeFrameSize = new Vector2(8, 8);
        static Vector2 radioSize = new Vector2(18, 18);
        static Vector2 commentSize = new Vector2(270, 120);
        static Vector2 contextSize = new Vector2(310, 400);

        public const int NODE_NULL = 0;
        public const int NODE_SEQUENCE = 1;
        public const int NODE_SELECTOR = 2;
        public const int NODE_QUEUE = 3;
        public const int NODE_CUSTOM = 4;
        public const int NODE_BEHAVIOUR = 5;
        public const int NODE_CONDITION = 6;
        public const int NODE_PATROL = 7;
        public const int NODE_REPOSITORY = 8;
        public const int NODE_PREFAB = 9;
        static string[] nodeNames = { "空节点", "序列", "选择", "队列", "自定义", "行为", "条件", "巡逻", "AI 库", "预设" };
        static EBTNode[] nodeTypes = new EBTNode[8];

        public const string EMPTY = "";
        public const string ELE_NAME = "BTDesign";
        public const string ELE_COMMENT = "BTComment";

        public static string IconNameForType(BTNode node)
        {
            return Path.Combine(Installizer.InstallRoot, string.Format("DevilFramework/Editor/Icons/btnode-{0}.png", node.NodeType));
        }

        public static string IconNameForType(int type)
        {
            return Path.Combine(Installizer.InstallRoot, string.Format("DevilFramework/Editor/Icons/btnode-{0}.png", type));
        }

        public static void CalculateBTBounds(BTNode root)
        {
            CalculateBTSize(root);
            CalculateBTPosition(root);
        }

        static void CalculateBTSize(BTNode root)
        {
            Vector2 size;
            if (root.Length > 0 && !root.collapse)
            {
                size = Vector2.zero;
                for (int i = root.Length - 1; i >= 0; i--)
                {
                    BTNode node = root[i];
                    CalculateBTSize(node);
                    size.x = Mathf.Max(size.x, node.bounds.width);
                    size.y += node.bounds.height;
                }
                size.x += root.size.x + nodeSpace.x + defaultNodeSize.x;
                size.y = Mathf.Max(root.size.y, size.y + nodeSpace.y * (root.Length - 1));
            }
            else
            {
                size = root.size;
            }
            Rect bounds = root.bounds;
            bounds.size = size;
            root.bounds = bounds;
        }

        static void CalculateBTPosition(BTNode rootNode)
        {
            BTNode brother = null;
            if (rootNode.Parent != null && rootNode.IndexInParent > 0)
            {
                brother = rootNode.Parent[rootNode.IndexInParent - 1];
            }
            float left = rootNode.Parent == null ? 100 : rootNode.Parent.bounds.xMin + rootNode.Parent.size.x + nodeSpace.x;
            float top = rootNode.Parent == null ? -rootNode.bounds.height * 0.5f : rootNode.Parent.bounds.yMin;
            if (brother != null)
                top = brother.bounds.yMax + nodeSpace.y;
            Rect rect = rootNode.bounds;
            rect.position = new Vector2(left, top);
            rootNode.bounds = rect;
            rect.position = rootNode.bounds.position + new Vector2(0, (rootNode.bounds.height - rootNode.size.y) * 0.5f);
            rect.size = rootNode.size;
            rootNode.position = rect;
            if (!rootNode.collapse)
            {
                for (int i = 0; i < rootNode.Length; i++)
                {
                    BTNode node = rootNode[i];
                    CalculateBTPosition(node);
                }
            }
        }

        public static void DrawNodeIcon(BTNode node, Vector2 viewOffset, float scale, Vector2 align)
        {
            if (!string.IsNullOrEmpty(node.icon))
            {
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(node.icon);
                if (tex)
                {
                    Rect rect = new Rect();
                    float wdh = tex.width / (float)tex.height;
                    float h = node.size.y;
                    float w = h * wdh;
                    if (w > node.size.x - 4)
                    {
                        w = node.size.x - 4;
                        h = w / wdh;
                    }
                    rect.size = new Vector2(w, h) * scale;
                    rect.center = new Vector2(Mathf.Lerp(node.position.xMin, node.position.xMax, align.x),
                        Mathf.Lerp(node.position.yMin, node.position.yMax, align.y)) + viewOffset;
                    GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit);
                }
            }
        }

        public uint dirtyFlag { get; private set; }

        public BehaviourGraph graph { get; private set; }
        public BTNode editNode { get; private set; }
        public BTNode editCommentNode { get; private set; }
        public BTNode raycastNode { get; private set; }
        public BTNode activeNode { get { return editNode ?? raycastNode; } }
        int activeAI = 1; // 当前编辑 行为树

        bool raycast;
        List<BTNode> allNodes = new List<BTNode>();
        public BTNode rootNode { get; private set; }
        public BTNode prefabRoot { get; private set; }

        public int NodeCount { get { return allNodes.Count; } }
        public BTNode NodeAt(int index) { return allNodes[index]; }

        GUIStyle commentStyle;
        GUIStyle moduleStyle;
        GUIContent moduleContent;
        GUIContent commentContent;
        Vector2 nodeScroll;
        bool showContextMenu;
        bool showModules;
        float moduleSlider;
        Rect contextMenuRect;
        public bool useMouseEvent { get; private set; }

        int selectedPaste;
        List<BTNode> pasteNodes = new List<BTNode>();
        BTNode pastedNode;

        List<BTModule> conditions = new List<BTModule>();
        List<BTModule> behaviours = new List<BTModule>();
        List<BTModule> customModules = new List<BTModule>();
        Dictionary<string, BTModule> moduleMap = new Dictionary<string, BTModule>();

        public BTNode ClonePaste()
        {
            BTNode node = pasteNodes[selectedPaste];
            return node.CloneThis();
        }

        void AddToPaste(BTNode node)
        {
            if (node == null)
                return;
            pastedNode = node;
            while (pasteNodes.Count > 6)
                pasteNodes.RemoveAt(6);
            pasteNodes.Add(node.CloneThis());
        }

        public BTDesignToolkit()
        {
            commentStyle = new GUIStyle();
            commentStyle.alignment = TextAnchor.LowerLeft;
            commentStyle.richText = true;
            commentStyle.fontSize = 14;
            commentStyle.wordWrap = true;
            commentStyle.fixedWidth = 270;
            commentStyle.normal.textColor = Color.gray;
            commentContent = new GUIContent();

            moduleStyle = new GUIStyle();
            moduleStyle.alignment = TextAnchor.MiddleCenter;
            moduleStyle.richText = false;
            moduleStyle.fontSize = 15;
            moduleStyle.wordWrap = false;
            moduleStyle.fontStyle = FontStyle.Bold;
            moduleStyle.normal.textColor = Color.gray;
            moduleStyle.onHover.textColor = Color.yellow;
            //moduleStyle.onHover.background = AssetDatabase.LoadAssetAtPath<Texture2D>("");
            moduleContent = new GUIContent();

            BTNode root = new BTNode();
            root.name = "";
            root.comment = @"AI 资源库
这是一个特殊节点，在这个节点中定义了所有的行为树，它的所有子节点平行，即彼此独立。";
            root.NodeType = NODE_REPOSITORY;
            root.editable = false;
            root.size = new Vector2(120, 120);
            rootNode = root;

            BTNode prefab;
            prefab = new BTNode();
            prefab.name = "";
            prefab.comment = @"预设行为库
这是一个特殊节点，您可以在它下面创建一些常用的行为树作为预设，以后可以快速选择其中的内容，它的所有子节点平行，表示一个个的预设体。";
            prefab.NodeType = NODE_PREFAB;
            prefab.collapse = true;
            prefab.size = new Vector2(90, 90);
            prefab.editable = false;
            root.AddChild(prefab);
            prefabRoot = prefab;

            for (int i = NODE_SEQUENCE; i < NODE_REPOSITORY; i++)
            {
                if (i == NODE_CUSTOM)
                    continue;
                BTNode node = new BTNode();
                node.size = defaultNodeSize * 0.8f;
                node.NodeType = i;
                pasteNodes.Add(node);
            }
        }

        public void InitGraph()
        {
            if (Selection.activeGameObject)
            {
                graph = Selection.activeGameObject.GetComponent<BehaviourGraph>();
            }
            else
            {
                graph = null;
            }
            SelectNode(null);
            pastedNode = null;

            InitModules();
            rootNode.Children.Clear();
            rootNode.AddChild(prefabRoot);

            ReadBehaviourGraph();
            dirtyFlag = DIRTY_ADD_OR_DELETE_NODE;
            for (int i = 0; i < rootNode.Length; i++)
            {
                rootNode[i].collapse = i != 1;
            }
        }

        void InitModules()
        {
            moduleMap.Clear();
            behaviours.Clear();
            conditions.Clear();
            customModules.Clear();
            if (Selection.activeGameObject == null)
            {
                return;
            }
            BehaviourModule[] modules = Selection.activeGameObject.GetComponents<BehaviourModule>();
            HashSet<string> tmp = new HashSet<string>();
            Type[] argType = new Type[] { typeof(BTCustomData) };
            for (int i = 0; i < modules.Length; i++)
            {
                tmp.Clear();
                modules[i].__getBehaviourModules(tmp);
                foreach (var modName in tmp)
                {
                    BTModule mod = new BTModule(modName, modules[i]);
                    behaviours.Add(mod);
                    moduleMap[mod.id] = mod;
                }
                tmp.Clear();
                modules[i].__getConditionModules(tmp);
                foreach (var modName in tmp)
                {
                    BTModule mod = new BTModule(modName, modules[i]);
                    conditions.Add(mod);
                    moduleMap[mod.id] = mod;
                }
                tmp.Clear();
                modules[i].__getCustomModules(tmp);
                foreach (var modName in tmp)
                {
                    BTModule mod = new BTModule(modName, modules[i]);
                    customModules.Add(mod);
                    moduleMap[mod.id] = mod;
                }
            }
            Vector2 size = contextSize - Vector2.one * 30;
            CalculateModulesBounds(behaviours, size);
            CalculateModulesBounds(conditions, size);
            CalculateModulesBounds(customModules, size);
        }

        void CalculateModulesBounds(List<BTModule> modules, Vector2 boundSize)
        {
            Vector2 endPos = Vector2.zero;
            Vector2 size = Vector2.zero;
            BehaviourModule target = null;
            for (int i = 0; i < modules.Count; i++)
            {
                BTModule mod = modules[i];
                if(target != mod.module)
                {
                    if (endPos.x > 0)
                        endPos.y += size.y;
                    // 留给模块组标题空行
                    endPos.x = 0;
                    endPos.y += 35;
                    target = mod.module;
                }
                moduleContent.text = mod.name;
                size = moduleStyle.CalcSize(moduleContent);
                size.y = Math.Min(25, size.y);
                mod.bounds.size = size;
                if (endPos.x > 0 && endPos.x + size.x > boundSize.x)
                {
                    endPos.x = 0;
                    endPos.y += size.y + nodeFrameSize.y;
                }
                mod.bounds.position = endPos;
                endPos.x += size.x + nodeFrameSize.x;
                if (endPos.x > boundSize.x)
                {
                    endPos.x = 0;
                    endPos.y += size.y + nodeFrameSize.y;
                }
            }
        }

        public void LoadPrefab()
        {
            BTNode prefab;
            prefab = BTNode.ReadXml(Path.Combine(Installizer.InstallRoot, "DevilFramework/Editor/BehaviourTreeDesigner/BTRepository.xml"));
            if (prefab != null)
            {
                for (int i = 0; i < prefab.Length; i++)
                {
                    prefabRoot.AddChild(prefab[i]);
                }
            }
        }

        void ReadBehaviourGraph()
        {
            if (!graph)
                return;
            int[] roots = graph.__getGrapRoots();
            if (roots == null)
                return;
            for (int i = 0; i < roots.Length; i++)
            {
                NODE node = graph.GetNodeById(roots[i]);
                BTNode t = GetNode(node);
                if (t != null)
                {
                    rootNode.AddChild(t);
                }
            }
        }

        public void ApplyBehaviourGraph()
        {
            if (!Selection.activeGameObject)
                return;
            if (!graph)
            {
                graph = Selection.activeGameObject.AddComponent<BehaviourGraph>();
            }
            InitModules();
            List<BTNode> nodes = new List<BTNode>();
            for (int i = 0; i < allNodes.Count; i++)
            {
                BTNode node = allNodes[i];
                node.Id = i + 1;
                if (node.editable && !node.IsChildOf(prefabRoot))
                    nodes.Add(node);
            }
            NODE[] bnodes = new NODE[nodes.Count];
            for (int i = 0; i < bnodes.Length; i++)
            {
                bnodes[i] = CreateNode(nodes[i]);
            }
            int[] roots = new int[rootNode.Length - 1];
            for (int i = 1; i < rootNode.Length; i++)
            {
                roots[i - 1] = rootNode[i].Id;
            }
            graph.__setNodes(bnodes);
            graph.__setGraphRoots(roots);

            if (Application.isPlaying)
            {
                graph.__resetTree();
            }
        }

        NODE CreateNode(BTNode node)
        {
            NODE nd = new NODE();
            nd.__setId(node.Id);
            nd.__setDisplayName(node.name);
            nd.__setBehaviourType((EBTNode)node.NodeType);
            nd.__setParentId(node.Parent != null && node.Parent.editable ? node.Parent.Id : -1);
            BTModule mod;
            if (moduleMap.TryGetValue(node.moduleId, out mod))
            {
                nd.__setModule(mod.name);
                nd.__setTarget(mod.module);
            }
            else if(node.NodeType == NODE_BEHAVIOUR || node.NodeType == NODE_CONDITION|| node.NodeType == NODE_PATROL)
            {
                nd.__setModule(BTModule.ModuleName(node.moduleId));
                nd.__setTarget(null);
                string log = string.Format("找不到模块[{0}]的实现，请确保添加了模块组件\n\t[modId:{1}]", nd.ModuleName, node.moduleId);
                Debug.LogError(log);
            }
            nd.__setComment(node.comment);
            if (nd.UserData == null)
                nd.__setUserData(new BTCustomData());
            nd.UserData.m_Priority = node.priority;
            nd.UserData.m_IntData = node.intInput;
            nd.UserData.m_FloatData = node.floatInput;
            nd.UserData.m_StringData = node.text;
            int[] children = new int[node.Length];
            for (int i = 0; i < children.Length; i++)
            {
                children[i] = node[i].Id;
            }
            nd.__children = children;
            return nd;
        }

        BTNode GetNode(NODE origin)
        {
            if (origin == null)
                return null;
            BTNode node = new BTNode();
            node.Id = origin.Id;
            node.editable = true;
            node.name = origin.DisplayName;
            node.NodeType = (int)origin.BehaviourType;
            node.moduleId = BTModule.ModuleId(origin.Target, origin.ModuleName); // origin.ModuleName;
            node.comment = origin.Comment;
            node.priority = origin.UserData.m_Priority;
            node.intInput = origin.UserData.m_IntData;
            node.floatInput = origin.UserData.m_FloatData;
            node.text = origin.UserData.m_StringData;
            for (int i = 0; i < origin.ChildrenCount; i++)
            {
                int id = origin.GetChildId(i);
                BTNode child = GetNode(graph.GetNodeById(id));
                if (child != null)
                    node.AddChild(child);
            }
            return node;
        }

        public void DrawNodeEditor(Vector2 viewOffset)
        {
            if (editNode == null || !showContextMenu)
            {
                editNode = null;
                showContextMenu = false;
                showModules = false;
                return;
            }
            if (showModules)
                DrawModuleSelector(viewOffset);
            else
                DrawTypeSelector(viewOffset);
        }

        public void DrawModuleSelector(Vector2 viewOffset)
        {
            Rect rect = new Rect();
            Vector2 size = contextSize;
            Vector2 pos = new Vector2(editNode.position.xMax + nodeFrameSize.x, editNode.position.yMin + defaultNodeSize.y * 0.5f - 225) + viewOffset;
            rect.size = size;
            rect.position = pos;
            contextMenuRect = rect;

            Vector2 delta = Vector2.up * 30;
            GUI.Label(rect, EMPTY, "As TextArea");

            size = contextSize - Vector2.one * 10;
            rect.size = size;
            pos.x = editNode.position.xMax + nodeFrameSize.x + 5 + viewOffset.x;
            pos.y = editNode.position.yMin + defaultNodeSize.y * 0.5f - 220 + viewOffset.y;
            rect.position = pos;
            GUI.Label(rect, EMPTY, "LightmapEditorSelectedHighlight");
            pos.x += 285;
            pos.y += 10;
            size.x = 20; 
            size.y -= 20;
            rect.position = pos;
            rect.size = size;
            moduleSlider = GUI.VerticalSlider(rect, moduleSlider, 0, 1, "MiniSliderVertical", "MiniSliderVertical");

            List<BTModule> modules = editNode.NodeType == NODE_BEHAVIOUR || editNode.NodeType == NODE_PATROL ?
                behaviours :
                (editNode.NodeType == NODE_CONDITION ? conditions : customModules);
            Vector2 endPos = modules.Count > 0 ? modules[modules.Count - 1].bounds.max : Vector2.zero;
            float height = endPos.y;
            float scrollOffset = Mathf.Max(0, height - contextSize.y + 40) * moduleSlider;
            Vector2 offset = new Vector2(0, -scrollOffset);
                //new Vector2(editNode.position.xMax + nodeFrameSize.x,
                //editNode.position.yMin + defaultNodeSize.y * 0.5f - 225 - scrollOffset) + viewOffset;
            size = contextSize - Vector2.one * 40;
            rect.size = size;
            pos = new Vector2(editNode.position.xMax + nodeFrameSize.x + 15, editNode.position.yMin + defaultNodeSize.y * 0.5f - 200) + viewOffset;
            rect.position = pos;
            GUI.BeginClip(rect);
            BehaviourModule target = null;
            pos = Vector2.zero;
            size = Vector2.zero;
            for(int i = 0; i < modules.Count; i++)
            {
                BTModule mod = modules[i];
                if(mod.module != target)
                {
                    target = mod.module;
                    rect.size = new Vector2(contextSize.x - 40, 30);
                    rect.position = pos + offset;
                    GUI.Label(rect, mod.module.GetType().ToString(), "ChannelStripAttenuationBar");
                    pos.y += 30;
                }
                size = mod.bounds.size;
                size.x -= 10;
                rect.size = size;
                rect.position = mod.bounds.position + offset;
                bool sel = mod.id == editNode.moduleId || rect.Contains(Event.current.mousePosition);
                bool newsel = GUI.Toggle(rect, sel, mod.name, "SearchModeFilter");
                if (sel ^ newsel)
                {
                    if (editNode.moduleId != mod.id)
                        dirtyFlag |= DIRTY_MODIFY_NODE;
                    editNode.moduleId = mod.id;
                    showModules = false;
                }
                pos.y = mod.bounds.yMax;
            }
            GUI.EndClip();
        }

        public void DrawPastePanel(Rect clipRect, Vector2 viweOffset)
        {
            Rect rect = new Rect();
            Vector2 cell = new Vector2(10, 8) + defaultNodeSize;

            rect.size = new Vector2(cell.x, 7 * cell.y + 40);
            rect.position = new Vector2(clipRect.xMin, clipRect.yMin + 40);
            float ymax = rect.yMax - 5;

            //if (pastedNode != null && pastedNode.IsChildOf(rootNode))
            //{
            //    Vector2 pos = pastedNode.position.position + new Vector2(pastedNode.size.x * 0.5f, 0) + viweOffset;
            //    if (pos.x > rect.xMin + cell.x && pos.x < clipRect.xMax && pos.y < clipRect.yMax && pos.y > clipRect.yMin)
            //    {
            //        Vector2 p0 = new Vector2(clipRect.xMin + cell.x * 0.5f, ymax - 6.5f * cell.y - 15f);
            //        Handles.DrawBezier(p0, pos, new Vector3(pos.x, p0.y), new Vector3(pos.x, p0.y), Color.gray, null, 2);
            //    }
            //}

            //GUI.Label(rect, EMPTY, "flow node 0");
            GUI.Label(rect, "剪贴板", "window");
            for (int i = 0; i < pasteNodes.Count; i++)
            {
                BTNode node = pasteNodes[i];
                node.size = defaultNodeSize * 0.8f;
                rect.size = node.size;
                rect.center = new Vector2(clipRect.xMin + cell.x * 0.5f, ymax - i * cell.y - cell.y * 0.5f - 15 * (i / 6));
                node.position = rect;
                node.collapse = node.Length > 0;
                DrawNodeFrame(node, Vector2.zero, i == selectedPaste, false);
                DrawNodeIcon(node, Vector2.zero, 0.8f, new Vector2(0.5f, 0.5f));
                DrawNodeTitile(node, Vector2.zero);
                if (i > 5)
                    DrawName(node, Vector2.zero);
                if (rect.Contains(Event.current.mousePosition) && Event.current.clickCount > 0)
                {
                    Event.current.Use();
                    selectedPaste = i;
                }
            }

            rect.size = new Vector2(cell.x - 10, 10);
            rect.center = new Vector2(clipRect.xMin + cell.x * 0.5f, ymax - 6f * cell.y - 10);
            GUI.Label(rect, EMPTY, "horizontalslider");
        }

        void DrawTypeSelector(Vector2 viewOffset)
        {
            Rect rect = new Rect();
            int len = NODE_REPOSITORY - 1;
            rect.size = new Vector2(310, 400);
            rect.position = new Vector2(editNode.position.xMax + nodeFrameSize.x, editNode.position.yMin + defaultNodeSize.y * 0.5f - 225) + viewOffset;
            contextMenuRect = rect;

            Vector2 delta = Vector2.up * 30;
            GUI.Label(rect, EMPTY, "As TextArea");

            rect.size = new Vector2(145, 210);
            rect.position = new Vector2(editNode.position.xMax + nodeFrameSize.x + 5, editNode.position.yMin + defaultNodeSize.y * 0.5f - 215) + viewOffset;
            GUI.Label(rect, EMPTY, "LightmapEditorSelectedHighlight");
            rect.size = new Vector2(300, 155);
            rect.position = new Vector2(editNode.position.xMax + nodeFrameSize.x + 5, editNode.position.yMin + defaultNodeSize.y * 0.5f + 10) + viewOffset;
            GUI.Label(rect, EMPTY, "LightmapEditorSelectedHighlight");
            rect.size = new Vector2(140, 210);
            rect.position = new Vector2(editNode.position.xMax + nodeFrameSize.x + 165, editNode.position.yMin + defaultNodeSize.y * 0.5f - 215) + viewOffset;
            GUI.Label(rect, EMPTY, "LightmapEditorSelectedHighlight");

            rect.size = new Vector2(140, 30);
            rect.position = new Vector2(editNode.position.xMax + nodeFrameSize.x + 5, editNode.position.yMin + defaultNodeSize.y * 0.5f - 30) + viewOffset;
            for (int i = 1; i <= len; i++)
            {
                bool osel = i == editNode.NodeType || rect.Contains(Event.current.mousePosition);
                bool sel = GUI.Toggle(rect, osel, "\t" + nodeNames[i], "SearchModeFilter");
                if (osel ^ sel)
                {
                    editNode.NodeType = i;
                    showModules = i == NODE_PATROL || i == NODE_BEHAVIOUR || i == NODE_CONDITION || i == NODE_CUSTOM;
                }
                string icon = IconNameForType(i);
                DrawIcon(rect, icon, 25, new Vector2(0.05f, 0.3f));
                rect.position -= delta;
            }
            rect.size = new Vector2(120, 190);
            rect.position = new Vector2(editNode.position.xMax + nodeFrameSize.x + 175, editNode.position.yMin + defaultNodeSize.y * 0.5f - 205) + viewOffset;
            if (GUI.Button(rect, "复制", "LargeButton"))
            {
                AddToPaste(editNode);
                selectedPaste = pasteNodes.Count - 1;
            }
            rect.position = new Vector2(editNode.position.xMax + nodeFrameSize.x + 5, editNode.position.yMin + defaultNodeSize.y * 0.5f + 15) + viewOffset;
            GUI.Label(rect, "命名");

            rect.position += delta;
            GUI.Label(rect, "优先级");

            rect.position += delta;
            GUI.Label(rect, "整数");

            rect.position += delta;
            GUI.Label(rect, "浮点数");

            rect.position += delta;
            GUI.Label(rect, "文本");

            string txt;
            rect.size = new Vector2(240f, 30f);
            rect.position = new Vector2(editNode.position.xMax + nodeFrameSize.x + 60, editNode.position.yMin + defaultNodeSize.y * 0.5f + 15) + viewOffset;
            txt = GUI.TextField(rect, editNode.name ?? EMPTY, "OL Title");
            if (txt != editNode.name)
                dirtyFlag |= DIRTY_MODIFY_NODE;
            editNode.name = txt;

            rect.position += delta;
            txt = GUI.TextField(rect, editNode.priority.ToString(), "OL Title");
            if (txt != editNode.priority.ToString())
                dirtyFlag |= DIRTY_MODIFY_NODE;
            int.TryParse(txt, out editNode.priority);

            rect.position += delta;
            txt = GUI.TextField(rect, editNode.intInput.ToString(), "OL Title");
            if (txt != editNode.intInput.ToString())
                dirtyFlag |= DIRTY_MODIFY_NODE;
            int.TryParse(txt, out editNode.intInput);

            rect.position += Vector2.up * 30;
            txt = GUI.TextField(rect, editNode.floatInput.ToString(), "OL Title");
            if (txt != editNode.floatInput.ToString())
                dirtyFlag |= DIRTY_MODIFY_NODE;
            float.TryParse(txt, out editNode.floatInput);

            rect.position += Vector2.up * 30;
            txt = GUI.TextField(rect, editNode.text ?? EMPTY, "OL Title");
            if (txt != editNode.text)
                dirtyFlag |= DIRTY_MODIFY_NODE;
            editNode.text = txt;
        }

        public void DrawIcon(Rect rect, string iconName, float controlHeight, Vector2 align)
        {
            if (string.IsNullOrEmpty(iconName) || !File.Exists(iconName))
                return;
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconName);
            if (tex)
            {
                float h = tex.height;
                float wdh = tex.width / h;
                float w = wdh * controlHeight;
                Rect r = new Rect();
                r.size = new Vector2(w, controlHeight);
                r.center = new Vector2(Mathf.Lerp(w * 0.5f + rect.xMin, rect.xMax - w * 0.5f, align.x), Mathf.Lerp(rect.yMin + controlHeight * 0.5f, rect.yMax - controlHeight * .5f, align.y));
                GUI.DrawTexture(r, tex);
            }
        }

        void DrawAddChildTool(BTNode node, Vector2 viewOffset, bool prev, bool next)
        {
            int accept = node.MoreChildCount;
            if (accept == 0)
                return;
            Rect rect = new Rect();
            bool raycast = false;
            if (next)
            {
                rect.size = radioSize * 2;
                rect.center = node.position.max + viewOffset;
                raycast = rect.Contains(Event.current.mousePosition);
                if (raycast)
                {
                    rect.size = radioSize;
                    rect.center = node.position.max + viewOffset;
                    if (GUI.Button(rect, EMPTY, "WinBtnMaxActiveMac") && node.InsertNewChild(node.Length, ClonePaste()))
                    {
                        showContextMenu = false;
                        node.collapse = false;
                        if (node.Parent == rootNode)
                        {
                            activeAI = rootNode.IndexOfChild(node);
                            dirtyFlag |= DIRTY_COLLAPSE_GRAPH;
                        }
                        dirtyFlag |= DIRTY_ADD_OR_DELETE_NODE;
                    }
                }
            }
            if (prev)
            {
                rect.size = radioSize * 2;
                rect.center = new Vector2(node.position.xMax, node.position.yMin) + viewOffset;
                raycast = rect.Contains(Event.current.mousePosition);
                if (raycast)
                {
                    rect.size = radioSize;
                    rect.center = new Vector2(node.position.xMax, node.position.yMin) + viewOffset;
                    if (GUI.Button(rect, EMPTY, "WinBtnMaxActiveMac") && node.InsertNewChild(0, ClonePaste()))
                    {
                        showContextMenu = false;
                        node.collapse = false;
                        if (node.Parent == rootNode)
                        {
                            activeAI = rootNode.IndexOfChild(node);
                            dirtyFlag |= DIRTY_COLLAPSE_GRAPH;
                        }
                        dirtyFlag |= DIRTY_ADD_OR_DELETE_NODE;
                    }
                }
            }
        }

        void DrawUpDownTool(BTNode node, Vector2 viewOffset, bool up, bool down)
        {
            if (node.Parent == null || node.Parent.Length < 2)
                return;
            Rect rect = new Rect();
            bool raycast = false;
            if (up && node.IndexInParent > 0)
            {
                rect.size = radioSize * 2;
                rect.center = new Vector2(node.position.xMin, node.position.yMin) + viewOffset;
                raycast = rect.Contains(Event.current.mousePosition);
                if (raycast)
                {
                    rect.size = radioSize;
                    rect.center = new Vector2(node.position.xMin, node.position.yMin) + viewOffset;
                    if (GUI.Button(rect, EMPTY, "Grad Up Swatch") && node.Parent.MoveChildPosition(node.IndexInParent, -1))
                    {
                        dirtyFlag |= DIRTY_RESIZE;
                    }
                }
            }
            if (down && node.IndexInParent < node.Parent.Length - 1)
            {
                rect.size = radioSize * 2;
                rect.center = new Vector2(node.position.xMin, node.position.yMax) + viewOffset;
                raycast = rect.Contains(Event.current.mousePosition);
                if (raycast)
                {
                    rect.size = radioSize;
                    rect.center = new Vector2(node.position.xMin, node.position.yMax) + viewOffset;
                    if (GUI.Button(rect, EMPTY, "Grad Down Swatch") && node.Parent.MoveChildPosition(node.IndexInParent, 1))
                    {
                        dirtyFlag |= DIRTY_RESIZE;
                    }
                }
            }
        }

        void DrawNodeTool(BTNode node, Vector2 viewOffset)
        {
            Rect tmpRect = new Rect();
            tmpRect.size = radioSize;
            Vector2 tmpP0;
            tmpP0.y = node.position.yMin + defaultNodeSize.y * 0.5f + viewOffset.y;
            bool b;
            if (node.Parent != null && node.NodeType < NODE_REPOSITORY)
            {
                tmpP0.x = node.position.xMin + viewOffset.x - nodeFrameSize.x;
                tmpRect.center = tmpP0;
                b = GUI.Button(tmpRect, EMPTY, "WinBtnCloseMac");
                if (b)
                {
                    node.Parent.RemoveChild(node);
                    if (editNode == node || (editNode != null && editNode.IsChildOf(node)))
                    {
                        editNode = null;
                        showContextMenu = false;
                    }
                    dirtyFlag |= DIRTY_ADD_OR_DELETE_NODE;
                }
            }
            if (node.Length > 0 && node.NodeType != NODE_REPOSITORY)
            {
                tmpP0.x = node.position.xMax + viewOffset.x + nodeFrameSize.x;
                tmpRect.center = tmpP0;
                b = GUI.Toggle(tmpRect, node.collapse, EMPTY, "WinBtnInactiveMac");
                if (b ^ node.collapse)
                {
                    node.collapse = b;
                    if (!node.collapse && node.Parent == rootNode)
                    {
                        activeAI = rootNode.IndexOfChild(node);
                        dirtyFlag |= DIRTY_COLLAPSE_GRAPH;
                    }
                    dirtyFlag |= DIRTY_RESIZE;
                }
            }
        }
        void ValidateNodes()
        {
            allNodes.Clear();
            GetNodeRecursion(rootNode);
        }

        public static uint DrawNodeTitile(BTNode node, Vector2 viewOffset)
        {
            Rect rect = new Rect();
            Vector2 pos = node.position.position + viewOffset;
            rect.size = new Vector2(node.size.x, 25);
            rect.position = pos;
            GUI.Label(rect, nodeNames[node.NodeType], "ChannelStripAttenuationBar");
            return 0;
        }

        void DrawName(BTNode node, Vector2 viewOffset)
        {
            string s = string.IsNullOrEmpty(node.name) ? node.moduleName : node.name;
            if (!string.IsNullOrEmpty(s))
            {
                Rect rect = new Rect();
                rect.size = new Vector2(node.size.x, node.size.y - 25);
                rect.position = new Vector2(node.position.xMin, node.position.yMin + 26) + viewOffset;
                GUI.Label(rect, s, "TL Selection H2");
            }
        }

        void DrawNodeFrame(BTNode node, Vector2 viewOffset, bool select, bool edit)
        {
            Rect tmpRect = new Rect();
            // 选中框
            if (select)
            {
                tmpRect.size = node.size + nodeFrameSize + Vector2.one * 3;
                tmpRect.center = node.position.center + viewOffset;
                string style = "flow node 0 on";
                GUI.Label(tmpRect, EMPTY, style);
                if (node.collapse && node.Length > 0)
                {
                    tmpRect.size = node.size + nodeFrameSize + Vector2.right * 12 + Vector2.one * 3;
                    tmpRect.center = node.position.center + viewOffset + Vector2.one * 6;
                    GUI.Label(tmpRect, EMPTY, style);
                    tmpRect.size = node.size + nodeFrameSize + Vector2.right * 6 + Vector2.one * 3;
                    tmpRect.center = node.position.center + viewOffset + Vector2.one * 3;
                    GUI.Label(tmpRect, EMPTY, style);
                }
            }
            if (node.collapse && node.Length > 0)
            {
                tmpRect.size = node.size + nodeFrameSize + Vector2.right * 12;
                tmpRect.center = node.position.center + viewOffset + Vector2.one * 6;
                GUI.Label(tmpRect, EMPTY, "GroupBox");
                tmpRect.size = node.size + nodeFrameSize + Vector2.right * 6;
                tmpRect.center = node.position.center + viewOffset + Vector2.one * 3;
                GUI.Label(tmpRect, EMPTY, "GroupBox");
            }
            tmpRect.size = node.size + nodeFrameSize;
            tmpRect.center = node.position.center + viewOffset;
            GUI.Label(tmpRect, EMPTY, edit ? "TE NodeBoxSelected" : "GroupBox");
            if (tmpRect.Contains(Event.current.mousePosition))
            {
                raycastNode = node;
                if (Event.current.type == EventType.ContextClick && raycastNode.editable)
                {
                    Event.current.Use();
                    useMouseEvent = true;
                    showContextMenu = true;
                    editNode = raycastNode;
                }
                raycast = true;
            }
        }

        public void ConnectNode(BTNode from, BTNode to, Vector2 viewOffset, Color color, float width = 3)
        {
            bool l2r = from.position.x < to.position.x;
            BTNode n0 = l2r ? from : to;
            BTNode n1 = l2r ? to : from;
            Vector2 tmpP0, tmpP1, tmpP2;
            tmpP0.x = n0.position.xMax + viewOffset.x;
            tmpP0.y = n0.position.yMin + defaultNodeSize.y * 0.5f + viewOffset.y;
            tmpP1.x = n1.position.xMin + viewOffset.x;
            tmpP1.y = n1.position.yMin + defaultNodeSize.y * 0.5f + viewOffset.y;
            tmpP2.x = tmpP1.x - tmpP0.x;
            tmpP2.y = 0;

            Handles.DrawBezier(tmpP0, tmpP1, tmpP0 + tmpP2 * 0.7f, tmpP1 - tmpP2 * 0.7f, color, null, width);

            BTNode child = from.Parent == to ? from : (to.Parent == from ? to : null);
            if (child == null)
                return;
            if (child == prefabRoot)
                return;
            Rect rect = new Rect();
            tmpP2 = (tmpP0 + tmpP1) * 0.5f;
            rect.size = radioSize * 2f;
            rect.center = tmpP2;
            if (rect.Contains(Event.current.mousePosition))
            {
                rect.size = radioSize;
                rect.center = tmpP2;
                if (GUI.Button(rect, EMPTY, "WinBtnMaxActiveMac") && child.InsertParent() != null)
                {
                    dirtyFlag |= DIRTY_ADD_OR_DELETE_NODE;
                }
            }
        }

        void GetNodeRecursion(BTNode node)
        {
            if (node != null)
            {
                for (int i = 0; i < node.Length; i++)
                {
                    GetNodeRecursion(node[i]);
                }
                allNodes.Add(node);
            }
        }

        public void DrawComment(BTNode node, Vector2 viewOffset)
        {
            Rect tmpRect = new Rect();
            tmpRect.size = new Vector2(node.size.x, nodeSpace.y);
            tmpRect.position = new Vector2(node.position.xMin, node.position.yMin - nodeFrameSize.y - nodeSpace.y) + viewOffset;
            if (editCommentNode == node && !showContextMenu)
            {
                tmpRect.size = commentSize;
                tmpRect.position = new Vector2(node.position.xMin, node.position.yMin - nodeFrameSize.y - commentSize.y) + viewOffset;
                GUI.SetNextControlName(ELE_COMMENT);
                node.comment = GUI.TextArea(tmpRect, node.comment ?? EMPTY, 120);
            }
            else if (tmpRect.Contains(Event.current.mousePosition) && !showContextMenu)
            {
                commentContent.text = string.IsNullOrEmpty(node.comment) ? "<i>#编辑注释</i>" : node.comment;
                float h = commentStyle.CalcHeight(commentContent, commentSize.x);
                tmpRect.size = new Vector2(commentSize.x + 10, h + 10);
                tmpRect.position = new Vector2(node.position.xMin - 5, node.position.yMin - nodeFrameSize.y - h - 5) + viewOffset;
                GUI.Label(tmpRect, EMPTY, "CurveEditorBackground");
                tmpRect.size = new Vector2(commentSize.x, h);
                tmpRect.position = new Vector2(node.position.xMin, node.position.yMin - nodeFrameSize.y - h) + viewOffset;
                GUI.Label(tmpRect, commentContent, commentStyle);
                if (node.editable && raycastNode == null && Event.current.clickCount > 0 && Event.current.button == 0)
                {
                    Event.current.Use();
                    useMouseEvent = true;
                    editCommentNode = node;
                    GUI.FocusControl(ELE_COMMENT);
                }
            }
            else
            {
                bool toolong = node.comment.Length > 8;
                string txt = string.Format("<size=12>{0}{1}</size>", toolong ? node.comment.Substring(0, 7).TrimEnd() : node.comment, toolong ? "..." : EMPTY);
                commentContent.text = txt;
                GUI.Label(tmpRect, commentContent, commentStyle);
            }

        }

        public void PrepareUpdate()
        {
            useMouseEvent = false;
            uint flag;
            flag = DIRTY_COLLAPSE_GRAPH;
            if ((dirtyFlag & flag) != 0)
            {
                for (int i = 0; i < rootNode.Length; i++)
                {
                    BTNode node = rootNode[i];
                    if (!node.collapse && i != activeAI)
                    {
                        node.collapse = true;
                        dirtyFlag |= DIRTY_RESIZE;
                    }
                }
            }
            flag = DIRTY_RESIZE | DIRTY_ADD_OR_DELETE_NODE;
            if ((dirtyFlag & flag) != 0)
            {
                CalculateBTBounds(rootNode);
                if (editNode != null && editNode.IsCollectedByParent)
                {
                    editNode = null;
                }
            }
            flag = DIRTY_ADD_OR_DELETE_NODE;
            if ((dirtyFlag & flag) != 0)
            {
                ValidateNodes();
            }
            dirtyFlag &= ~DIRTY_MASK_CLEAR_VIR_FRAME;
            if (!raycast)
            {
                raycastNode = null;
            }
            raycast = false;
        }

        public void SelectControl()
        {
            if (!raycast)
            {
                raycastNode = null;
            }
            if (showContextMenu && Event.current != null && contextMenuRect.Contains(Event.current.mousePosition))
            {
                return;
            }
            showContextMenu = false;
            SelectNode(raycastNode);
        }

        void SelectNode(BTNode node)
        {
            BTNode oldEdit = editNode;
            if (node != null && node.editable)
                editNode = node;
            else
                editNode = null;
            editCommentNode = null;
        }

        public void OnPaint(BTNode node, ref Vector2 viewOffset)
        {
            DrawNodeFrame(node, viewOffset, raycastNode == node, editNode == node);
            if (node.NodeType >= NODE_REPOSITORY || node.NodeType == NODE_NULL)
                DrawNodeIcon(node, viewOffset, .8f, new Vector2(0.5f, 0.6f));
            DrawNodeTool(node, viewOffset);
            DrawName(node, viewOffset);
            DrawNodeTitile(node, viewOffset);
            if (node.NodeType < NODE_REPOSITORY && node.NodeType > NODE_NULL)
                DrawNodeIcon(node, viewOffset, 0.6f, new Vector2(0.1f, 0.2f));
            if (raycastNode == node && !showContextMenu)
            {
                DrawUpDownTool(node, viewOffset, node.Parent != rootNode, node.Parent != rootNode);
                DrawAddChildTool(node, viewOffset, node != rootNode, true);
            }

            DrawDebug(node, viewOffset);
        }

        void DrawDebug(BTNode node, Vector2 viewOffset)
        {
            if (Application.isPlaying && graph && node.NodeType < NODE_REPOSITORY)
            {
                NODE nd = graph.GetNodeById(node.Id);
                if (nd != null)
                {
                    Rect rect = new Rect(node.position.position + new Vector2(0, node.size.y) + viewOffset, new Vector2(50, 25));
                    GUI.Label(rect, nd.__ticks.ToString(), "WarningOverlay");
                }
            }
        }
    }
}
