using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Devil.AI;

namespace DevilEditor
{
    public class BTWireGUI : PaintElement
    {
        const float LINE_SIZE = 4;
        const float ARROW_SCALE = 4;
        readonly static Color LINE_COLOR = new Color(0.7f, 0.7f, 0.7f);
        readonly static Color LINE_CONFIRM_COLOR = new Color(0.7f, 0.7f, 0.1f);

        readonly static Color GOOD_COLOR = new Color(0.1f, 1f, 0.1f);
        readonly static Color BAD_COLOR = new Color(1f, 0.1f, 0.1f);
        readonly static Color RUN_COLOR = new Color(0.2f, 0.3f, 1f);

        public class LinkPath
        {
            public BehaviourNode from;
            public BehaviourNode to;

            float progress;
            Color color;

            public LinkPath()
            {
                progress = 1;
                color = LINE_COLOR;
            }

            public Color GetColor()
            {
                var f = progress;
                if (progress > 0)
                    progress -= EditorCanvasWindow.deltaTime * 0.25f;
                return Color.Lerp(LINE_COLOR, color, f);
            }

            public void SetState(EBTState stat)
            {
                switch (stat)
                {
                    case EBTState.running:
                        color = RUN_COLOR;
                        progress = 1;
                        break;
                    case EBTState.success:
                        color = GOOD_COLOR;
                        progress = 1;
                        break;
                    case EBTState.failed:
                        color = BAD_COLOR;
                        progress = 1;
                        break;
                    default:
                        //color = LINE_COLOR;
                        break;
                }
            }
            
        }

        List<LinkPath> links = new List<LinkPath>();
        public BehaviourTreeEditor editor { get; private set; }
        bool mUpdate;

        public BTWireGUI(BehaviourTreeEditor editor) : base()
        {
            this.editor = editor;
            DontClip = true;
        }

        public void Link(Rect from, Vector3 to, float size, Color color)
        {
            float dx = (from.center.x - to.x) / from.width;
            dx = Mathf.Clamp(dx * 0.3f, -0.95f, 0.95f);
            dx = (dx + 1) * 0.5f;
            Vector3 p0;
            p0.x = Mathf.Lerp(from.xMax, from.xMin, dx);
            p0.y = from.yMax;
            p0.z = 0;
            Link(p0, to, size, color);
        }

        public void Link(Vector3 p0, Vector3 p1, float size, Color color)
        {
            //var off = Vector3.up * LINE_HEIGHT * GlobalScale * height;
            //var p = p0 + off;
            //var px = p1 - off;
            //Handles.DrawBezier(p0, p1, p, px, color, null, size);

            Handles.color = color;
            var dir = p1 - p0;
            var length = dir.magnitude;
            if (length < 0.1f)
                return;
            dir /= length;
            var dir2 = Vector3.Cross(Vector3.forward, dir);
            if (length > size * ARROW_SCALE)
            {
                var a = p0 + dir2 * size * 0.5f;
                var b = p0 - dir2 * size * 0.5f;
                var c = b + dir * (length - size * ARROW_SCALE);
                var d = c - dir2 * size * (ARROW_SCALE * 0.5f - 0.5f);
                var e = p1;
                var f = d + dir2 * size * ARROW_SCALE;
                var g = c + dir2 * size;
                Handles.DrawAAConvexPolygon(a, b, c, g);
                Handles.DrawAAConvexPolygon(d, e, f);
            }
            else
            {
                var a = p1 - dir * length - dir2 * length * 0.5f;
                var b = p1;
                var c = a + dir2 * length;
                Handles.DrawAAConvexPolygon(a, b, c);
            }
        }

        void AddLink(BehaviourNode from, BehaviourNode to)
        {
            LinkPath p = new LinkPath();
            p.from = from;
            p.to = to;
            links.Add(p);
        }

        public void UpdateWires()
        {
            mUpdate = true;
        }

        void DoUpdate()
        {
            mUpdate = false;
            links.Clear();
            for (int i = editor.AIGraph.ElementCount - 1; i >= 0; i--)
            {
                var node = editor.AIGraph.GetElement<BehaviourNode>(i);
                if (node == null || node.GetNode() == null)
                    continue;
                var parent = node.GetNode().Parent;
                if (parent != null)
                {
                    editor.EditNodes((x) =>
                    {
                        if (x.GetNode() == parent)
                            AddLink(x, node);
                    });
                }
            }
        }
        
        public override void OnGUI(Rect clipRect)
        {
            float size = LINE_SIZE * GlobalScale;
            Vector2 p0, p1;
            var root = editor.TargetTree == null ? null : editor.TargetTree.GetNodeById(editor.TargetTree.RootId);
            if(root != null)
            {
                //p0 = new Vector2(editor.RootNode.GlobalRect.center.x, editor.RootNode.GlobalRect.yMax);
                p1 = editor.AIGraph.CalculateGlobalPosition(root.position);
                Link(editor.RootNode.GlobalRect, p1, size * 1.5f, LINE_CONFIRM_COLOR * 0.8f);
            }
            Color color = LINE_COLOR;
            var looper = editor.Binder.looper;
            for (int i = links.Count - 1; i >= 0; i--)
            {
                var l = links[i];
                if (l.to == editor.PresentParentRequest)
                    continue;
                if (Application.isPlaying)
                {
                    var nd = l.to.GetRuntimeNode();
                    var runtime = nd == null ? null : nd.Asset as BTNodeAsset;
                    l.SetState(runtime == null || looper == null || !looper.EditorAccessed.Contains(runtime) ? EBTState.inactive : runtime.State);
                    color = l.GetColor();
                }
               // p0 = new Vector2(l.from.GlobalRect.center.x, l.from.GlobalRect.yMax);
                p1 = new Vector2(l.to.GlobalRect.center.x, l.to.GlobalRect.yMin);
                Link(l.from.GlobalRect, p1, size, color);
            }
            Rect rect;
            if (editor.PresentParentRequest != null)
            {
                if (editor.RaycastNode != null)
                {
                    rect = editor.RaycastNode.GlobalRect;
                    p0 = new Vector2(rect.center.x, rect.yMax);
                    color = editor.PresentParentRequest.EnableParentAs(editor.RaycastNode) ? Color.green : Color.red;
                }
                else
                {
                    color = LINE_CONFIRM_COLOR;
                    p0 = editor.GlobalMousePosition;
                }
                rect = editor.PresentParentRequest.GlobalRect;
                p1 = new Vector2(rect.center.x, rect.yMin);
                Link(p0, p1, size, color);
            }
            if (editor.PresentChildRequest != null)
            {
                //rect = editor.PresentChildRequest.GlobalRect;
                //p0 = new Vector2(rect.center.x, rect.yMax);
                if (editor.RaycastNode != null)
                {
                    rect = editor.RaycastNode.GlobalRect;
                    p1 = new Vector2(rect.center.x, rect.yMin);
                    color = editor.RaycastNode.EnableParentAs(editor.PresentChildRequest) ? Color.green : Color.red;
                }
                else
                {
                    color = LINE_CONFIRM_COLOR;
                    p1 = editor.GlobalMousePosition;
                }
                Link(editor.PresentChildRequest.GlobalRect, p1, size, color);
            }
            if (mUpdate)
                DoUpdate();
        }
    }
}