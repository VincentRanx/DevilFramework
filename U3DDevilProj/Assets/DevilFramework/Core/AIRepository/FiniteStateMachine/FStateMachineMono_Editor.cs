using Devil.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Devil.AI
{
    public partial class FStateMachineMono
    {

#if UNITY_EDITOR
        /// <summary>
        /// 注意，这个属性只对Editor模式提供
        /// </summary>
        public FiniteState[] States { get { return m_States; } }
        /// <summary>
        /// 注意，这个属性只对Editor模式提供
        /// </summary>
        public FiniteStateTransition[] Transitions { get { return m_Transitions; } }

        public bool IsDirty { get; set; }

        public bool HasState(string stateName)
        {
            for (int i = 0; i < StateLength; i++)
            {
                if (m_States[i].m_StateName == stateName)
                    return true;
            }
            return false;
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

        public static void CopyTransitionToSerializedField(FiniteStateTransition trans, SerializedProperty property)
        {
            var prop = property.FindPropertyRelative("m_ConditionMethod");
            prop.stringValue = trans.m_ConditionMethod;
            prop = property.FindPropertyRelative("m_FromState");
            prop.stringValue = trans.m_FromState;
            prop = property.FindPropertyRelative("m_ToState");
            prop.stringValue = trans.m_ToState;
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
                    if (!string.IsNullOrEmpty(ft.m_FromState) && !fsm.HasState(ft.m_FromState))
                        continue;
                    ft.m_ToState = t.To ?? "";
                    if (!string.IsNullOrEmpty(ft.m_ToState) && !fsm.HasState(ft.m_ToState))
                        continue;
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

        //public static void GenerateStateMachine(FStateMachineMono fsm)
        //{
        //    MonoBehaviour impl = fsm.m_OtherImplement == null ? fsm : fsm.m_OtherImplement;
        //    Type type = impl.GetType();
        //    var ser = new SerializedObject(fsm);
        //    ser.Update();
        //    GenerateStates(ser, type);
        //    GenerateTransitions(ser, type);
        //    ser.ApplyModifiedPropertiesWithoutUndo();
        //    EditorUtility.SetDirty(fsm);
        //    fsm.IsDirty = true;
        //}

        public virtual void GenerateStateMachine()
        {
            MonoBehaviour impl = m_OtherImplement == null ? this : m_OtherImplement;
            Type type = impl.GetType();
            var ser = new SerializedObject(this);
            ser.Update();
            GenerateStates(ser, type);
            GenerateTransitions(ser, type);
            ser.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(this);
            this.IsDirty = true;
        }
#endif

    }
}