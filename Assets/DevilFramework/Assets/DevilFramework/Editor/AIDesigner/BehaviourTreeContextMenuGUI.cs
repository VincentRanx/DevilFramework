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
        string mSearchContext = "";
        float mMinTaskWidth = 200;
        float mMinDecoratorWidth = 200;
        BehaviourMeta mRaycastMeta;
        bool mInited = false;
        
        float mScrollOffset;
        Rect mScrollRect;

        Rect mScrollRectL;
        float mScrollOffsetL;

        bool mDragEnd;

        BehaviourInputProperty mFocusProperty;
        BehaviourInputProperty mRaycastProperty;

        BehaviourNodeGUI.Decorator mFocusDecorator;
        BehaviourNodeGUI.Decorator mRaycastDecorator;

        bool mResizeNode;

        public BehaviourTreeContextMenuGUI(BehaviourTreeDesignerWindow window) : base()
        {
            mWindow = window;
            SortOrder = 10;
           
        }

        void InitSize()
        {
            if (mInited)
                return;
            mInited = true;
            Installizer.contentStyle.fontSize = (int)BehaviourNodeGUI.FONT_SIZE;
            for (int i = 0; i < Installizer.BTTasks.Count; i++)
            {
                Vector2 size = Installizer.SizeOfContent(Installizer.BTTasks[i].DisplayName);
                mMinTaskWidth = Mathf.Max(mMinTaskWidth, size.x);
            }
            for (int i = 0; i < Installizer.BTControllers.Count; i++)
            {
                Vector2 size = Installizer.SizeOfContent(Installizer.BTControllers[i].DisplayName);
                mMinTaskWidth = Mathf.Max(mMinTaskWidth, size.x);
            }
            for (int i = 0; i < Installizer.BTConditions.Count; i++)
            {
                Vector2 size = Installizer.SizeOfContent(Installizer.BTConditions[i].DisplayName);
                mMinDecoratorWidth = Mathf.Max(mMinDecoratorWidth, size.x);
            }
            for (int i = 0; i < Installizer.BTServices.Count; i++)
            {
                Vector2 size = Installizer.SizeOfContent(Installizer.BTServices[i].DisplayName);
                mMinDecoratorWidth = Mathf.Max(mMinDecoratorWidth, size.x);
            }
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

        public void ShowContext(BehaviourNodeGUI context)
        {
            if (EditorApplication.isPlaying || context == null)
                return;
            Mode = EMode.alter_node;
            Visible = true;
            Context = context;
            Rect rect = new Rect();
            rect.size = new Vector2(mMinDecoratorWidth + mMinTaskWidth, 270);
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
            rect.size = new Vector2(mMinTaskWidth, 270); // 170 + 200
            rect.position = Parent.CalculateLocalPosition(mWindow.GlobalMousePosition) - new Vector2(mMinTaskWidth * 0.5f, 0);
            LocalRect = rect;
            mDragEnd = true;
        }

        public void Hide()
        {
            Visible = false;
            Context = null;
            Mode = EMode.none;
            mRaycastProperty = null;
            mFocusProperty = null;
            mFocusDecorator = null;
            mRaycastDecorator = null;
        }

        void BeginScroll(Rect rect, ref Rect scrollRect)
        {
            scrollRect = rect;
            GUI.BeginClip(rect);
        }

        void EndScroll(float ymax, ref Rect scrollRect, ref float scrollOffset)
        {
            if (mDragEnd)
            {
                float offmin = Mathf.Min(scrollRect.height - (ymax - scrollOffset), 0);
                if (scrollOffset > 0)
                {
                    scrollOffset = Mathf.MoveTowards(scrollOffset, 0, Mathf.Max(1, scrollOffset * 0.1f));
                }
                else if (scrollOffset < offmin)
                {
                    scrollOffset = Mathf.MoveTowards(scrollOffset, offmin, Mathf.Max(1, (offmin - scrollOffset) * 0.1f));
                }
            }
            GUI.EndClip();
        }

        public override void OnGUI(Rect clipRect)
        {
            if (!Visible)
                return;
            mResizeNode = false;
            InitSize();
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
            if(mResizeNode)
            {
                BehaviourNodeGUI node = Context as BehaviourNodeGUI;
                if (node != null)
                    node.Resize();
                mResizeNode = false;
            }
            if (!Visible)
            {
                Hide();
            }
        }

        // 附加到对象的装饰节点
        void OnAttachedDecoratorListGUI(ref Rect r, Vector2 tsize, Vector2 dsize, BehaviourNodeGUI node, List<BehaviourNodeGUI.Decorator> decorators)
        {
            Rect btn = new Rect();
            btn.size = Vector2.one * Mathf.Clamp(20 * GlobalScale, 15, 30);
            for (int i = 0; i < decorators.Count; i++)
            {
                Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
                GUI.Label(r, "", "flow overlay box");
                bool inter = r.Contains(Event.current.mousePosition);
                if (inter || mFocusDecorator == decorators[i])
                    GUI.Label(r, "", "Icon.ClipSelected");
                if(inter)
                    mRaycastDecorator = decorators[i];
                Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12f * GlobalScale);
                Installizer.contentContent.text = string.Format("<b>{0}</b>", decorators[i].BTMeta.DisplayName);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - btn.size.x * 0.5f, r.center.y);
                r.position += Vector2.up * tsize.y;
                if (tsize.y > 8 && inter && GUI.Button(btn, "", "WinBtnCloseActiveMac"))
                {
                    node.RemoveDecorator(decorators[i].BTMeta);
                    mResizeNode = true;
                    break;
                }
                if (mFocusDecorator == decorators[i])
                {
                    OnPropertiesList(ref r, mFocusDecorator, tsize, dsize);
                }
            }
        }

        void OnPropertiesList(ref Rect r, BehaviourNodeGUI.Decorator decorator, Vector2 tsize, Vector2 dsize)
        {
            BehaviourInputProperty[] properties = decorator.Properties;
            if (properties.Length == 0)
                return;
            float delta = 5 * GlobalScale;
            r.position += Vector2.up * delta;
            Rect h = new Rect();
            float x0 = 80 * GlobalScale + 1;
            Installizer.contentStyle.alignment = TextAnchor.MiddleLeft;
            bool dirty = false;
            for (int i = 0; i < properties.Length; i++)
            {
                if (i > 0)
                    r.position += Vector2.up * (dsize.y + delta);
                h.size = new Vector2(x0, dsize.y);
                h.position = new Vector2(r.xMin + delta, r.yMin);
                Installizer.contentContent.text = properties[i].PropertyName;
                GUI.Label(h, Installizer.contentContent, Installizer.contentStyle);
                h.size = new Vector2(r.width - x0 - delta, dsize.y);
                h.position = new Vector2(x0, r.yMin);
                GUI.Label(h, "", "textfield");// "TL LoopSection");
                if (h.Contains(Event.current.mousePosition))
                    mRaycastProperty = properties[i];
                if (mFocusProperty == properties[i])
                {
                    string str = GUI.TextField(h, properties[i].InputData, Installizer.contentStyle);
                    dirty |= str != properties[i].InputData;
                    properties[i].InputData = str;
                }
                else
                {
                    GUI.Label(h, properties[i].InputData, Installizer.contentStyle);
                }
            }
            r.position += Vector2.up * tsize.y;
            if (dirty)
            {
                decorator.UpdatePropertiesInfo();
                mResizeNode = true;
            }
        }

        void OnContextDecoratorGUI()
        {
            mRaycastProperty = null;
            mRaycastDecorator = null;
            float l = Mathf.Max(mMinTaskWidth, mMinDecoratorWidth);
            float w = l + mMinDecoratorWidth;
            if (w > LocalRect.width)
            {
                Rect rect = new Rect();
                rect.size = new Vector2(w, LocalRect.height);
                rect.position = new Vector2(LocalRect.center.x - w * 0.5f, LocalRect.yMin);
                LocalRect = rect;
            }
            Vector2 tsize = new Vector2(l * GlobalScale, 30 * GlobalScale);
            Vector2 dsize = new Vector2(l * GlobalScale - 2, 20 * GlobalScale);

            BehaviourNodeGUI node = Context as BehaviourNodeGUI;
            TextAnchor align = Installizer.contentStyle.alignment;

            Rect r = new Rect();
            r.size = new Vector2(tsize.x, GlobalRect.height);
            r.position = GlobalRect.position;
            BeginScroll(r, ref mScrollRectL);

            //
            r.size = tsize;
            r.position = new Vector2(0, mScrollOffsetL);
            BehaviourInputProperty[] properties;
            // conditions
            OnAttachedDecoratorListGUI(ref r, tsize, dsize, node, node.conditions);
            // self
            properties = node.Self.Properties;
            r.size = new Vector2(tsize.x, tsize.y + properties.Length * tsize.y + 5 * GlobalScale);
            GUI.Label(r, "", node.Self.BTMeta.FrameStyle);
            r.size = tsize;
            Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12f * GlobalScale);
            Installizer.contentContent.text = string.Format("<b>{0} ({1})</b>", node.Self.BTMeta.DisplayName, node.Self.BTId);
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * (tsize.y + 5);
            OnPropertiesList(ref r, node.Self, tsize, dsize);

            //services
            OnAttachedDecoratorListGUI(ref r, tsize, dsize, node, node.services);

            EndScroll(r.yMax, ref mScrollRectL, ref mScrollOffsetL);

            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(GlobalRect.xMin + tsize.x, GlobalRect.yMin), 
                new Vector3(GlobalRect.xMin + tsize.x, GlobalRect.yMax));
            Installizer.contentStyle.alignment = align;
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
            mRaycastMeta = null;
            Vector2 tsize = new Vector2(mMinDecoratorWidth * GlobalScale, 30 * GlobalScale);
            Vector2 dsize = new Vector2(mMinDecoratorWidth * GlobalScale, 20 * GlobalScale);
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
            BeginScroll(r,ref mScrollRect);

            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12f * GlobalScale);
            r.size = tsize;
            r.position = new Vector2(0, mScrollOffset);
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = "<b>Condtions</b>";
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;

            Rect btn = new Rect();
            btn.size = new Vector2(20, 20);
            bool editmode = dsize.y > 13 && node != null; // 可编辑装饰节点

            // decorators
            r.size = dsize;
            for (int i = 0; i < Installizer.BTConditions.Count; i++)
            {
                if(search && !Installizer.BTConditions[i].SearchName.Contains(mSearchContext))
                {
                    continue;
                }
                if (r.yMax < 0 || r.yMin > GlobalRect.height)
                {
                    r.position += Vector2.up * dsize.y;
                    continue;
                }
                bool owns = node.GetDecorator(Installizer.BTConditions[i]) != null;
                if (owns)
                    continue;
                bool inter = r.Contains(Event.current.mousePosition);
                if (inter && editmode)
                {
                    GUI.Label(r, "", "Icon.ClipSelected");
                    mRaycastMeta = Installizer.BTConditions[i];
                }
                Installizer.contentContent.text = string.Format("<color={0}>{1} <b>?</b></color>",
                   inter ? "yellow" : "white", Installizer.BTConditions[i].DisplayName);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - 12, r.center.y);
                
                r.position += Vector2.up * tsize.y;
            }

            r.size = tsize;
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = "<b>Services</b>";
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;

            // services
            r.size = dsize;
            for (int i = 0; i < Installizer.BTServices.Count; i++)
            {
                if (search && !Installizer.BTServices[i].SearchName.Contains(mSearchContext))
                {
                    continue;
                }
                if (r.yMax < 0 || r.yMin > GlobalRect.height)
                {
                    r.position += Vector2.up * dsize.y;
                    continue;
                }
                bool owns = node.GetDecorator(Installizer.BTServices[i]) != null;
                if (owns)
                    continue;
                bool inter = r.Contains(Event.current.mousePosition);
                if (inter && editmode)
                {
                    GUI.Label(r, "", "Icon.ClipSelected");
                    mRaycastMeta = Installizer.BTServices[i];
                }
                Installizer.contentContent.text = string.Format("<color={0}>{1}</color>",
                   inter ? "yellow" : "white", Installizer.BTServices[i].DisplayName);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - 12, r.center.y);
                r.position += Vector2.up * dsize.y;
            }
            EndScroll(r.yMax, ref mScrollRect, ref mScrollOffset);
        }

        void OnNewNodeGUI()
        {
            if (mMinTaskWidth > LocalRect.width)
            {
                Rect rect = new Rect();
                rect.size = new Vector2(mMinTaskWidth, LocalRect.height);
                rect.position = new Vector2(LocalRect.center.x - mMinTaskWidth * 0.5f, LocalRect.yMin);
                LocalRect = rect;
            }
            OnTaskList();
        }
        
        void OnTaskList()
        {
            mRaycastMeta = null;
            Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
            Vector2 tsize = new Vector2(mMinTaskWidth * GlobalScale, 30 * GlobalScale);
            Vector2 dsize = new Vector2(mMinTaskWidth * GlobalScale, 20 * GlobalScale);
            PaintElement node = Context;
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
            BeginScroll(r, ref mScrollRect);

            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12f * GlobalScale);
            r.size = tsize;
            r.position = new Vector2(0, mScrollOffset);
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = "<b>Composites</b>";
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;

            Rect btn = new Rect();
            btn.size = new Vector2(20, 20);
            bool editmode = dsize.y > 13 && node != null; // 可编辑装饰节点

            // controllers
            r.size = dsize;
            for (int i = 0; i < Installizer.BTControllers.Count; i++)
            {
                if (search && !Installizer.BTControllers[i].SearchName.Contains(mSearchContext))
                {
                    continue;
                }
                if (r.yMax < 0 || r.yMin > GlobalRect.height)
                {
                    r.position += Vector2.up * dsize.y;
                    continue;
                }
                bool inter = r.Contains(Event.current.mousePosition);
                if (inter)
                {
                    GUI.Label(r, "", "Icon.ClipSelected");
                    mRaycastMeta = Installizer.BTControllers[i];//SelectionRect
                }
                Installizer.contentContent.text = string.Format("<color={0}>{1}</color>",
                   inter ? "yellow" : "white", Installizer.BTControllers[i].DisplayName);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - 12, r.center.y);
                r.position += Vector2.up * dsize.y;
            }

            r.size = tsize;
            GUI.Label(r, "", "flow overlay box");
            Installizer.contentContent.text = "<b>Tasks</b>";
            GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
            r.position += Vector2.up * tsize.y;

            // tasks
            r.size = dsize;
            for (int i = 0; i < Installizer.BTTasks.Count; i++)
            {
                if (search && !Installizer.BTTasks[i].SearchName.Contains(mSearchContext))
                {
                    continue;
                }
                if (r.yMax < 0 || r.yMin > GlobalRect.height)
                {
                    r.position += Vector2.up * dsize.y;
                    continue;
                }
                bool inter = r.Contains(Event.current.mousePosition);
                if (inter)
                {
                    GUI.Label(r, "", "Icon.ClipSelected");
                    mRaycastMeta = Installizer.BTTasks[i];//SelectionRect
                }
                Installizer.contentContent.text = string.Format("<color={0}>{1}</color>",
                   inter ? "yellow" : "white", Installizer.BTTasks[i].DisplayName);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(r.xMax - 12, r.center.y);
                r.position += Vector2.up * dsize.y;
                
            }
            EndScroll(r.yMax, ref mScrollRect, ref mScrollOffset);
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            if (!Visible)
                return false;
            if(mRaycastMeta != null)
            {
                switch (mRaycastMeta.NodeType)
                {
                    case EBTNodeType.task:
                    case EBTNodeType.controller:
                        mWindow.AddChild(Context, mRaycastMeta, new Vector2(LocalRect.center.x, LocalRect.yMin));
                        Hide();
                        return true;
                    case EBTNodeType.condition:
                    case EBTNodeType.service:
                        BehaviourNodeGUI node = Context as BehaviourNodeGUI;
                        if (node != null)
                        {
                            BehaviourNodeGUI.Decorator decor = node.AddDecorator(mRaycastMeta);
                            if (decor != null && decor.Properties.Length > 0)
                            {
                                mFocusDecorator = decor;
                                decor.UpdatePropertiesInfo();
                            }
                            //Hide();
                            node.Resize();
                        }
                        return true;
                    default:
                        break;
                }
            }
            mFocusProperty = mRaycastProperty;
            if (mRaycastDecorator != null && mRaycastDecorator.Properties.Length > 0)
            {
                mFocusDecorator = mRaycastDecorator;
            }
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
            mFocusProperty = null;
            return Visible;
        }

        public override bool InteractDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if(Visible && button == EMouseButton.left)
            {
                if (mScrollRect.Contains(mWindow.GlobalMousePosition))
                {
                    mScrollOffset += mouseDelta.y * GlobalScale;
                }
                else if (mScrollRectL.Contains(mWindow.GlobalMousePosition))
                {
                    mScrollOffsetL += mouseDelta.y * GlobalScale;
                }
            }
            return Visible;
        }

    }
}