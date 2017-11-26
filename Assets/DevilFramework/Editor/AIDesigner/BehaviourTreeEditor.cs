using UnityEngine;
using UnityEditor;
using DevilTeam.AI;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using DevilTeam.Utility;
using System;

namespace DevilTeam.Editor
{
    public class BehaviourTreeEditor : GraphEditorWindow
    {
        public const int MASK_NEW_NODE = 1;
        public const int MASK_MODIFY_PARENT = 2;
        static int indexPtr = 0;

        public class Node : IGraphNode, IXmlSerializable
        {
            public int index { get; private set; }
            public string comment;
            public string module;
            public EBTNode nodeType;
            public BTCustomData data;
            public Vector2 Position { get; set; }
            public Vector2 size;
            public string errorMsg;

            Rect bounds;

            public void CalculateBounds(Vector2 viewOffset)
            {
                switch (nodeType)
                {
                    case EBTNode.behaviour:
                    case EBTNode.condition:
                    case EBTNode.custom_control:
                    case EBTNode.patrol:
                        nodeTitleContent.text = BTModule.ModuleName(module ?? "");
                        size = nodeTitleStyle.CalcSize(nodeTitleContent) + new Vector2(50, 20);
                        break;
                    case EBTNode.selector:
                    case EBTNode.sequence:
                    case EBTNode.queue:
                        size = new Vector2(40, 40);
                        break;
                    default:
                        size = Vector2.one * 20;
                        break;
                }
                bounds.size = size;
                bounds.position = new Vector2(Position.x + viewOffset.x - size.x * 0.5f, Position.y + viewOffset.y);
            }

            public Rect ClipRect { get { return bounds; } }

            public Node(int index)
            {
                this.index = index;
                indexPtr = Math.Max(index + 1, indexPtr);
                data = new BTCustomData();
                Position = Vector2.zero;
                size = new Vector2(100, 50);
            }

            public void CheckError(BehaviourTreeEditor editor)
            {
                if (editor.graph.GetParentCount(0, this) == 0)
                {
                    errorMsg = "不可到达的节点";
                    return;
                }
                errorMsg = null;
                BTModule mod;
                switch (nodeType)
                {
                    case EBTNode.behaviour:
                    case EBTNode.condition:
                        if (editor.graph.GetChildCount(0, this) > 0)
                        {
                            errorMsg = string.Format("{0} 不能包含任何子节点", nodeType);
                        }
                        else if (!editor.moduleMap.TryGetValue(module, out mod) || !mod.module)
                        {
                            errorMsg = string.Format("{0}\n模块不可用", module);
                        }
                        break;
                    case EBTNode.patrol:
                        if (editor.graph.GetChildCount(0, this) > 1)
                        {
                            errorMsg = string.Format("{0} 不能超过 1 个的子节点", nodeType);
                        }
                        else if (!editor.moduleMap.TryGetValue(module, out mod) || !mod.module)
                        {
                            errorMsg = string.Format("{0}\n模块不可用", module);
                        }
                        break;
                    case EBTNode.custom_control:
                        if (!editor.moduleMap.TryGetValue(module ?? "", out mod) || !mod.module)
                        {
                            errorMsg = string.Format("{0}\n模块不可用", module);
                        }
                        break;
                    case EBTNode.selector:
                    case EBTNode.sequence:
                    case EBTNode.queue:
                        if (editor.graph.GetChildCount(0, this) == 0)
                        {
                            errorMsg = string.Format("{0} 至少需要 1 个的子节点", nodeType);
                        }
                        break;
                    default:
                        errorMsg = string.Format("不可取的节点类型({0})", (int)nodeType);
                        break;
                }
            }

            public void CopyTo(Graph<IGraphNode> graph, IGraphNode parent, Vector2 position)
            {
                Node node = new Node(++indexPtr);
                node.Position = position;
                node.comment = comment;
                node.module = module;
                node.nodeType = nodeType;
                node.size = size;
                node.errorMsg = errorMsg;
                node.data = new BTCustomData();
                node.data.m_Priority = data.m_Priority;
                node.data.m_IntData = data.m_IntData;
                node.data.m_FloatData = data.m_FloatData;
                node.data.m_StringData = data.m_StringData;
                node.data.m_SortType = data.m_SortType;
                node.data.m_InputData = data.m_InputData;
                graph.AddNode(node);
                HashSet<IGraphNode> nodes = new HashSet<IGraphNode>();
                graph.GetAllChildren(0, this, nodes);
                foreach (IGraphNode tmp in nodes)
                {
                    Node nd = tmp as Node;
                    if (nd == null)
                        continue;
                    nd.CopyTo(graph, node, nd.Position - Position + position);
                }
                graph.AddPath(0, parent, node);
            }

