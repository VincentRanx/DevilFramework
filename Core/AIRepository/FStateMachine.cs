using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{
    public interface IFiniteState
    {
        string GetName();

        //是否保留到状态堆栈中
        bool KeepInStack();

        // 状态开始
        void OnBegin();

        // 执行状态的更新方法
        void OnTick();

        // 状态结束
        void OnEnd();
    }

    public class FStateMachine : IFiniteState
    {
        public delegate bool TransitionCondition();

        public class Transition
        {
            public string m_FromState;
            public string m_ToState;
            public TransitionCondition m_Condition;
        }

        #region fields

        // 状态机名字
        private string m_Name;
        // 所有的状态列表
        private Dictionary<string, IFiniteState> m_States;
        // 所有的条件列表
        private List<Transition> m_Transitions;
        private string m_DefaultState;
        // 当前状态名字
        private string m_CurrentStateName;
        // 当前状态
        private IFiniteState m_CurrentState;
        // 前一个状态
        private IFiniteState m_PrevState;
        // 当前检测的条件列表
        private List<Transition> m_CurrentTransitions;
        // tick次数
        private int m_TickCount;
        private int m_StateTickCount;

        private bool m_UseRealTime;
        // 总体开始时间
        private float m_StartTime;
        // 总体运行时间
        private float m_Runtime;
        // 当前状态开始时间
        private float m_StateStartTime;
        // 当前状态运行时间
        private float m_StateRuntime;
        // 是否运行
        private bool m_Active;
        private int m_StateCount;
        private int m_StackSize;
        private List<string> m_StateStack; // 状态切换堆栈，当To状态没有明确指定时，返回上一次状态
        #endregion

        #region property

        public string CurrentStateName { get { return m_CurrentStateName; } }
        public float TotalRuntime { get{ return m_Runtime; } }
        public float CurrentStateTime { get { return m_StateRuntime; } }
        public bool IsRealtime { get { return m_UseRealTime; } }
        public int TotalTicks { get { return m_TickCount; } }
        public int CurrentStateTicks { get { return m_StateTickCount; } }
        public int StackSize { get { return m_StateStack.Count; } }
        public string StateAtStack(int index) { return m_StateStack[index]; }
        public string StateStackPeek { get { return m_StateStack.Count > 0 ? m_StateStack[m_StateStack.Count - 1] : null; } }
        public int MaxStackSize { get { return m_StackSize; } set { m_StackSize = Mathf.Max(0, value); } }
       
        #endregion

        #region constructor

        public FStateMachine(bool useRealTime = false)
        {
            m_Name = "FiniteStateMachine";
            m_States = new Dictionary<string, IFiniteState>();
            m_Transitions = new List<Transition>();
            m_CurrentTransitions = new List<Transition>();
            m_StateStack = new List<string>();
            m_UseRealTime = useRealTime;
            m_StackSize = 10;
        }

        public FStateMachine(string name,bool useRealTime = false, int stateCapacity = 0, int transitionCapactity = 0)
        {
            m_Name = name;
            m_States = new Dictionary<string, IFiniteState>(stateCapacity);
            m_Transitions = new List<Transition>(transitionCapactity);
            m_CurrentTransitions = new List<Transition>();
            m_StateStack = new List<string>();
            m_UseRealTime = useRealTime;
            m_StackSize = 10;
        }
        
        #endregion

        #region private methods

        // 更新当前状态切换条件
        void UpdateTransitions()
        {
            m_CurrentTransitions.Clear();
            for (int i = 0; i < m_Transitions.Count; i++)
            {
                Transition t = m_Transitions[i];
                if (t.m_FromState == m_CurrentStateName || string.IsNullOrEmpty(t.m_FromState) && t.m_ToState != m_CurrentStateName)
                {
                    m_CurrentTransitions.Add(t);
                }
            }
        }

        Transition FindSuccessCondition()
        {
            for (int i = 0; i < m_CurrentTransitions.Count; i++)
            {
                Transition t = m_CurrentTransitions[i];
                if (t.m_Condition())
                {
                    return t;
                }
            }
            return null;
        }

        void ChangeState(Transition trans)
        {
            string toState = trans.m_ToState;
            bool push = true;
            if (string.IsNullOrEmpty(toState))
            {
                if (m_StateStack.Count > 0)
                {
                    toState = m_StateStack[m_StateStack.Count - 1];
                    m_StateStack.RemoveAt(m_StateStack.Count - 1);
                    push = false;
                }
                else
                {
                    return;
                }
            }
            if (toState == m_CurrentStateName)
                return;
            m_PrevState = m_CurrentState;
            m_CurrentState = null;
            m_CurrentStateName = toState;
            if (m_PrevState != null)
            {
                m_PrevState.OnEnd();
                // 入栈
                if (m_StackSize > 0 && push && m_PrevState.KeepInStack() && StateStackPeek != m_PrevState.GetName())
                {
                    int del = m_StateStack.Count + 1 - m_StackSize;
                    if (del > 0)
                        m_StateStack.RemoveRange(0, del);
                    m_StateStack.Add(m_PrevState.GetName());
                }
            }
            UpdateTransitions();
        }

        #endregion

        #region opened method

        public void AddState(IFiniteState state, bool isDefaultState = false)
        {
            if (state == null || state.GetName() == null)
            {
                return;
            }
            m_States[state.GetName()] = state;
            if (isDefaultState || string.IsNullOrEmpty(m_DefaultState))
            {
                m_DefaultState = state.GetName();
            }
            m_StateCount = m_States.Count;
        }

        /// <summary>
        /// 添加状态切换条件，from 不指定时表示任意状态， to 不指定时表示堆栈中的上一次的状态
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="condition"></param>
        public void AddTransition(string from, string to, TransitionCondition condition)
        {
            if (condition == null)
            {
                return;
            }
            Transition trans = new Transition();
            trans.m_FromState = from;
            trans.m_ToState = to;
            trans.m_Condition = condition;
            m_Transitions.Add(trans);
        }

        // 释放对象
        public void Release()
        {
            if(m_Active)
            {
                OnEnd();
            }
            m_DefaultState = null;
            m_Transitions.Clear();
            m_States.Clear();
            m_StateCount = 0;
        }

        public void SetDefaultState(string stateName)
        {
            m_DefaultState = stateName;
        }

        public string GetName()
        {
            return m_Name;
        }

        public bool KeepInStack()
        {
            return true;
        }

        // 开始状态机
        public void OnBegin()
        {
#if UNITY_EDITOR
            Debug.Log("Begin StateMachine: " + m_Name + " @ " + m_TickCount);
#endif
            m_Active = true;
            m_StartTime = m_UseRealTime ? Time.realtimeSinceStartup : Time.time;
            m_StateStartTime = m_StartTime;
            m_Runtime = 0;
            m_StateStartTime = 0;
            m_TickCount = 0;
            m_StateTickCount = 0;
            m_PrevState = null;
            m_CurrentState = null;
            m_CurrentStateName = m_DefaultState;
            UpdateTransitions();
        }

        // 更新状态
        public void OnTick()
        {
            if (m_Active)
            {
                m_TickCount++;
                float t = m_UseRealTime ? Time.realtimeSinceStartup : Time.time;
                m_Runtime = t - m_StartTime;
                if (m_StateCount == 0)
                    return;
                if (m_CurrentState != null)
                {
                    m_StateRuntime = t - m_StateStartTime;
                    m_StateTickCount++;
                }
                else if (m_States.TryGetValue(m_CurrentStateName, out m_CurrentState))
                {
                    m_StateStartTime = t;
                    m_StateRuntime = 0;
                    m_CurrentState.OnBegin();
                    m_StateTickCount = 1;
                }
                if (m_CurrentState != null)
                {
                    m_CurrentState.OnTick();
                }
                Transition trans = FindSuccessCondition();
                if (trans != null)
                {
                    ChangeState(trans);
                }
            }
        }

        public void OnEnd()
        {
            if (m_CurrentState != null)
            {
                m_CurrentState.OnEnd();
            }
#if UNITY_EDITOR
            Debug.Log("End StateMachine: " + m_Name + " @ " + m_TickCount);
#endif
            m_TickCount = 0;
            m_CurrentState = null;
            m_PrevState = null;
            m_CurrentStateName = null;
            m_CurrentTransitions.Clear();
            m_Active = false;
        }

        public string GetCurrentStateName()
        {
            return m_CurrentStateName;
        }

        #endregion
    }
}