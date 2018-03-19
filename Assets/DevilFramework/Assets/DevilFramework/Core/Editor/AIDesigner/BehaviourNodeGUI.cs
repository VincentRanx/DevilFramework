using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Devil.AI;

namespace DevilEditor
{
    public class BehaviourNodeGUI : PaintElement
    {
        public int BTExecutionOrder { get; set; }
        public EBTNodeType BTNodeType { get; set; }
        public List<string> decorators = new List<string>();
        public List<string> services = new List<string>();
        public int BTNodeId { get; set; }
        public string BTNodeName { get; set; }
        public EBTTaskState BTRuntimeState { get; set; }

        bool mUseEvents;
        public bool Selected { get; set; }
        BehaviourTreeDesignerWindow mWindow;
        bool mDrag;
        float mDecoratorHeight;
        string mInteractDecorator;
        string mInteractService;

        public BehaviourNodeGUI(BehaviourTreeDesignerWindow window) : base()
        {
            mWindow = window;
        }

        public Vector2 CalculateLocalSize()
        {
            Installizer.titleStyle.fontSize = 20;
            Vector2 size = Installizer.SizeOfTitle(BTNodeName);
            Installizer.contentStyle.fontSize = 12;
            size.x += 7;
            size.y += 7;
            Vector2 tmp;
            for(int i = 0; i < decorators.Count; i++)
            {
                tmp = Installizer.SizeOfContent(decorators[i]);
                mDecoratorHeight = tmp.y;
                size.x = Mathf.Max(tmp.x + 5, size.x);
                size.y += mDecoratorHeight;
            }
            for(int i = 0; i < services.Count; i++)
            {
                tmp = Installizer.SizeOfContent(services[i]);
                mDecoratorHeight = tmp.y;
                size.x = Mathf.Max(tmp.x + 5, size.x);
                size.y += mDecoratorHeight;
            }
            size.y += 30;
            return size;
        }

        public override void OnGUI(Rect clipRect)
        {
            if (Application.isPlaying)
            {

            }
            string style;
            if (Application.isPlaying && mWindow.Runner)
            {
                if (BTRuntimeState == EBTTaskState.success)
                {
                    style = "flow node 3 on";
                }
                else if (BTRuntimeState == EBTTaskState.faild)
                {
                    style = "flow node 6 on";
                }
                else if (BTRuntimeState == EBTTaskState.running)
                {
                    style = "flow node 1 on";
                }
                else
                {
                    style = "flow node 0 on";
                }
            }
            else
            {
                style = Selected ? "flow node 0 on" : "flow node 0";
            }
            GUI.Label(GlobalRect, "", style);
            OnNodeGUI();
            OnSocketGUI();
        }

        public void Resize()
        {
            Rect localr = LocalRect;
            localr.size = CalculateLocalSize();
            LocalRect = localr;
        }

