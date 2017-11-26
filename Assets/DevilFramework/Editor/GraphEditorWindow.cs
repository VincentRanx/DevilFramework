using DevilTeam.Editor;
using DevilTeam.Utility;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class GraphEditorWindow : EditorWindow
{
    public interface IGraphNode
    {
        Vector2 Position { get; set; }

        void CalculateBounds(Vector2 viewOffset);

        Rect ClipRect { get; }

        void OnNodeGUI(GraphEditorWindow window, bool selected);
    }

    public class EmptyNode : IGraphNode
    {
        Rect mBouds;
        bool mAsRoot;

        public EmptyNode(bool asRoot)
        {
            Position = new Vector2(0, 40);
            mAsRoot = asRoot;
        }

        public Vector2 Position { get; set; }

        public void CalculateBounds(Vector2 viewOffset)
        {
            if (mAsRoot)
            {
                mBouds.size = new Vector2(100, 50);
                mBouds.position = new Vector2((Position.x - 50), Position.y) + viewOffset;
            }
            else
            {
                mBouds.size = Vector2.one * 30;
                mBouds.position = new Vector2((Position.x - 15), Position.y) + viewOffset;
            }
        }

        public Rect ClipRect { get { return mBouds; } }

        public void OnNodeGUI(GraphEditorWindow window, bool selected)
        {
            if (mAsRoot)
            {
                GUI.Label(mBouds, "ROOT", "LODLevelNotifyText");
            }
        }
    }

    protected string mViewportName = "Graph Editor";
    protected string mViewportTitle = "Graph Editor";
    protected Vector2 viewHalfSize = new Vector2(5000, 5000);
    protected int mGraphLayers = 1;
    Graph<IGraphNode> mGraph;
    public Graph<IGraphNode> graph { get { return mGraph; } }
    IGraphNode mRaycastNode;
    IGraphNode mContextNode;
    public IGraphNode RaycastNode { get { return mRaycastNode; } }
    public IGraphNode ContextNode { get { return mContextNode; } }

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
    bool onFocus;
    bool mInterceptMouse;
    bool doSelect;
    bool onNodeDrag;
    bool catchDrag;
    IGraphNode mLinkNodeStart; // 连接节点起点
    public IGraphNode LinkNodeStart { get { return mLinkNodeStart; } }
    bool mShowLinkPanel; // 连接新节点panel
    int mLinkNodeLayer; // 创建连接的层
    public int LinkNodeLayer { get { return mLinkNodeLayer; } }
    int mLinkMask; // 创建连接识别码
    public int LinkMask { get { return mLinkNodeStart == null ? 0 : mLinkMask; } }

    Rect mPanelRect;
    Vector2 mPanelPosition;
    bool mContextMenu;
    public bool IsContextMenuShown { get { return mContextMenu; } }
    public bool IsNewPanelShown { get { return mShowLinkPanel; } }

    protected HashSet<IGraphNode> selections = new HashSet<IGraphNode>();
    List<IGraphNode> tmpNodes = new List<IGraphNode>();

    protected virtual void InitParameters()
    {
    }

    void Init()
    {
        catchDrag = false;
        InitParameters();
        mGraph = new Graph<IGraphNode>(mGraphLayers);
        mGraph.AddNode(new EmptyNode(true));
    }

    protected virtual void OnEnable()
    {
        Init();
    }

    protected void ResetGraph()
    {
        mGraph = new Graph<IGraphNode>(mGraphLayers);
        mGraph.AddNode(new EmptyNode(true));
    }

    private void OnGUI()
    {
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

        if (repaint || onFocus)
        {
            Repaint();
        }
        repaint = false;
    }

    protected virtual void OnTitleGUI()
    {
        GUILayout.Label(mViewportTitle);
    }

    // 计算鼠标移动量
    void ProcessMouseDeltaPos()
    {
        if (Event.current.type == EventType.mouseDown)
        {
            mousePos = Event.current.mousePosition;
            mouseDeltaPos = Vector2.zero;
        }
        else if (Event.current.type == EventType.mouseDrag)
        {
            Vector2 pos = Event.current.mousePosition;
            mouseDeltaPos = pos - mousePos;
            mousePos = pos;
        }
        else if (Event.current.type == EventType.mouseUp)
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
                for(int i = 0; i < graph.NodeLength; i++)
                {
                    graph[i].Position += delta;
                }
            }
        }
        if (Event.current.type == EventType.mouseDown && Event.current.button == 2)
        {
            mouseDrag = true;
        }
        else if (mouseDrag && Event.current.type == EventType.mouseDrag)
        {
            mViewPos -= mouseDeltaPos;
            mViewPos.x = Mathf.Clamp(mViewPos.x, -viewHalfSize.x, viewHalfSize.x);
            mViewPos.y = Mathf.Clamp(mViewPos.y, -viewHalfSize.y, viewHalfSize.y);
        }
        if (Event.current.type == EventType.mouseUp)
        {
            mouseDrag = false;
        }
        if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete)
        {
            Event.current.Use();
            OnDeleteSelections();
        }
    }

    protected virtual void OnDrawGUI()
    {
        CalculateBounds();
        PaintPath();
        PaintNode();
        PaintLinkNode();
        PaintContextMenu();
        ProcessSelectionRect();
        if (mRaycastNode != null && !mRaycastNode.ClipRect.Contains(Event.current.mousePosition))
            mRaycastNode = null;
    }

    void CalculateBounds()
    {
        for (int i = 0; i < mGraph.NodeLength; i++)
        {
            mGraph[i].CalculateBounds(mViewOffset);
        }
    }

    protected virtual void BezierConnect(int layer, int mask, Vector2 p0, Vector2 p1, Color color, float width)
    {
        Handles.DrawBezier(p0, p1, p0, p1, color, null, width);
    }

    void PaintPath()
    {
        Vector2 p0, p1;
        Rect rect = new Rect();
        for(int i = 0; i < mGraph.Layers; i++)
        {
            for (int j = 0; j < mGraph.PathLength(i); j++)
            {
                int from, to;
                mGraph.PathAt(i, j, out from, out to);
                if (EnableLinkNode(i, from, to))
                {
                    GetNodeSocketPos(i, 0, mGraph[from], mGraph[to], out p0, out p1);
                    BezierConnect(i, 0, p0, p1, GetLinkColor(mGraph[from], mGraph[to], i), GetLinkWidth(mGraph[from], mGraph[to], i));
                    if (mLinkNodeStart == null && EnableDestroyLink(j, from, to))
                    {
                        rect.size = new Vector2(20, 20);
                        rect.center = (p1 + p0) * 0.5f;
                        if (rect.Contains(Event.current.mousePosition) && GUI.Button(rect, "", "OL Minus"))
                        {
                            OnDeleteConnection(i, from, to);
                        }
                    }
                }
            }
        }
    }

    void PaintNode()
    {
        for (int i = mGraph.NodeLength - 1; i >= 0; i--)
        {
            IGraphNode node = mGraph[i];
            if (mRaycastNode == null && node.ClipRect.Contains(Event.current.mousePosition))
            {
                mRaycastNode = node;
            }
            bool selected = selections.Contains(node);
            bool on = selected || node == (mContextMenu ? mContextNode : mRaycastNode);
            OnNodeFrameGUI(node, on);
            node.OnNodeGUI(this, selected);
            if (mLinkNodeStart == null && selections.Count <= 1 && !mContextMenu)
                OnNodeToolGUI(node, selected);
        }
    }

    protected virtual void OnNodeFrameGUI(IGraphNode node, bool highlight)
    {
        if (mContextNode == node)
        {
            GUI.Label(node.ClipRect, "", highlight ? "flow node 1 on" : "flow node 1");
        }
        else
        {
            GUI.Label(node.ClipRect, "", highlight ? "flow node 0 on" : "flow node 0");
        }
    }

    void PaintLinkNode()
    {
        if (mLinkNodeStart == null)
        {
            if (mShowLinkPanel)
            {
                mShowLinkPanel = false;
                OnNewNodePanelGUIEnd();
            }
            return;
        }
        Vector2 p0, p1;
        GetNodeSocketPos(mLinkNodeLayer, mLinkMask, mLinkNodeStart, null, out p0, out p1);
        if (mShowLinkPanel)
            p1 = mPanelPosition + mViewOffset;
        BezierConnect(LinkNodeLayer, LinkMask, p0, p1, GetLinkColor(mLinkNodeStart, null, mLinkNodeLayer), GetLinkWidth(mLinkNodeStart, null, mLinkNodeLayer));
        if ((!mShowLinkPanel || !mPanelRect.Contains(Event.current.mousePosition)) && Event.current.button == 1 && Event.current.type == EventType.mouseUp)
        {
            mLinkNodeStart = null;
            return;
        }
        bool click = Event.current.button == 0 && Event.current.type == EventType.mouseUp;
        if(!mShowLinkPanel && click)
        {
            if(mRaycastNode == null)
            {
                mShowLinkPanel = true;
                mPanelPosition = Event.current.mousePosition - mViewOffset;
            }
            else if(TryConnectNode(mLinkNodeLayer, mLinkMask, mGraph.IndexOf(mLinkNodeStart), mGraph.IndexOf(mRaycastNode)))
            {
                mLinkNodeStart = null;
            }
        }
        else if (mShowLinkPanel)
        {
            mShowLinkPanel = OnNewNodePanelGUI(mPanelPosition + mViewOffset, mLinkNodeStart, ref mPanelRect);
            if (!mShowLinkPanel)
            {
                mLinkNodeStart = null;
                OnNewNodePanelGUIEnd();
            }
        }
    }

    void PaintContextMenu()
    {
        if(mLinkNodeStart != null || !mInterceptMouse)
        {
            if (mContextMenu)
                OnContextMenuGUIEnd();
            mContextMenu = false;
            mContextNode = null;
            return;
        }
        if (mContextMenu)
        {
            mContextMenu = OnContextMenuGUI(mPanelPosition + mViewOffset, ref mPanelRect);
            if (!mContextMenu)
                OnContextMenuGUIEnd();
        }
        else
        {
            mContextNode = null;
        }
        if (mContextMenu && mPanelRect.Contains(Event.current.mousePosition))
        {
            return;
        }
        if (Event.current.type == EventType.mouseDown)
        {
            mContextMenu = Event.current.button == 1;
            if (mContextMenu)
            {
                mPanelPosition = Event.current.mousePosition - mViewOffset;
                mContextNode = mRaycastNode;
            }
        }
    }
    
    protected void ShowContextMenu(Vector2 position, IGraphNode contextNode)
    {
        mContextMenu = true;
        mContextNode = contextNode;
        mPanelPosition = position;
    }

    void ProcessSelectionRect()
    {
        if (mLinkNodeStart != null || mContextMenu)
            return;
        if (Event.current.type == EventType.mouseDown && Event.current.button == 0 && mRaycastNode == null)
        {
            catchDrag = true;
        }
        if (Event.current.type == EventType.mouseDrag && Event.current.button == 0)
        {
            if (!onNodeDrag && !doSelect && mRaycastNode == null && catchDrag)
            {
                if (!Event.current.shift)
                    selections.Clear();
                selectionRect.size = Vector2.zero;
                selectionRect.position = Event.current.mousePosition;
                doSelect = true;
            }
            if (!doSelect)
            {
                if (!onNodeDrag && mRaycastNode != null && selections.Count <= 1)
                {
                    selections.Clear();
                    SelectNode(mRaycastNode, Event.current.alt);
                }
                if (selections.Contains(mRaycastNode))
                {
                    foreach (IGraphNode nd in selections)
                    {
                        Vector2 p = nd.Position + mouseDeltaPos;
                        nd.Position = p;
                    }
                }
            }
            onNodeDrag = true;
        }
        if (Event.current.type == EventType.mouseUp && Event.current.button != 2)
        {
            if (!onNodeDrag)
            {
                if (!Event.current.shift)
                    selections.Clear();
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
        if(Event.current.type == EventType.mouseUp)
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
                selections.Add(mGraph[i]);
            }
        }
    }

    protected virtual void OnDeleteConnection(int graphLayer, int from,int to)
    {
        mGraph.RemovePath(graphLayer, from, to);
    }

    protected void CreateConnection(int graphLayer, IGraphNode from, int mask)
    {
        if (mLinkNodeStart != null)
            return;
        selections.Clear();
        mLinkNodeStart = from;
        mShowLinkPanel = false;
        mLinkMask = mask;
        mLinkNodeLayer = graphLayer;
    }

    protected virtual void GetNodeSocketPos(int graphLayer, int mask, IGraphNode from, IGraphNode to, out Vector2 p0, out Vector2 p1)
    {
        p0 = from == null ? Event.current.mousePosition : new Vector2(from.ClipRect.center.x, from.ClipRect.yMax);
        p1 = to == null ? Event.current.mousePosition : new Vector2(to.ClipRect.center.x, to.ClipRect.yMin);
    }

    protected virtual void OnGraphGUIEnd() { }

    protected virtual void OnNodeToolGUI(IGraphNode node, bool selected)
    {
        Vector2 p0, p1;
        GetNodeSocketPos(0, 0, node, null, out p0, out p1);
        Rect rect = new Rect();
        rect.size = new Vector2(30, 30);
        rect.center = p0;
        if (rect.Contains(Event.current.mousePosition) || mRaycastNode == node)
        {
            rect.size = new Vector2(20, 20);
            rect.center = p0;
            if (GUI.Button(rect, "", "OL Plus"))
            {
                CreateConnection(0, node, 0);
            }
        }
    }

    protected virtual bool EnableLinkNode(int graphLayer, int from, int to)
    {
        return true;
    }

    protected virtual Color GetLinkColor(IGraphNode from, IGraphNode to, int graphLayer)
    {
        return Color.white;
    }
    
    protected virtual float GetLinkWidth(IGraphNode from, IGraphNode to, int graphLayer)
    {
        return 2;
    }

    protected virtual bool EnableDestroyLink(int graphLayer,int from, int to)
    {
        return true;
    }

    protected virtual bool TryConnectNode(int graphLayer, int mask, int from, int to)
    {
        if (!mGraph.FindPath(graphLayer, to, from))
        {
            mGraph.AddPath(graphLayer, from, to);
            return true;
        }
        else
        {
            return false;
        }
    }

    protected virtual bool OnNewNodePanelGUI(Vector2 position, IGraphNode parent, ref Rect rect)
    {
        return false;
    }

    protected virtual void OnNewNodePanelGUIEnd() { }

    protected virtual void SelectNode(IGraphNode node, bool recursive)
    {
        selections.Add(node);
    }

    protected virtual void OnDeleteSelections()
    {
        foreach(IGraphNode node in selections)
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

    protected virtual XmlElement SerializeNode(XmlDocument doc, IGraphNode node)
    {
        return null;
    }

    protected virtual IGraphNode DeserializeNode(XmlElement element)
    {
        return null;
    }

    protected bool ImportFromXml(string fileName)
    {
        if (!File.Exists(fileName))
            return false;
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);

        XmlElement root = doc.FirstChild as XmlElement;
        XmlNodeList linkList = doc.GetElementsByTagName("Link-List");

        Graph<IGraphNode> newgraph = new Graph<IGraphNode>(linkList.Count);

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
                IGraphNode nd = null;
                if(ser != null)
                {
                    nd = DeserializeNode(ser);
                }
                if(nd == null)
                {
                    nd = new EmptyNode(asRoot);
                }
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
        XmlElement root = doc.CreateElement("BehaviourGraph");
        root.SetAttribute("root", graph.RootIndex.ToString());
        doc.AppendChild(root);
        XmlElement nodeList = doc.CreateElement("Node-List");
        root.AppendChild(nodeList);
        for (int i = 0; i < graph.NodeLength; i++)
        {
            XmlElement node = doc.CreateElement("Node");
            nodeList.AppendChild(node);
            node.SetAttribute("position", string.Format("({0},{1})", graph[i].Position.x, graph[i].Position.y));
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