            public void OnNodeGUI(GraphEditorWindow window, bool selected)
            {
                Rect rect = bounds;
                Texture2D icon;

                switch (nodeType)
                {
                    case EBTNode.behaviour:
                    case EBTNode.condition:
                    case EBTNode.patrol:
                    case EBTNode.custom_control:
                        if (nodeIcons.TryGetValue(nodeType, out icon))
                        {
                            rect.size = new Vector2(34, 34);
                            rect.center = new Vector2(bounds.xMin + 20, bounds.center.y);
                            GUI.DrawTexture(rect, icon);
                        }
                        rect.size = new Vector2(size.x - size.y, size.y);
                        rect.center = new Vector2(bounds.xMax - (size.x - 40) * 0.5f, bounds.center.y);
                        GUI.Label(rect, BTModule.ModuleName(module ?? ""), nodeTitleStyle);
                        break;
                    case EBTNode.sequence:
                    case EBTNode.selector:
                    case EBTNode.queue:
                        if (nodeIcons.TryGetValue(nodeType, out icon))
                        {
                            rect.size = new Vector2(size.x - 4, size.y - 4);
                            rect.center = new Vector2(bounds.xMin + 20, bounds.center.y);
                            GUI.DrawTexture(rect, icon);
                        }
                        else
                        {
                            GUI.Label(bounds, nodeType.ToString(), nodeTitleStyle);
                        }
                        break;
                    default:
                        break;
                }
                if (IsCtrlNode(nodeType))
                {
                    switch (data.m_SortType)
                    {
                        case ESortType.asc:
                            GUI.Label(bounds, "\u25B2", nodeTitleStyle);
                            break;
                        case ESortType.desc:
                            GUI.Label(bounds, "\u25BC", nodeTitleStyle);
                            break;
                        case ESortType.random:
                            GUI.Label(bounds, "\u25C4\u25BA", nodeTitleStyle);
                            break;
                        default:
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    rect.position = new Vector2(bounds.xMin, bounds.yMax);
                    rect.size = new Vector2(300, 40);
                    GUI.Label(rect, string.Format("<color=red>{0}</color>", errorMsg));
                }
            }

            public XmlElement Serialize(XmlDocument doc)
            {
                XmlElement ele = doc.CreateElement("Behaviour");
                ele.SetAttribute("id", index.ToString());
                ele.SetAttribute("comment", comment ?? "");
                ele.SetAttribute("module", module ?? "");
                ele.SetAttribute("nodeType", ((int)nodeType).ToString());
                //ele.SetAttribute("position", string.Format("({0},{1})", Position.x, Position.y));
                ele.SetAttribute("priority", data.m_Priority.ToString());
                ele.SetAttribute("sort", ((int)data.m_SortType).ToString());
                ele.SetAttribute("int", data.m_IntData.ToString());
                ele.SetAttribute("float", data.m_FloatData.ToString());
                ele.SetAttribute("string", data.m_StringData ?? "");
                return ele;
            }

            public void Deserialize(XmlElement element)
            {
                if (element.Name != "Behaviour")
                    return;
                index = int.Parse(element.GetAttribute("id"));
                comment = element.GetAttribute("comment");
                module = element.GetAttribute("module");
                nodeType = (EBTNode)int.Parse(element.GetAttribute("nodeType"));
                //Position = StringUtil.ParseVector2(element.GetAttribute("position"));
                data.m_Priority = int.Parse(element.GetAttribute("priority"));
                data.m_SortType = (ESortType)int.Parse(element.GetAttribute("sort"));
                data.m_IntData = int.Parse(element.GetAttribute("int"));
                data.m_FloatData = float.Parse(element.GetAttribute("float"));
                data.m_StringData = element.GetAttribute("string");
            }

            public BehaviourGraph.BehaviourNode CreateNode(BehaviourTreeEditor editor)
            {
                BehaviourGraph.BehaviourNode node = new BehaviourGraph.BehaviourNode();
                node.__setId(index);
                BTModule mod;
                node.__setBehaviourType(nodeType);
                if (editor.moduleMap.TryGetValue(module ?? "", out mod))
                {
                    node.__setModule(mod.name);
                    node.__setTarget(mod.module);
                }
                node.__setUserData(data);
                return node;
            }
        }

        public class InputNode : IGraphNode, IXmlSerializable
        {
            Rect bounds;

            public string priority = "0";
            public string intData = "0";
            public string floatData = "0";
            public string strData = "";
            public Vector2 Position { get; set; }

            public Rect ClipRect { get { return bounds; } }

            public InputNode()
            {
            }

            public InputNode(Node nd)
            {
                if (nd != null)
                {
                    priority = nd.data.m_Priority.ToString();
                    intData = nd.data.m_IntData.ToString();
                    floatData = nd.data.m_FloatData.ToString();
                    strData = nd.data.m_StringData;
                }
            }

            public void CalculateBounds(Vector2 viewOffset)
            {
                bounds.size = new Vector2(150, 100);
                bounds.position = new Vector2(Position.x - 75 + viewOffset.x, Position.y + viewOffset.y);
            }

