using UnityEngine;
using UnityEditor;
using Devil.AI;
using System.Text;
using System.Collections.Generic;

namespace DevilEditor
{
    [CustomEditor(typeof(FStateMachineMono), true)]
    public class FStateMachineInspector : UnityEditor.Editor
    {
        //static bool reloaded;

        //[InitializeOnLoadMethod]
        //private static void OnInstallize()
        //{
        //    reloaded = true;
        //}

        public void Reposition(FStateMachineMono fsm)
        {
            stateRects = new Rect[fsm.StateLength];
            float deltaRad = 2f * Mathf.PI / fsm.StateLength ;
            float xdy = (defaultCellSize.x / defaultCellSize.y + 1.6f) * 0.5f;
            Vector2 stateCellSize = defaultCellSize * cellScale;
            float r = Mathf.Max(50f * cellScale, stateCellSize.x * fsm.StateLength * 0.4f / Mathf.PI);
            for (int i = 0; i < stateRects.Length; i++)
            {
                float rad = i * deltaRad - Mathf.PI * 0.5f;
                Vector2 center = new Vector2(r * Mathf.Cos(rad) * xdy, r * Mathf.Sin(rad));
                Rect rect = new Rect(center - stateCellSize * 0.5f, stateCellSize);
                stateRects[i] = rect;
            }
        }

        FStateMachineMono fsm;
        FStateMachine fsmImpl;
        Rect[] stateRects;
        Rect stateClipRect;
        Vector2 stateClipRectCenter;
        Vector2 stateViewPos;
        Vector2 stateViewHalfSize = new Vector2(5000, 5000);

        bool foldoutStates = true;
        Vector2 defaultCellSize = new Vector2(100, 30);
        float cellScale = 1;

        Vector2 mousePos;
        Vector2 mouseDeltaPos;
        bool mouseDrag;
        bool focusCenter;
        bool repaint;

        StringBuilder strbuffer = new StringBuilder();

        int interceptedState = -1;
        Vector2 interaceptPos;

        private void OnEnable()
        {
            cellScale = EditorPrefs.GetFloat("fsmScale", 1);
        }

