using Devil.Utility;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class GraphViewEditorWindow : EditorWindow
    {

        public const int PORT_PARENT = 1;
        public const int PORT_CHILD = 2;

        public struct NodeSocket
        {
            public int sockPort; // 对接码
            public int toPort; // 下级对接码
            public int layer; // 所在层
            public uint layerMask; // 显示层
            public Vector2 uvCoord;

            public Vector2 GetSockPos(Rect bounds)
            {
                return new Vector2(Mathf.Lerp(bounds.xMin, bounds.xMax, uvCoord.x), Mathf.Lerp(bounds.yMin, bounds.yMax, uvCoord.y));
            }
        }

        public class GraphNode
        {
            bool mAsRoot;
            protected Rect mBouds;
            public Vector2 PixelSize { get; set; }
            public Vector2 Position { get; set; }
            public uint LayerMask { get; set; }
            public string Name { get; set; }
            public bool AsRoot { get { return mAsRoot; } }
            protected NodeSocket[] mSockets;
            protected int mExtSocks = 0;

            public GraphNode(bool asRoot)
            {
                mAsRoot = asRoot;
                InitDefaultData();
            }

            protected virtual void InitDefaultData()
            {
                Position = new Vector2(0, 40);
                LayerMask = 0xffffffffu;
                if (mAsRoot)
                {
                    PixelSize = new Vector2(100, 50);
                }
                else
                {
                    PixelSize = Vector2.one * 30;
                }
                mSockets = new NodeSocket[2 + mExtSocks];

                mSockets[0].sockPort = PORT_PARENT;
                mSockets[0].toPort = PORT_CHILD;
                mSockets[0].layer = 0;
                mSockets[0].layerMask = 0xffffffffu;
                mSockets[0].uvCoord = new Vector2(0.5f, 1f);

                mSockets[1].sockPort = PORT_CHILD;
                mSockets[1].toPort = PORT_PARENT;
                mSockets[1].layer = -1;
                mSockets[1].layerMask = AsRoot ? 0 : 0xffffffffu;
                mSockets[1].uvCoord = new Vector2(0.5f, 0f);
            }

            public virtual void CalculateBounds(Vector2 viewOffset)
            {
                PixelSize = Installizer.SizeOfTitle(Name) + Vector2.right * 20 + Vector2.up * 40;
                mBouds.size = PixelSize;
                mBouds.position = new Vector2((Position.x - PixelSize.x * 0.5f), Position.y) + viewOffset;
            }

            public Rect ClipRect { get { return mBouds; } }

            public virtual GraphNode Clone()
            {
                GraphNode node = new GraphNode(mAsRoot);
                node.Name = Name;
                node.Position = Position;
                return node;
            }

            public virtual void OnNodeGUI(GraphViewEditorWindow window, bool selected)
            {
                Rect rect = new Rect();
                if (AsRoot)
                {
                    rect.size = mBouds.size - new Vector2(20, 20);
                    rect.position = mBouds.position + new Vector2(10, 3);
                }
                else
                {
                    rect.size = mBouds.size - new Vector2(20, 35);
                    rect.center = mBouds.center;
                }
                GUI.Label(rect, "", "Icon.OutlineBorder");
                GUI.Label(rect, Name, Installizer.titleStyle);
            }

            public virtual void OnSocketsGUI(GraphViewEditorWindow window)
            {
                for (int i = 0; i < mSockets.Length; i++)
                {
                    NodeSocket sock = mSockets[i];
                    if ((sock.layerMask & window.PaintLayers) == 0)
                    {
                        continue;
                    }
                    OnSocketGUI(window, mSockets[i]);
                }
            }

            protected virtual void OnSocketGUI(GraphViewEditorWindow window, NodeSocket sock)
            {
                Rect rect = new Rect();
                rect.size = new Vector2(PixelSize.x - 40, 15);
                rect.center = sock.GetSockPos(ClipRect) + Vector2.up * 8 * (sock.uvCoord.y == 0 ? 1 : -1);
                if (window.mShowContextMenu)
                {
                    GUI.Label(rect, "", "Icon.ClipSelected");
                }
                else if (GUI.Button(rect, "", "Icon.ClipSelected"))
                {
                    window.CreateConnection(this, sock.layer, sock.sockPort, sock.toPort);
                }
            }

            public virtual int SocketCount { get { return mSockets.Length; } }

            public NodeSocket GetSocket(int index)
            {
                return mSockets[index];
            }

            public bool GetSocket(int port, ref NodeSocket sock)
            {
                for (int i = 0; i < mSockets.Length; i++)
                {
                    if (mSockets[i].sockPort == port)
                    {
                        sock = mSockets[i];
                        return true;
                    }
                }
                return false;
            }
            
        }

        protected string mViewportName = "Graph Editor";
        protected string mViewportTitle = "Graph Editor";
        protected Vector2 viewHalfSize = new Vector2(5000, 5000);
        protected int mGraphLayers = 1;
        protected GraphNode mDefaultRoot;
        Graph<GraphNode> mGraph;
        public Graph<GraphNode> graph { get { return mGraph; } }
        GraphNode mRaycastNode;
        GraphNode mContextNode;
        bool mShowContextMenu;
        public bool IsContextMenuShown { get { return mShowContextMenu; } }
        public GraphNode RaycastNode { get { return mRaycastNode; } }
        public GraphNode ContextNode { get { return mContextNode; } }


        Rect mClipRect;
        public Rect ClipRect { get { return mClipRect; } }
        Vector2 mViewPos;
        bool repaint;
        bool focusCenter;
        Vector2 mousePos;
        Vector2 mouseDeltaPos;
        Vector2 scroll;
        bool mouseDrag;
        Rect tmpRect;
        Vector2 mViewOffset;
        public Vector2 ViewOffset { get { return mViewOffset; } }
        Vector2 centerOffset;
        Rect selectionRect;
        //bool onFocus;
        bool mInterceptMouse;
        bool doSelect;
        bool onNodeDrag;
        bool catchDrag;

        // 新建连接的起点
        protected GraphNode mWireStart;
        protected NodeSocket mWireA;
        protected NodeSocket mWireB;
        // 新建连接层
        protected int mWireLayer;

        Rect mPanelRect;
        Vector2 mPanelPosition;
        bool mShowNewNodePanel;

        protected uint mPaintLayers = 1; // 绘制的节点层
        public uint PaintLayers { get { return mPaintLayers; } }
        protected HashSet<GraphNode> mSelections = new HashSet<GraphNode>();
        //List<GraphNode> tmpNodes = new List<GraphNode>();
        NodeSocket tmpSockA;
        NodeSocket tmpSockB;
        public bool IsNodeInEditting { get { return mWireStart != null || mShowContextMenu; } }
        public bool Editable { get; set; }

        protected virtual void InitParameters()
        {
        }

        void Init()
        {
            catchDrag = false;
            InitParameters();
            ResetGraph();
        }

        protected virtual void OnEnable()
        {
            Init();
        }

        protected virtual void ResetGraph()
        {
            if (mWireStart != null && mShowNewNodePanel)
                OnNewNodePanelGUIEnd();
            mWireStart = null;
            if (mShowContextMenu)
                OnContextMenuGUIEnd();
            mShowContextMenu = false;
            mGraph = new Graph<GraphNode>(mGraphLayers);
            GraphNode node = mDefaultRoot == null ? new GraphNode(true) : mDefaultRoot;
            node.Name = "ROOT";
            mGraph.AddNode(node);
        }

        private void OnGUI()
        {
            OnPrepareGraphGUI();
            ProcessMouseDeltaPos();

            GUI.skin.label.richText = true;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal("TE toolbar");
            OnTitleGUI();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUI.SetNextControlName(mViewportName);

            QuickGUI.ReportView(ref mClipRect, mViewPos, OnDrawGUI, position.height, 100, mViewportTitle);
            mViewOffset = -mViewPos + new Vector2(mClipRect.size.x * 0.5f, 100f);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            ProcessEvent();
            ProcessFocusCenter();

            OnGraphGUIEnd();

            if (repaint)
            {
                Repaint();
            }
            repaint = false;
        }

        protected virtual void OnPrepareGraphGUI()
        {

        }

        protected virtual void OnTitleGUI()
        {
            if (GUILayout.Button("导出", "TE toolbarbutton", GUILayout.Width(70)))
            {
                SelectFileAndExport();
            }
            if (GUILayout.Button("导入", "TE toolbarbutton", GUILayout.Width(70)))
            {
                SelectFileAndImport();
            }
        }

        // 计算鼠标移动量
        void ProcessMouseDeltaPos()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                mousePos = Event.current.mousePosition;
                mouseDeltaPos = Vector2.zero;
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                Vector2 pos = Event.current.mousePosition;
                mouseDeltaPos = pos - mousePos;
                mousePos = pos;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                mouseDeltaPos = Vector2.zero;
            }
        }

        //聚焦状态图中心
        private void ProcessFocusCenter()
        {
            if (focusCenter && Vector2.Distance(Vector2.zero, mViewPos) > 1)
            {
                repaint |= true;
                mViewPos = Vector2.Lerp(mViewPos, Vector2.zero, 0.1f);
            }
            else
            {
                focusCenter = false;
            }
        }

        // 响应鼠标事件
        void ProcessEvent()
        {
            mInterceptMouse = mClipRect.Contains(Event.current.mousePosition);
            if (!mInterceptMouse)
            {
                return;
            }
            repaint |= true;
            if (Event.current.isKey && Event.current.control && Event.current.keyCode == KeyCode.F)
            {
                focusCenter = true;
                if (graph.Root != null)
                {
                    Vector2 delta = -graph.Root.Position;
                    for (int i = 0; i < graph.NodeLength; i++)
                    {
                        graph[i].Position += delta;
                    }
                }
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
            {
                mouseDrag = true;
            }
            else if (mouseDrag && Event.current.type == EventType.MouseDrag)
            {
                mViewPos -= mouseDeltaPos;
                mViewPos.x = Mathf.Clamp(mViewPos.x, -viewHalfSize.x, viewHalfSize.x);
                mViewPos.y = Mathf.Clamp(mViewPos.y, -viewHalfSize.y, viewHalfSize.y);
            }
            if (Event.current.type == EventType.MouseUp)
            {
                mouseDrag = false;
            }
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete)
            {
                Event.current.Use();
                OnDeleteSelections();
            }
        }

        protected virtual void OnDrawGUI()
        {
            CalculateBounds();
            PaintWires();
            PaintNode();
            PaintLinkNode();
            PaintContextMenu();
            ProcessSelectionRect();
            if (mRaycastNode != null && !mRaycastNode.ClipRect.Contains(Event.current.mousePosition))
                mRaycastNode = null;
        }

        void CalculateBounds()
        {
            for (int i = 0; i < graph.NodeLength; i++)
            {
                GraphNode node = graph[i];
                if ((mPaintLayers & node.LayerMask) != 0)
                    node.CalculateBounds(mViewOffset);
            }
        }

        protected virtual void DrawBezierWire(NodeSocket sockA, NodeSocket sockB, Vector2 p0, Vector2 p1, Color color, float width)
        {
            Handles.DrawBezier(p0, p1, p0, p1, color, null, width);
        }

        protected virtual void GetWireColorAndWidth(GraphNode nodeA, NodeSocket sockA, GraphNode nodeB, NodeSocket sockB, out Color color, out float width)
        {
            width = 3;
            color = nodeB == null ? Color.yellow : Color.white;
        }

        void PaintWires()
        {
            Color color;
            float wireWidth;
            for (int i = 0; i < mGraph.Layers; i++)
            {
                uint layer = 1u << i;
                if ((mPaintLayers & layer) == 0)
                    continue;
                for (int j = 0; j < mGraph.PathLength(i); j++)
                {
                    int from, to;
                    mGraph.PathAt(i, j, out from, out to);
                    GraphNode fromNode = graph[from];
                    GraphNode toNode = graph[to];
                    for (int n = 0; n < fromNode.SocketCount; n++)
                    {
                        tmpSockA = fromNode.GetSocket(n);
                        if (tmpSockA.layer != i)
                            continue;
                        if (!toNode.GetSocket(tmpSockA.toPort, ref tmpSockB))
                            continue;
                        GetWireColorAndWidth(fromNode, tmpSockA, toNode, tmpSockB, out color, out wireWidth);
                        DrawBezierWire(tmpSockA, tmpSockB, tmpSockA.GetSockPos(fromNode.ClipRect), tmpSockB.GetSockPos(toNode.ClipRect), color, wireWidth);
                    }
                }
            }
        }

        void PaintNode()
        {
            for (int i = graph.NodeLength - 1; i >= 0; i--)
            {
                GraphNode node = mGraph[i];
                if ((mPaintLayers & node.LayerMask) == 0)
                    continue;
                if (mRaycastNode == null && node.ClipRect.Contains(Event.current.mousePosition))
                {
                    mRaycastNode = node;
                }
                bool selected = mSelections.Contains(node);
                bool on = selected || node == (mShowContextMenu ? mContextNode : mRaycastNode);
                OnNodeFrameGUI(node, on);
                node.OnNodeGUI(this, selected);
                node.OnSocketsGUI(this);
                //if (mWireStart == null && mSelections.Count <= 1 && !mShowContextMenu)
                //    OnNodeSocketGUI(node, selected);
            }
        }

        protected virtual void OnNodeFrameGUI(GraphNode node, bool highlight)
        {
            if (mContextNode == node)
            {//dockarea
                GUI.Label(node.ClipRect, "", highlight ? "flow node 1 on" : "flow node 1");
            }
            else
            {
                GUI.Label(node.ClipRect, "", highlight ? "flow node 0 on" : "flow node 0");
            }
            //GUI.Label(node.ClipRect, "", "box");
        }

        void PaintLinkNode()
        {
            if (mWireStart == null)
            {
                if (mShowNewNodePanel)
                    OnNewNodePanelGUIEnd();
                mShowNewNodePanel = false;
                return;
            }
            Vector2 p0, p1;
            float wireWidth;
            Color wireColor;
            if (mShowNewNodePanel)
                p1 = mPanelPosition + mViewOffset;
            else
                p1 = Event.current.mousePosition;
            p0 = mWireA.GetSockPos(mWireStart.ClipRect);
            GetWireColorAndWidth(mWireStart, mWireA, null, mWireB, out wireColor, out wireWidth);
            DrawBezierWire(mWireA, mWireB, p0, p1, wireColor, wireWidth);

            if ((!mShowNewNodePanel || !mPanelRect.Contains(Event.current.mousePosition)) && Event.current.button == 1 && Event.current.type == EventType.MouseUp)
            {
                mWireStart = null;
                return;
            }
            bool click = Event.current.button == 0 && Event.current.type == EventType.MouseUp;
            if (!mShowNewNodePanel && click)
            {
                if (mRaycastNode == null)
                {
                    mShowNewNodePanel = true;
                    mPanelPosition = Event.current.mousePosition - mViewOffset;
                }
                else if (TryConnectNode(mWireLayer, mWireA, mWireB, mWireStart, mRaycastNode))
                {
                    mWireStart = null;
                }
            }
            else if (mShowNewNodePanel)
            {
                mShowNewNodePanel = OnNewNodePanelGUI(mPanelPosition + mViewOffset, ref mPanelRect);
                if (!mShowNewNodePanel)
                {
                    mWireStart = null;
                    OnNewNodePanelGUIEnd();
                }
            }
        }

        void PaintContextMenu()
        {
            if (mWireStart != null || !mInterceptMouse)
            {
                if (mShowContextMenu)
                    OnContextMenuGUIEnd();
                mShowContextMenu = false;
                mContextNode = null;
                return;
            }
            if (mShowContextMenu)
            {
                mShowContextMenu = OnContextMenuGUI(mPanelPosition + mViewOffset, ref mPanelRect);
                if (!mShowContextMenu)
                    OnContextMenuGUIEnd();
            }
            else
            {
                mContextNode = null;
            }
            if (mShowContextMenu && mPanelRect.Contains(Event.current.mousePosition))
            {
                return;
            }
            if (Event.current.type == EventType.MouseDown)
            {
                GraphNode gnode = mContextNode;
                bool shown = mShowContextMenu;
                mShowContextMenu = Event.current.button == 1 && Editable;
                if (mShowContextMenu)
                {
                    mPanelPosition = Event.current.mousePosition - mViewOffset;
                    mContextNode = mRaycastNode;
                    if (shown && gnode != mContextNode)
                        OnContextMenuGUIEnd();
                }
                else if (shown)
                {
                    OnContextMenuGUIEnd();
                }
            }
        }

        void ProcessSelectionRect()
        {
            if (mWireStart != null || mShowContextMenu)
                return;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && mRaycastNode == null)
            {
                catchDrag = true;
            }
            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                if (!onNodeDrag && !doSelect && mRaycastNode == null && catchDrag)
                {
                    if (!Event.current.shift)
                        mSelections.Clear();
                    selectionRect.size = Vector2.zero;
                    selectionRect.position = Event.current.mousePosition;
                    doSelect = true;
                }
                if (!doSelect)
                {
                    if (!onNodeDrag && mRaycastNode != null && mSelections.Count <= 1)
                    {
                        mSelections.Clear();
                        SelectNode(mRaycastNode, Event.current.alt);
                    }
                    if (Editable && mSelections.Contains(mRaycastNode))
                    {
                        foreach (GraphNode nd in mSelections)
                        {
                            Vector2 p = nd.Position + mouseDeltaPos;
                            nd.Position = p;
                        }
                    }
                }
                onNodeDrag = true;
            }
            if (Event.current.type == EventType.MouseUp && Event.current.button != 2)
            {
                if (!onNodeDrag)
                {
                    if (!Event.current.shift)
                        mSelections.Clear();
                    if (mRaycastNode != null)
                        SelectNode(mRaycastNode, Event.current.alt);
                }
                if (doSelect)
                {
                    CheckSelectedNode();
                    doSelect = false;
                }
                onNodeDrag = false;
                catchDrag = false;
            }
            if (doSelect)
            {
                selectionRect.size = Event.current.mousePosition - selectionRect.position;
                GUI.Label(selectionRect, "", "SelectionRect");
            }
            if (Event.current.type == EventType.MouseUp)
            {
                GUI.FocusControl(mViewportName);
            }
        }

        void CheckSelectedNode()
        {
            for (int i = 0; i < mGraph.NodeLength; i++)
            {
                if (selectionRect.Overlaps(mGraph[i].ClipRect, true))
                {
                    mSelections.Add(mGraph[i]);
                }
            }
        }

        protected virtual void OnDeleteConnection(int graphLayer, int from, int to)
        {
            mGraph.RemovePath(graphLayer, from, to);
        }

        public void CreateConnection(GraphNode startAt, int graphLayer, int portA, int portB)
        {
            if (!Editable || mWireStart != null || startAt == null || !startAt.GetSocket( portA, ref mWireA))
                return;
            mSelections.Clear();
            mWireStart = startAt;
            mShowNewNodePanel = false;
            mWireLayer = graphLayer;
            mWireB.sockPort = mWireA.toPort;
            mWireB.toPort = mWireA.sockPort;
            mWireB.layer = mWireA.layer;
        }

        protected virtual void OnGraphGUIEnd() { }

        protected virtual bool TryConnectNode(int layer, NodeSocket sockA, NodeSocket sockB, GraphNode startNode, GraphNode endNode)
        {
            if (sockA.toPort != sockB.sockPort)
                return false;
            if(sockA.layer != -1)
            {
                mGraph.AddPath(layer, startNode, endNode);
                return true;
            }
            if (!endNode.GetSocket(sockB.sockPort, ref tmpSockA) || tmpSockA.toPort != sockA.sockPort)
                return false;
            mGraph.AddPath(tmpSockA.layer, endNode, startNode);
            return true;
        }

        protected virtual bool OnNewNodePanelGUI(Vector2 position, ref Rect rect)
        {
            if (mWireLayer == -1)
                return false;
            GraphNode nd = new GraphNode(false);
            nd.Name = "NODE";
            nd.Position = Event.current.mousePosition - ViewOffset;
            graph.AddNode(nd);
            graph.AddPath(mWireLayer, mWireStart, nd);
            return false;
        }

        protected virtual void OnNewNodePanelGUIEnd() { }

        protected virtual void SelectNode(GraphNode node, bool recursive)
        {
            mSelections.Add(node);
        }

        protected virtual void OnDeleteSelections()
        {
            foreach (GraphNode node in mSelections)
            {
                if (node != graph.Root)
                    graph.RemoveNode(node);
            }
        }

        protected virtual bool OnContextMenuGUI(Vector2 position, ref Rect rect)
        {
            return false;
        }
        
        protected virtual void OnContextMenuGUIEnd() { }

        protected virtual XmlElement SerializeNode(XmlDocument doc, GraphNode node)
        {
            return null;
        }

        protected virtual GraphNode DeserializeNode(XmlElement element)
        {
            return null;
        }

        public bool SelectFileAndExport()
        {
            string path = EditorPrefs.GetString("graph.path");
            string fileName = EditorPrefs.GetString("graph.file");
            if (string.IsNullOrEmpty(path))
                path = Application.dataPath;
            if (string.IsNullOrEmpty(fileName))
                fileName = "graph.xml";
            path = EditorUtility.SaveFilePanel("导出图", path, fileName, "xml");
            if (!string.IsNullOrEmpty(path))
            {
                fileName = Path.GetFileName(path);
                path = Path.GetDirectoryName(path);
                EditorPrefs.SetString("graph.path", path);
                EditorPrefs.SetString("graph.file", fileName);
                ExportToXml(path, fileName);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SelectFileAndImport()
        {
            string path = EditorPrefs.GetString("graph.path");
            if (string.IsNullOrEmpty(path))
                path = Application.dataPath;
            path = EditorUtility.OpenFilePanel("导入图", path, "xml");
            if (!string.IsNullOrEmpty(path))
            {
                bool res = ImportFromXml(path);
                path = Path.GetDirectoryName(path);
                EditorPrefs.SetString("graph.path", path);
                return res;
            }
            else
            {
                return false;
            }
        }

        protected bool ImportFromXml(string fileName)
        {
            if (!File.Exists(fileName))
                return false;
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            //XmlElement root = doc.FirstChild as XmlElement;
            XmlNodeList linkList = doc.GetElementsByTagName("Link-List");

            Graph<GraphNode> newgraph = new Graph<GraphNode>(mGraphLayers);

            XmlNodeList nodes = doc.GetElementsByTagName("Node-List");
            XmlElement nodeList = nodes.Count > 0 ? nodes[0] as XmlElement : null;
            if (nodeList != null)
            {
                XmlElement node = nodeList.FirstChild as XmlElement;
                while (node != null)
                {
                    XmlElement ser = node.FirstChild as XmlElement;
                    Vector2 pos = StringUtil.ParseVector2(node.GetAttribute("position") ?? "(0,0)");
                    bool asRoot = node.GetAttribute("isRoot") == "true";
                    GraphNode nd = null;
                    if (ser != null)
                    {
                        nd = DeserializeNode(ser);
                    }
                    if (nd == null)
                    {
                        nd = new GraphNode(asRoot);
                    }
                    nd.Name = node.GetAttribute("name");
                    nd.LayerMask = uint.Parse(node.GetAttribute("layers"));
                    nd.Position = pos;
                    newgraph.AddNode(nd);
                    if (asRoot)
                        newgraph.SetRootNodeIndex(newgraph.NodeLength - 1);
                    node = node.NextSibling as XmlElement;
                }
            }

            foreach (XmlNode links in linkList)
            {
                XmlElement lin = links as XmlElement;
                int layer = int.Parse(lin.GetAttribute("layer"));
                foreach (XmlNode linInst in lin.ChildNodes)
                {
                    XmlElement ele = linInst as XmlElement;
                    newgraph.AddPath(layer, int.Parse(ele.GetAttribute("from")),
                        int.Parse(ele.GetAttribute("to")));
                }
            }
            mGraph = newgraph;
            return true;
        }

        // 保存到xml
        protected void ExportToXml(string path, string fileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ManagedGraph");
            root.SetAttribute("root", graph.RootIndex.ToString());
            doc.AppendChild(root);
            XmlElement nodeList = doc.CreateElement("Node-List");
            root.AppendChild(nodeList);
            for (int i = 0; i < graph.NodeLength; i++)
            {
                XmlElement node = doc.CreateElement("Node");
                nodeList.AppendChild(node);
                node.SetAttribute("id", i.ToString());
                node.SetAttribute("position", string.Format("({0},{1})", graph[i].Position.x, graph[i].Position.y));
                node.SetAttribute("name", graph[i].Name);
                node.SetAttribute("layers", graph[i].LayerMask.ToString());
                if (i == graph.RootIndex)
                    node.SetAttribute("isRoot", "true");
                XmlElement child = SerializeNode(doc, graph[i]);
                if (child != null)
                    node.AppendChild(child);
            }
            for (int i = 0; i < graph.Layers; i++)
            {
                XmlElement links = doc.CreateElement("Link-List");
                root.AppendChild(links);
                links.SetAttribute("layer", i.ToString());
                int from, to;
                for (int j = 0; j < graph.PathLength(i); j++)
                {
                    graph.PathAt(i, j, out from, out to);
                    XmlElement link = doc.CreateElement("Link");
                    link.SetAttribute("from", from.ToString());
                    link.SetAttribute("to", to.ToString());
                    links.AppendChild(link);
                }
            }
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string file = Path.Combine(path, fileName);
            doc.Save(file);
            AssetDatabase.Refresh();
        }
    }
}