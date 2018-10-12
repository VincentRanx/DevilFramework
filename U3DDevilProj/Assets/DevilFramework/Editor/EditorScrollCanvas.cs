using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevilEditor
{
    public class EditorScrollCanvas : EditorGUICanvas
    {
        Vector2 mScrollPos;
        bool mDrag;
        Vector2 mLocalSize;
        public string Style { get; set; }

        protected PaintElement mRaycastElement;

        public EditorScrollCanvas() : base()
        {
            OnDragBegin = OnScrollDragBegin;
            OnDrag = OnScrollDrag;
            OnDragEnd = OnScrollDragEnd;
            OnMouseClick = OnScrollClick;
            Style = "box";
        }

        protected virtual bool OnScrollClick(EMouseButton button, Vector2 mousePosition)
        {
            return Visible;
        }

        protected virtual bool OnScrollDragBegin(EMouseButton button, Vector2 mousePosition)
        {
            mDrag = true;
            return true;
        }

        protected virtual bool OnScrollDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            if (mDrag)
            {
                mDrag = false;
                return true;
            }
            return false;
        }

        protected virtual bool OnScrollDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if (mDrag)
            {
                mScrollPos += mouseDelta / GlobalScale;
                return true;
            }
            return false;
        }

        // return height
        protected virtual float OnTitleRect(Rect clipRect)
        {
            return 0;
        }

        public override void OnGUI(Rect clipRect)
        {
            mRaycastElement = null;
            float gsize = GridSize * GlobalScale;
            if (GlobalScale > 0 && ShowGridLine && GridSize > 1)
            {
                OnDrawGridLine(clipRect, gsize);
            }
            bool clean = false;
            if (LocalScale > 0)
            {
                bool style = !string.IsNullOrEmpty(Style);
                if (style)
                    GUI.Label(GlobalRect, "", Style);
                float title = OnTitleRect(clipRect);
                GUI.BeginClip(new Rect(GlobalRect.x, GlobalRect.y + title, GlobalRect.width, GlobalRect.height - title));
                clipRect.position = Vector2.zero;
                clipRect.size = new Vector2(GlobalRect.width, GlobalRect.height - title);
                Vector2 offset = mScrollPos - new Vector2(GlobalRect.x, GlobalRect.y + title);
                mLocalSize = Vector2.zero;
                for (int i = 0; i < Elements.Count; i++)
                {
                    PaintElement ele = Elements[i];
                    if (!ele.Visible)
                        continue;
                    Rect rect = ele.GlobalRect;
                    rect.position += offset;
                    ele.GlobalRect = rect;
                    mLocalSize.x = Math.Max(ele.LocalRect.xMax, mLocalSize.x);
                    mLocalSize.y = Math.Max(ele.LocalRect.yMax, mLocalSize.y);
                    if (ele.DontClip || clipRect.Overlaps(ele.GlobalRect))
                    {
                        ele.OnGUI(clipRect);
                        if (ele.GlobalRect.Contains(Event.current.mousePosition))
                            mRaycastElement = ele;
                    }
                    if (ele.Parent != this)
                    {
                        clean = true;
                    }
                }
                GUI.EndClip();
                if (!mDrag)
                {
                    Vector2 scroll = new Vector2();
                    scroll.x = Mathf.Clamp(mScrollPos.x, Mathf.Min(10, LocalRect.width - mLocalSize.x), 10);
                    scroll.y = Mathf.Clamp(mScrollPos.y, Mathf.Min(10, LocalRect.height - mLocalSize.y), 10);
                    if (Vector2.SqrMagnitude(scroll - mScrollPos) > 1)
                        mScrollPos = Vector2.Lerp(mScrollPos, scroll, 0.1f);
                    else
                        mScrollPos = scroll;
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

    }
}