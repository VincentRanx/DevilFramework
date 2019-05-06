using UnityEngine;
using UnityEditor;
using Devil.AI;
using System.Text;
using Devil.Utility;
using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace DevilEditor
{
    [CustomEditor(typeof(FStateMachineMono), true)]
    public class FStateMachineInspector : Editor
    {
        public class Arrow
        {
            public int from;
            public int to;
            public bool twoSide;
        }
        
        public static void CopyStateToSerializedField(FiniteState stat, SerializedProperty property)
        {
            var prop = property.FindPropertyRelative("m_StateName");
            prop.stringValue = stat.m_StateName;
            prop = property.FindPropertyRelative("m_IsDefaultState");
            prop.boolValue = stat.m_IsDefaultState;
            prop = property.FindPropertyRelative("m_KeepInStack");
            prop.boolValue = stat.m_KeepInStack;
            prop = property.FindPropertyRelative("m_BeginMethod");
            prop.stringValue = stat.m_BeginMethod;
            prop = property.FindPropertyRelative("m_TickMethod");
            prop.stringValue = stat.m_TickMethod;
            prop = property.FindPropertyRelative("m_EndMethod");
            prop.stringValue = stat.m_EndMethod;
            prop = property.FindPropertyRelative("m_UseSubState");
            prop.boolValue = stat.m_UseSubState;
        }

        public static void CopySerializedField(SerializedProperty property, FiniteState stat)
        {
            var prop = property.FindPropertyRelative("m_StateName");
            stat.m_StateName = prop.stringValue;
            prop = property.FindPropertyRelative("m_IsDefaultState");
            stat.m_IsDefaultState = prop.boolValue;
            prop = property.FindPropertyRelative("m_KeepInStack");
            stat.m_KeepInStack = prop.boolValue;
            prop = property.FindPropertyRelative("m_BeginMethod");
            stat.m_BeginMethod = prop.stringValue;
            prop = property.FindPropertyRelative("m_TickMethod");
            stat.m_TickMethod = prop.stringValue;
            prop = property.FindPropertyRelative("m_EndMethod");
            stat.m_EndMethod = prop.stringValue;
            prop = property.FindPropertyRelative("m_UseSubState");
            stat.m_UseSubState = prop.boolValue;
        }

        public static void CopyTransitionToSerializedField(FiniteStateTransition trans, SerializedProperty property)
        {
            var prop = property.FindPropertyRelative("m_ConditionMethod");
            prop.stringValue = trans.m_ConditionMethod;
            prop = property.FindPropertyRelative("m_FromState");
            prop.stringValue = trans.m_FromState;
            prop = property.FindPropertyRelative("m_ToState");
            prop.stringValue = trans.m_ToState;
        }

        public static void CopySerializedField(SerializedProperty property, FiniteStateTransition trans)
        {
            var prop = property.FindPropertyRelative("m_ConditionMethod");
            trans.m_ConditionMethod = prop.stringValue;
            prop = property.FindPropertyRelative("m_FromState");
            trans.m_FromState = prop.stringValue;
            prop = property.FindPropertyRelative("m_ToState");
            trans.m_ToState = prop.stringValue;
        }

        private static void GenerateStates(SerializedObject target, Type type)
        {
            var prop = target.FindProperty("m_States");
            object[] methods = Ref.GetMethodsWithAttribute<FStateAttribute>(type, true);
            if (methods == null)
            {
                prop.ClearArray();
                return;
            }
            FiniteState tmp;
            FiniteState defaultState = null;
            FiniteState firstState = null;
            Dictionary<string, FiniteState> allstates = new Dictionary<string, FiniteState>();
            for (int i = 0; i < methods.Length; i++)
            {
                System.Reflection.MethodInfo mtd = methods[i] as System.Reflection.MethodInfo;
                if (!Ref.MatchMethodRetAndParams(mtd, typeof(void), null))
                {
                    RTLog.LogError(LogCat.AI, string.Format("{0} 不能作为状态方法，因为类型不能和 \" void Method() \" 匹配.", mtd.Name));
                    continue;
                }
                object[] states = mtd.GetCustomAttributes(typeof(FStateAttribute), true);
                for (int j = 0; j < states.Length; j++)
                {
                    FStateAttribute state = states[j] as FStateAttribute;
                    string sname = state.Name;
                    if (string.IsNullOrEmpty(sname))
                    {
                        sname = mtd.Name;
                    }
                    if (!allstates.TryGetValue(sname, out tmp))
                    {
                        tmp = new FiniteState();
                        tmp.m_StateName = sname;
                        allstates[sname] = tmp;
                    }
                    if (firstState == null)
                    {
                        firstState = tmp;
                    }
                    if (state.IsDefault)
                    {
                        tmp.m_IsDefaultState = true;
                        if (defaultState != null)
                            defaultState.m_IsDefaultState = false;
                        defaultState = tmp;
                    }
                    if (state.KeepInStack)
                        tmp.m_KeepInStack = true;
                    if (state.IsSubState)
                        tmp.m_UseSubState = true;
                    if ((state.Event & EStateEvent.OnBegin) != 0)
                    {
                        tmp.m_BeginMethod = mtd.Name;
                    }
                    if ((state.Event & EStateEvent.OnTick) != 0)
                    {
                        tmp.m_TickMethod = mtd.Name;
                    }
                    if ((state.Event & EStateEvent.OnEnd) != 0)
                    {
                        tmp.m_EndMethod = mtd.Name;
                    }
                }
            }
            if (defaultState == null && firstState != null)
                firstState.m_IsDefaultState = true;
            prop.arraySize = allstates.Count;
            int num = 0;
            foreach (var stat in allstates.Values)
            {
                var p = prop.GetArrayElementAtIndex(num++);
                CopyStateToSerializedField(stat, p);
            }
        }

        private static void GenerateTransitions(SerializedObject target, Type type)
        {
            var fsm = target.targetObject as FStateMachineMono;
            var prop = target.FindProperty("m_Transitions");
            object[] methods = Ref.GetMethodsWithAttribute<FStateTransitionAttribute>(type, true);
            if (methods == null)
            {
                prop.ClearArray();
                return;
            }
            List<FiniteStateTransition> transitions = new List<FiniteStateTransition>();
            for (int i = 0; i < methods.Length; i++)
            {
                System.Reflection.MethodInfo mtd = methods[i] as System.Reflection.MethodInfo;
                if (!Ref.MatchMethodRetAndParams(mtd, typeof(bool), null))
                {
                    RTLog.LogError(LogCat.AI, string.Format("{0} 不能作为状态方法，因为类型不能和 \" bool Method() \" 匹配.", mtd.Name));
                    continue;
                }
                object[] trans = mtd.GetCustomAttributes(typeof(FStateTransitionAttribute), true);
                for (int j = 0; j < trans.Length; j++)
                {
                    FStateTransitionAttribute t = trans[j] as FStateTransitionAttribute;
                    FiniteStateTransition ft = new FiniteStateTransition();
                    ft.m_FromState = t.From ?? "";
                    //if (!string.IsNullOrEmpty(ft.m_FromState) && !fsm.HasState(ft.m_FromState))
                    //    continue;
                    ft.m_ToState = t.To ?? "";
                    //if (!string.IsNullOrEmpty(ft.m_ToState) && !fsm.HasState(ft.m_ToState))
                    //    continue;
                    ft.m_ConditionMethod = mtd.Name ?? "";
                    transitions.Add(ft);
                }
            }
            GlobalUtil.Sort(transitions, (x, y) => string.IsNullOrEmpty(x.m_FromState) || string.IsNullOrEmpty(x.m_ToState) ? 1 : -1);
            prop.arraySize = transitions.Count;
            for (int i = 0; i < transitions.Count; i++)
            {
                var p = prop.GetArrayElementAtIndex(i);
                CopyTransitionToSerializedField(transitions[i], p);
            }
        }

        public static void GenerateStateMachine(SerializedObject target)
        {
            var t = target.FindProperty("m_OtherImplement");
            var impl = (t.objectReferenceValue == null ? target.targetObject : t.objectReferenceValue);
            Type type = impl.GetType();
            GenerateStates(target, type);
            GenerateTransitions(target, type);
            target.ApplyModifiedProperties();
            EditorUtility.SetDirty(target.targetObject);

            var go = target.targetObject as FStateMachineMono;
            if(go != null && go.gameObject.activeInHierarchy)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        [MenuItem("GameObject/Utils/Update FSM", priority = -1)]
        public static void UpdateFSM()
        {
            var go = Selection.activeGameObject;
            if (go == null)
                return;
            var fsms = go.GetComponentsInChildren<FStateMachineMono>(true);
            foreach(var fsm in fsms)
            {
                var ser = new SerializedObject(fsm);
                GenerateStateMachine(ser);
                ser.ApplyModifiedProperties();
                Debug.Log("Update FSM: " + fsm, fsm);
            }
            if (go.activeInHierarchy)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
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

        SerializedProperty m_States;
        FiniteState[] m_StateCopys;
        SerializedProperty m_Transitions;
        FiniteStateTransition[] m_TransitionCopys;
        List<Arrow> m_TransArrows = new List<Arrow>();
        SerializedProperty m_OtherImplement;
        bool focusGraph;

        private void OnEnable()
        {
            cellScale = EditorPrefs.GetFloat("fsmScale", 1);
            m_States = serializedObject.FindProperty("m_States");
            m_Transitions = serializedObject.FindProperty("m_Transitions");
            m_OtherImplement = serializedObject.FindProperty("m_OtherImplement");
            GetCopys();
        }

        void GetCopys()
        {
            m_StateCopys = new FiniteState[m_States.arraySize];
            for (int i = 0; i < m_StateCopys.Length; i++)
            {
                m_StateCopys[i] = new FiniteState();
                CopySerializedField(m_States.GetArrayElementAtIndex(i), m_StateCopys[i]);
            }
            m_TransitionCopys = new FiniteStateTransition[m_Transitions.arraySize];
            for (int i = 0; i < m_TransitionCopys.Length; i++)
            {
                m_TransitionCopys[i] = new FiniteStateTransition();
                CopySerializedField(m_Transitions.GetArrayElementAtIndex(i), m_TransitionCopys[i]);
            }

            m_TransArrows.Clear();
            HashSet<int> execlude = new HashSet<int>();
            for(int i = 0; i < m_TransitionCopys.Length; i++)
            {
                if (execlude.Contains(i))
                    continue;
                var arrow = new Arrow();
                var trans = m_TransitionCopys[i];
                arrow.from = GlobalUtil.FindIndex(m_StateCopys, (x) => x.m_StateName == trans.m_FromState);
                arrow.to = GlobalUtil.FindIndex(m_StateCopys, (x) => x.m_StateName == trans.m_ToState);
                var reverse = arrow.from == -1 || arrow.to == -1 ? -1 : GlobalUtil.FindIndex(m_TransitionCopys, (x) => x.m_FromState == trans.m_ToState && x.m_ToState == trans.m_FromState);
                if (reverse != -1)
                {
                    execlude.Add(reverse);
                    arrow.twoSide = true;
                }
                m_TransArrows.Add(arrow);
            }

            Reposition();
        }

        void Reposition()
        {
            stateRects = new Rect[m_StateCopys.Length];
            float deltaRad = 2f * Mathf.PI / m_StateCopys.Length;
            float xdy = (defaultCellSize.x / defaultCellSize.y + 1.6f) * 0.5f;
            Vector2 stateCellSize = defaultCellSize * cellScale;
            float r = Mathf.Max(50f * cellScale, stateCellSize.x * m_StateCopys.Length * 0.4f / Mathf.PI);
            for (int i = 0; i < stateRects.Length; i++)
            {
                float rad = i * deltaRad - Mathf.PI * 0.5f;
                Vector2 center = new Vector2(r * Mathf.Cos(rad) * xdy, r * Mathf.Sin(rad));
                Rect rect = new Rect(center - stateCellSize * 0.5f, stateCellSize);
                stateRects[i] = rect;
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetFloat("fsmScale", cellScale);
        }

        public override void OnInspectorGUI()
        {
            fsm = target as FStateMachineMono;
            fsmImpl = fsm.FSM;

            base.OnInspectorGUI();
            bool repos = false;
            bool regen = false;

            ProcessMouseDeltaPos();
            DrawFSMGraph(ref regen, ref repos);
            ProcessMouseEvent(ref regen, ref repos);
            ProcessFocusCenter();
            serializedObject.ApplyModifiedProperties();
            if (regen)
            {
                GenerateStateMachine(serializedObject);
                GetCopys();
            }
            else if (repos)
            {
                GetCopys();
            }
            if (repaint || regen || repos || Application.isPlaying)
            {
                Repaint();
                repaint = false;
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
        void ProcessMouseEvent(ref bool regen, ref bool repos)
        {
            bool intercept = stateClipRect.Contains(Event.current.mousePosition);
            if (!intercept)
            {
                focusGraph = false;
                return;
            }
            if (Event.current.type == EventType.MouseDown)
                focusGraph = true;
            if (Event.current.type == EventType.ContextClick)
            {
                Event.current.Use();
                focusCenter = true;
                focusGraph = true;
                repaint |= true;
                return;
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 2)
            {
                mouseDrag = true;
                repaint |= true;
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
                repaint |= true;
            }
            else if(focusGraph && Event.current.type == EventType.ScrollWheel )
            {
                float f = Mathf.Clamp(cellScale - Event.current.delta.y * cellScale * 0.05f, 0.1f, 2f);
                if ((f < 1 && cellScale > 1) || (f > 1 && cellScale < 1))
                    f = 1;
                cellScale = f;
                repos |= true;
                Event.current.Use();
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

        void DrawFSMGraph(ref bool regen, ref bool repos)
        {
            foldoutStates = QuickGUI.DrawHeader("状态预览图", "foldout", false);
            if (foldoutStates)
            {
                QuickGUI.BeginContents(50);
                //EditorGUILayout.BeginVertical("helpbox");

                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.EndHorizontal();
                
                QuickGUI.ReportView(ref stateClipRect, stateViewPos, OnDrawFSMGraphCallback, Mathf.Max(350f, stateClipRect.width * 0.75f), 80);
                stateClipRectCenter = stateClipRect.size * 0.5f;

                EditorGUI.BeginDisabledGroup(Application.isPlaying);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(40);
                regen |= GUILayout.Button("Rebuild State Machine");
                GUILayout.Space(40);
                EditorGUILayout.EndHorizontal();

                EditorGUI.EndDisabledGroup();

                QuickGUI.EndContents();
                //EditorGUILayout.EndVertical();
            }
            
        }

        int GetStateIndex(string state)
        {
            for (int i = 0; i < m_StateCopys.Length; i++)
            {
                if (m_StateCopys[i].m_StateName == state)
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

        void HandleTransition(Rect from, Rect to, Color baseColor, bool twoside)
        {
            Vector2 dir = to.center - from.center;
            Vector2 p0 = GetCollidePoint(from, dir);
            Vector2 p1 = GetCollidePoint(to, -dir);

            if (twoside)
                DevilEditorUtility.Draw2SideArrow(p0, p1, baseColor, 3 * cellScale);
            else
                DevilEditorUtility.DrawArrow(p0, p1, baseColor, 3 * cellScale);
            //Handles.color = baseColor;
            //DrawArrow(p0, p1);

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
            //int from, to;
            Rect fromRect, toRect;
            for (int i = 0; i < m_TransArrows.Count; i++)
            {
                var arrow = m_TransArrows[i];
                if(arrow.from == -1 && arrow.to == -1)
                {
                    continue;
                }
                if(arrow.from != -1 && arrow.to != -1)
                {
                    bool active = activeTrans && (arrow.from == currentIndex || arrow.to == currentIndex);
                    fromRect = stateRects[arrow.from];
                    toRect = stateRects[arrow.to];
                    fromRect.position -= stateViewPos - stateClipRectCenter;
                    toRect.position -= stateViewPos - stateClipRectCenter;
                    HandleTransition(fromRect, toRect, active ? Color.green : Color.red, arrow.twoSide);
                }
                else
                {
                    int p = arrow.from != -1 ? arrow.from : arrow.to;
                    float sign = p == arrow.from ? -1 : 1;
                    bool active = p == currentIndex && activeTrans;
                    fromRect = stateRects[p];
                    fromRect.y -= fromRect.height * sign;
                    fromRect.x += (defaultCellSize.x * cellScale * 0.5f - 10) * sign;
                    toRect = fromRect;
                    toRect.y += fromRect.height * 2f * sign;
                    fromRect.position -= stateViewPos - stateClipRectCenter;
                    toRect.position -= stateViewPos - stateClipRectCenter;
                    HandleTransition(fromRect, toRect, active ? Color.green : Color.red, arrow.twoSide);
                }
            }
            
        }

        void DrawStates()
        {
            bool click = Event.current.type == EventType.MouseDown && Event.current.button == 0;
            if (click)
            {
                interceptedState = -1;
                repaint = true;
            }
            for (int i = 0; i < m_StateCopys.Length; i++)
            {
                FiniteState state = m_StateCopys[i];
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
                Installizer.contentStyle.normal.textColor = Color.white;
                Installizer.contentStyle.fontSize = Mathf.Max(1, (int)((rect.height - 10) * cellScale));
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
                FiniteState state = m_StateCopys[interceptedState];
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
                    FiniteStateTransition trans = m_TransitionCopys[i];
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
                    FiniteStateTransition trans = m_TransitionCopys[i];
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
            DrawTransition(Application.isPlaying);
            DrawStates();
            //if (Application.isPlaying)
            //    DrawTransition(true);
            DrawStateStack();
            DrawStateInfo();
        }
    }
}