using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{

    public class EditorCanvasContainer : EditorGUICanvas
    {

        string viewportName = "Viewport";

        bool focusCenter;
        Vector2 mousePos;
        Vector2 mouseDeltaPos;
        bool onFocus;
        bool mInterceptMouse;
        Rect cachedViewportRect;
        Rect clipRect;

        EMouseAction mouseAction;
        EMouseButton mouseButton;

        public Vector2 GlobalMousePosition { get; private set; }
        protected float mMinScale = 0.1f;
        protected float mMaxScale = 5f;
        
        public EditorCanvasContainer() : base()
        {
        }

        protected virtual void OnGUI()
        {
            ProcessMouseDeltaPos();

            Input.imeCompositionMode = IMECompositionMode.On;

            GUI.skin.label.richText = true;

            Rect rect = EditorGUILayout.BeginHorizontal("TE toolbar");
            OnTitleGUI();
            EditorGUILayout.EndHorizontal();

            if (rect.width > 1)
            {
                cachedViewportRect = rect;
            }

            clipRect = new Rect(cachedViewportRect.xMin + 1, cachedViewportRect.yMax + 1, GlobalRect.width - 2, GlobalRect.height - cachedViewportRect.height - 2);
            GUI.Label(clipRect, "", "CurveEditorBackground");
            OnCanvasGUI();
            ProcessEvent();
            OnPostGUI();
        }

        protected virtual void OnTitleGUI()
        {
        }

        protected virtual void OnPostGUI()
        {

        }

        protected void OnCanvasGUI()
        {
            CalculateGlobalRect(true);
            GUI.BeginClip(clipRect);
            GlobalMousePosition = Event.current.mousePosition;
            Rect r = new Rect(Vector2.zero, clipRect.size);
            OnGUI(r);
            GUI.EndClip();
        }

        // 计算鼠标移动量
        void ProcessMouseDeltaPos()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                mousePos = Event.current.mousePosition;
                mouseDeltaPos = Vector2.zero;
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                Vector2 pos = Event.current.mousePosition;
                mouseDeltaPos = pos - mousePos;
                mousePos = pos;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                mouseDeltaPos = Vector2.zero;
            }
        }

        // 响应鼠标事件
        void ProcessEvent()
        {
            mInterceptMouse = clipRect.Contains(Event.current.mousePosition);
            if (!mInterceptMouse)
            {
                return;
            }
            if(Event.current.type == EventType.KeyDown)
            {
                InteractKeyDown(Event.current.keyCode);
            }
            else if(Event.current.type == EventType.KeyUp)
            {
                InteractKeyUp(Event.current.keyCode);
            }
            if (Event.current.type == EventType.MouseDrag)
            {
                if (mouseAction == EMouseAction.none)
                {
                    mouseButton = (EMouseButton)Event.current.button;
                    mouseAction = EMouseAction.drag;
                    InteractDragBegin(mouseButton, GlobalMousePosition);
                }
                if (mouseAction == EMouseAction.drag)
                {
                    InteractDrag(mouseButton, GlobalMousePosition, mouseDeltaPos);
                }
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                if (mouseAction == EMouseAction.none)
                {
                    mouseButton = (EMouseButton)Event.current.button;
                    mouseAction = EMouseAction.click;
                    InteractMouseClick(mouseButton, GlobalMousePosition);
                }
                else if (mouseAction == EMouseAction.drag)
                {
                    InteractDragEnd(mouseButton, GlobalMousePosition);
                }
                mouseAction = EMouseAction.none;
            }
        }

    }
}