        protected virtual void OnNodeGUI()
        {
            mInteractDecorator = null;
            mInteractService = null;
            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12 * GlobalScale);
            Rect r = new Rect();
            float w = (LocalRect.width - 10) * GlobalScale;
            float h = mDecoratorHeight * GlobalScale;
            float y0 = GlobalRect.yMin + 15 * GlobalScale;
            float x0 = GlobalRect.center.x - w * 0.5f;
            bool editmode = !mWindow.ContextMenu.Visible && mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.none && h > 13;
            // decorators
            r.size = new Vector2(w, h * decorators.Count);
            r.position = new Vector2(x0, y0);
            GUI.Label(r, "", "ProjectBrowserIconAreaBg");
            r.size = new Vector2(w, h);
            Rect btn = new Rect();
            btn.size = new Vector2(20, 20);
            float bx = x0 + w - 10;
            for (int i = 0; i < decorators.Count; i++)
            {
                r.position = new Vector2(x0, y0 + i * h);
                btn.center = new Vector2(bx, r.center.y);
                bool inter = editmode && r.Contains(mWindow.GlobalMousePosition);
                if (inter)
                    mInteractDecorator = decorators[i];
                Installizer.contentContent.text = string.Format("<color={0}>{1} <b>?</b></color>", inter ? "yellow" : "white", decorators[i]);
                if (inter && GUI.Button(btn, "", "WinBtnCloseActiveMac"))
                {
                    decorators.RemoveAt(i);
                    Resize();
                    break;
                }
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            }
            // services
            y0 = GlobalRect.yMax - 15 * GlobalScale - services.Count * h;
            r.size = new Vector2(w, h * services.Count);
            r.position = new Vector2(x0, y0);
            GUI.Label(r, "", "ProjectBrowserIconAreaBg");
            r.size = new Vector2(w, h);
            for (int i = 0; i < services.Count; i++)
            {
                r.position = new Vector2(x0, y0 + i * h);
                btn.center = new Vector2(bx, r.center.y);
                bool inter = editmode && r.Contains(mWindow.GlobalMousePosition);
                if (inter)
                    mInteractService = services[i];
                Installizer.contentContent.text = string.Format("<i><color={0}>{1}</color></i>", inter ? "yellow" : "white", services[i]);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                if (inter && GUI.Button(btn, "", "WinBtnCloseActiveMac"))
                {
                    services.RemoveAt(i);
                    Resize();
                    break;
                }
            }
            // content
            r.size = new Vector2(w, GlobalRect.height - 30 * GlobalScale - h * (decorators.Count + services.Count));
            y0 = GlobalRect.yMin + 15 * GlobalScale + decorators.Count * h;
            r.position = new Vector2(x0, y0);
            switch (BTNodeType)
            {
                case EBTNodeType.selector:
                    GUI.Label(r, "", "flow node 1");
                    break;
                case EBTNodeType.sequence:
                    GUI.Label(r, "", "flow node 2");
                    break;
                case EBTNodeType.parallel:
                    GUI.Label(r, "", "flow node 5");
                    break;
                case EBTNodeType.task:
                    GUI.Label(r, "", "flow node 3");
                    break;
                default:
                    GUI.Label(r, "", "flow node 0");
                    break;
            }
            //GUI.Label(r, "", "Icon.OutlineBorder");
            Installizer.titleStyle.fontSize = (int)Mathf.Max(1, 20 * GlobalScale);
            Installizer.titleContent.text = BTNodeName;
            GUI.Label(r, Installizer.titleContent, Installizer.titleStyle);

            r.size = new Vector2(GlobalRect.width, 20);
            r.position = new Vector2(GlobalRect.xMin, GlobalRect.yMin - 20);
            GUI.Label(r, string.Format("<size=12><color={0}>[{1}]</color></size>",
                BTExecutionOrder == 0 ? "red" : "green",
                BTExecutionOrder == 0 ? "-" : BTExecutionOrder.ToString()));
        }

        bool CanLinkBetween(BehaviourNodeGUI from, BehaviourNodeGUI to)
        {
            if (from == null || to == null || from == to)
                return false;
            if (mWindow.TreeGraph.FindPath(0, to, from))
                return false;
            return true;
        }

        void ModifyThisAsParent()
        {
            PaintElement ele = mWindow.GetNodeBTParent(mWindow.EditTarget);
            if (ele != null)
            {
                mWindow.TreeGraph.RemovePath(0, ele, mWindow.EditTarget);
            }
            mWindow.TreeGraph.AddPath(0, this, mWindow.EditTarget);
            mWindow.BeginEditNode(null, BehaviourTreeDesignerWindow.ENodeEditMode.none);
            mWindow.RebuildExecutionOrder();
        }

        void ModifyThisAsChild()
        {
            mWindow.TreeGraph.AddPath(0, mWindow.EditTarget, this);
            mWindow.BeginEditNode(null, BehaviourTreeDesignerWindow.ENodeEditMode.none);
            mWindow.RebuildExecutionOrder();
        }

