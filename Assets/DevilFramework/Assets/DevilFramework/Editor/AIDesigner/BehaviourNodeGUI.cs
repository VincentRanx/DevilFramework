using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Devil.AI;
using Devil.Utility;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DevilEditor
{
    public class BehaviourNodeGUI : PaintElement
    {
        public const float FONT_SIZE = 17;
        public const float SUB_FONT_SIZE = 13;

        public class Decorator
        {
            public int BTId { get; private set; }
            public BehaviourMeta BTMeta { get; private set; }
            public BehaviourInputProperty[] Properties { get; private set; }
            public string PropertiesInfo { get; private set; }
            public float TextHeight { get; set; }
            public float SubTextHeight { get; set; }

            public Decorator(int id, BehaviourMeta meta)
            {
                BTId = id;
                BTMeta = meta;
                Properties = new BehaviourInputProperty[meta.Properties.Length];
                for(int i = 0; i < meta.Properties.Length; i++)
                {
                    Properties[i] = new BehaviourInputProperty(meta.Properties[i]);
                }
            }

            public void ParseData(string data)
            {
                if (Properties.Length > 0)
                {
                    JObject obj = JsonConvert.DeserializeObject<JObject>(data);
                    if (obj != null)
                    {
                        for (int i = 0; i < Properties.Length; i++)
                        {
                            Properties[i].InputData = obj.Value<string>(Properties[i].PropertyName);
                        }
                    }
                }
                UpdatePropertiesInfo();
            }

            public BehaviourTreeAsset.BTData NewData()
            {
                BehaviourTreeAsset.BTData data = new BehaviourTreeAsset.BTData();
                data.m_Id = BTId;
                data.m_Name = BTMeta.Name;
                data.m_Type = BTMeta.NodeType;
                StringBuilder builder = new StringBuilder();
                builder.Append('{');
                for(int i = 0; i < Properties.Length; i++)
                {
                    if (i > 0)
                        builder.Append(',');
                    builder.Append(Properties[i].GetJsonPattern());
                }
                builder.Append('}');
                data.m_JsonData = builder.ToString();
                return data;
            }

            public void UpdatePropertiesInfo()
            {
                StringBuilder str = new StringBuilder();
                string subt = BTMeta.SubTitle;
                if (!string.IsNullOrEmpty(subt))
                {
                    str.Append("<color=#808080>");
                    for (int i = 0; i < Properties.Length; i++)
                    {
                        subt = subt.Replace("{" + Properties[i].PropertyName + "}",
                            string.Format("<b>{0}</b>", string.IsNullOrEmpty(Properties[i].InputData) ? "-" : Properties[i].InputData));
                    }
                    str.Append(subt);
                    str.Append("</color>");
                }
                else if (Properties.Length > 0)
                {
                    str.Append("<color=#808080>");
                    for (int i = 0; i < Properties.Length; i++)
                    {
                        if (i > 0)
                            str.Append('\n');
                        str.Append("<b>").Append(Properties[i].PropertyName).Append(": </b>");
                        str.Append(Properties[i].InputData);
                    }
                    str.Append("</color>");
                }
                PropertiesInfo = str.ToString();
            }
        }

        public Decorator Self { get; set; }
        public int BTExecutionOrder { get; set; }
        public List<Decorator> conditions = new List<Decorator>();
        public List<Decorator> services = new List<Decorator>();
        public EBTTaskState BTRuntimeState { get; set; }

        bool mUseEvents;
        public bool Selected { get; set; }
        BehaviourTreeDesignerWindow mWindow;
        bool mDrag;
        string mError;

        public BehaviourNodeGUI(BehaviourTreeDesignerWindow window) : base()
        {
            mWindow = window;
        }

        public Decorator GetDecorator(BehaviourMeta meta)
        {
            if (meta.NodeType == EBTNodeType.condition)
                return GlobalUtil.Find(conditions, (x) => x.BTMeta == meta);
            else if (meta.NodeType == EBTNodeType.service)
                return GlobalUtil.Find(services, (x) => x.BTMeta == meta);
            else
                return null;
        }

        public Decorator AddDecorator(BehaviourMeta meta)
        {
            List<Decorator> lst = null;
            if (meta.NodeType == EBTNodeType.condition)
                lst = conditions;
            else if (meta.NodeType == EBTNodeType.service)
                lst = services;
            else
                lst = null;
            if (lst == null)
                return null;
            Decorator decor = new Decorator(mWindow.GenerateId, meta);
            lst.Add(decor);
            return decor;
        }

        public void RemoveDecorator(BehaviourMeta meta)
        {
            List<Decorator> lst = null;
            if (meta.NodeType == EBTNodeType.condition)
                lst = conditions;
            else if (meta.NodeType == EBTNodeType.service)
                lst = services;
            else
                lst = null;
            if (lst != null)
            {
                for (int i = lst.Count - 1; i >= 0; i--)
                {
                    if (lst[i].BTMeta == meta)
                    {
                        lst.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        void AddDecoratorSize(List<Decorator> decors, ref Vector2 size)
        {
            int size0 = (int)SUB_FONT_SIZE;
            int size1 = (int)Mathf.Max(1, size0 * 0.9f);
            Vector2 tmp;
            for (int i = 0; i < decors.Count; i++)
            {
                Installizer.contentStyle.fontSize = size0;
                tmp = Installizer.SizeOfContent(decors[i].BTMeta.DisplayName);
                decors[i].TextHeight = tmp.y;

                size.x = Mathf.Max(tmp.x + 10, size.x);
                size.y += tmp.y;
                Installizer.contentStyle.fontSize = size1;
                if (!string.IsNullOrEmpty(decors[i].PropertiesInfo))
                {
                    tmp = Installizer.SizeOfContent(decors[i].PropertiesInfo);
                    decors[i].SubTextHeight = tmp.y;
                    size.x = Mathf.Max(tmp.x + 10, size.x);
                    size.y += tmp.y;
                }
                else
                {
                    decors[i].SubTextHeight = 0;
                }
            }
        }

        public Vector2 CalculateLocalSize()
        {
            Installizer.titleStyle.fontSize = (int)FONT_SIZE;
            Vector2 size = Installizer.SizeOfTitle(Self.BTMeta.DisplayName);
            Self.TextHeight = size.y + 5;
            size.x += FONT_SIZE;
            if (!string.IsNullOrEmpty(Self.PropertiesInfo))
            {
                Installizer.contentStyle.fontSize = (int)Mathf.Max(1, SUB_FONT_SIZE * 0.9f);
                Vector2 subsize = Installizer.SizeOfContent(Self.PropertiesInfo);
                Self.SubTextHeight = subsize.y;
                size.x = Mathf.Max(size.x, subsize.x + SUB_FONT_SIZE);
            }
            else
            {
                Self.SubTextHeight = 0;
            }
            size.y = Self.SubTextHeight + Self.TextHeight + 35;
            AddDecoratorSize(conditions, ref size);
            AddDecoratorSize(services, ref size);
            return size;
        }

        public void CheckError()
        {
            //mError = "No Parent.";
        }

        public override void OnGUI(Rect clipRect)
        {
            string style;

            style = Selected ? "flow node 0 on" : "flow node 0";
            GUI.Label(GlobalRect, "", style);
            OnNodeGUI();
            OnSocketGUI();
            if (Application.isPlaying && mWindow.Runner)
            {
                
            }
        }

        public void Resize()
        {
            Rect localr = LocalRect;
            localr.size = CalculateLocalSize();
            LocalRect = localr;
        }

        void OnDecoratorListGUI(List<Decorator> decors, ref Rect rect)
        {
            int size0 = (int)Mathf.Max(1, SUB_FONT_SIZE * GlobalScale);
            int size1 = (int)Mathf.Max(1, size0 * 0.9f);
            float h0;
            float h1;
            Rect r = new Rect();
            float x0 = rect.xMin;
            float x = 5 * GlobalScale;
            r.size = new Vector2(rect.width - x * 2, rect.height);
            r.position = new Vector2(x + x0, rect.yMax);
            for (int i = 0; i < decors.Count; i++)
            {
                h0 = decors[i].TextHeight * GlobalScale;
                h1 = decors[i].SubTextHeight * GlobalScale;
                rect.height = h0 + h1;
                rect.position = new Vector2(x0, r.yMin);
                GUI.Label(rect, "", "sv_iconselector_back");
                r.height = h0;
                Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
                Installizer.contentStyle.fontSize = size0;
                Installizer.contentContent.text = decors[i].BTMeta.DisplayName;
                Installizer.contentContent.text = string.Format("<color=white>{0}</color>", decors[i].BTMeta.DisplayName);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                r.position += Vector2.up * h0;
                if (!string.IsNullOrEmpty(decors[i].PropertiesInfo))
                {
                    Installizer.contentStyle.alignment = TextAnchor.MiddleLeft;
                    r.height = h1;
                    Installizer.contentStyle.fontSize = size1;
                    Installizer.contentContent.text = decors[i].PropertiesInfo;
                    GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                    r.position += Vector2.up * h1;
                }
            }
        }

        protected virtual void OnNodeGUI()
        {
            Rect r = new Rect();
            float w = (LocalRect.width - 10) * GlobalScale;
            float y0 = GlobalRect.yMin + 15 * GlobalScale;
            float x0 = GlobalRect.center.x - w * 0.5f;
            bool editmode = !mWindow.ContextMenu.Visible && mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.none;
            // decorators
            r.size = new Vector2(w, 1);
            r.position = new Vector2(x0, y0);
            OnDecoratorListGUI(conditions, ref r);
            r.position = new Vector2(x0, r.yMax);
            r.size = new Vector2(w, (Self.SubTextHeight + Self.TextHeight + 5) * GlobalScale);
            Texture2D bg = Self.BTMeta.Background;
            if (bg != null)
                GUI.DrawTexture(r, bg, ScaleMode.StretchToFill);
            else
               GUI.Label(r, "", Self.BTMeta.FrameStyle);
            float dx = 5 * GlobalScale;
            Rect r2 = new Rect(x0 + dx, r.yMin, w - dx, Self.TextHeight * GlobalScale);
            Installizer.titleStyle.fontSize = (int)Mathf.Max(1, FONT_SIZE * GlobalScale);
            Installizer.titleStyle.normal.textColor = Color.white;
            Installizer.titleContent.text = Self.BTMeta.DisplayName;
            GUI.Label(r2, Installizer.titleContent, Installizer.titleStyle);
            if (!string.IsNullOrEmpty(Self.PropertiesInfo))
            {
                r2.position = new Vector2(x0 + dx, r2.yMax);
                r2.size = new Vector2(w - dx, Self.SubTextHeight * GlobalScale);
                Installizer.contentContent.text = Self.PropertiesInfo;
                Installizer.contentStyle.alignment = TextAnchor.MiddleLeft;
                Installizer.contentStyle.fontSize = (int)Mathf.Max(1, SUB_FONT_SIZE * GlobalScale * 0.9f);
                GUI.Label(r2, Installizer.contentContent, Installizer.contentStyle);
            }
            OnDecoratorListGUI(services, ref r);
            r.size = new Vector2(50, 20);
            r.position = new Vector2(GlobalRect.xMin, GlobalRect.yMin - 20);
            GUI.Label(r, string.Format("<color={0}>[{1}]</color>", BTExecutionOrder > 0 ? "green" : "red",
                BTExecutionOrder > 0 ? BTExecutionOrder.ToString() : "-"));
            if (!string.IsNullOrEmpty(mError))
            {
                r.size = new Vector2(200, 50);
                r.position = new Vector2(GlobalRect.xMin, GlobalRect.yMax);
                GUI.Label(r, string.Format("<size=13><b><color=red>{0}</color></b></size>", mError));
            }
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
                bool editmode = !EditorApplication.isPlayingOrWillChangePlaymode && !mWindow.ContextMenu.Visible && mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.none;
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
                mWindow.ContextMenu.ShowContext(this);
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

        public BehaviourTreeAsset.BTData ExportNodeData(ICollection<BehaviourTreeAsset.BTData> collections)
        {
            BehaviourTreeAsset.BTData node = Self.NewData();
            node.m_Pos = new Vector2(LocalRect.center.x, LocalRect.yMin);
            int[] servs = new int[services.Count];
            for(int i = 0; i < servs.Length; i++)
            {
                Decorator decor = services[i];
                BehaviourTreeAsset.BTData data = decor.NewData();
                collections.Add(data);
                servs[i] = decor.BTId;
            }
            node.m_Services = servs;
            int[] decos = new int[conditions.Count];
            for(int i = 0; i < decos.Length; i++)
            {
                Decorator decor = conditions[i];
                BehaviourTreeAsset.BTData data = decor.NewData();
                collections.Add(data);
                decos[i] = decor.BTId;
            }
            node.m_Conditions = decos;
            collections.Add(node);
            return node;
        }
    }
}