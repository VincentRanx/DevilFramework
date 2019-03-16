using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [CreateAssetMenu(fileName = "FSM", menuName = "AI/Finite State Machine")]
    public class FStateMachineAsset : ScriptableObject, IFiniteState
    {
        [System.Serializable]
        public class State : IFiniteState, IIdentified
        {
            public int m_Id;
            public string m_Name;
            public bool m_KeepInStack;
            public Vector2 m_Position;
            public BTTaskAsset m_StateTask;

            public int Identify { get { return m_Id; } }

            public string GetName()
            {
                return m_Name;
            }

            public bool KeepInStack()
            {
                return m_KeepInStack;
            }

            public void OnBegin()
            {
                m_StateTask.Start();
            }

            public void OnEnd()
            {
                m_StateTask.Stop();
            }

            public void OnTick(float deltaTime)
            {
                if (m_StateTask.State == EBTState.running)
                {
                    m_StateTask.OnTick(deltaTime);
                }
            }
        }

        [System.Serializable]
        public class Transition : ICondition
        {
            public int m_FromState;
            public int m_ToState;
            public BTConditionAsset m_ConditionAsset;

            public Transition() { }

            public bool IsSuccess
            {
                get
                {
                    if (m_ConditionAsset != null)
                        return m_ConditionAsset.IsSuccess;
                    else
                        return false;
                }
            }
        }

        public Vector2 m_Position;

        [SerializeField]
        bool m_KeepThisInStack;

        [SerializeField]
        List<State> m_States = new List<State>();

        [SerializeField]
        List<Transition> m_Transitions = new List<Transition>();
        
        [SerializeField]
        int m_DefaultState;

        public string GetName()
        {
            return name;
        }

        public IFiniteState GetState(int id)
        {
            return GlobalUtil.Binsearch(m_States, id);
        }

        public bool KeepInStack()
        {
            return m_KeepThisInStack;
        }

        public void OnBegin()
        {
            throw new System.NotImplementedException();
        }

        public void OnEnd()
        {
            throw new System.NotImplementedException();
        }

        public void OnTick(float deltaTime)
        {
            throw new System.NotImplementedException();
        }
    }
}