        private void OnDisable()
        {
            EditorPrefs.SetFloat("fsmScale", cellScale);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            repaint = false;
            ProcessMouseDeltaPos();
            DrawFSMGraph();
            ProcessMouseEvent();
            ProcessFocusCenter();
            serializedObject.ApplyModifiedProperties();
            if (repaint || Application.isPlaying)
            {
                repaint = false;
                Repaint();
            }
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
        void ProcessMouseEvent()
        {
            bool intercept = stateClipRect.Contains(Event.current.mousePosition);
            if (!intercept)
            {
                return;
            }
            if (Event.current.type == EventType.ContextClick)
            {
                Event.current.Use();
                focusCenter = true;
                repaint |= true;
                return;
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
            {
                mouseDrag = true;
            }
            else if (mouseDrag && Event.current.type == EventType.MouseDrag)
            {
                repaint |= true;
                stateViewPos -= mouseDeltaPos;
                stateViewPos.x = Mathf.Clamp(stateViewPos.x, -stateViewHalfSize.x, stateViewHalfSize.x);
                stateViewPos.y = Mathf.Clamp(stateViewPos.y, -stateViewHalfSize.y, stateViewHalfSize.y);
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                mouseDrag = false;
            }
        }

        //聚焦状态图中心
        private void ProcessFocusCenter()
        {
            if (focusCenter && Vector2.Distance(Vector2.zero, stateViewPos) > 1)
            {
                repaint |= true;
                stateViewPos = Vector2.Lerp(stateViewPos, Vector2.zero, 0.1f);
            }
            else
            {
                focusCenter = false;
            }
        }

        void DrawFSMGraph()
        {
            FStateMachineMono f = target as FStateMachineMono;
            fsmImpl = f.FSM;
            if (fsm != f)
            {
                fsm = f;
                fsm.IsDirty = true;
            }
            if (fsm.IsDirty)
            {
                Reposition(fsm);
                fsm.IsDirty = false;
            }
            foldoutStates = QuickGUI.DrawHeader("状态预览图", "foldout", false);
            if (foldoutStates)
            {
                QuickGUI.BeginContents(50);
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                float scale = EditorGUILayout.Slider("滑动缩放视图", cellScale, 0.5f, 3f);
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                bool regen = GUILayout.Button("刷新", GUILayout.Height(20));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                if (regen )
                {
                    fsm.GenerateStateMachine();
                    //reloaded = false;
                }
                if (Mathf.Abs(scale - cellScale) > 0.01f)
                {
                    cellScale = scale;
                    fsm.IsDirty = true;
                }
                if (fsm.IsDirty)
                {
                    Reposition(fsm);
                    fsm.IsDirty = false;
                    serializedObject.Update();
                }
                QuickGUI.ReportView(ref stateClipRect, stateViewPos, OnDrawFSMGraphCallback, Mathf.Max(350f, stateClipRect.width * 0.75f), 80);
                stateClipRectCenter = stateClipRect.size * 0.5f;

                QuickGUI.EndContents();
            }
            if (fsm.IsDirty)
            {
                Reposition(fsm);
                fsm.IsDirty = false;
            }

        }

        int GetStateIndex(string state)
        {
            for (int i = 0; i < fsm.StateLength; i++)
            {
                if (fsm.States[i].m_StateName == state)
                    return i;
            }
            return -1;
        }

        Vector2 GetCollidePoint(Rect rect, Vector2 dir)
        {
            Vector2 vec = new Vector2();
            if (dir.x == 0)
            {
                vec.x = rect.center.x;
                vec.y = dir.y > 0 ? rect.yMax : rect.yMin;
            }
            else if (dir.y == 0)
            {
                vec.x = dir.x > 0 ? rect.xMax : rect.xMin;
                vec.y = rect.center.y;
            }
            float xdy = rect.width / rect.height;
            float xdy2 = dir.x / dir.y;
            if (Mathf.Abs(xdy2) < xdy)
            {
                vec.y = dir.y > 0 ? rect.yMax : rect.yMin;
                vec.x = rect.height * 0.5f * xdy2;
                if (dir.y < 0)
                    vec.x = -vec.x;
                vec.x += rect.center.x;
            }
            else
            {
                vec.x = dir.x > 0 ? rect.xMax : rect.xMin;
                vec.y = rect.width * 0.5f / xdy2;
                if (dir.x < 0)
                    vec.y = -vec.y;
                vec.y += rect.center.y;
            }
            return vec;
        }

        void DrawArrow(Vector2 from, Vector2 to)
        {
            Vector2 dir = to - from;
            Handles.DrawLine(from, to);
            Vector2 nor = dir.normalized * 15f;
            Vector2 normal = new Vector2(-nor.y, nor.x);
            normal = normal * 0.5f;
            //Vector2 p = to - nor;
            Vector2 tmp1 = to - (nor - normal);
            Handles.DrawLine(tmp1, to);
            Vector2 tmp2 = to - (nor + normal);
            Handles.DrawLine(tmp2, to);
        }

        void HandleTransition(Rect from, Rect to, Color baseColor)
        {
            Vector2 dir = to.center - from.center;
            Vector2 p0 = GetCollidePoint(from, dir);
            Vector2 p1 = GetCollidePoint(to, -dir);

            Handles.color = baseColor;
            DrawArrow(p0, p1);

        }

        void DrawStateStack()
        {
            if (fsmImpl != null)
            {
                Rect rect = new Rect(10, 10, 40, 30);
                float w = stateClipRect.width - 160;
                int offset = Mathf.Max(0, Mathf.CeilToInt(fsmImpl.StackSize - (w / 101f)));
                if (offset > 0)
                {
                    GUI.Label(rect, "...", "GUIEditor.BreadcrumbMid");
                    rect.x += 41;
                }
                rect.width = 100;
                for (int i = offset; i < fsmImpl.StackSize; i++)
                {
                    GUI.Label(rect, fsmImpl.StateAtStack(i), "GUIEditor.BreadcrumbMid");
                    rect.x += 101;
                }
                GUI.Toggle(rect, true, fsmImpl.CurrentStateName, "GUIEditor.BreadcrumbMid");
            }
        }

        void DrawTransition(bool activeTrans)
        {
            string current = fsmImpl == null ? null : fsmImpl.CurrentStateName;
            int currentIndex = GetStateIndex(current);
            int from, to;
            Rect fromRect, toRect;
            for(int i = 0; i < fsm.TransitionLength; i++)
            {
                FiniteStateTransition trans = fsm.Transitions[i];
                to = GetStateIndex(trans.m_ToState);
                from = GetStateIndex(trans.m_FromState);
                if (to == -1 && from == -1)
                {
                    for (int j = 0; j < stateRects.Length; j++)
                    {
                        if (activeTrans ^ (Application.isPlaying && currentIndex == j))
                            continue;
                        fromRect = stateRects[j];
                        fromRect.y += fromRect.height;
                        fromRect.x -= (defaultCellSize.x * cellScale * 0.5f - 10);
                        toRect = fromRect;
                        toRect.y -= fromRect.height * 2f;
                        fromRect.position -= stateViewPos - stateClipRectCenter;
                        toRect.position -= stateViewPos - stateClipRectCenter;
                        HandleTransition(fromRect, toRect, activeTrans ? Color.green : Color.red);
                    }
                }
                else if (to != -1 && from != -1)
                {
                    if (activeTrans ^ (Application.isPlaying && currentIndex == from))
                        continue;
                    fromRect = stateRects[from];
                    toRect = stateRects[to];
                    fromRect.position -= stateViewPos - stateClipRectCenter;
                    toRect.position -= stateViewPos - stateClipRectCenter;
                    HandleTransition(fromRect, toRect, activeTrans ? Color.green : Color.red);
                }
                else
                {
                    int p = from != -1 ? from : to;
                    if (activeTrans ^ (Application.isPlaying && (currentIndex == from || from == -1)))
                        continue;
                    float sign = p == from ? -1 : 1;
                    fromRect = stateRects[p];
                    fromRect.y -= fromRect.height * sign;
                    fromRect.x += (defaultCellSize.x * cellScale * 0.5f - 10) * sign;
                    toRect = fromRect;
                    toRect.y += fromRect.height * 2f * sign;
                    fromRect.position -= stateViewPos - stateClipRectCenter;
                    toRect.position -= stateViewPos - stateClipRectCenter;
                    HandleTransition(fromRect, toRect, activeTrans ? Color.green : Color.red);
                }
            }
        }

        void DrawStates()
        {
            bool click = Event.current.type == EventType.MouseDown && Event.current.button == 0;
            //bool release = Event.current.type == EventType.mouseUp && Event.current.button == 0;
            if (click)
            {
                interceptedState = -1;
                repaint = true;
            }
            for (int i = 0; i < fsm.StateLength; i++)
            {
                FiniteState state = fsm.States[i];
                bool act;
                if (Application.isPlaying && fsmImpl.IsFSMActive)
                    act = fsmImpl == null ? false : fsmImpl.CurrentStateName == state.m_StateName;
                else
                    act = false;
                Rect rect = stateRects[i];
                rect.position -= stateViewPos - stateClipRectCenter;
                if (click && rect.Contains(Event.current.mousePosition))
                {
                    interceptedState = i;
                    interaceptPos = new Vector2(10, 10);
                }
                bool inter = i == interceptedState;
                string style;
                if (act)
                    style = inter ? "flow node 3 on" : "flow node 3";
                else if(state.m_IsDefaultState)
                    style = inter ? "flow node 2 on" : "flow node 2";
                else
                    style = inter ? "flow node 1 on" : "flow node 1";
                GUI.Label(rect, "", style);
                Installizer.contentContent.text = state.m_StateName;
                Installizer.contentStyle.fontSize = (int)((rect.height - 10) * cellScale);
                Installizer.contentStyle.alignment = TextAnchor.MiddleCenter;
                GUI.Label(rect, Installizer.contentContent, Installizer.contentStyle);

            }
        }

        void DrawStateInfo()
        {
            if (interceptedState != -1)
            {
                Rect rect;
                Vector2 pos = interaceptPos;
                //pos -= stateViewPos - stateClipRectCenter;
                rect = new Rect(pos, new Vector2(stateClipRect.width - 20, stateClipRect.height - 20));
                GUI.skin.label.richText = true;
                FiniteState state = fsm.States[interceptedState];
                strbuffer.Remove(0, strbuffer.Length);
                strbuffer.Append("State: ").Append(state.m_StateName).Append('\n');
                strbuffer.Append("Keep In Stack: ").Append(state.m_KeepInStack).Append('\n');
                if (!string.IsNullOrEmpty(state.m_BeginMethod))
                {
                    strbuffer.Append('\n').Append("OnBegin: void ");
                    strbuffer.Append(fsm.Implement.GetType().Name).Append('.').Append(state.m_BeginMethod).Append(" ()");
                }
                if (!string.IsNullOrEmpty(state.m_TickMethod))
                {
                    strbuffer.Append('\n').Append("OnTick: void ");
                    strbuffer.Append(fsm.Implement.GetType().Name).Append('.').Append(state.m_TickMethod).Append(" ()");
                }
                if (!string.IsNullOrEmpty(state.m_EndMethod))
                {
                    strbuffer.Append('\n').Append("OnEnd: void ");
                    strbuffer.Append(fsm.Implement.GetType().Name).Append('.').Append(state.m_EndMethod).Append(" ()");
                }
                strbuffer.Append('\n');
                for (int i = 0; i < fsm.TransitionLength; i++)
                {
                    FiniteStateTransition trans = fsm.Transitions[i];
                    if (trans.m_ToState == state.m_StateName)
                    {
                        strbuffer.Append("\nFROM \"")
                            .Append(string.IsNullOrEmpty(trans.m_FromState) ? "[Any]" : trans.m_FromState)
                            .Append("\": bool ");
                        strbuffer.Append(fsm.Implement.GetType().Name).Append('.')
                            .Append(trans.m_ConditionMethod).Append(" ()");
                    }
                }
                strbuffer.Append('\n');
                for (int i = 0; i < fsm.TransitionLength; i++)
                {
                    FiniteStateTransition trans = fsm.Transitions[i];
                    if (trans.m_FromState == state.m_StateName || string.IsNullOrEmpty(trans.m_FromState) && trans.m_ToState != state.m_StateName)
                    {
                        strbuffer.Append("\nTO \"")
                            .Append(string.IsNullOrEmpty(trans.m_ToState) ? "[Return]" : trans.m_ToState)
                            .Append("\": bool ");
                        strbuffer.Append(fsm.Implement.GetType().Name).Append('.')
                            .Append(trans.m_ConditionMethod).Append(" ()");
                    }
                }
                if (Application.isPlaying)
                {
                    strbuffer.Append('\n');
                    strbuffer.Append('\n').Append("Ticks: ").Append(fsmImpl.TotalTicks);
                    strbuffer.Append('\n').Append("Time: ").Append(fsmImpl.TotalRuntime.ToString("0.##")).Append('s');
                    if (fsmImpl.CurrentStateName == state.m_StateName)
                    {
                        strbuffer.Append('\n').Append("State Ticks: ").Append(fsmImpl.CurrentStateTicks);
                        strbuffer.Append('\n').Append("State Time: ").Append(fsmImpl.CurrentStateTime.ToString("0.##")).Append('s');
                    }
                }
                GUI.Label(rect, strbuffer.ToString(), "window");
            }
        }

        void OnDrawFSMGraphCallback()
        {
            DrawStates();
            DrawTransition(false);
            if (Application.isPlaying)
                DrawTransition(true);
            DrawStateStack();
            DrawStateInfo();
        }
    }
}