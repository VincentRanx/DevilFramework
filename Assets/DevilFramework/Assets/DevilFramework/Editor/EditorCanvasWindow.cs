using Devil.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private string statInfo = "";

        public EditorGUICanvas RootCanvas { get; private set; }
        public EditorGUICanvas GraphCanvas { get; private set; }
        public EditorGUICanvas ScaledCanvas { get; private set; }
        public Vector2 GlobalMousePosition { get; private set; }
        protected float mMinScale = 0.1f;
        protected float mMaxScale = 5f;

        protected virtual void UpdateStateInfo()
        {
            statInfo = string.Format("<b><size=20>[{0} : 1]</size></b>", ScaledCanvas.LocalScale.ToString("0.00"));
        }

        protected virtual void InitCanvas()
        {
            RootCanvas = new EditorGUICanvas();
            RootCanvas.Pivot = new Vector2(0, 0);

            ScaledCanvas = new EditorGUICanvas();
            ScaledCanvas.SortOrder = -1;
            RootCanvas.AddElement(ScaledCanvas);

            GraphCanvas = new EditorGUICanvas();
            ScaledCanvas.AddElement(GraphCanvas);

            GraphCanvas.GridLineColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            GraphCanvas.ShowGridLine = true;
            GraphCanvas.GridSize = 100;
            GraphCanvas.LocalRect = new Rect(-10000, -10000, 20000, 20000);

            UpdateStateInfo();
        }


        public EditorCanvasWindow() : base()
        {
            InitCanvas();
        }

        protected virtual void OnEnable()
        {
            ReadData();
        }

        protected virtual void OnDisable()
        {
            SaveData();
        }

        void ReadData()
        {
            string str = EditorPrefs.GetString("can.sav");
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JsonConvert.DeserializeObject<JObject>(str);
                ScaledCanvas.LocalScale = obj.Value<float>("scale");
                Rect rect = GraphCanvas.LocalRect;
                rect.x = Mathf.Clamp(obj.Value<float>("x"), -20000, 20000);
                rect.y = Mathf.Clamp(obj.Value<float>("y"), -20000, 20000);
                GraphCanvas.LocalRect = rect;
            }
        }

        void SaveData()
        {
            JObject obj = new JObject();
            obj["scale"] = ScaledCanvas.LocalScale;
            obj["x"] = GraphCanvas.LocalRect.x;
            obj["y"] = GraphCanvas.LocalRect.y;
            EditorPrefs.SetString("can.sav", JsonConvert.SerializeObject(obj));
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
            RootCanvas.LocalRect = r;
            ScaledCanvas.LocalRect = r;
            RootCanvas.CalculateGlobalRect(true);
            RootCanvas.OnGUI(r);
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
            if (focusCenter)
            {
                Vector2 delta =  ScaledCanvas.GlobalCentroid- GraphCanvas.GlobalCentroid;
                if (delta.sqrMagnitude > 1)
                {
                    repaint |= true;
                    Rect rect = GraphCanvas.LocalRect;
                    rect.position += Vector2.Lerp(Vector2.zero, delta / GraphCanvas.GlobalScale, 0.1f);
                    GraphCanvas.LocalRect = rect;
                }
                else
                {
                    focusCenter = false;
                }
            }
        }

        void OnDragBegin()
        {
            RootCanvas.InteractDragBegin(mouseButton, GlobalMousePosition);
        }

        void OnDrag()
        {
            if (RootCanvas.InteractDrag(mouseButton, GlobalMousePosition, mouseDeltaPos))
                return;
            if (mouseButton == EMouseButton.middle || mouseButton == EMouseButton.right)
            {
                Rect rect = GraphCanvas.LocalRect;
                rect.position += mouseDeltaPos / (GraphCanvas.GlobalScale > 0 ? GraphCanvas.GlobalScale : 1);
                GraphCanvas.LocalRect = rect;
            }
        }

        void OnDragEnd()
        {
            RootCanvas.InteractDragEnd(mouseButton, GlobalMousePosition);
        }

        void OnClick()
        {
            RootCanvas.InteractMouseClick(mouseButton, GlobalMousePosition);
        }

        void OnKeyDown()
        {
            RootCanvas.InteractKeyDown(Event.current.keyCode);
        }

        void OnKeyUp()
        {
            if(RootCanvas.InteractKeyUp(Event.current.keyCode))
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
                Vector2 cen = Vector2.zero;
                if (!Event.current.control)
                {
                    cen = GraphCanvas.Parent.CalculateLocalPosition(GlobalMousePosition);
                }
                float f = Mathf.Clamp(ScaledCanvas.LocalScale - Event.current.delta.y * ScaledCanvas.LocalScale * 0.05f, mMinScale, mMaxScale);
                if ((f < 1 && ScaledCanvas.LocalScale > 1) || (f > 1 && ScaledCanvas.LocalScale < 1))
                    f = 1;
                ScaledCanvas.LocalScale = f;
                if (!Event.current.control)
                {
                    Rect r = GraphCanvas.LocalRect;
                    Vector2 p = GraphCanvas.Parent.CalculateLocalPosition(GlobalMousePosition);
                    Vector2 delta = p - cen;
                    r.position += delta;
                    GraphCanvas.LocalRect = r;
                }
                UpdateStateInfo();
            }
        }

    }
}