        protected virtual void OnSocketGUI()
        {
            Rect rect = new Rect();
            rect.size = new Vector2(LocalRect.width - 30, 15) * GlobalScale;
            if(rect.size.y > 3)
            {
                bool editmode = !mWindow.ContextMenu.Visible && mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.none;
                rect.center = new Vector2(GlobalRect.center.x, GlobalRect.yMax - rect.size.y * 0.5f);
                bool inrect = rect.Contains(mWindow.GlobalMousePosition);
                if(mWindow.EditTarget == this && mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.modify_child)
                {
                    GUI.Label(rect, "", "flow node 0 on");
                }
                else if (mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.modify_parent
                    && inrect && CanLinkBetween(this, mWindow.EditTarget as BehaviourNodeGUI))
                {
                    if (GUI.Button(rect, "", "flow node 0 on"))
                        ModifyThisAsParent();
                }
                else if (editmode)
                {
                    if (GUI.Button(rect, "", inrect ? "flow node 0 on" : "textarea"))
                    {
                        mWindow.BeginEditNode(this, BehaviourTreeDesignerWindow.ENodeEditMode.modify_child);
                    }
                }
                else
                {
                    GUI.Label(rect, "", "textarea");
                }

                rect.center = new Vector2(GlobalRect.center.x, GlobalRect.yMin + rect.size.y * 0.5f);
                inrect = rect.Contains(mWindow.GlobalMousePosition);
                if (mWindow.EditTarget == this && mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.modify_parent)
                {
                    GUI.Label(rect, "", "flow node 0 on");
                }
                else if (mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.modify_child
                    && inrect && mWindow.TreeGraph.GetParentCount(0, this) == 0)
                {
                    if (GUI.Button(rect, "", "flow node 0 on"))
                        ModifyThisAsChild();
                }
                else if (editmode)
                {
                    if (GUI.Button(rect, "", inrect ? "flow node 0 on" : "textarea"))
                    {
                        mWindow.BeginEditNode(this, BehaviourTreeDesignerWindow.ENodeEditMode.modify_parent);
                    }
                }
                else
                {
                    GUI.Label(rect, "", "textarea");
                }
                Rect btn = new Rect();
                btn.size = new Vector2(20, 20);
                btn.center = new Vector2(rect.center.x, rect.yMin - 10);
                if (editmode && (rect.Contains(mWindow.GlobalMousePosition) || btn.Contains(mWindow.GlobalMousePosition)))
                {
                    PaintElement parent = mWindow.GetNodeBTParent(this);
                    if (parent != null && GUI.Button(btn, "", "WinBtnCloseActiveMac"))
                    {
                        mWindow.TreeGraph.RemovePath(0, parent, this);
                        mWindow.RebuildExecutionOrder();
                    }
                }
            }
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePositoin)
        {
            if (mUseEvents)
            {
                mUseEvents = false;
            }
            else if (button == EMouseButton.right)
            {
                mWindow.BeginEditNode(null, BehaviourTreeDesignerWindow.ENodeEditMode.none);
                mWindow.ContextMenu.ShowContext(this, mInteractDecorator, mInteractService);
            }
            else if (mWindow.ContextMenu.Visible)
            {
                mWindow.ContextMenu.Hide();
                return true;
            }
            else if (Event.current.control)
            {
                mWindow.SelectNodes((x) => x.Selected || x == this);
            }
            else
            {
                mWindow.SelectNodes((x) => x == this);
            }
            return true;
        }

        public override bool InteractDragBegin(EMouseButton button, Vector2 mousePositoin)
        {
            if (button == EMouseButton.left)
            {
                mDrag = true;
                return true;
            }
            return false;
        }

        public override bool InteractDrag(EMouseButton button, Vector2 mousePositoin, Vector2 mouseDelta)
        {
            if (EditorApplication.isPlaying)
                return false;
            if(mDrag && button == EMouseButton.left)
            {
                Rect rect = LocalRect;
                float scale = (GlobalScale > 0 ? GlobalScale : 1);
                rect.position += mouseDelta / scale;
                LocalRect = rect;
                if (Selected)
                {
                    mWindow.EditNodes((x) =>
                    {
                        if (x.Selected && x != this)
                        {
                            Rect local = x.LocalRect;
                            local.position += mouseDelta / scale;
                            x.LocalRect = local;
                        }
                    });
                }
                return true;
            }
            return false;
        }

        public override bool InteractDragEnd(EMouseButton button, Vector2 mousePositoin)
        {
            if (button == EMouseButton.left)
            {
                mDrag = false;
                mWindow.RebuildExecutionOrder();
                return true;
            }
            return false;
        }

        public BehaviourTreeAsset.BTNodeInfo InstantNodeInfo()
        {
            BehaviourTreeAsset.BTNodeInfo node = new BehaviourTreeAsset.BTNodeInfo();
            node.m_Name = BTNodeName;
            node.m_Id = BTNodeId;
            node.m_Pos = new Vector2(LocalRect.center.x, LocalRect.yMin);
            node.m_Type = BTNodeType;
            node.m_Services = services.ToArray();
            node.m_Decorators = decorators.ToArray();
            return node;
        }
    }
}