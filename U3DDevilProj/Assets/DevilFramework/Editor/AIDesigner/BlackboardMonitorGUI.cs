using UnityEngine;
using Devil.AI;
using Devil.Utility;
using UnityEditor;

namespace DevilEditor
{
    public class BlackboardMonitorGUI : PaintElement
    {
        EditorCanvasWindow mWindow;
        bool mDragEnd;
        Rect mScrollRect;
        float mScrollOffset;
        bool mDrop = false;
        public BTBlackboard Blackboard { get; set; }
        IBlackboardProperty mRaycastProperty;
        //long mTick;

        public BlackboardMonitorGUI(EditorCanvasWindow window) : base()
        {
            mWindow = window;
        }
        
        public override void OnGUI(Rect clipRect)
        {
            mRaycastProperty = null;
            if(Blackboard == null)
            {
                Visible = false;
                return;
            }
            //bool vis = mVisible;
            //if (!vis && mVisible)
            //    mTick = JDateTime.NowMillies;
            Rect rec = LocalRect;
            rec.width = Mathf.Clamp(Parent.LocalRect.width * 0.35f, 200, 500);
            rec.height = mDrop ? 180 : 25;
            rec.position = new Vector2(0, Parent.LocalRect.height - LocalRect.height);
            LocalRect = rec;

            BTBlackboard blackboard = Blackboard;
            //bool set;
            QuickGUI.DrawBox(GlobalRect, Color.gray * 0.5f, Color.black, 1, true);
            Rect r = new Rect(GlobalRect.xMin, GlobalRect.yMin, GlobalRect.width, 20);
            //bool drop = mDrop;
            mDrop = GUI.Toggle(r, mDrop, "Blackboard", "PreDropDown") && blackboard.Length > 0;
            //if (!drop && mDrop)
            //    mTick = JDateTime.NowMillies;
            if (mDrop)
            {
                r.height = GlobalRect.height - 20;
                r.y = GlobalRect.yMin + 25;
                BeginScroll(r, ref mScrollRect);
                r.position = new Vector2(0, mScrollOffset);
                r.height = 20;
                for (int i = 0; i < blackboard.Length; i++)
                {
                    r.width = 100;
                    r.position = new Vector2(0, i * 20 + mScrollOffset);
                    GUI.Label(r, blackboard.GetPropertyName(i));
                    r.width = GlobalRect.width - 100;
                    r.x = 100;
                    if (blackboard.IsSet(i))
                    {
                        GUI.Label(r, blackboard[i].ToString(), "AssetLabel");
                    }
                    else
                    {
                        GUI.Label(r, "[NOT SET]");
                    }
                    if (r.Contains(Event.current.mousePosition))
                        mRaycastProperty = blackboard[i];
                }
                EndScroll(r.yMax, ref mScrollRect, ref mScrollOffset);
            }

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

        public override bool InteractDragBegin(EMouseButton button, Vector2 mousePosition)
        {
            //mTick = JDateTime.NowMillies;
            if (Visible && button == EMouseButton.left)
            {
                mDragEnd = false;
                return true;
            }
            else
            {
                return true;
            }
        }
        
        public override bool InteractDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            //mTick = JDateTime.NowMillies;
            if (Visible && button == EMouseButton.left)
            {
                mDragEnd = true;
                return true;
            }
            else
            {
                return true;
            }
        }

        public override bool InteractDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if (Visible && button == EMouseButton.left)
            {
                if (mScrollRect.Contains(mWindow.GlobalMousePosition))
                {
                    mScrollOffset += mouseDelta.y * GlobalScale;
                }
            }
            return Visible;
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            //if (button == EMouseButton.left && mRaycastProperty != null && mRaycastProperty.Value != null)
            //{
            //    Object o = mRaycastProperty.Value as Object;
            //    if (o != null)
            //    {
            //        EditorGUIUtility.PingObject(o);
            //        return true;
            //    }
            //}
            return true;
        }
    }
}