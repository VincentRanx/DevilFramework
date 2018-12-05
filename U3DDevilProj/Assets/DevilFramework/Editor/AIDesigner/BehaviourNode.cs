using Devil.AI;
using UnityEngine;

namespace DevilEditor
{
    public class BehaviourNode : PaintElement
    {
        public static Vector2 ICON_SIZE = new Vector2(20, 20);
        public static Vector2 SPACE = new Vector2(5, 5);
        public static GUIContent TITLE_CONTENT = new GUIContent();
        public static GUIStyle TITLE_STYLE = new GUIStyle();
        public static GUIContent SUB_CONTENT = new GUIContent();
        public static GUIStyle SUB_STYLE = new GUIStyle();
        public static Color SELECTED_COLOR = new Color(1f, 1f, 0.1f, 1f);
        public static Color BOARDER_COLOR = new Color(0.2f, 0.2f, 0.2f, 1f);
        public static Color SLOT_COLOR = new Color(0.3f, 0.3f, 0.3f, 1f);
        public static Color TITLE_COLOR = new Color(0.3f, 1f, 0.3f);
        public static Color SUBTITLE_COLOR = new Color(0.9f, 0.9f, 0.9f);
        public readonly static int TITLE_SIZE = 14;
        public readonly static int SUB_SIZE = 12;
        public readonly static Vector2 SLOT_SIZE = new Vector2(15, 15f);
        static float sFontScale = 1;

        public static void InitGUIStyle()
        {
            TITLE_STYLE.alignment = TextAnchor.MiddleCenter;
            TITLE_STYLE.richText = true;
            TITLE_STYLE.fontSize = TITLE_SIZE;
            TITLE_STYLE.wordWrap = false;
            TITLE_STYLE.fontStyle = FontStyle.Bold;
            TITLE_STYLE.normal.textColor = Color.green;
            TITLE_STYLE.onHover.textColor = Color.yellow;

            SUB_STYLE.alignment = TextAnchor.UpperLeft;
            SUB_STYLE.richText = true;
            SUB_STYLE.fontSize = SUB_SIZE;
            SUB_STYLE.wordWrap = false;
            SUB_STYLE.fontStyle = FontStyle.Normal;
            SUB_STYLE.normal.textColor = Color.white;
            SUB_STYLE.onHover.textColor = Color.white;
        }

        public static void SetFontScale(float scale)
        {
            sFontScale = scale;
            TITLE_STYLE.fontSize = Mathf.FloorToInt(scale * TITLE_SIZE);
            SUB_STYLE.fontSize = Mathf.FloorToInt(scale * SUB_SIZE);
        }

        public static void GUITitle(Rect rect, string title, FontStyle style = FontStyle.Bold, TextAnchor align = TextAnchor.MiddleCenter)
        {
            GUITitle(rect, title, TITLE_COLOR, style, align);
        }

        public static void GUITitle(Rect rect, string title, Color color, FontStyle style = FontStyle.Bold, TextAnchor align = TextAnchor.MiddleCenter)
        {
            TITLE_STYLE.normal.textColor = color;
            TITLE_STYLE.fontStyle = style;
            TITLE_STYLE.alignment = align;
            TITLE_CONTENT.text = title;
            GUI.Label(rect, TITLE_CONTENT, TITLE_STYLE);
        }

        public static void GUITitle(Rect rect, string title, Color color, int size, FontStyle style = FontStyle.Bold, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var old = TITLE_STYLE.fontSize;
            TITLE_STYLE.fontSize = Mathf.CeilToInt(size * sFontScale);
            TITLE_STYLE.normal.textColor = color;
            TITLE_STYLE.fontStyle = style;
            TITLE_STYLE.alignment = align;
            TITLE_CONTENT.text = title;
            GUI.Label(rect, TITLE_CONTENT, TITLE_STYLE);
            TITLE_STYLE.fontSize = old;
        }

        public static void GUISubtitle(Rect rect, string subtitle, FontStyle style = FontStyle.Normal, TextAnchor align = TextAnchor.UpperLeft)
        {
            GUISubtitle(rect, subtitle, SUBTITLE_COLOR, style, align);
        }

