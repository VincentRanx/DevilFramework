using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Devil.AI;

namespace DevilEditor
{

    public class BehaviourTreeContextMenuGUI: PaintElement
    {
        public enum EMode
        {
            none,
            alter_node,
            new_node,
        }

        BehaviourTreeDesignerWindow mWindow;
        public bool Visible { get; private set; }
        public PaintElement Context { get; private set; }
        public EMode Mode { get; private set; }
        float mScrollOffset;
        bool mDragEnd;
        string mContextDecorator;
        string mContextService;
        int mScrollStartIndex = 0;
        int mScrollEndIndex = 100;
        string mSearchContext = "";

        Rect mScrollRect;

        public BehaviourTreeContextMenuGUI(BehaviourTreeDesignerWindow window) : base()
        {
            mWindow = window;
            SortOrder = 10;
        }

        public Vector2 AttachPoint
        {
            get
            {
                if (Mode == EMode.new_node)
                {
                    return new Vector2(GlobalRect.xMin + 85 * GlobalScale, GlobalRect.yMin);
                }
                else
                {
                    return new Vector2(GlobalRect.center.x, GlobalRect.yMin);
                }
            }
        }

        public void ShowContext(PaintElement context, string contextDecorator = null, string contextService = null)
        {
            if (EditorApplication.isPlaying)
                return;
            Mode = EMode.alter_node;
            Visible = true;
            Context = context;
            mContextDecorator = contextDecorator;
            mContextService = contextService;
            bool subContext = contextDecorator != null || contextService != null;
            Rect rect = new Rect();
            rect.size = new Vector2(subContext ? 400 : 200, 250);
            Vector2 gsize = rect.size * GlobalScale;
            Vector2 delta = Vector2.zero;
            if (mWindow.GlobalMousePosition.y + gsize.y > mWindow.RootCanvas.GlobalRect.height)
                delta.y = -rect.size.y;
            if (mWindow.GlobalMousePosition.x + gsize.x > mWindow.RootCanvas.GlobalRect.width)
                delta.x = -rect.size.x;
            rect.position = Parent.CalculateLocalPosition(mWindow.GlobalMousePosition) + delta;
            LocalRect = rect;
            mWindow.SelectNodes((x) => x == context);
            mDragEnd = true;
        }

        public void NewNode(PaintElement context)
        {
            if (EditorApplication.isPlaying)
                return;
            Mode = EMode.new_node;
            Visible = true;
            Context = context;
            Rect rect = new Rect();
            rect.size = new Vector2(370, 70 * 3 + 10); // 170 + 200
            rect.position = Parent.CalculateLocalPosition(mWindow.GlobalMousePosition) - new Vector2(85,0);
            LocalRect = rect;
            mDragEnd = true;
        }

        public void Hide()
        {
            Visible = false;
            Context = null;
            Mode = EMode.none;
            mContextDecorator = null;
            mContextService = null;
        }

        public override void OnGUI(Rect clipRect)
        {
            if (!Visible)
                return;
            GUI.Label(GlobalRect, "", "sv_iconselector_back");
            //GUI.Label(GlobalRect, "", "flow node 0 on");
            if (Mode == EMode.alter_node)
            {
                OnContextDecoratorGUI();
                OnDecoratorsListGUI();
            }
            else if(Mode == EMode.new_node)
            {
                OnNewNodeGUI();
            }
            GUI.Label(GlobalRect, "", "Icon.OutlineBorder");
            if (!Visible)
            {
                Hide();
            }
        }

