using Devil.Utility;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
  
    public abstract class EditorCanvasWindow : EditorWindow
    {

        string viewportName = "Viewport";

        bool repaint;
        bool focusCenter;
        Vector2 mousePos;
        Vector2 mouseDeltaPos;
        bool onFocus;
        bool mInterceptMouse;
        Rect cachedViewportRect;
        Rect clipRect;

        EMouseAction mouseAction;
        EMouseButton mouseButton;

        protected EditorGUICanvas mClipCanvas;
        protected EditorGUICanvas mScaledCanvas;
        protected EditorGUICanvas mGridCanvas;
        protected string statInfo = "";

        public EditorGUICanvas RootCanvas { get { return mClipCanvas; } }
        public EditorGUICanvas GraphCanvas { get { return mGridCanvas; } }
        public EditorGUICanvas ScaledCanvas { get { return mScaledCanvas; } }
        public Vector2 GlobalMousePosition { get; private set; }
        protected float mMinScale = 0.1f;
        protected float mMaxScale = 5f;

        protected virtual void UpdateStateInfo()
        {
            statInfo = string.Format("<b><size=20>[{0} : 1]</size></b>", mScaledCanvas.LocalScale.ToString("0.00"));
        }

        protected virtual void InitCanvas()
        {
            mClipCanvas = new EditorGUICanvas();
            mClipCanvas.Pivot = new Vector2(0, 0);

            mScaledCanvas = new EditorGUICanvas();
            mScaledCanvas.SortOrder = -1;
            mClipCanvas.AddElement(mScaledCanvas);

            mGridCanvas = new EditorGUICanvas();
            mScaledCanvas.AddElement(mGridCanvas);

            mGridCanvas.GridLineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            mGridCanvas.ShowGridLine = true;
            mGridCanvas.GridSize = 100;
            mGridCanvas.LocalRect = new Rect(-10000, -10000, 20000, 20000);

            UpdateStateInfo();
        }


        public EditorCanvasWindow() : base()
        {
            InitCanvas();
        }

        protected void OnGUI()
        {
            ProcessMouseDeltaPos();

            GUI.skin.label.richText = true;

            Rect rect = EditorGUILayout.BeginHorizontal("TE toolbar");
            OnTitleGUI();
            EditorGUILayout.EndHorizontal();

            if (rect.width > 1)
            {
                cachedViewportRect = rect;
            }

            clipRect = new Rect(cachedViewportRect.xMin + 1, cachedViewportRect.yMax + 1, position.width - 2, position.height - cachedViewportRect.height - 2);
            GUI.Label(clipRect, "", "CurveEditorBackground");
            OnCanvasGUI();
            GUI.Label(clipRect, statInfo);
            ProcessEvent();
            ProcessFocusCenter();
            OnPostGUI();
            if (repaint || onFocus)
            {
                Repaint();
            }
            repaint = false;
        }

        protected virtual void OnTitleGUI()
        {
        }

        protected virtual void OnPostGUI()
        {

        }

        protected void OnCanvasGUI()
        {
            GUI.BeginClip(clipRect);
            GlobalMousePosition = Event.current.mousePosition;
            Rect r = new Rect(Vector2.zero, clipRect.size);
            mClipCanvas.LocalRect = r;
            mScaledCanvas.LocalRect = r;
            mClipCanvas.CalculateGlobalRect(true);
            mClipCanvas.OnGUI(r);
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

        //聚焦状态图中心
        private void ProcessFocusCenter()
        {
            if (focusCenter && Vector2.Distance(-0.5f * mGridCanvas.LocalRect.size, mGridCanvas.LocalRect.position) > 1)
            {
                repaint |= true;
                Rect rect = mGridCanvas.LocalRect;
                rect.position = Vector2.Lerp(rect.position, -0.5f * mGridCanvas.LocalRect.size, 0.1f);
                mGridCanvas.LocalRect = rect;
            }
            else
            {
                focusCenter = false;
            }
        }

        void OnDragBegin()
        {
            mClipCanvas.InteractDragBegin(mouseButton, GlobalMousePosition);
        }

        void OnDrag()
        {
            if (mClipCanvas.InteractDrag(mouseButton, GlobalMousePosition, mouseDeltaPos))
                return;
            if (mouseButton == EMouseButton.middle || mouseButton == EMouseButton.right)
            {
                Rect rect = mGridCanvas.LocalRect;
                rect.position += mouseDeltaPos / (mGridCanvas.GlobalScale > 0 ? mGridCanvas.GlobalScale : 1);
                mGridCanvas.LocalRect = rect;
            }
        }

        void OnDragEnd()
        {
            mClipCanvas.InteractDragEnd(mouseButton, GlobalMousePosition);
        }

        void OnClick()
        {
            mClipCanvas.InteractMouseClick(mouseButton, GlobalMousePosition);
        }

        void OnKeyDown()
        {
            mClipCanvas.InteractKeyDown(Event.current.keyCode);
        }

        void OnKeyUp()
        {
            if(mClipCanvas.InteractKeyUp(Event.current.keyCode))
            {
                return;
            }
            if (Event.current.control && Event.current.keyCode == KeyCode.F)
            {
                focusCenter = true;
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
            repaint |= true;
            if(Event.current.type == EventType.KeyDown)
            {
                OnKeyDown();
            }
            else if(Event.current.type == EventType.KeyUp)
            {
                OnKeyUp();
            }
            if (Event.current.type == EventType.MouseDrag)
            {
                if (mouseAction == EMouseAction.none)
                {
                    mouseButton = (EMouseButton)Event.current.button;
                    mouseAction = EMouseAction.drag;
                    OnDragBegin();
                }
                if (mouseAction == EMouseAction.drag)
                {
                    OnDrag();
                }
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                if (mouseAction == EMouseAction.none)
                {
                    mouseButton = (EMouseButton)Event.current.button;
                    mouseAction = EMouseAction.click;
                    OnClick();
                }
                else if (mouseAction == EMouseAction.drag)
                {
                    OnDragEnd();
                }
                mouseAction = EMouseAction.none;
            }
            if (Event.current.type == EventType.ScrollWheel)
            {
                float f = Mathf.Clamp(mScaledCanvas.LocalScale - Event.current.delta.y * mScaledCanvas.LocalScale * 0.05f, mMinScale, mMaxScale);
                if ((f < 1 && mScaledCanvas.LocalScale > 1) || (f > 1 && mScaledCanvas.LocalScale < 1))
                    f = 1;
                mScaledCanvas.LocalScale = f;
                UpdateStateInfo();
            }
        }

    }
}