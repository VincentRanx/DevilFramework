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
        public const float SUB_FONT_SIZE = 12;
        public const float ICON_SIZE = 30;

        public class Decorator
        {
            public int BTId { get; private set; }
            public BehaviourMeta BTMeta { get; private set; }
            public BTInputProperty[] Properties { get; private set; }
            public string PropertiesInfo { get; private set; }
            public float TextHeight { get; set; }
            public float SubTextHeight { get; set; }
            public EBTTaskState BTRuntimeState { get; set; }
            public float TimeOffset { get; set; }
            public bool NotFlag { get; set; }

            public Decorator(int id, BehaviourMeta meta)
            {
                BTId = id;
                BTMeta = meta;
                Properties = new BTInputProperty[meta.Properties.Length];
                for(int i = 0; i < meta.Properties.Length; i++)
                {
                    Properties[i] = new BTInputProperty(meta.Properties[i]);
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
                            Properties[i].InputData = DevilCfg.ReguexTypeValue(Properties[i].TypeName, obj.Value<string>(Properties[i].PropertyName), Properties[i].DefaultValue);
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
                data.m_NotFlag = NotFlag;
                JObject obj = new JObject();
                for(int i = 0; i < Properties.Length; i++)
                {
                    obj[Properties[i].PropertyName] = Properties[i].InputData;
                }
                data.m_JsonData = JsonConvert.SerializeObject(obj);
                return data;
            }

            public void UpdatePropertiesInfo()
            {
                StringBuilder str = new StringBuilder();
                string subt = BTMeta.SubTitle;
                bool sub = !string.IsNullOrEmpty(subt);
                if (sub)
                {
                    //str.Append("<color=#a8a8a8>");
                    for (int i = 0; i < Properties.Length; i++)
                    {
                        subt = subt.Replace("{" + Properties[i].PropertyName + "}",
                            string.Format("<b>{0}</b>", string.IsNullOrEmpty(Properties[i].InputData) ? "-" : Properties[i].InputData));
                    }
                    str.Append(subt);
                    //str.Append("</color>");
                }
                if (!BTMeta.HideProperty && Properties.Length > 0)
                {
                    //str.Append("<color=#a8a8a8>");
                    for (int i = 0; i < Properties.Length; i++)
                    {
                        if (sub || i > 0)
                            str.Append('\n');
                        str.Append("<b>").Append(Properties[i].PropertyName).Append(": </b>");
                        str.Append(Properties[i].InputData);
                    }
                    //str.Append("</color>");
                }
                PropertiesInfo = str.ToString();
            }
        }

        public Decorator Self { get; set; }
        public int BTExecutionOrder { get; set; }
        public List<Decorator> conditions = new List<Decorator>();
        public List<Decorator> services = new List<Decorator>();
        public BTNodeBase RuntimeNode { get; set; }

        bool mUseEvents;
        public bool IsSelected { get; set; }
        BehaviourTreeDesignerWindow mWindow;
        bool mDrag;
        string mError;
        Decorator mRaycastDecorator;

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
            if (meta.NodeType == EBTNodeType.service)
                lst = services;
            else if (meta.NodeType == EBTNodeType.condition && conditions.Count < 32)
                lst = conditions;
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
            int size1 = (int)Mathf.Max(1, size0 * 0.95f);
            Vector2 tmp;
            for (int i = 0; i < decors.Count; i++)
            {
                Installizer.contentStyle.fontSize = size0;
                Decorator decor = decors[i];
                tmp = Installizer.SizeOfContent(decor.BTMeta.NodeType == EBTNodeType.condition ? decor.BTMeta.NotDisplayName : decor.BTMeta.DisplayName);
                decor.TextHeight = tmp.y + 2;

                size.x = Mathf.Max(tmp.x + 10, size.x);

                Installizer.contentStyle.fontSize = size1;
                if (!string.IsNullOrEmpty(decor.PropertiesInfo))
                {
                    tmp = Installizer.SizeOfContent(decor.PropertiesInfo);
                    decors[i].SubTextHeight = tmp.y + 2;
                    size.x = Mathf.Max(tmp.x + 10, size.x);
                }
                else
                {
                    decor.SubTextHeight = 2;
                }
                size.y += decor.TextHeight + decor.SubTextHeight;
            }
        }

        public Vector2 CalculateLocalSize()
        {
            Installizer.titleStyle.fontSize = (int)FONT_SIZE;
            Vector2 size = Installizer.SizeOfTitle(Self.BTMeta.DisplayName);
            size.x += ICON_SIZE;
            Self.TextHeight = size.y + 5;
            size.x += FONT_SIZE;
            if (!string.IsNullOrEmpty(Self.PropertiesInfo))
            {
                Installizer.contentStyle.fontSize = (int)Mathf.Max(1, SUB_FONT_SIZE * 0.95f);
                Vector2 subsize = Installizer.SizeOfContent(Self.PropertiesInfo);
                Self.SubTextHeight = subsize.y;
                size.x = Mathf.Max(size.x, subsize.x + SUB_FONT_SIZE);
            }
            else
            {
                Self.SubTextHeight = 0;
            }
            size.y = Self.SubTextHeight + Self.TextHeight + 30;
            AddDecoratorSize(conditions, ref size);
            AddDecoratorSize(services, ref size);
            return size;
        }

        public void CheckError()
        {
            //mError = "No Parent.";
        }

        public bool ContainsDecorator(BehaviourMeta meta)
        {
            if(meta.NodeType == EBTNodeType.condition)
            {
                for (int i = 0; i < conditions.Count; i++)
                {
                    if (conditions[i].BTMeta.Name == meta.Name)
                        return true;
                }
            }
            else if(meta.NodeType == EBTNodeType.service)
            {
                for(int i = 0; i < services.Count; i++)
                {
                    if (services[i].BTMeta.Name == meta.Name)
                        return true;
                }
            }
            return false;
        }

        public override void OnGUI(Rect clipRect)
        {
            mRaycastDecorator = null;
            string style;
            if (mWindow.IsPlaying && mWindow.BreakNode == this.RuntimeNode && Mathf.Repeat((float)EditorApplication.timeSinceStartup, 1) < 0.5f)
                style = "flow node 5 on";
            else
                style = IsSelected ? "flow node 0 on" : "flow node 0";
            GUI.Label(GlobalRect, "", style);
            OnNodeGUI();
            OnSocketGUI();
            if (mWindow.IsPlaying && RuntimeNode != null && RuntimeNode.BreakToggle)
            {
                Rect r = new Rect();
                r.size = Vector2.one * 15;
                r.center = new Vector2(GlobalRect.xMax - 8, GlobalRect.yMin - 8);
                GUI.Label(r, "", "MeTransPlayhead");
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
            int size1 = (int)Mathf.Max(1, size0 * 0.95f);
            float h0;
            float h1;
            Rect r = new Rect();
            float x0 = rect.xMin;
            float x = 5 * GlobalScale;
            r.size = new Vector2(rect.width - x * 2, rect.height);
            r.position = new Vector2(x + x0, rect.yMax);
            Rect btn = new Rect();
            btn.size = Vector2.one * Mathf.Clamp(20 * GlobalScale, 15, 30);
            for (int i = 0; i < decors.Count; i++)
            {
                h0 = decors[i].TextHeight * GlobalScale;
                h1 = decors[i].SubTextHeight * GlobalScale;
                rect.height = h0 + h1;
                rect.position = new Vector2(x0, r.yMin);
                if (rect.Contains(Event.current.mousePosition))
                    mRaycastDecorator = decors[i];
                bool sel = mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.none && !mWindow.ContextMenu.Visible && decors[i] == mRaycastDecorator && rect.height >= btn.height;
                QuickGUI.DrawBox(rect, new Color(0.3f, 0.3f, 0.3f), sel ? Color.yellow : Color.black, sel ? 3 : 0);
                //GUI.Label(rect, "", sel ? "flow node 0 on" : "flow node 0");
                if(decors[i].BTMeta.Icon != null)
                {
                    Rect tmp = new Rect(rect.x + 1, rect.y + 1, h0, h0);
                    GUI.DrawTexture(tmp, decors[i].BTMeta.Icon, ScaleMode.ScaleToFit);
                }
                EBTTaskState stat = mWindow.IsPlaying ? decors[i].BTRuntimeState : EBTTaskState.inactive;
                if (stat == EBTTaskState.success)
                {
                    QuickGUI.DrawBox(rect, Color.clear, Color.green, 2);
                }
                else if(stat == EBTTaskState.faild)
                {
                    QuickGUI.DrawBox(rect, Color.clear, Color.red, 2);
                }
                r.height = h0;
                Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
                Installizer.contentStyle.fontSize = size0;
                Installizer.contentStyle.normal.textColor = Color.white;
                Installizer.contentContent.text = decors[i].NotFlag ? decors[i].BTMeta.NotDisplayName : decors[i].BTMeta.DisplayName;
                //Installizer.contentContent.text = string.Format("<color=white>{0}</color>", decors[i].BTMeta.DisplayName);
                GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                r.position += Vector2.up * h0;
                if (!string.IsNullOrEmpty(decors[i].PropertiesInfo))
                {
                    Installizer.contentStyle.alignment = TextAnchor.MiddleLeft;
                    r.height = h1;
                    Installizer.contentStyle.fontSize = size1;
                    Installizer.contentStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                    Installizer.contentContent.text = decors[i].PropertiesInfo;
                    GUI.Label(r, Installizer.contentContent, Installizer.contentStyle);
                    r.position += Vector2.up * h1;
                }
                if (!mWindow.IsPlaying && sel)
                {
                    btn.position = new Vector2(rect.xMax - btn.width, rect.yMin);
                    if (GUI.Button(btn, "", "WinBtnCloseActiveMac"))
                    {
                        RemoveDecorator(decors[i].BTMeta);
                        Resize();
                        break;
                    }
                }
                if (stat == EBTTaskState.running)
                {
                    Rect process = rect;
                    float dx = Mathf.PingPong((mWindow.Runner.BehaviourTime - decors[i].TimeOffset) * 0.3f, 2f);
                    process.xMax = Mathf.Lerp(rect.xMin, rect.xMax, Mathf.Clamp01(dx));
                    process.xMin = Mathf.Lerp(rect.xMin, rect.xMax, Mathf.Clamp01(dx - 1));
                    //GUI.Label(process, "", "U2D.createRect");
                    QuickGUI.DrawBox(process, Color.blue * 0.3f, Color.blue, 1, true);
                    //U2D.createRect
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
            // conditions
            r.size = new Vector2(w, 1);
            r.position = new Vector2(x0, y0);
            OnDecoratorListGUI(conditions, ref r);
            //self
            r.position = new Vector2(x0, r.yMax);
            r.size = new Vector2(w, (Self.SubTextHeight + Self.TextHeight) * GlobalScale);
            QuickGUI.DrawBox(r, Self.BTMeta.color, Color.black, 1);
            Texture2D icon = Self.BTMeta.Icon;
            if (icon != null)
            {
                Rect tmp = new Rect();
                tmp.size = Vector2.one * ICON_SIZE * GlobalScale;
                tmp.position = r.position + Vector2.one;
                GUI.DrawTexture(tmp, icon, ScaleMode.ScaleToFit);
            }
            //if (bg != null)
            //    GUI.DrawTexture(r, bg, ScaleMode.StretchToFill, true, 0, Color.blue, 5, 5);
            //else
            //    GUI.Label(r, "", Self.BTMeta.FrameStyle);
            float dx = 5 * GlobalScale;
            Rect r2 = new Rect(x0 + dx + ICON_SIZE * GlobalScale, r.yMin, w - dx, Self.TextHeight * GlobalScale);
            Installizer.titleStyle.fontSize = (int)Mathf.Max(1, FONT_SIZE * GlobalScale);
            Installizer.titleStyle.normal.textColor = Color.white;
            Installizer.titleStyle.alignment = TextAnchor.MiddleLeft;
            Installizer.titleContent.text = Self.BTMeta.DisplayName;
            GUI.Label(r2, Installizer.titleContent, Installizer.titleStyle);
            if (!string.IsNullOrEmpty(Self.PropertiesInfo))
            {
                r2.position = new Vector2(x0 + dx, r2.yMax);
                r2.size = new Vector2(w - dx, Self.SubTextHeight * GlobalScale);
                Installizer.contentContent.text = Self.PropertiesInfo;
                Installizer.contentStyle.alignment = TextAnchor.MiddleLeft;
                Installizer.contentStyle.fontSize = (int)Mathf.Max(1, SUB_FONT_SIZE * GlobalScale * 0.95f);
                GUI.Label(r2, Installizer.contentContent, Installizer.contentStyle);
            }
            if (mWindow.IsPlaying && Self.BTRuntimeState == EBTTaskState.running && RuntimeNode.ChildLength == 0)
            {
                Rect tmp = r;
                float x = Mathf.PingPong(mWindow.Runner.TaskTime * 0.5f, 2f);
                tmp.xMax = Mathf.Lerp(r.xMin, r.xMax, Mathf.Clamp01(x));
                tmp.xMin = Mathf.Lerp(r.xMin, r.xMax, Mathf.Clamp01(x - 1));
                //GUI.Label(tmp, "", "U2D.createRect");
                QuickGUI.DrawBox(tmp, Color.blue * 0.3f, Color.blue, 1, true);
            }

            // services
            OnDecoratorListGUI(services, ref r);
            r.size = new Vector2(100, 20);
            r.position = new Vector2(GlobalRect.xMin, GlobalRect.yMin - 20);
            if (mWindow.IsPlaying && RuntimeNode != null)
            {
                GUI.Label(r, string.Format("<color={0}>#{1}</color>", BTExecutionOrder > 0 ? "#10ff10" : "red",
                    BTExecutionOrder > 0 ? BTExecutionOrder.ToString() : "-"));
            }
            else
            {
                GUI.Label(r, string.Format("<color={0}>#{1}</color>", BTExecutionOrder > 0 ? "#10ff10" : "red",
                    BTExecutionOrder > 0 ? BTExecutionOrder.ToString() : "-"));
            }
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
            if (mWindow.IsPlaying && RuntimeNode != null)
            {
                RuntimeNode.BreakToggle = !RuntimeNode.BreakToggle;
                return true;
            }
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
            else if(mRaycastDecorator!= null && mRaycastDecorator.BTMeta.NodeType == EBTNodeType.condition)
            {
                mRaycastDecorator.NotFlag = !mRaycastDecorator.NotFlag;
            }
            else if (Event.current.control)
            {
                mWindow.SelectNodes((x) => x.IsSelected || x == this);
            }
            else
            {
                mWindow.SelectNodes((x) => x == this);
            }
            mWindow.SelectComment(null, false);
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
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return false;
            if(mDrag && button == EMouseButton.left)
            {
                Rect rect = LocalRect;
                float scale = (GlobalScale > 0 ? GlobalScale : 1);
                rect.position += mouseDelta / scale;
                LocalRect = rect;
                if (IsSelected)
                {
                    mWindow.EditNodes((x) =>
                    {
                        if (x.IsSelected && x != this)
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

        public void SyncRuntimeState(BehaviourTreeRunner btree)
        {
            if (RuntimeNode == null)
            {
                Self.BTRuntimeState = EBTTaskState.inactive;
                for (int i = 0; i < conditions.Count; i++)
                {
                    conditions[i].BTRuntimeState = EBTTaskState.inactive;
                }
                for (int i = 0; i < services.Count; i++)
                {
                    services[i].BTRuntimeState = EBTTaskState.inactive;
                }
            }
            else
            {
                Self.BTRuntimeState = RuntimeNode.State;
                for (int i = 0; i < RuntimeNode.ConditionBuffer.Length; i++)
                {
                    conditions[i].BTRuntimeState = RuntimeNode.State == EBTTaskState.inactive ? EBTTaskState.inactive 
                        : (RuntimeNode.ConditionBuffer[i] ? EBTTaskState.success : EBTTaskState.faild);
                }
                for (int i = 0; i < services.Count; i++)
                {
                    EBTTaskState stat = services[i].BTRuntimeState;
                    services[i].BTRuntimeState = btree.IsServiceActive(services[i].BTId) ? EBTTaskState.running : EBTTaskState.inactive;
                    if(stat != EBTTaskState.running && services[i].BTRuntimeState == EBTTaskState.running)
                    {
                        services[i].TimeOffset = btree.BehaviourTime;
                    }
                }
            }
        }
    }
}