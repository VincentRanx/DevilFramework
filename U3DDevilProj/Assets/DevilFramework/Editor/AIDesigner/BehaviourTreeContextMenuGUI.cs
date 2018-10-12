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

        public class MetaUI
        {
            public BehaviourMeta BTMeta { get; private set; }
            public Vector2 TitleSize { get; private set; }
            public Vector2 DetailSize { get; private set; }
            public float Width { get { return Mathf.Max(TitleSize.x + BehaviourNodeGUI.FONT_SIZE, DetailSize.x); } }
            public float Height { get { return TitleSize.y + DetailSize.y; } }
            public string Category { get; private set; }
            public bool IsTitle { get { return BTMeta == null; } }
            public bool Collaped { get; set; }

            public MetaUI(BehaviourMeta meta)
            {
                BTMeta = meta;
                Installizer.contentStyle.fontSize = (int)BehaviourNodeGUI.SUB_FONT_SIZE;
                Vector2 size = Installizer.SizeOfContent(meta.DisplayName) + new Vector2(10, 10);
                size.y = Mathf.Max(size.y, BehaviourNodeGUI.FONT_SIZE);
                TitleSize = size;
                if (string.IsNullOrEmpty(meta.SubTitle))
                {
                    DetailSize = new Vector2(TitleSize.x, 0);
                }
                else
                {
                    Installizer.contentStyle.fontSize = (int)(BehaviourNodeGUI.SUB_FONT_SIZE * 0.9f);
                    DetailSize = Installizer.SizeOfContent(meta.SubTitle) + new Vector2(6, 6);
                }
            }

            public MetaUI(string category)
            {
                Category = category;
                Installizer.titleStyle.fontSize = (int)BehaviourNodeGUI.SUB_FONT_SIZE;
                TitleSize = Installizer.SizeOfTitle(category) + new Vector2(10, 10);
                DetailSize = Vector2.zero;
            }

            public void OnGUI(Rect rect, float scale)
            {
                if (BTMeta != null)
                {
                    QuickGUI.DrawBox(rect, new Color(0.3f, 0.3f, 0.3f), Color.yellow, rect.Contains(Event.current.mousePosition) ? 2 : 0);
                    Rect r = rect;
                    r.size = Vector2.one * (BehaviourNodeGUI.FONT_SIZE * scale);
                    r.position += Vector2.one * (3 * scale);
                    Texture icon = BTMeta.Icon;
                    if (icon != null)
                        GUI.DrawTexture(r, icon, ScaleMode.ScaleToFit);
                    r.position = new Vector2(rect.x + r.size.x, rect.y );
                    r.width = rect.width - r.size.x;
                    r.height = TitleSize.y * scale;
                    Installizer.contentStyle.fontStyle = FontStyle.Bold;
                    Installizer.contentStyle.normal.textColor = Color.white;
                    Installizer.contentStyle.fontSize = (int)Mathf.Max(1, BehaviourNodeGUI.SUB_FONT_SIZE * scale);
                    Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
                    Installizer.contentContent.text = BTMeta.DisplayName;
                    GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                    r.y = rect.y + TitleSize.y * scale;
                    r.height = DetailSize.y * scale;
                    r.width = rect.width - 6 * scale;
                    r.x = rect.x + 5 * scale;
                    Installizer.contentStyle.fontStyle = FontStyle.Normal;
                    Installizer.contentStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                    Installizer.contentStyle.fontSize = (int)Mathf.Max(1, BehaviourNodeGUI.SUB_FONT_SIZE * 0.9f * scale);
                    Installizer.contentStyle.alignment = TextAnchor.MiddleLeft;
                    Installizer.contentContent.text = BTMeta.SubTitle;
                    GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                }
                else
                {
                    Color c = BehaviourModuleManager.GetOrNewInstance().GetCategoryColor(Category);
                    QuickGUI.DrawBox(rect, Collaped ? c : new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f), Color.yellow, 0);
                    Installizer.titleStyle.fontSize = (int)Mathf.Max(1, BehaviourNodeGUI.SUB_FONT_SIZE * scale);
                    Installizer.titleStyle.alignment = TextAnchor.MiddleCenter;
                    Installizer.titleContent.text = Category;
                    GUI.Label(rect, Installizer.titleContent, Installizer.titleStyle);
                }
            }
        }

        BehaviourTreeDesignerWindow mWindow;
        //public bool Visible { get; private set; }
        public PaintElement Context { get; private set; }
        public EMode Mode { get; private set; }
        string mSearchContext = "";
        float mMinTaskWidth = 200;
        float mMinDecoratorWidth = 200;
        MetaUI mRaycastMeta;
        MetaUI mDropDownMeta;
        float mScrollOffset;
        Rect mScrollRect;

        Rect mScrollRectL;
        float mScrollOffsetL;

        bool mDragEnd;

        BTInputProperty mFocusProperty;
        BTInputProperty mRaycastProperty;

        BehaviourNodeGUI.Decorator mFocusDecorator;
        BehaviourNodeGUI.Decorator mRaycastDecorator;
        List<MetaUI> mTaskList = new List<MetaUI>();
        List<MetaUI> mDecoratorList = new List<MetaUI>();

        bool mRaycastSearch;
        bool mFocusSearch;

        bool mResizeNode;

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

        public void ShowContext(BehaviourNodeGUI context)
        {
            if (EditorApplication.isPlaying || context == null)
                return;
            List<BehaviourMeta> lst;
            if (mDecoratorList.Count < 2 || !BehaviourModuleManager.GetOrNewInstance().Decorators.Contains(mDecoratorList[1].BTMeta))
            {
                mMinDecoratorWidth = 200;
                mDecoratorList.Clear();
                lst = BehaviourModuleManager.GetOrNewInstance().Decorators;
                string category = null;
                for (int i = 0; i < lst.Count; i++)
                {
                    MetaUI m = new MetaUI(lst[i]);
                    if (m.BTMeta.Category != category)
                    {
                        category = m.BTMeta.Category;
                        mDropDownMeta = new MetaUI(category);
                        mDecoratorList.Add(mDropDownMeta);
                    }
                    mDecoratorList.Add(m);
                    mMinDecoratorWidth = Mathf.Max(mMinDecoratorWidth, m.Width);
                }
            }

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
            if (mTaskList.Count < 2 || !BehaviourModuleManager.GetOrNewInstance().Composites.Contains(mTaskList[1].BTMeta))
            {
                mTaskList.Clear();
                mMinTaskWidth = 200;
                List<BehaviourMeta> lst;
                lst = BehaviourModuleManager.GetOrNewInstance().Composites;
                string category = null;
                for (int i = 0; i < lst.Count; i++)
                {
                    MetaUI m = new MetaUI(lst[i]);
                    if (m.BTMeta.Category != category)
                    {
                        category = m.BTMeta.Category;
                        mDropDownMeta = new MetaUI(category);
                        mTaskList.Add(mDropDownMeta);
                    }
                    mTaskList.Add(m);
                    mMinTaskWidth = Mathf.Max(mMinTaskWidth, m.Width);
                }
            }

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
            if (mFocusProperty != null && mFocusProperty.ReguexData() && mFocusDecorator != null)
            {
                mFocusDecorator.UpdatePropertiesInfo();
            }
            Visible = false;
            Context = null;
            Mode = EMode.none;
            mRaycastProperty = null;
            mFocusProperty = null;
            mFocusDecorator = null;
            mRaycastDecorator = null;
            mDropDownMeta = null;
            mSearchContext = "";
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
            QuickGUI.DrawBox(GlobalRect, new Color(0.3f,0.3f,0.3f), Color.black, 2, true);
            //GUI.Label(GlobalRect, "", "sv_iconselector_back");
            if (Mode == EMode.alter_node)
            {
                OnContextDecoratorGUI();
                OnDecoratorsListGUI();
            }
            else if(Mode == EMode.new_node)
            {
                OnNewNodeGUI();
            }
            //GUI.Label(GlobalRect, "", "Icon.OutlineBorder");
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
            Rect tmp = new Rect();
            for (int i = 0; i < decorators.Count; i++)
            {
                BehaviourNodeGUI.Decorator decor = decorators[i];
                float h = (tsize.y + dsize.y) * decor.Properties.Length + tsize.y + tsize.y - dsize.y;
                r.height = h;
                bool inter = r.Contains(Event.current.mousePosition);
                //GUI.Label(r, "", inter ? "flow node 0" : "sv_iconselector_back");
                QuickGUI.DrawBox(r, new Color(0.3f, 0.3f, 0.3f), inter ? Color.yellow : Color.black, inter ? 2 : 0);
                Texture icon = decor.BTMeta.Icon;
                if(icon != null)
                {
                    tmp.size = Vector2.one * tsize.y;
                    tmp.position = new Vector2(r.xMin + 1, r.yMin + 1);
                    GUI.DrawTexture(tmp, icon, ScaleMode.ScaleToFit);
                }
                if (inter)//|| mFocusDecorator == decorators[i])
                {
                    mRaycastDecorator = decor;
                }
                tmp.size = tsize;
                tmp.position = new Vector2(r.xMin, r.yMin);
                Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
                Installizer.contentStyle.fontSize = (int)Mathf.Max(1, 12f * GlobalScale);
                Installizer.contentContent.text = decor.NotFlag?decor.BTMeta.NotDisplayName: decor.BTMeta.DisplayName;
                GUI.Label(tmp, Installizer.contentContent, Installizer.contentStyle);
                btn.center = new Vector2(tmp.xMax - btn.size.x * 0.5f, tmp.center.y);
                if (tsize.y > 8 && inter && GUI.Button(btn, "", "WinBtnCloseActiveMac"))
                {
                    node.RemoveDecorator(decor.BTMeta);
                    mResizeNode = true;
                    break;
                }
                tmp.size = dsize;
                tmp.position = new Vector2(r.xMin, r.yMin + tsize.y);
                OnPropertiesList(tmp, decor, tsize, dsize);
                r.position += Vector2.up * h;
            }
            r.height = 0;
        }

        void OnPropertiesList(Rect r, BehaviourNodeGUI.Decorator decorator, Vector2 tsize, Vector2 dsize)
        {
            BTInputProperty[] properties = decorator.Properties;
            if (properties.Length == 0)
                return;
            Rect h = new Rect();
            h.size = new Vector2(dsize.x - 6 * GlobalScale, dsize.y);
            h.position = r.position + Vector2.one * 3 * GlobalScale;
            Installizer.contentStyle.alignment = TextAnchor.MiddleLeft;
            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, BehaviourNodeGUI.SUB_FONT_SIZE * GlobalScale);
            bool dirty = false;
            for (int i = 0; i < properties.Length; i++)
            {
                BTInputProperty prop = properties[i];
                Installizer.contentContent.text = prop.PropertyName;
                GUI.Label(h, Installizer.contentContent, Installizer.contentStyle);
                h.y += dsize.y;//  = new Vector2(r.xMin + x0, r.yMin + i * dsize.y);
                //GUI.Label(h, "", "textfield");// "TL LoopSection");
                QuickGUI.DrawBox(h, Color.clear, Color.gray, 1);
                if (h.Contains(Event.current.mousePosition))
                    mRaycastProperty = prop;
                if (mFocusProperty == prop)
                {
                    string str = GUI.TextField(h, prop.InputData, Installizer.contentStyle);
                    dirty |= str != prop.InputData;
                    prop.InputData = str;
                }
                else
                {
                    prop.ReguexData();
                    GUI.Label(h, prop.IsDefaultValue ?
                        string.Format("<i><color=#a0a0a0>({0})</color></i> {1} ", prop.TypeName, prop.InputData) : prop.InputData, Installizer.contentStyle);
                }
                h.y += tsize.y;
            }
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
            Vector2 dsize = new Vector2(l * GlobalScale - 2, BehaviourNodeGUI.SUB_FONT_SIZE * GlobalScale);
            dsize.y += 5 * GlobalScale;
            Vector2 tsize = new Vector2(dsize.x, dsize.y * 1.3f);

            BehaviourNodeGUI node = Context as BehaviourNodeGUI;
            TextAnchor align = Installizer.contentStyle.alignment;

            Rect r = new Rect();
            r.size = new Vector2(tsize.x, GlobalRect.height);
            r.position = GlobalRect.position;
            BeginScroll(r, ref mScrollRectL);

            //
            r.size = tsize;
            r.position = new Vector2(0, mScrollOffsetL);
            BTInputProperty[] properties;
            // conditions
            OnAttachedDecoratorListGUI(ref r, tsize, dsize, node, node.conditions);
            // self
            properties = node.Self.Properties;
            r.size = new Vector2(tsize.x, tsize.y + properties.Length * (tsize.y + dsize.y) + tsize.y);
            QuickGUI.DrawBox(r, node.Self.BTMeta.color, Color.black);
            if (r.Contains(Event.current.mousePosition))
                mRaycastDecorator = node.Self;
            Rect tmp = new Rect();
            Texture icon = node.Self.BTMeta.Icon;
            if (icon != null)
            {
                tmp.size = Vector2.one * tsize.y;
                tmp.position = new Vector2(r.xMin + 1, r.yMin + 1);
                GUI.DrawTexture(tmp, icon, ScaleMode.ScaleToFit);
            }
            tmp.size = tsize;
            tmp.position = new Vector2(r.xMin, r.yMin);
            Installizer.titleStyle.alignment = TextAnchor.MiddleCenter;
            Installizer.titleStyle.fontSize = (int)Mathf.Max(1, BehaviourNodeGUI.FONT_SIZE * GlobalScale);
            Installizer.titleContent.text = string.Format("<b>{0}</b>", node.Self.BTMeta.DisplayName);
            GUI.Label(tmp, Installizer.titleContent, Installizer.titleStyle);
            tmp.y += tsize.y + 4 * GlobalScale;
            OnPropertiesList(tmp, node.Self, tsize, dsize);
            r.y = r.yMax;
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
            //bool search = false;
            if (GlobalRect.height > 30)
            {
                Rect r = new Rect();
                r.size = new Vector2(rect.size.x - 20, 20);
                r.position = rect.position;
                mRaycastSearch = r.Contains(Event.current.mousePosition);
                if (mFocusSearch)
                {
                    mSearchContext = GUI.TextField(r, mSearchContext, "SearchTextField").ToLower();
                }
                else
                {
                    GUI.Label(r, mSearchContext, "SearchTextField");
                }
                r.size = new Vector2(20, 20);
                r.position = new Vector2(GlobalRect.xMax - 20, GlobalRect.yMin + 2);
                if (GUI.Button(r, "", "SearchCancelButton"))
                    mSearchContext = "";
                //search = !string.IsNullOrEmpty(mSearchContext);
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
            //Vector2 dsize = new Vector2(mMinDecoratorWidth * GlobalScale, 20 * GlobalScale);
            //BehaviourNodeGUI node = Context as BehaviourNodeGUI;
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
            OnMetaList(mDecoratorList, r, search, true);
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

        void OnMetaList(List<MetaUI> metas, Rect r, bool search, bool skipExist)
        {
            Vector2 dsize = new Vector2(r.width, BehaviourNodeGUI.SUB_FONT_SIZE * GlobalScale);
            dsize.y += 10 * GlobalScale;
            Vector2 tsize = new Vector2(dsize.x, dsize.y * 1.2f);
            BeginScroll(r, ref mScrollRect);

            Installizer.contentStyle.fontSize = (int)Mathf.Max(1, BehaviourNodeGUI.SUB_FONT_SIZE * GlobalScale);
            r.size = tsize;
            r.position = new Vector2(0, mScrollOffset);
            
            // controllers
            r.size = dsize;
            int len = metas.Count;
            Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
            BehaviourNodeGUI context = Context as BehaviourNodeGUI;
            Color color = Color.gray;
            bool collape = false;
            for (int i = 0; i < len; i++)
            {
                MetaUI meta = metas[i];
                if (!meta.IsTitle)
                {
                    if (collape && !search)
                        continue;
                    if (search && !meta.BTMeta.SearchName.Contains(mSearchContext))
                    {
                        continue;
                    }
                    if (skipExist && context != null && context.ContainsDecorator(meta.BTMeta))
                    {
                        continue;
                    }
                }
                else
                {
                    collape = meta.Collaped;// != mDropDownMeta;
                }
                r.height = meta.Height * GlobalScale;
                if (r.yMax >= 0 && r.yMin < GlobalRect.height)
                {
                    bool inter = r.Contains(Event.current.mousePosition);
                    if (inter)
                    {
                        //GUI.Label(r, "", "flow node 0");// "Icon.ClipSelected");
                        mRaycastMeta = meta;//SelectionRect
                    }
                    meta.OnGUI(r, GlobalScale);
                    //QuickGUI.DrawBox(r, new Color(0.3f, 0.3f, 0.3f), Color.yellow, inter ? 2 : 0);
                    //Installizer.contentContent.text = meta.DisplayName;
                    //GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                }
                r.y += meta.Height * GlobalScale;

            }
            
            EndScroll(r.yMax, ref mScrollRect, ref mScrollOffset);
        }
        
        void OnTaskList()
        {
            mRaycastMeta = null;
            Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
            Vector2 tsize = new Vector2(mMinTaskWidth * GlobalScale, 30 * GlobalScale);
            //Vector2 dsize = new Vector2(mMinTaskWidth * GlobalScale, 20 * GlobalScale);
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

            OnMetaList(mTaskList, r, search, false);
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            if (!Visible)
                return false;
            mFocusSearch = mRaycastSearch;
            if(mRaycastMeta != null && button == EMouseButton.left)
            {
                if (mRaycastMeta.IsTitle)
                    mRaycastMeta.Collaped = !mRaycastMeta.Collaped;
                    //mDropDownMeta = mRaycastMeta == mDropDownMeta ? null : mRaycastMeta;
                else
                {
                    switch (mRaycastMeta.BTMeta.NodeType)
                    {
                        case EBTNodeType.task:
                        case EBTNodeType.controller:
                            mWindow.AddChild(Context, mRaycastMeta.BTMeta, new Vector2(LocalRect.center.x, LocalRect.yMin));
                            Hide();
                            return true;
                        case EBTNodeType.condition:
                        case EBTNodeType.service:
                            BehaviourNodeGUI node = Context as BehaviourNodeGUI;
                            if (node != null)
                            {
                                BehaviourNodeGUI.Decorator decor = node.AddDecorator(mRaycastMeta.BTMeta);
                                //Hide();
                                if (decor == null)
                                {
                                    EditorCanvasTip.NewTip("不能添加<color=yellow>" + mRaycastMeta.BTMeta.DisplayName + "</color>", 2)
                                        .Show(mWindow.RootCanvas, mWindow.RootCanvas.CalculateLocalPosition(mousePosition - Vector2.up * 20));
                                }
                                else
                                {
                                    decor.UpdatePropertiesInfo();
                                    node.Resize();
                                }
                            }
                            return true;
                        default:
                            break;
                    }
                }
            }
            bool act = mFocusProperty == null && mRaycastProperty == null;
            if (mFocusProperty != mRaycastProperty)
                SubmitProperty();
            mFocusProperty = mRaycastProperty;
            mFocusDecorator = mRaycastDecorator;
            if (act && mRaycastDecorator != null && mRaycastDecorator.BTMeta.NodeType == EBTNodeType.condition)
                mRaycastDecorator.NotFlag = !mRaycastDecorator.NotFlag;
            //if (mRaycastDecorator != null && mRaycastDecorator.Properties.Length > 0)
            //{
            //    mFocusDecorator = mRaycastDecorator;
            //}
            return Visible;
        }

        public override bool InteractDragBegin(EMouseButton button, Vector2 mousePosition)
        {
            if (Visible && button == EMouseButton.left)
            {
                mDragEnd = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        void SubmitProperty()
        {
            if (mFocusProperty != null && mFocusProperty.ReguexData() && mFocusDecorator != null)
            {
                mFocusDecorator.UpdatePropertiesInfo();
            }
            mFocusProperty = null;

        }

        public override bool InteractDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            if (Visible && button == EMouseButton.left)
            {
                mDragEnd = true;
                SubmitProperty();
                return true;
            }
            else
            {
                return false;
            }
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

        public override bool InteractKeyUp(KeyCode key)
        {
            if(mFocusProperty!=null && key == KeyCode.Return)
            {
                SubmitProperty();
                return true;
            }
            return false;
        }
    }
}