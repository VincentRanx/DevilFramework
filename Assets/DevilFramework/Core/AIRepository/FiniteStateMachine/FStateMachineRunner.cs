using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{
    public class FStateMachineRunner : MonoBehaviour
    {
        FStateMachine m_StateMachine;

        [SerializeField]
        string m_Uid;
        [SerializeField]
        bool m_UseRealTime;
        [SerializeField]
        [Range(0,100)]
        int m_StackSize = 10;
        [HideInInspector]
        [SerializeField]
        FiniteState[] m_States;
        [HideInInspector]
        [SerializeField]
        FiniteStateTransition[] m_Transitions;
        [SerializeField]
        MonoBehaviour m_OtherImplement;

        void Awake()
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

        void Start()
        {
            m_StateMachine.OnBegin();
        }

        void Update()
        {
            m_StateMachine.OnTick();
        }

        void OnDestroy()
        {
            m_StateMachine.Release();
        }

        public FStateMachine FSM { get { return m_StateMachine; } }

        public int StateLength { get { return m_States == null ? 0 : m_States.Length; } }

        public int TransitionLength { get { return m_Transitions == null ? 0 : m_Transitions.Length; } }

        public MonoBehaviour Implement { get { return m_OtherImplement ?? this; } }

    }
}