        void OnContextDecoratorGUI()
        {
            string title = mContextDecorator ?? mContextService;
            if (title == null)
                return;
            Vector2 tsize = new Vector2(200 * GlobalScale, 25 * GlobalScale);
            Vector2 dsize = new Vector2(200 * GlobalScale, 20 * GlobalScale);

            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12f * GlobalScale);
            Rect r = new Rect();
            r.size = tsize;
            r.position = GlobalRect.position;
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = string.Format(
                mContextDecorator != null ? "<color=yellow><b>{0}</b> ?</color>" : "<color=yellow><b><i>{0}</i></b></color>", title);
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(GlobalRect.xMin + tsize.x, GlobalRect.yMin), 
                new Vector3(GlobalRect.xMin + tsize.x, GlobalRect.yMax));
        }

        void BeginScroll(Rect rect)
        {
            mScrollRect = rect;
            GUI.BeginClip(rect);
        }

        void EndScroll(float ymax)
        {
            if (mDragEnd)
            {
                float offmin = Mathf.Min(mScrollRect.height - (ymax - mScrollOffset), 0);
                if (mScrollOffset > 0)
                {
                    mScrollOffset = Mathf.MoveTowards(mScrollOffset, 0, Mathf.Max(1, mScrollOffset * 0.1f));
                }
                else if (mScrollOffset < offmin)
                {
                    mScrollOffset = Mathf.MoveTowards(mScrollOffset, offmin, Mathf.Max(1, (offmin - mScrollOffset) * 0.1f));
                }
            }
            GUI.EndClip();
        }

        bool OnSearchFieldGUI(Rect rect)
        {
            bool search = false;
            if (GlobalRect.height > 30)
            {
                Rect r = new Rect();
                r.size = new Vector2(rect.size.x - 20, 20);
                r.position = rect.position;
                mSearchContext = GUI.TextField(r, mSearchContext, "SearchTextField").ToLower();
                r.size = new Vector2(20, 20);
                r.position = new Vector2(GlobalRect.xMax - 20, GlobalRect.yMin + 2);
                if (GUI.Button(r, "", "SearchCancelButton"))
                    mSearchContext = "";
                search = !string.IsNullOrEmpty(mSearchContext);
                return true;
            }
            else
            {
                return false;
            }
        }

        void OnDecoratorsListGUI()
        {
            Vector2 tsize = new Vector2(200 * GlobalScale, 25 * GlobalScale);
            Vector2 dsize = new Vector2(200 * GlobalScale, 20 * GlobalScale);
            BehaviourNodeGUI node = Context as BehaviourNodeGUI;
            Rect r = new Rect();
            float delta = 0;
            r.size = new Vector2(tsize.x, 22);
            r.position = new Vector2(GlobalRect.xMax - tsize.x, GlobalRect.yMin + 2);
            if (OnSearchFieldGUI(r))
            {
                delta += 22;
            }
            bool search = !string.IsNullOrEmpty(mSearchContext);

            r.size = new Vector2(tsize.x, GlobalRect.height - delta);
            r.position = new Vector2(GlobalRect.xMax - tsize.x, GlobalRect.yMin + 20);
            BeginScroll(r);

            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12f * GlobalScale);
            r.size = tsize;
            r.position = new Vector2(0, mScrollOffset);
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = "<b>NEW CONDITION</b>";
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;

            Rect btn = new Rect();
            btn.size = new Vector2(20, 20);
            bool editmode = dsize.y > 13 && node != null; // 可编辑装饰节点

            // decorators
            r.size = dsize;
            for (int i = 0; i < mWindow.Decorators.Count; i++)
            {
                if(search && !mWindow.Decorators[i].ToLower().Contains(mSearchContext))
                {
                    continue;
                }
                if (r.yMax < 0 || r.yMin > GlobalRect.height)
                {
                    r.position += Vector2.up * dsize.y;
                    continue;
                }
                bool owns = node.decorators.Contains(mWindow.Decorators[i]);
                bool inter = r.Contains(Event.current.mousePosition);
                Installizer.contentContent.text = string.Format("<color={0}>{1} <b>?</b></color>",
                   inter ? "yellow" : "white", mWindow.Decorators[i]);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - 12, r.center.y);
                if (inter && editmode && GUI.Button(btn, "", owns ? "WinBtnCloseActiveMac" : "WinBtnMaxActiveMac"))
                {
                    Visible = false;
                    if (owns)
                        node.decorators.Remove(mWindow.Decorators[i]);
                    else
                        node.decorators.Add(mWindow.Decorators[i]);
                    node.Resize();
                }
                r.position += Vector2.up * dsize.y;
            }

            r.size = tsize;
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = "<b>NEW SERVICE</b>";
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;

            // services
            r.size = dsize;
            for (int i = 0; i < mWindow.Services.Count; i++)
            {
                if (search && !mWindow.Services[i].ToLower().Contains(mSearchContext))
                {
                    continue;
                }
                if (r.yMax < 0 || r.yMin > GlobalRect.height)
                {
                    r.position += Vector2.up * dsize.y;
                    continue;
                }
                bool owns = node.services.Contains(mWindow.Services[i]);
                bool inter = r.Contains(Event.current.mousePosition);
                Installizer.contentContent.text = string.Format("<color={0}><i>{1}</i></color>",
                   inter ? "yellow" : "white", mWindow.Services[i]);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - 12, r.center.y);
                if (inter && editmode && GUI.Button(btn, "", owns ? "WinBtnCloseActiveMac" : "WinBtnMaxActiveMac"))
                {
                    Visible = false;
                    if (owns)
                        node.services.Remove(mWindow.Services[i]);
                    else
                        node.services.Add(mWindow.Services[i]);
                    node.Resize();
                }
                r.position += Vector2.up * dsize.y;
            }
            EndScroll(r.yMax);
        }

        void OnNewNodeGUI()
        {
            Rect rect = new Rect();
            rect.size = new Vector2(160, 70) * GlobalScale;
            float h = rect.size.y;
            rect.position = new Vector2(GlobalRect.xMin + 5 * GlobalScale, GlobalRect.yMin + 5 * GlobalScale);
            DrawNode(EBTNodeType.selector, rect);
            rect.position += Vector2.up * h;
            DrawNode(EBTNodeType.sequence, rect);
            rect.position += Vector2.up * h;
            DrawNode(EBTNodeType.parallel, rect);
            float x = GlobalRect.xMin + 170 * GlobalScale;
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(x, GlobalRect.yMin), new Vector3(x, GlobalRect.yMax));
            OnTaskList();
        }

        void DrawNode(EBTNodeType type, Rect rect)
        {
            GUI.Label(rect, "", "flow node 0");
            Rect r = new Rect();
            r.size = new Vector2(rect.width - 30 * GlobalScale, 15 * GlobalScale);
            r.center = new Vector2(rect.center.x, rect.yMin + r.size.y * 0.5f);
            GUI.Label(r, "", "textarea");
            r.center = new Vector2(rect.center.x, rect.yMax - r.size.y * 0.5f);
            GUI.Label(r, "", "textarea");
            r.size = new Vector2(rect.width - 10 * GlobalScale, rect.height - 30 * GlobalScale);
            r.center = rect.center;
            Installizer.titleStyle.fontSize = (int)Mathf.Max(1, 20 * GlobalScale);
            switch (type)
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
                default:
                    break;
            }
            Installizer.titleContent.text = type.ToString().ToUpper();
            if (GUI.Button(rect, Installizer.titleContent, Installizer.titleStyle))
            {
                Visible = false;
                mWindow.AddChild(Context, type, null, new Vector2(LocalRect.xMin + 85, LocalRect.yMin));
            }
        }

        void OnTaskList()
        {
            Vector2 tsize = new Vector2(200 * GlobalScale, 25 * GlobalScale);
            Vector2 dsize = new Vector2(200 * GlobalScale, 20 * GlobalScale);
            BehaviourNodeGUI node = Context as BehaviourNodeGUI;
            Rect r = new Rect();
            float delta = 0;
            r.size = new Vector2(tsize.x, 22);
            r.position = new Vector2(GlobalRect.xMax - tsize.x, GlobalRect.yMin + 2);
            if (OnSearchFieldGUI(r))
            {
                delta += 22;
            }
            bool search = !string.IsNullOrEmpty(mSearchContext);

            r.size = new Vector2(tsize.x, GlobalRect.height - delta);
            r.position = new Vector2(GlobalRect.xMax - tsize.x, GlobalRect.yMin + 20);
            BeginScroll(r);

            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12f * GlobalScale);
            r.size = tsize;
            r.position = new Vector2(0, mScrollOffset);
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = "<b>NEW PLUGIN</b>";
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;

            Rect btn = new Rect();
            btn.size = new Vector2(20, 20);
            bool editmode = dsize.y > 13 && node != null; // 可编辑装饰节点

            // plugins
            r.size = dsize;
            for (int i = 0; i < mWindow.Plugins.Count; i++)
            {
                if (search && !mWindow.Plugins[i].ToLower().Contains(mSearchContext))
                {
                    continue;
                }
                if (r.yMax < 0 || r.yMin > GlobalRect.height)
                {
                    r.position += Vector2.up * dsize.y;
                    continue;
                }
                bool inter = r.Contains(Event.current.mousePosition);
                Installizer.contentContent.text = string.Format("<color={0}>{1}</color>",
                   inter ? "yellow" : "white", mWindow.Plugins[i]);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - 12, r.center.y);
                if (inter && editmode && GUI.Button(btn, "", "WinBtnMaxActiveMac"))
                {
                    Visible = false;
                    mWindow.AddChild(Context, EBTNodeType.plugin, mWindow.Plugins[i], new Vector2(LocalRect.xMin + 85, LocalRect.yMin));
                }
                r.position += Vector2.up * dsize.y;
            }

            r.size = tsize;
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = "<b>NEW TASK</b>";
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;

            // tasks
            r.size = dsize;
            for (int i = 0; i < mWindow.Tasks.Count; i++)
            {
                if (search && !mWindow.Tasks[i].ToLower().Contains(mSearchContext))
                {
                    continue;
                }
                if (r.yMax < 0 || r.yMin > GlobalRect.height)
                {
                    r.position += Vector2.up * dsize.y;
                    continue;
                }
                bool inter = r.Contains(Event.current.mousePosition);
                Installizer.contentContent.text = string.Format("<color={0}>{1}</color>",
                   inter ? "yellow" : "white", mWindow.Tasks[i]);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - 12, r.center.y);
                if (inter && editmode && GUI.Button(btn, "", "WinBtnMaxActiveMac"))
                {
                    Visible = false;
                    mWindow.AddChild(Context, EBTNodeType.task, mWindow.Tasks[i], new Vector2(LocalRect.xMin + 85, LocalRect.yMin));
                }
                r.position += Vector2.up * dsize.y;
            }
            EndScroll(r.yMax);
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            return Visible;
        }

        public override bool InteractDragBegin(EMouseButton button, Vector2 mousePosition)
        {
            mDragEnd = false;
            return Visible;
        }

        public override bool InteractDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            mDragEnd = true;
            return Visible;
        }

        public override bool InteractDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if(Visible && button == EMouseButton.left)
            {
                mScrollOffset += mouseDelta.y * GlobalScale;
            }
            return Visible;
        }

    }
}