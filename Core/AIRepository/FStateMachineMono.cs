using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{

    public class FStateMachineMono : MonoBehaviour
    {
        private FStateMachine m_StateMachine;
        [SerializeField]
        private bool m_UseRealTime;
        [SerializeField]
        [Range(0,100)]
        private int m_StackSize = 10;
        [HideInInspector]
        [SerializeField]
        private FiniteState[] m_States;
        [HideInInspector]
        [SerializeField]
        private FiniteStateTransition[] m_Transitions;
        [SerializeField]
        private MonoBehaviour m_OtherImplement;

        protected virtual void Awake()
        {
            m_StateMachine = new FStateMachine(name, m_UseRealTime, m_States.Length, m_Transitions.Length);
            m_StateMachine.MaxStackSize = m_StackSize;
            MonoBehaviour impl = m_OtherImplement ?? this;
            for(int i = 0; i < m_States.Length; i++)
            {
                FiniteState state = m_States[i];
                if (state.Init(impl))
                {
                    m_StateMachine.AddState(state, state.m_IsDefaultState);
                }
            }
            for(int i = 0; i < m_Transitions.Length; i++)
            {
                FiniteStateTransition trans = m_Transitions[i];
                if (trans.Init(impl))
                {
                    m_StateMachine.AddTransition(trans.m_FromState, trans.m_ToState, trans.IsSuccess);
                }
            }
        }

        protected virtual void Start()
        {
            m_StateMachine.OnBegin();
        }

        protected virtual void Update()
        {
            m_StateMachine.OnTick();
        }

        protected virtual void OnDestroy()
        {
            m_StateMachine.Release();
        }

        public FStateMachine FSM { get { return m_StateMachine; } }

        public int StateLength { get { return m_States == null ? 0 : m_States.Length; } }

        public int TransitionLength { get { return m_Transitions == null ? 0 : m_Transitions.Length; } }

        public MonoBehaviour Implement { get { return m_OtherImplement ?? this; } }

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

        private void GenerateStates(System.Type type)
        {
            object[] methods = Utility.Ref.GetMethodsWithAttribute<FStateAttribute>(type, true);
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
                if (!Utility.Ref.MatchMethodRetAndParams(mtd, typeof(void), null))
                {
                    Debug.LogError(string.Format("{0} 不能作为状态方法，因为类型不能和 \" void Method() \" 匹配.", mtd.Name));
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

        public bool HasState(string stateName)
        {
            for(int i = 0; i < StateLength; i++)
            {
                if (m_States[i].m_StateName == stateName)
                    return true;
            }
            return false;
        }

        private void GenerateTransitions(System.Type type)
        {

            object[] methods = Utility.Ref.GetMethodsWithAttribute<FStateTransitionAttribute>(type, true);
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
                    Debug.LogError(string.Format("{0} 不能作为状态方法，因为类型不能和 \" bool Method() \" 匹配.", mtd.Name));
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
            transitions.Sort((x, y) => string.IsNullOrEmpty(x.m_FromState) ? 1 : -1);
            m_Transitions = transitions.ToArray();
        }

        [ContextMenu("初始化状态机", true)]
        public bool CanGenerateStateMachine()
        {
            return !Application.isPlaying;
        }

        [ContextMenu("初始化状态机")]
        public void GenerateStateMachine()
        {
            MonoBehaviour impl = m_OtherImplement ?? this;
            System.Type type = impl.GetType();
            GenerateStates(type);
            GenerateTransitions(type);
            IsDirty = true;
        }

#endif

    }
}