            public void OnNodeGUI(GraphEditorWindow window, bool selected)
            {
                GUI.Label(bounds, "INPUT", "window");

                Rect rect = new Rect();

                rect.size = new Vector2(40, 25);
                rect.position = new Vector2(bounds.xMin + 5, bounds.yMin + 17);
                GUI.Label(rect, "优先级");

                rect.position += Vector2.up * 20;
                GUI.Label(rect, "int");

                rect.position += Vector2.up * 20;
                GUI.Label(rect, "float");

                rect.position += Vector2.up * 20;
                GUI.Label(rect, "string");

                rect.size = new Vector2(bounds.width - 50, 20);
                rect.position = new Vector2(bounds.xMin + 45, bounds.yMin + 17);
                if (window.IsContextMenuShown || window.LinkNodeStart != null)
                    GUI.Label(rect, priority ?? "0", "textfield");
                else
                    priority = GUI.TextField(rect, priority ?? "0");

                rect.position += Vector2.up * 20;
                if (window.IsContextMenuShown || window.LinkNodeStart != null)
                    GUI.Label(rect, intData ?? "0", "textfield");
                else
                    intData = GUI.TextField(rect, intData ?? "0");

                rect.position += Vector2.up * 20;
                if (window.IsContextMenuShown || window.LinkNodeStart != null)
                    GUI.Label(rect, floatData ?? "0", "textfield");
                else
                    floatData = GUI.TextField(rect, floatData ?? "0");

                rect.position += Vector2.up * 20;
                if (window.IsContextMenuShown || window.LinkNodeStart != null)
                    GUI.Label(rect, strData ?? "", "textfield");
                else
                    strData = GUI.TextField(rect, strData ?? "");

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("优先级", GUILayout.Width(40));
                //string pri = GUILayout.TextField(priority);
                //priority = pri;
                //GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("int", GUILayout.Width(40));
                //pri = GUILayout.TextField(intData);
                //intData = pri;
                //GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("float", GUILayout.Width(40));
                //pri = GUILayout.TextField(floatData);
                //floatData = pri;
                //GUILayout.EndHorizontal();

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("text", GUILayout.Width(40));
                //strData = GUILayout.TextField(strData);
                //GUILayout.EndHorizontal();
                //GUILayout.EndArea();
            }

            public XmlElement Serialize(XmlDocument doc)
            {
                XmlElement ele = doc.CreateElement("InputData");
                //ele.SetAttribute("position", string.Format("({0},{1})", Position.x, Position.y));
                ele.SetAttribute("priority", priority);
                ele.SetAttribute("int", intData);
                ele.SetAttribute("float", floatData);
                ele.SetAttribute("string", strData);
                return ele;
            }

            public void Deserialize(XmlElement element)
            {
                if (element.Name != "InputData")
                    return;
                //Position = StringUtil.ParseVector2(element.GetAttribute("position"));
                priority = element.GetAttribute("priority") ?? "0";
                intData = element.GetAttribute("int") ?? "0";
                floatData = element.GetAttribute("float") ?? "0";
                strData = element.GetAttribute("string") ?? "";
            }
        }

        List<BTModule> conditions = new List<BTModule>();
        List<BTModule> behaviours = new List<BTModule>();
        List<BTModule> customModules = new List<BTModule>();
        Dictionary<string, BTModule> moduleMap = new Dictionary<string, BTModule>();
        BehaviourGraph btgraph;
        bool lockSelected;
        string uid;
        Vector2 newNodeScroll;
        Node copyTarget;
        List<IGraphNode> childrens = new List<IGraphNode>();
        string viewportTitle = @"<size=30><b>BEHAVIOUR DESIGN</b></size><size=23> (ver.1.1.0)</size>";

        readonly int ctrlNodeEnd = 3;


        static EBTNode[] nodeList = { EBTNode.sequence, EBTNode.selector, EBTNode.queue, EBTNode.custom_control, EBTNode.condition, EBTNode.behaviour, EBTNode.patrol };
        static string[] nodeListName = { "序列", "选择", "队列", "自定义", "条件", "行为", "巡逻" };
        static Dictionary<EBTNode, Texture2D> nodeIcons = new Dictionary<EBTNode, Texture2D>();
        static GUIStyle nodeTitleStyle = new GUIStyle();
        static GUIContent nodeTitleContent = new GUIContent();
        const uint ctrlNodes = 0
            | (1 << (int)EBTNode.selector)
            | (1 << (int)EBTNode.sequence)
            | (1 << (int)EBTNode.queue)
            | (1 << (int)EBTNode.custom_control);
        static bool IsCtrlNode(EBTNode nodeType)
        {
            return ((1 << (int)nodeType) & ctrlNodes) != 0;
        }
        Node newNode = null;
        int newNodeIndex;

        bool hasError = true;
        bool checkError;
        [MenuItem("Devil Framework/Behaviour Editor v1.1")]
        public static void OpenBTDesigner()
        {
            BehaviourTreeEditor window = GetWindow<BehaviourTreeEditor>("Behaviour Design");
            window.minSize = new Vector2(600, 480);
            window.Show();
        }

