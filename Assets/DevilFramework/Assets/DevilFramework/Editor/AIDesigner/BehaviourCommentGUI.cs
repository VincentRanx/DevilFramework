using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{

    public class BehaviourCommentGUI : PaintElement
    {
        public enum EDragBoarder
        {
            none,
            top = 1,
            left = 2,
            right = 4,
            bottom = 8,
        }

        BehaviourTreeDesignerWindow mWindow;
        List<BehaviourNodeGUI> targets = new List<BehaviourNodeGUI>();
        public bool IsSelected { get; set; }
        EDragBoarder mBoarders;
        bool mDragEnd;
        bool mEditMode;
        public string Comment { get; set; }

        public BehaviourCommentGUI(BehaviourTreeDesignerWindow window)
        {
            SortOrder = -2;
            mWindow = window;
            mDragEnd = true;
            Comment = "";
        }

        void GetContainsTarget()
        {
            targets.Clear();
            for (int i = 0; i < mWindow.TreeCanvas.ElementCount; i++)
            {
                BehaviourNodeGUI node = mWindow.TreeCanvas.GetElement<BehaviourNodeGUI>(i);
                if (node == null)
                    continue;
                if (GlobalRect.Contains(node.GlobalRect.min) && GlobalRect.Contains(node.GlobalRect.max))
                {
                    targets.Add(node);
                }
            }
        }

        public override void OnGUI(Rect clipRect)
        {
            GUI.Label(GlobalRect, "", IsSelected ? "SelectionRect" : "box");//U2D.createRect
            Vector2 offset = Vector2.one * 10 * GlobalScale;
            if (mEditMode)
            {
                Input.imeCompositionMode = IMECompositionMode.On;
                Comment = GUI.TextArea(GlobalRect, Comment, "textarea");
            }
            else
            {
                GUI.Label(GlobalRect, Comment);
            }
            RaycastBoarder();
            if (mBoarders != 0 && IsSelected && !mEditMode)
            {
                Rect r = new Rect();
                r.size = new Vector2(15, 15);
                r.center = mWindow.GlobalMousePosition;
                GUI.Label(r, "", "GridToggle");
            }
            if (!IsSelected)
                mEditMode = false;
        }

        void RaycastBoarder()
        {
            if (!mDragEnd)
                return;
            mBoarders = EDragBoarder.none;
            Rect r = new Rect();
            float size = 15;
            r.size = new Vector2(GlobalRect.width, size);
            r.position = GlobalRect.position;
            if (r.Contains(mWindow.GlobalMousePosition))
            {
                mBoarders |= EDragBoarder.top;
            }
            r.position = new Vector2(GlobalRect.xMin, GlobalRect.yMax - size);
            if (r.Contains(mWindow.GlobalMousePosition))
            {
                mBoarders |= EDragBoarder.bottom;
            }
            r.size = new Vector2(size, GlobalRect.height);
            r.position = GlobalRect.position;
            if (r.Contains(mWindow.GlobalMousePosition))
            {
                mBoarders |= EDragBoarder.left;
            }
            r.position = new Vector2(GlobalRect.xMax - size, GlobalRect.yMin);
            if (r.Contains(mWindow.GlobalMousePosition))
            {
                mBoarders |= EDragBoarder.right;
            }
        }

        public override bool InteractKeyUp(KeyCode key)
        {
            if (key == KeyCode.Delete && IsSelected)
            {
                mWindow.CommentCanvas.RemoveElement(this);
                return true;
            }
            return false;
        }

        public override bool InteractMouseClick(EMouseButton button, Vector2 mousePosition)
        {
            if (button == EMouseButton.left && mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.none)
            {
                if (IsSelected)
                    mEditMode = true;
                else
                    mWindow.SelectComment(this);
                mWindow.SelectNodes((x) => false);
                mWindow.ContextMenu.Hide();
                return true;
            }
            return false;
        }

        public override bool InteractDragBegin(EMouseButton button, Vector2 mousePosition)
        {
            if (!IsSelected)
                return false;
            if (button == EMouseButton.left)
            {
                mDragEnd = false;
                mWindow.SelectComment(this);
                if (mBoarders == EDragBoarder.none)
                {
                    GetContainsTarget();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool InteractDrag(EMouseButton button, Vector2 mousePosition, Vector2 mouseDelta)
        {
            if (!IsSelected)
                return false;
            if (button == EMouseButton.left)
            {
                Rect rect = LocalRect;
                Vector2 delta = mouseDelta / GlobalScale;
                if (mBoarders == EDragBoarder.none)
                {
                    rect.position += delta;
                    LocalRect = rect;
                    for (int i = 0; i < targets.Count; i++)
                    {
                        rect = targets[i].LocalRect;
                        rect.position += mouseDelta / targets[i].GlobalScale;
                        targets[i].LocalRect = rect;
                    }
                }
                else if(!mEditMode)
                {
                    if ((mBoarders & EDragBoarder.left) != 0)
                        rect.xMin += delta.x;
                    if ((mBoarders & EDragBoarder.right) != 0)
                        rect.xMax += delta.x;
                    if ((mBoarders & EDragBoarder.top) != 0)
                        rect.yMin += delta.y;
                    if ((mBoarders & EDragBoarder.bottom) != 0)
                        rect.yMax += delta.y;
                    if (rect.width > 30 && rect.height > 30)
                        LocalRect = rect;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool InteractDragEnd(EMouseButton button, Vector2 mousePosition)
        {
            if (!IsSelected)
                return false;
            if (button == EMouseButton.left)
            {
                mDragEnd = true;
                targets.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}