        public static void GUISubtitle(Rect rect, string subtitle, Color color, FontStyle style = FontStyle.Normal, TextAnchor align = TextAnchor.UpperLeft)
        {
            SUB_STYLE.normal.textColor = color;
            SUB_STYLE.fontStyle = style;
            SUB_STYLE.alignment = align;
            SUB_CONTENT.text = subtitle;
            GUI.Label(rect, SUB_CONTENT, SUB_STYLE);
        }

        public static void GUISubtitle(Rect rect, string subtitle, Color color, int size, FontStyle style = FontStyle.Normal, TextAnchor align = TextAnchor.UpperLeft)
        {
            var old = SUB_STYLE.fontSize;
            SUB_STYLE.fontSize = Mathf.CeilToInt(size * sFontScale);
            SUB_STYLE.normal.textColor = color;
            SUB_STYLE.fontStyle = style;
            SUB_STYLE.alignment = align;
            SUB_CONTENT.text = subtitle;
            GUI.Label(rect, SUB_CONTENT, SUB_STYLE);
            SUB_STYLE.fontSize = old;
        }

        public static Vector2 CalculateTitleSize(string title)
        {
            return CalculateTitleSize(title, TITLE_SIZE);
        }

        public static Vector2 CalculateTitleSize(string title, int tsize)
        {
            TITLE_CONTENT.text = title;
            var size = TITLE_STYLE.fontSize;
            TITLE_STYLE.fontSize = tsize;
            var ret = TITLE_STYLE.CalcSize(TITLE_CONTENT) + new Vector2(tsize, 3);
            TITLE_STYLE.fontSize = size;
            return ret;
        }


        public static Vector2 CalculateSubtitleSize(string subtitle)
        {
            return CalculateSubtitleSize(subtitle, SUB_SIZE);
        }

        public static Vector2 CalculateSubtitleSize(string subtitle, int tsize)
        {
            SUB_CONTENT.text = subtitle;
            var size = SUB_STYLE.fontSize;
            SUB_STYLE.fontSize = tsize;
            var ret = SUB_STYLE.CalcSize(SUB_CONTENT) + new Vector2(tsize, 2);
            SUB_STYLE.fontSize = size;
            return ret;
        }

        public bool selected;
        public Color color;
        public string title;
        public BehaviourTreeEditor editor { get; private set; }
        public bool enableParent = true;
        public bool enableChild = true;

        bool rayparent;
        bool raychild;
        protected virtual bool IsHighlight { get { return false; } }

        public BehaviourNode(BehaviourTreeEditor editor) : base()
        {
            color = new Color(0.4f, 0.4f, 0.5f, 1f);
            title = "Behaviour Node";
            LocalRect = new Rect(0, 0, 100, 70);
            this.editor = editor;
        }

        public Vector2 LocalPosition
        {
            get { return new Vector2(LocalRect.center.x, LocalRect.yMin); }
            set
            {
                var rect = LocalRect;
                rect.position = new Vector2(value.x - rect.width * 0.5f, value.y);
                LocalRect = rect;
            }
        }

        public virtual void Resize() { }

        public virtual BTNode GetNode() { return null; }

        public virtual BTNode GetRuntimeNode() { return null; }

        public virtual BTNode GetContext() { return null; }

        protected virtual void OnBaseLayerGUI(Rect rect)
        {
            var box = new Rect();
            box.size = GlobalScale * (LocalRect.size + new Vector2(5, 30));
            box.center = GlobalRect.center;
            if (IsHighlight)
                GUI.Label(rect, DevilEditorUtility.EmptyContent, "flow node 5 on");
            else if (selected || rect.Contains(editor.GlobalMousePosition))
                GUI.Label(rect, DevilEditorUtility.EmptyContent, "flow node 0 on");
            else
                GUI.Label(rect, DevilEditorUtility.EmptyContent, "flow node 0");

            //QuickGUI.DrawBox(GlobalRect, color, selected ? selectedColor : boarderColor, selected ? 2 : 1, true);
        }

        protected virtual void OnParentSlotLayer(Rect rect)
        {
            rayparent = !editor.IsRequestParentOrChild && rect.Contains(editor.GlobalMousePosition);
            var highlight = rayparent || editor.PresentParentRequest == this;
            QuickGUI.DrawBox(rect, highlight ? SELECTED_COLOR * 0.8f : SLOT_COLOR, highlight ? SELECTED_COLOR : BOARDER_COLOR, 2, rayparent);
        }

