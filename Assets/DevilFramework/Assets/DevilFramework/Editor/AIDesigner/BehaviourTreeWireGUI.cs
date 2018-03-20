using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DevilEditor
{

    public class BehaviourTreeWireGUI : PaintElement
    {
        BehaviourTreeDesignerWindow mWindow;
        public Color color { get; set; }

        public BehaviourTreeWireGUI(BehaviourTreeDesignerWindow window) : base()
        {
            mWindow = window;
            SortOrder = -1;
            color = Color.white * 0.7f;
        }

        public override void OnGUI(Rect clipRect)
        {
            //DrawGridLine(clipRect);
            int len = mWindow.TreeGraph.PathLength(0);
            float width = GlobalScale * 5;
            if (EditorApplication.isPlaying)
                width *= 1.7f;
            width = Mathf.Clamp(width, 1, 20);
            float height = 15 * GlobalScale;
            int from, to;
            Vector2 start;
            Vector2 end;
            PaintElement a, b;
            Color c = color;
            for (int i = 0; i < len; i++)
            {
                mWindow.TreeGraph.PathAt(0, i, out from, out to);
                a = mWindow.TreeGraph[from];
                b = mWindow.TreeGraph[to];
                if (b == mWindow.EditTarget && mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.modify_parent)
                    continue;
                start.x = a.GlobalRect.center.x;
                start.y = a.GlobalRect.yMax;
                end.x = b.GlobalRect.center.x;
                end.y = b.GlobalRect.yMin;
                if (EditorApplication.isPlaying)
                {
                    BehaviourNodeGUI bb = b as BehaviourNodeGUI;
                    if(bb != null && bb.BTRuntimeState != Devil.AI.EBTTaskState.inactive)
                    {
                        c = bb.BTRuntimeState == Devil.AI.EBTTaskState.success ? Color.green : (bb.BTRuntimeState == Devil.AI.EBTTaskState.running ? Color.blue : Color.red);
                    }
                    else
                    {
                        c = color;
                    }
                }
                ConnectNode(start, end, c, width, height);
            }
            if (mWindow.EditTarget != null)
            {
                start.x = mWindow.EditTarget.GlobalRect.center.x;
                if (mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.modify_child)
                {
                    start.y = mWindow.EditTarget.GlobalRect.yMax;
                    ConnectNode(start, mWindow.GlobalMousePosition, Color.yellow, width, height);
                }
                else if (mWindow.EditMode == BehaviourTreeDesignerWindow.ENodeEditMode.modify_parent)
                {
                    start.y = mWindow.EditTarget.GlobalRect.yMin;
                    ConnectNode(mWindow.GlobalMousePosition, start, Color.yellow, width, height);
                }
            }
            else if(mWindow.ContextMenu.Mode == BehaviourTreeContextMenuGUI.EMode.new_node)
            {
                a = mWindow.ContextMenu.Context;
                start.x = a.GlobalRect.center.x;
                start.y = a.GlobalRect.yMax;
                end = mWindow.ContextMenu.AttachPoint;
                ConnectNode(start, end, Color.yellow, width, height);
            }
        }

        void ConnectNode(Vector2 start, Vector2 end, Color color, float width, float height)
        {
            if (Mathf.Abs(start.x - end.x) < 1)
            {
                Handles.DrawBezier(start, end, start, end, color, null, width);
                return;
            }
            Vector2 a = start;
            Vector2 b = start;
            b.y = a.y + height;
            Handles.DrawBezier(a, b, a, b, color, null, width);
            if (end.y < b.y + 2)
            {
                a.x = end.x + 10;
                a.y = b.y;
                Handles.DrawBezier(a, b, a, b, color, null, width);
                b.x = a.x;
                b.y = end.y - 15;
                Handles.DrawBezier(a, b, a, b, color, null, width);
                a.x = end.x;
                a.y = b.y;
                Handles.DrawBezier(a, b, a, b, color, null, width);
                b.x = a.x;
                b.y = end.y;
                Handles.DrawBezier(a, b, a, b, color, null, width);
            }
            else
            {
                a.x = end.x;
                a.y = b.y;
                Handles.DrawBezier(a, b, a, b, color, null, width);
                b.x = a.x;
                b.y = end.y;
                Handles.DrawBezier(a, b, a, b, color, null, width);
            }
        }
    }
}