using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public enum EMouseAction
    {
        none,
        click,
        drag,
    }

    public enum EMouseButton
    {
        left = 0,
        right = 1,
        middle = 2,
    }

    public delegate bool InteractMouseEvent(EMouseButton button, Vector2 mousePosition);
    public delegate bool InteractMouseMoveEvent(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta);
    public delegate bool InteractKeyEvent(KeyCode key);

    public abstract class PaintElement
    {
        static int sIdNum;

        public float LocalScale { get; set; }
        public float GlobalScale { get; private set; }
        public Vector2 Pivot { get; set; }
        public Rect LocalRect { get; set; }
        public PaintElement Parent { get; set; }
        public int SortOrder { get; set; }
        public int Id { get; private set; }
        public bool IsActive { get; set; }

        public bool DontClip { get; set; }
        public bool Visible { get; set; }

        public Rect GlobalRect;// 变换后的 rect
        public Vector2 GlobalCentroid
        {
            get
            {
                return new Vector2(Mathf.Lerp(GlobalRect.xMin, GlobalRect.xMax, Pivot.x),
                    Mathf.Lerp(GlobalRect.yMin, GlobalRect.yMax, Pivot.y));
            }
        }

        public PaintElement()
        {
            Id = ++sIdNum;
            LocalScale = 1;
            Pivot = new Vector2(0.5f, 0.5f);
            Visible = true;
        }

        public virtual void OnRemoved() { }

        public virtual void CalculateGlobalRect(bool recursive)
        {
            GlobalScale = 1;
            PaintElement p = Parent;
            while (p != null)
            {
                GlobalScale *= p.LocalScale;
                p = p.Parent;
            }
            GlobalRect.size = LocalRect.size * GlobalScale;
            if (Parent == null)
            {
                GlobalRect.position = LocalRect.position;
            }
            else
            {
                GlobalRect.position = LocalRect.position * GlobalScale + Parent.GlobalCentroid;
            }
        }

        public Vector2 CalculateGlobalPosition(Vector2 localPos)
        {
            return localPos * GlobalScale * LocalScale + GlobalCentroid;
        }

        // calculate child local rect
        public Rect CalculateLocalRect(Rect globalRect)
        {
            Rect rect = new Rect();
            float scale = GlobalScale * LocalScale;
            scale = scale > 0 ? scale : 1;
            rect.size = globalRect.size / scale;
            rect.position = (globalRect.position - GlobalCentroid) / scale;
            return rect;
        }

        public Vector2 CalculateLocalPosition(Vector2 globalPos)
        {
            float scale = GlobalScale * LocalScale;
            scale = scale > 0 ? scale : 1;
            return (globalPos - GlobalCentroid) / scale;
        }

        public abstract void OnGUI(Rect clipRect);

        public virtual bool InteractDragBegin(EMouseButton button, Vector2 mousePosition) { return false; }
        public virtual bool InteractDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta) { return false; }
        public virtual bool InteractDragEnd(EMouseButton button, Vector2 mousePosition) { return false; }
        public virtual bool InteractMouseClick(EMouseButton button, Vector2 mousePosition) { return false; }
        public virtual bool InteractKeyDown(KeyCode key) { return false; }
        public virtual bool InteractKeyUp(KeyCode key) { return false; }
    }

    public class EditorGUICanvas : PaintElement
    {
        public float GridSize { get; set; }
        public bool ShowGridLine { get; set; }
        public bool ColorAxis { get; set; }
        public bool BoldAxis { get; set; }
        public Color GridLineColor { get; set; }
        protected List<PaintElement> Elements;

        public InteractMouseEvent OnMouseClick;
        public InteractMouseEvent OnDragBegin;
        public InteractMouseMoveEvent OnDrag;
        public InteractMouseEvent OnDragEnd;
        public InteractKeyEvent OnKeyDown;
        public InteractKeyEvent OnKeyUp;
        protected bool mOnSelfDrag;
        protected PaintElement mDragTarget;

        public EditorGUICanvas() : base()
        {
            GridSize = 50;
            GridLineColor = Color.gray;
            LocalRect = new Rect(Vector2.zero, Vector2.one * 30);
            Elements = new List<PaintElement>();
            ColorAxis = false;
            BoldAxis = true;
        }

        public void AddElement(PaintElement ele)
        {
            if (ele.Parent != null || Elements.Contains(ele))
                return;
            Elements.Add(ele);
            ele.Parent = this;
            ele.IsActive = true;
        }

        public void RemoveElement(PaintElement ele)
        {
            if(ele.Parent == this)
            {
                ele.Parent = null;
                ele.IsActive = false;
                //Elements.Remove(ele);
            }
        }

        public int ElementCount { get { return Elements.Count; } }

        public T GetElement<T>(int index) where T: PaintElement
        {
            T t = Elements[index] as T;
            if (t != null && t.Parent != this)
                t = null;
            return t;
        }

        public void ClearElements()
        {
            for(int i = 0; i < Elements.Count; i++)
            {
                Elements[i].Parent = null;
            }
            Elements.Clear();
        }

        public void Resort(bool recursive)
        {
            GlobalUtil.Sort(Elements, (x, y) => x.SortOrder > y.SortOrder ? 1 : -1);
            if (recursive)
            {
                for (int i = 0; i < Elements.Count; i++)
                {
                    PaintElement can = Elements[i];
                    if (can is EditorGUICanvas)
                        ((EditorGUICanvas)can).Resort(recursive);
                }
            }
        }

        protected virtual void OnDrawGridLine(Rect clipRect, float grid_size)
        {
            float xmin = Mathf.Max(clipRect.xMin, GlobalRect.xMin);
            float xmax = Mathf.Min(clipRect.xMax, GlobalRect.xMax);
            float ymin = Mathf.Max(clipRect.yMin, GlobalRect.yMin);
            float ymax = Mathf.Min(clipRect.yMax, GlobalRect.yMax);
            if (xmin < xmax && ymin < ymax)
            {
                Vector2 cent = GlobalCentroid;
                int nx = Mathf.CeilToInt((xmin - cent.x) / grid_size);
                int ny = Mathf.CeilToInt((ymin - cent.y) / grid_size);
                float x = nx * grid_size + cent.x;
                float y = ny* grid_size + cent.y;
                Vector3 a = new Vector3(), b = new Vector3();
                a.y = ymin;
                b.y = ymax;
                while (x <= xmax)
                {
                    Handles.color = ColorAxis && nx == 0 ? Color.green : GridLineColor;
                    if (BoldAxis && nx == 0)
                    {
                        a.x = x + 1;
                        b.x = x + 1;
                        Handles.DrawLine(a, b);
                        a.x = x - 1;
                        b.x = x - 1;
                        Handles.DrawLine(a, b);
                    }
                    a.x = x;
                    b.x = x;
                    Handles.DrawLine(a, b);
                    x += grid_size;
                    nx++;
                }
                a.x = xmin;
                b.x = xmax;
                while (y <= ymax)
                {
                    Handles.color = ColorAxis && ny == 0 ? Color.red : GridLineColor;
                    if (BoldAxis && ny == 0)
                    {
                        a.y = y + 1;
                        b.y = y + 1;
                        Handles.DrawLine(a, b);
                        a.y = y - 1;
                        b.y = y - 1;
                        Handles.DrawLine(a, b);
                    }
                    a.y = y;
                    b.y = y;
                    Handles.DrawLine(a, b);
                    y += grid_size;
                    ny++;
                }
            }
        }

        public override void CalculateGlobalRect(bool recursive)
        {
            base.CalculateGlobalRect(false);
            if (recursive)
            {
                for (int i = 0; i < Elements.Count; i++)
                {
                    if(Elements[i].Visible)
                        Elements[i].CalculateGlobalRect(recursive);
                }
            }
        }

        public override void OnGUI(Rect clipRect)
        {
            float gsize = GridSize * GlobalScale;
            if (GlobalScale > 0 && ShowGridLine && GridSize > 1)
            {
                OnDrawGridLine(clipRect, gsize);
            }
            bool clean = false;
            if(LocalScale > 0)
            {
                for (int i = 0; i < Elements.Count; i++)
                {
                    PaintElement ele = Elements[i];
                    if (!ele.Visible)
                        continue;
                    if (ele.DontClip || clipRect.Overlaps(ele.GlobalRect))
                        ele.OnGUI(clipRect);
                    if (ele.Parent != this)
                        clean = true;
                }
            }
            if (clean)
            {
                for (int i = Elements.Count - 1; i >= 0; i--)
                {
                    PaintElement ele = Elements[i];
                    if (ele.Parent != this)
                    {
                        Elements.RemoveAt(i);
                        ele.OnRemoved();
                    }
                }
            }
        }

        public override bool InteractDragBegin(EMouseButton button, Vector2 mousePosition)
        {
            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                PaintElement ele = Elements[i];
                if (ele.Visible && ele.GlobalRect.Contains(mousePosition) && ele.InteractDragBegin(button, mousePosition))
                {
                    mDragTarget = ele;
                    return true;
                }
            }
            mOnSelfDrag = OnDragBegin == null ? false : OnDragBegin(button, mousePosition);
            return mOnSelfDrag;
        }

        public override bool InteractDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if (mOnSelfDrag)
            {
                return OnDrag == null ? false : OnDrag(button, mousePosition, mouseDelta);
            }
            if (mDragTarget != null)
            {
                return mDragTarget.InteractDrag(button, mousePosition, mouseDelta);
            }
            return mOnSelfDrag;
        }

        public override bool InteractDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            if (mOnSelfDrag)
            {
                mOnSelfDrag = false;
                return OnDragEnd == null ? false : OnDragEnd(button, mousePosition);
            }
            if (mDragTarget != null)
            {
                bool ret = mDragTarget.InteractDragEnd(button, mousePosition);
                mDragTarget = null;
                return ret;
            }
            return false;
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                PaintElement ele = Elements[i];
                if (ele.GlobalRect.Contains(mousePosition) && ele.InteractMouseClick(button, mousePosition))
                    return true;
            }
            return OnMouseClick == null ? false : OnMouseClick(button, mousePosition);
        }

        public override bool InteractKeyDown(KeyCode key)
        {
            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                PaintElement ele = Elements[i];
                if (ele.InteractKeyDown(key))
                    return true;
            }
            return OnKeyDown == null ? false : OnKeyDown(key);
        }

        public override bool InteractKeyUp(KeyCode key)
        {
            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                PaintElement ele = Elements[i];
                if (ele.InteractKeyUp(key))
                    return true;
            }
            return OnKeyUp == null ? false : OnKeyUp(key);
        }

    }

    public class SelectionGUI : PaintElement
    {
        public SelectionGUI() : base()
        {
            SortOrder = 10;
        }

        public override void OnGUI(Rect clipRect)
        {
            GUI.Label(GlobalRect, "", "SelectionRect");
        }
    }
}