        protected override Color GetLinkColor(IGraphNode from, IGraphNode to, int graphLayer)
        {
            if (Application.isPlaying && btgraph && graphLayer == 0)
            {
                Node nd = to as Node;
                if (nd != null)
                {
                    BehaviourGraph.BehaviourNode bnd = btgraph.GetNodeById(nd.index);
                    if (bnd != null)
                    {
                        Color c2 = bnd.__resultStat == EBTState.failed ? Color.red : (bnd.__resultStat == EBTState.running) ? Color.blue : Color.green;
                        return Color.Lerp(c2, Color.white, Mathf.Clamp01(bnd.__deltaTime * 0.5f));
                    }
                }
            }
            return graphLayer == 0 ? Color.white : Color.yellow;
        }

        protected override float GetLinkWidth(IGraphNode from, IGraphNode to, int graphLayer)
        {
            if (Application.isPlaying && btgraph && graphLayer == 0)
            {
                Node nd = to as Node;
                if (nd != null)
                {
                    BehaviourGraph.BehaviourNode bnd = btgraph.GetNodeById(nd.index);
                    if (bnd != null)
                    {
                        return Mathf.Lerp(10, 3, Mathf.Clamp01(bnd.__deltaTime * 0.5f));
                    }
                }
            }
            return graphLayer == 0 ? 3 : 2;
        }

        protected override void InitParameters()
        {
            mGraphLayers = 2;
            mViewportTitle = @"<size=30><b>BEHAVIOUR DESIGN</b></size><size=23> (ver.1.1.0)</size>
<size=18>  NOTE: 
  按住【滚轮】平移视图
  按住【左键】拖动节点或者框选节点
  点击【右键】取消当前操作
  按住【SHIFT】多选
  按住【ALT】选择所有子节点
  点击【CTRL+F】居中视图</size>";
            nodeIcons.Clear();
            for (int i = 0; i < nodeList.Length; i++)
            {
                string iconName = BTDesignToolkit.IconNameForType((int)nodeList[i]);
                if (File.Exists(iconName))
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconName);
                    if (tex)
                        nodeIcons[nodeList[i]] = tex;
                }
            }
            nodeTitleStyle.alignment = TextAnchor.MiddleCenter;
            nodeTitleStyle.richText = false;
            nodeTitleStyle.fontSize = 13;
            nodeTitleStyle.wordWrap = false;
            nodeTitleStyle.fontStyle = FontStyle.Bold;
            nodeTitleStyle.normal.textColor = Color.green;
            nodeTitleStyle.onHover.textColor = Color.yellow;
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
        }

