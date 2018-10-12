using Devil.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class FStateMachineMono : MonoBehaviour
    {
        protected FStateMachine m_StateMachine;
        [SerializeField]
        protected bool m_UseRealTime;
        [SerializeField]
        [Range(0,100)]
        protected int m_StackSize = 10;
        [HideInInspector]
        [SerializeField]
        protected FiniteState[] m_States;
        [HideInInspector]
        [SerializeField]
        protected FiniteStateTransition[] m_Transitions;
        [SerializeField]
        protected MonoBehaviour m_OtherImplement;
        [SerializeField]
        protected bool m_IsSubStateMachine;

        protected void InitFSM()
        {
            if (m_StateMachine == null)
            {
                m_StateMachine = new FStateMachine(name, m_States.Length, m_Transitions.Length);
                m_StateMachine.MaxStackSize = m_StackSize;
                MonoBehaviour impl = m_OtherImplement ?? this;
                for (int i = 0; i < m_States.Length; i++)
                {
                    FiniteState state = m_States[i];
                    if (state.Init(impl))
                    {
                        m_StateMachine.AddState(state, state.m_IsDefaultState);
                    }
                }
                for (int i = 0; i < m_Transitions.Length; i++)
                {
                    FiniteStateTransition trans = m_Transitions[i];
                    if (trans.Init(impl))
                    {
                        m_StateMachine.AddTransition(trans.m_FromState, trans.m_ToState, trans.IsSuccess);
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            InitFSM();
        }

        protected virtual void Start()
        {
            if (!m_IsSubStateMachine)
                m_StateMachine.OnBegin();
        }

        protected virtual void Update()
        {
            m_StateMachine.OnTick(m_UseRealTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        protected virtual void OnDestroy()
        {
            if (m_StateMachine != null)
            {
                m_StateMachine.Release();
                m_StateMachine = null;
            }
        }

        public FStateMachine FSM
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return m_StateMachine;
#endif
                InitFSM();
                return m_StateMachine;
            }
        }

        public int StateLength { get { return m_States == null ? 0 : m_States.Length; } }

        public int TransitionLength { get { return m_Transitions == null ? 0 : m_Transitions.Length; } }

        public string CurrentStateName { get { return m_StateMachine == null ? null : m_StateMachine.CurrentStateName; } }

        public float CurrentStateTime { get { return m_StateMachine == null ? 0 : m_StateMachine.CurrentStateTime; } }

        public float TotalRuntime { get { return m_StateMachine == null ? 0 : m_StateMachine.TotalRuntime; } }

        public int CurrentStateTicks { get { return m_StateMachine == null ? 0 : m_StateMachine.CurrentStateTicks; } }

        public int TotalTicks { get { return m_StateMachine == null ? 0 : m_StateMachine.TotalTicks; } }

        public MonoBehaviour Implement
        {
            get { return m_OtherImplement ?? this; }
            set
            {
                if (m_OtherImplement != value)
                {
                    bool isactive = false;
                    if (m_StateMachine != null)
                    {
                        isactive = m_StateMachine.IsFSMActive;
                        m_StateMachine.Release();
                        m_StateMachine = null;
                    }
                    m_OtherImplement = value;
                    var fsm = FSM;
                    if (isactive)
                        fsm.OnBegin();
                }
            }
        }

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

        private void GenerateStates(Type type)
        {
            object[] methods = Ref.GetMethodsWithAttribute<FStateAttribute>(type, true);
            if (methods == null)
            {
                m_States = new FiniteState[0];
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
            m_States = new FiniteState[allstates.Count];
            allstates.Values.CopyTo(m_States, 0);
        }

        private void GenerateTransitions(Type type)
        {

            object[] methods = Ref.GetMethodsWithAttribute<FStateTransitionAttribute>(type, true);
            if (methods == null)
            {
                m_Transitions = new FiniteStateTransition[0];
                return;
            }
            List<FiniteStateTransition> transitions = new List<FiniteStateTransition>();
            for (int i = 0; i < methods.Length; i++)
            {
                System.Reflection.MethodInfo mtd = methods[i] as System.Reflection.MethodInfo;
                if (!Utility.Ref.MatchMethodRetAndParams(mtd, typeof(bool), null))
                {
                    RTLog.LogError(LogCat.AI, string.Format("{0} 不能作为状态方法，因为类型不能和 \" bool Method() \" 匹配.", mtd.Name));
                    continue;
                }
                object[] trans = mtd.GetCustomAttributes(typeof(FStateTransitionAttribute), true);
                for(int j = 0; j < trans.Length; j++)
                {
                    FStateTransitionAttribute t = trans[j] as FStateTransitionAttribute;
                    FiniteStateTransition ft = new FiniteStateTransition();
                    ft.m_FromState = t.From ?? "";
                    if (!string.IsNullOrEmpty(ft.m_FromState) && !HasState(ft.m_FromState))
                        continue;
                    ft.m_ToState = t.To ?? "";
                    if (!string.IsNullOrEmpty(ft.m_ToState) && !HasState(ft.m_ToState))
                        continue;
                    ft.m_ConditionMethod = mtd.Name ?? "";
                    transitions.Add(ft);
                }
            }
            transitions.Sort((x, y) => string.IsNullOrEmpty(x.m_FromState) || string.IsNullOrEmpty(x.m_ToState) ? 1 : -1);
            m_Transitions = transitions.ToArray();
        }

        public virtual void GenerateStateMachine()
        {
            MonoBehaviour impl = m_OtherImplement ?? this;
            Type type = impl.GetType();
            GenerateStates(type);
            GenerateTransitions(type);
            UnityEditor.EditorUtility.SetDirty(this);
            IsDirty = true;
        }

#endif

    }
}