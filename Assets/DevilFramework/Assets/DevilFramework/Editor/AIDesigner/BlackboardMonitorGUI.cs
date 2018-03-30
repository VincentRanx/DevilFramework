using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Devil.AI;
using Devil.Utility;
using UnityEditor;

namespace DevilEditor
{
    public class BlackboardMonitorGUI : PaintElement
    {
        BehaviourTreeDesignerWindow mWindow;
        bool mDragEnd;
        Rect mScrollRect;
        float mScrollOffset;
        bool mVisible;
        bool mDrop = false;
        BTBlackboarProperty mRaycastProperty;
        long mTick;

        public BlackboardMonitorGUI(BehaviourTreeDesignerWindow window) : base()
        {
            mWindow = window;
        }

        public override void OnGUI(Rect clipRect)
        {
            mRaycastProperty = null;
            bool vis = mVisible;
            mVisible = mWindow.IsPlaying && mWindow.Runner != null;
            if (!vis && mVisible)
                mTick = JDateTime.NowMillies;
            if (mVisible)
            {
                //if (JDateTime.NowMillies - mTick > 8000)
                //    mDrop = false;
                Rect rec = LocalRect;
                rec.width = Mathf.Clamp(Parent.LocalRect.width * 0.35f, 200, 500);
                rec.height = mDrop ? 180 : 25;
                rec.position = new Vector2(0, Parent.LocalRect.height - LocalRect.height);
                LocalRect = rec;

                BTBlackboard blackboard = mWindow.Runner.Blackboard;
                BTBlackboarProperty[] props = (BTBlackboarProperty[])Ref.GetField(blackboard, "mVariables");
                QuickGUI.DrawBox(GlobalRect, Color.gray * 0.5f, Color.black, 1, true);
                Rect r = new Rect(GlobalRect.xMin, GlobalRect.yMin, GlobalRect.width, 20);
                bool drop = mDrop;
                mDrop = GUI.Toggle(r, mDrop, "Blackboard", "PreDropDown");
                if (!drop && mDrop)
                    mTick = JDateTime.NowMillies;
                if (mDrop)
                {
                    r.height = GlobalRect.height - 20;
                    r.y = GlobalRect.yMin + 25;
                    BeginScroll(r, ref mScrollRect);
                    r.position = new Vector2(0, mScrollOffset);
                    r.height = 20;
                    for (int i = 0; i < props.Length; i++)
                    {
                        r.width = 100;
                        r.position = new Vector2(0, i * 20 + mScrollOffset);
                        GUI.Label(r, props[i].Name);
                        r.width = GlobalRect.width - 100;
                        r.x = 100;
                        if (props[i].IsSet)
                        {
                            GUI.Label(r, props[i].Value == null ? "[NULL]" : props[i].Value.ToString(), "AssetLabel");
                        }
                        else
                        {
                            GUI.Label(r, "[NOT SET]");
                        }
                        if (r.Contains(Event.current.mousePosition))
                            mRaycastProperty = props[i];
                    }
                    EndScroll(r.yMax, ref mScrollRect, ref mScrollOffset);
                }
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
            mTick = JDateTime.NowMillies;
            if (mVisible && button == EMouseButton.left)
            {
                mDragEnd = false;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public override bool InteractDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            mTick = JDateTime.NowMillies;
            if (mVisible && button == EMouseButton.left)
            {
                mDragEnd = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool InteractDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if (mVisible && button == EMouseButton.left)
            {
                if (mScrollRect.Contains(mWindow.GlobalMousePosition))
                {
                    mScrollOffset += mouseDelta.y * GlobalScale;
                }
            }
            return mVisible;
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            mTick = JDateTime.NowMillies;
            if (button == EMouseButton.left && mRaycastProperty !=null && mRaycastProperty.Value != null)
            {
                Object o = mRaycastProperty.Value as Object;
                if(o != null)
                {
                    EditorGUIUtility.PingObject(o);
                    return true;
                }
            }
            return false;
        }
    }
}