        List<BTModule> GetModules(EBTNode node)
        {
            switch (node)
            {
                case EBTNode.custom_control:
                    return customModules;
                case EBTNode.condition:
                    return conditions;
                case EBTNode.behaviour:
                case EBTNode.patrol:
                    return behaviours;
                default:
                    return null;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Selection.selectionChanged += OnReloadGraph;

#if UNITY_2017_2
            EditorApplication.playModeStateChanged += OnSelectionChanged;
#else
            EditorApplication.playmodeStateChanged += OnSelectionChanged;
#endif
            ValidateGraph();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnReloadGraph;

#if UNITY_2017_2
            EditorApplication.playModeStateChanged -= OnSelectionChanged;
#else
            EditorApplication.playmodeStateChanged -= OnSelectionChanged;
#endif
        }

        protected override XmlElement SerializeNode(XmlDocument doc, IGraphNode node)
        {
            IXmlSerializable ser = node as IXmlSerializable;
            return ser == null ? null : ser.Serialize(doc);
        }

        protected override IGraphNode DeserializeNode(XmlElement element)
        {
            if (element.Name == "Behaviour")
            {
                Node nd = new Node(int.Parse(element.GetAttribute("id")));
                nd.Deserialize(element);
                return nd;
            }
            else if (element.Name == "InputData")
            {
                InputNode nd = new InputNode();
                nd.Deserialize(element);
                return nd;
            }
            else
            {
                return null;
            }
        }

        private void OnReloadGraph()
        {
            if(!lockSelected || btgraph == null)
                ValidateGraph();
        }

        private void ValidateGraph(GameObject oldObj = null)
        {
            if (oldObj == Selection.activeGameObject && oldObj != null)
                return;
            if (Selection.activeGameObject)
            {
                btgraph = Selection.activeGameObject.GetComponent<BehaviourGraph>();
                uid = btgraph == null ? "" : ((string)Ref.GetField(btgraph, "m_Uid")).Trim();
                if (string.IsNullOrEmpty(uid))
                {
                    uid = string.Format("g_{0}", Selection.activeGameObject.GetInstanceID().ToString("x"));
                    if (btgraph)
                    {
                        Ref.SetField(btgraph, "m_Uid", uid);
                        EditorUtility.SetDirty(btgraph);
                    }
                }
            }
            else
            {
                btgraph = null;
                uid = "nil";
            }
            indexPtr = 0;
            InitModules();
            if (!ImportFromXml() && !ImportFromBTGraph())
                ResetGraph();
            checkError = true;
        }

        bool ImportFromXml()
        {
            string file = Path.Combine(Installizer.InstallRoot, "DevilFramework/Editor/AIDesigner/BehaviourTree/" + uid + ".xml");
            if (ImportFromXml(file))
            {
                checkError = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        bool ImportFromBTGraph()
        {
            if (!btgraph || btgraph.__getGrapRoots() == null || btgraph.__getGrapRoots().Length == 0)
                return false;
            ResetGraph();
            InitNode(btgraph, null, btgraph.__getGrapRoots()[0]);
            ScatteNodes();
            return true;
        }

        void InitNode(BehaviourGraph btg, Node parent, int nodeId)
        {
            BehaviourGraph.BehaviourNode node = btg.GetNodeById(nodeId);
            if(node != null)
            {
                Node nd = new Node(nodeId);
                nd.nodeType = node.BehaviourType;
                nd.module = BTModule.ModuleId(node.Target, node.ModuleName);
                nd.comment = node.Comment;
                nd.data = node.UserData;
                graph.AddNode(nd);
                graph.AddPath(0, parent == null ? graph.Root : parent, nd);
                if(!string.IsNullOrEmpty(nd.data.m_StringData) || nd.data.m_Priority != 0 || nd.data.m_IntData != 0 || nd.data.m_FloatData != 0)
                {
                    InputNode input = new InputNode(nd);
                    graph.AddNode(input);
                    graph.AddPath(1, nd, input);
                }
                for(int i = 0; i < node.ChildrenCount; i++)
                {
                    InitNode(btg, nd, node.GetChildId(i));
                }
            }
        }

        void ScatteNodes()
        {
            for(int i = 0; i < graph.NodeLength; i++)
            {
                graph[i].CalculateBounds(Vector2.zero);
            }
            List<IGraphNode> root = new List<IGraphNode>();
            root.Add(graph.Root);
            ScatteChildren(root, graph.Root.Position + Vector2.up * (graph.Root.ClipRect.height + 30));
        }

        void ScatteChildren(List<IGraphNode> roots, Vector2 center)
        {
            List<IGraphNode> nodes = new List<IGraphNode>();
            for (int i = 0; i < roots.Count; i++)
            {
                graph.GetAllChildren(0, roots[i], nodes);
                graph.GetAllChildren(1, roots[i], nodes);
            }
            Vector2 size = Vector2.zero;
            for(int i = 0; i < nodes.Count; i++)
            {
                if (i > 0)
                    size.x += 20;
                size.x += nodes[i].ClipRect.width;
                size.y = Math.Max(size.y, nodes[i].ClipRect.height);
            }
            Vector2 pos = new Vector2();
            pos.x = -size.x * 0.5f + center.x;
            pos.y = center.y;
            for(int i = 0; i < nodes.Count; i++)
            {
                pos.x += nodes[i].ClipRect.width * 0.5f;
                nodes[i].Position = pos;
                pos.x += nodes[i].ClipRect.width * 0.5f;
                if (i > 0)
                    pos.x += 20;
                center.y = Math.Max(center.y, nodes[i].ClipRect.height + 30 + nodes[i].Position.y);
            }
            if (nodes.Count > 0)
            {
                center.x = nodes[nodes.Count >> 1].Position.x;
                ScatteChildren(nodes, center);
            }
        }

        // 保存到xml
        void ExportToXml()
        {
            string dic = Path.Combine(Installizer.InstallRoot, "DevilFramework/Editor/AIDesigner/BehaviourTree");
            if (!Directory.Exists(dic))
                Directory.CreateDirectory(dic);
            ExportToXml(dic, uid + ".xml");
        }

        void ExportToBTGraph(BehaviourGraph bgraph)
        {
            if (hasError)
            {
                EditorUtility.DisplayDialog("Error", "请先修改正配置错误", "好的");
                return;
            }
            List<IGraphNode> tmp = new List<IGraphNode>();
            List<BehaviourGraph.BehaviourNode> nodes = new List<BehaviourGraph.BehaviourNode>();
            int vari;
            float varf;
            for(int i = 0; i < graph.NodeLength; i++)
            {
                Node gnode = graph[i] as Node;
                if (gnode == null)
                    continue;
                tmp.Clear();
                graph.GetAllChildren(1, i, tmp);
                foreach(IGraphNode tnd in tmp)
                {
                    InputNode input = tnd as InputNode;
                    if(input != null)
                    {
                        if (int.TryParse(input.priority, out vari))
                            gnode.data.m_Priority = vari;
                        if (int.TryParse(input.intData, out vari))
                            gnode.data.m_IntData = vari;
                        if (float.TryParse(input.floatData, out varf))
                            gnode.data.m_FloatData = varf;
                        gnode.data.m_StringData = input.strData ?? "";
                    }
                }
                BehaviourGraph.BehaviourNode bnode = gnode.CreateNode(this);
                nodes.Add(bnode);
                tmp.Clear();
                graph.GetAllParent(0, i, tmp);
                if (tmp != null && tmp.Count == 1)
                {
                    Node nd = tmp[0] as Node;
                    bnode.__setParentId(nd == null ? 0 : nd.index);
                }
                tmp.Clear();
                graph.GetAllChildren(0, i, tmp);
                GlobalUtil.Sort(tmp, (x, y) => x.Position.x <= y.Position.x ? -1 : 1);
                int[] children = new int[tmp.Count];
                for(int c = 0; c < children.Length; c++)
                {
                    Node nd = tmp[c] as Node;
                    children[c] = nd.index;
                }
                bnode.__children = children;
            }
            nodes.Sort((x, y) => x.Id - y.Id);
            bgraph.__setNodes(nodes.ToArray());
            tmp.Clear();
            graph.GetAllChildren(0, graph.RootIndex, tmp);
            List<int> roots = new List<int>();
            for(int i = 0; i < tmp.Count; i++)
            {
                Node nd = tmp[i] as Node;
                if (nd != null)
                    roots.Add(nd.index);
            }
            bgraph.__setGraphRoots(roots.ToArray());
            EditorUtility.SetDirty(bgraph);
        }

#if UNITY_2017_2
        private void OnSelectionChanged(PlayModeStateChange state)
        {
            ValidateGraph();
            Repaint();
        }
#else
        private void OnSelectionChanged()
        {
            ValidateGraph();
            Repaint();
        }
#endif

        void ValidateUID(string newid)
        {
            uid = (newid ?? "").Trim();
            if (!string.IsNullOrEmpty(uid) && btgraph)
            {
                Ref.SetField(btgraph, "m_Uid", uid);
                EditorUtility.SetDirty(btgraph);
            }
        }

        protected override void OnTitleGUI()
        {
            GUILayout.Label("UID:", GUILayout.Width(30));
            string id = GUILayout.TextField(uid);
            if (id != uid)
            {
                ValidateUID(id);
            }
            bool tog = GUILayout.Toggle(lockSelected, "", "IN LockButton", GUILayout.Width(40));
            if(lockSelected ^ tog)
            {
                lockSelected = tog;
                if (!lockSelected)
                    ValidateGraph(btgraph ? btgraph.gameObject : null);
            }
            //EditorGUILayout.SelectableLabel("UID:" + uid, "textfield");
            if( !Application.isPlaying && GUILayout.Button("刷新", "TE toolbarbutton", GUILayout.Width(70)))
            {
                ValidateGraph();
            }
            if (!Application.isPlaying && Selection.activeGameObject != null &&
                GUILayout.Button("保存", "TE toolbarbutton", GUILayout.Width(70)))
            {
                ExportToXml();
                if (!btgraph)
                {
                    btgraph = Selection.activeGameObject.GetComponent<BehaviourGraph>();
                    ValidateUID(uid);
                }
                if (btgraph)
                {
                    ExportToBTGraph(btgraph);
                }
            }
        }

        protected override bool OnNewNodePanelGUI(Vector2 position, IGraphNode parent, ref Rect rect)
        {
            if (LinkMask != MASK_NEW_NODE)
                return false;
            if(LinkNodeLayer == 0)
            {
                return OnNewBTNode(position, parent, ref rect);
            }
            else if(LinkNodeLayer == 1)
            {
                return OnNewInputNode(position, parent, ref rect);
            }
            else
            {
                return false;
            }
        }

        bool OnNewInputNode(Vector2 position, IGraphNode parent, ref Rect rect)
        {
            if (graph.GetChildCount(1, parent) == 0)
            {
                InputNode node = new InputNode(parent as Node);
                node.Position = position - ViewOffset;
                graph.AddNode(node);
                graph.AddPath(1, parent, node);
            }
            return false;
        }

        bool OnNewBTNode(Vector2 position, IGraphNode parent, ref Rect rect)
        {
            bool ret = true;
            rect.size = new Vector2(210, 150);
            rect.position = position + Vector2.left * 105;
            GUI.Label(rect, "添加节点", "dragtabdropwindow");
            GUILayout.BeginArea(new Rect(rect.position + Vector2.one * 5, rect.size - Vector2.one * 10));
            if (copyTarget != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(150);
                if (GUILayout.Button("粘贴"))
                {
                    copyTarget.CopyTo(graph, parent, position - ViewOffset);
                    checkError = true;
                    ret = false;
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(20);
            }
            newNodeScroll = GUILayout.BeginScrollView(newNodeScroll);
            if (newNode == null)
            {
                for (int i = 0; i < nodeListName.Length; i++)
                {
                    List<BTModule> modules = GetModules(nodeList[i]);
                    if (i >= ctrlNodeEnd && (modules == null || modules.Count == 0))
                    {
                        continue;
                    }
                    if (GUILayout.Button(nodeListName[i], "TE toolbarbutton"))
                    {
                        newNode = new Node( ++indexPtr );
                        newNodeIndex = i;
                        newNode.Position = position - ViewOffset;
                        newNode.nodeType = nodeList[i];
                    }
                }
            }
            else if (newNodeIndex < ctrlNodeEnd)
            {
                graph.AddNode(newNode);
                graph.AddPath(0, parent, newNode);
                checkError = true;
                ret = false;
            }
            else
            {
                List<BTModule> modules = GetModules(newNode.nodeType);
                if (modules == null || modules.Count == 0)
                {
                    ret = false;
                }
                else
                {
                    ret = OnNewModuleGUI(modules, parent, newNode);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            return ret;
        }

        protected override void OnNewNodePanelGUIEnd()
        {
            newNode = null;
        }

        bool OnNewModuleGUI(List<BTModule> modules,IGraphNode parent, Node node)
        {
            GUILayout.Toggle(true, nodeListName[newNodeIndex], "TE toolbarbutton");
            for (int i = 0; i < modules.Count; i++)
            {
                if (GUILayout.Button(modules[i].name, "TE toolbarbutton"))
                {
                    newNode.module = modules[i].id;
                    graph.AddNode(newNode);
                    graph.AddPath(0, parent, newNode);
                    checkError = true;
                    return false;
                }
            }
            return true;
        }

        protected override void GetNodeSocketPos(int graphLayer, int mask, IGraphNode from, IGraphNode to, out Vector2 p0, out Vector2 p1)
        {
            if(graphLayer == 1)
            {
                p0 = from == null ? Event.current.mousePosition : new Vector2(from.ClipRect.center.x, from.ClipRect.yMin);
                p1 = to == null ? Event.current.mousePosition : new Vector2(to.ClipRect.center.x, to.ClipRect.yMax);
                if (p0.x > p1.x)
                {
                    if (from != null)
                        p0.x = from.ClipRect.xMin;
                    if (to != null)
                        p1.x = to.ClipRect.xMax;
                }
                else
                {
                    if (from != null)
                        p0.x = from.ClipRect.xMax;
                    if (to != null)
                        p1.x = to.ClipRect.xMin;
                }
            }
            else if (mask == MASK_MODIFY_PARENT)
            {
                p0 = from == null ? Event.current.mousePosition : new Vector2(from.ClipRect.center.x, from.ClipRect.yMin);
                p1 = to == null ? Event.current.mousePosition : new Vector2(to.ClipRect.center.x, to.ClipRect.yMax);
            }
            else
            {
                p0 = from == null ? Event.current.mousePosition : new Vector2(from.ClipRect.center.x, from.ClipRect.yMax);
                p1 = to == null ? Event.current.mousePosition : new Vector2(to.ClipRect.center.x, to.ClipRect.yMin);
            }
        }

        protected override void OnNodeToolGUI(IGraphNode node, bool selected)
        {
            if (Application.isPlaying)
                return;
            if (node == graph.Root && graph.GetChildCount(0, node) > 0)
                return;
            if (node is InputNode)
                return;
            Vector2 p0;
            p0 = new Vector2(node.ClipRect.center.x, node.ClipRect.yMax + 10);
            Rect rect = new Rect();
            rect.size = new Vector2(30, 30);
            rect.center = p0;
            if (rect.Contains(Event.current.mousePosition) || RaycastNode == node)
            {
                rect.size = new Vector2(20, 20);
                rect.center = p0;
                if (GUI.Button(rect, "", "OL Plus"))
                {
                    CreateConnection(0, node, MASK_NEW_NODE);
                }
            }
            if (node != graph.Root)
            {
                p0 = new Vector2(node.ClipRect.center.x, node.ClipRect.yMin - 5);
                rect.size = new Vector2(30, 30);
                rect.center = p0;
                if (rect.Contains(Event.current.mousePosition) || RaycastNode == node)
                {
                    rect.size = new Vector2(20, 20);
                    rect.center = p0;
                    if (GUI.Button(rect, "", "OL Plus"))
                    {
                        CreateConnection(0, node, MASK_MODIFY_PARENT);
                    }
                }
                p0 = new Vector2(node.ClipRect.xMax + 5, node.ClipRect.yMin - 5);
                rect.size = new Vector2(30, 30);
                rect.center = p0;
                Node tmp = node as Node;
                if(tmp != null && tmp.nodeType >= EBTNode.custom_control && (rect.Contains(Event.current.mousePosition) || RaycastNode == node))
                {
                    rect.size = new Vector2(20, 20);
                    rect.center = p0;
                    if(GUI.Button(rect,"", "Grad Down Swatch"))
                    {
                        CreateConnection(1, node, MASK_NEW_NODE);
                    }
                }
            }
        }

        protected override bool TryConnectNode(int graphLayer, int mask, int from, int to)
        {
            if (mask == MASK_MODIFY_PARENT)
            {
                if (graph[to] is InputNode)
                    return false;
                if (to == graph.RootIndex && graph.GetChildCount(0, graph.Root) > 0)
                    return false;
                if (graph.FindPath(graphLayer, from, to))
                    return false;
                graph.RemovePath(graphLayer, (x, y) => y == from);
                graph.AddPath(0, to, from);
                checkError = true;
                return true;
            }
            else if (graphLayer == 0)
            {
                if (graph[to] is InputNode)
                    return false;
                if (to != graph.RootIndex && graph.GetParentCount(0, to) == 0)
                {
                    graph.AddPath(graphLayer, from, to);
                    checkError = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (graphLayer == 1)
            {
                if ((graph[from] is Node) && (graph[to] is InputNode))
                {
                    graph.RemovePath(graphLayer, (x, y) => x == from);
                    graph.AddPath(graphLayer, from, to);
                    checkError = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected override bool EnableLinkNode(int graphLayer, int from, int to)
        {
            if (graphLayer == 0)
            {
                return LinkMask != MASK_MODIFY_PARENT || graph[to] != LinkNodeStart;
            }
            else
            {
                return LinkNodeLayer != 1 || graph[from] != LinkNodeStart;
            }
        }

        void GetChildRecursive(IGraphNode root)
        {
            int off = childrens.Count;
            graph.GetAllChildren(0, root, childrens);
            graph.GetAllChildren(1, root, childrens);
            int end = childrens.Count;
            for (int i = off; i < end; i++)
            {
                GetChildRecursive(childrens[i]);
            }
        }

        protected override void SelectNode(IGraphNode node, bool recursive)
        {
            selections.Add(node);
            if (recursive)
            {
                childrens.Clear();
                GetChildRecursive(node);
                for(int i = 0; i < childrens.Count; i++)
                {
                    selections.Add(childrens[i]);
                }
                childrens.Clear();
            }
        }

        protected override bool EnableDestroyLink(int graphLayer, int from, int to)
        {
            if (Application.isPlaying)
                return false;
            return base.EnableDestroyLink(graphLayer, from, to);
        }

        protected override void OnDeleteConnection(int graphLayer, int from, int to)
        {
            if (Application.isPlaying)
                return;
            base.OnDeleteConnection(graphLayer, from, to);
            checkError = true;
        }

        protected override void OnDeleteSelections()
        {
            if (Application.isPlaying)
                return;
            base.OnDeleteSelections();
            checkError = true;
        }

        protected override bool OnContextMenuGUI(Vector2 position, ref Rect rect)
        {
            if (ContextNode == null || ContextNode == graph.Root)
                return false;
            if (position.y > ClipRect.height - rect.height)
            {
                position.y -= rect.height;
            }
            rect.position = position;
            GUI.SetNextControlName("ContextMenu");
            GUI.Label(rect, "", "flow overlay box");
            Rect item = new Rect();
            item.size = new Vector2(90, 30);
            Vector2 pos = position + Vector2.one * 5;
            pos.y += 5;
            item.position = pos;
            pos.y += 30;
            bool tog = item.Contains(Event.current.mousePosition);
            if (GUI.Toggle(item, tog, "删除", "SearchModeFilter") ^ tog)
            {
                graph.RemoveNode(ContextNode);
                checkError = true;
                return false;
            }
            if (ContextNode is Node)
            {
                item.position = pos;
                pos.y += 30;
                tog = item.Contains(Event.current.mousePosition);
                if (GUI.Toggle(item, tog, "复制", "SearchModeFilter") ^ tog)
                {
                    copyTarget = ContextNode as Node;
                    return false;
                }
                Node context = ContextNode as Node;
                if (!SetSort(context, ref item, ref pos))
                    return false;
            }
            pos.y += 5;
            rect.size = new Vector2(100, pos.y - position.y);
            return true;
        }

        bool SetSort(Node context, ref Rect item, ref Vector2 pos)
        {
            if (context != null && IsCtrlNode(context.nodeType))
            {
                item.position = pos;
                pos.y += 10;
                GUI.Label(item, "", "IN Title");
                bool tog;
                item.position = pos;
                pos.y += 30;
                tog = context.data.m_SortType == ESortType.none || item.Contains(Event.current.mousePosition);
                if (tog ^ GUI.Toggle(item, tog, "顺序", "SearchModeFilter"))
                {
                    context.data.m_SortType = ESortType.none;
                    return false;
                }
                item.position = pos;
                pos.y += 30;
                tog = context.data.m_SortType == ESortType.asc || item.Contains(Event.current.mousePosition);
                if (tog ^ GUI.Toggle(item, tog, "升序", "SearchModeFilter"))
                {
                    context.data.m_SortType = ESortType.asc;
                    return false;
                }
                item.position = pos;
                pos.y += 30;
                tog = context.data.m_SortType == ESortType.desc || item.Contains(Event.current.mousePosition);
                if (tog ^ GUI.Toggle(item, tog, "降序", "SearchModeFilter"))
                {
                    context.data.m_SortType = ESortType.desc;
                    return false;
                }
                item.position = pos;
                pos.y += 30;
                tog = context.data.m_SortType == ESortType.random || item.Contains(Event.current.mousePosition);
                if (tog ^ GUI.Toggle(item, tog, "无序", "SearchModeFilter"))
                {
                    context.data.m_SortType = ESortType.random;
                    return false;
                }
            }
            return true;
        }

        protected override void OnGraphGUIEnd()
        {
            if (checkError)
            {
                hasError = false;
                for (int i = 0; i < graph.NodeLength; i++)
                {
                    Node node = graph[i] as Node;
                    if (node != null)
                    {
                        node.CheckError(this);
                        if (!string.IsNullOrEmpty(node.errorMsg))
                            hasError = true;
                    }
                }
                checkError = false;

            }
        }
    }

}