        protected virtual void OnChildSlotLayer(Rect rect)
        {
            raychild = !editor.IsRequestParentOrChild && rect.Contains(editor.GlobalMousePosition);
            var highlight = raychild || editor.PresentChildRequest == this;
            QuickGUI.DrawBox(rect, highlight ? SELECTED_COLOR * 0.8f : SLOT_COLOR, highlight ? SELECTED_COLOR : BOARDER_COLOR, 2, raychild);
        }

        protected virtual void OnContentLayer(Rect rect)
        {
            QuickGUI.DrawBox(rect, color, selected ? SELECTED_COLOR : BOARDER_COLOR, selected ? 2 : 1, true);
            if (!string.IsNullOrEmpty(title))
            {
                GUITitle(rect, title);
            }
        }

        public override void OnGUI(Rect clipRect)
        {
            var rect = GlobalRect;
            OnBaseLayerGUI(rect);
            var slot = SLOT_SIZE * GlobalScale;
            if (enableParent)
            {
                rect.y += slot.y;
                rect.height -= slot.y;
            }
            if (enableChild)
            {
                rect.height -= slot.y;
            }
            rect.width -= slot.x;
            rect.x += slot.x * 0.5f;
            OnContentLayer(rect);
            rect.x = GlobalRect.x + slot.x;
            rect.width = GlobalRect.width - slot.x * 2;
            rect.height = slot.y - 1;
            if (enableParent)
            {
                rect.y = GlobalRect.y;
                OnParentSlotLayer(rect);
            }
            else
                rayparent = false;
            if (enableChild)
            {
                rect.y = GlobalRect.yMax - slot.y + 1;
                OnChildSlotLayer(rect);
            }
            else
                raychild = false;
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            if (button == EMouseButton.left)
            {
                if (editor.PresentParentRequest != null)
                {
                    if (editor.PresentParentRequest.EnableParentAs(this))
                        editor.PresentParentRequest.ModifyParentAs(this);
                }
                else if (editor.PresentChildRequest != null)
                {
                    if (this.EnableParentAs(editor.PresentChildRequest))
                        this.ModifyParentAs(editor.PresentChildRequest);
                }
                else if (enableParent && rayparent)
                {
                    if (Event.current.alt && EnableParentAs(null))
                        ModifyParentAs(null);
                    else
                        editor.RequestParent(this);
                }
                else if (enableChild && raychild)
                {
                    editor.RequestChild(this);
                }
                else if (DoContentClick(mousePosition))
                {

                }
                else
                {
                    bool mul = Event.current.control || Event.current.shift;
                    editor.SetSelections((x) =>
                    {
                        if (x == this)
                            return Event.current.control ? !x.selected : true;
                        else
                            return mul ? x.selected : false;
                    });
                }
                return true;
            }
            return base.InteractMouseClick(button, mousePosition);
        }

        protected virtual bool DoContentClick(Vector2 mousePos)
        {
            return false;
        }

        public override bool InteractDragBegin(EMouseButton button, Vector2 mousePosition)
        {
            if (button == EMouseButton.left && !Application.isPlaying)
            {
                return true;
            }
            return base.InteractDragBegin(button, mousePosition);
        }

        public override bool InteractDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if (button == EMouseButton.left && !Application.isPlaying)
            {
                Vector2 delta = mouseDelta / GlobalScale;
                if (selected)
                {
                    for (int i = 0; i < editor.SelectionNodes.Count; i++)
                    {
                        var node = editor.SelectionNodes[i];
                        node.MoveDelta(delta);
                    }
                }
                else
                {
                    MoveDelta(delta);
                }
                return true;
            }
            return base.InteractDrag(button, mousePosition, mouseDelta);
        }

        protected virtual void MoveDelta(Vector2 delta)
        {
            var pos = LocalRect;
            pos.position += delta;
            LocalRect = pos;
        }

        public virtual void DrawComment(bool raycast) { }

        public virtual bool EnableParentAs(BehaviourNode node)
        {
            return enableParent && node == null || (node.enableChild && node != this);
        }
        
        public virtual void ModifyParentAs(BehaviourNode node)
        {
            editor.RequestParent(null);
        }
        
    }
}