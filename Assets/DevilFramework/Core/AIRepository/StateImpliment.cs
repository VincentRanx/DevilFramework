using UnityEngine;

namespace DevilTeam.AI
{
    public enum EStateEvent
    {
        OnTick = 1,
        OnBegin = 2,
        OnEnd = 4,
    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class FStateAttribute : System.Attribute
    {
        public string Name { get; set; }
        public EStateEvent Event { get; set; }
        public bool IsDefault { get; set; }
        public bool KeepInStack { get; set; }

        public FStateAttribute()
        {
            Event = EStateEvent.OnTick;
        }

        public FStateAttribute(string stateName)
        {
            Name = stateName;
            Event = EStateEvent.OnTick;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class FStateTransitionAttribute : System.Attribute
    {
        public string From { get; set; }
        public string To { get; set; }

        public FStateTransitionAttribute() { }

        public FStateTransitionAttribute(string from, string to)
        {
            From = from;
            To = to;
        }
    }

    [System.Serializable]
    public class FiniteState : IFiniteState
    {
        public string m_StateName;

        public bool m_IsDefaultState;

        public bool m_KeepInStack;

        public string m_BeginMethod;

        public string m_TickMethod;

        public string m_EndMethod;

        private System.Action m_BeginDelegate;
        private System.Action m_TickDelegate;
        private System.Action m_EndDelegate;

        public bool Init<T>(T target) where T : MonoBehaviour
        {
            if (target == null)
                return false;
            bool ret = false;
            if (!string.IsNullOrEmpty(m_BeginMethod))
            {
                m_BeginDelegate = (System.Action)System.Delegate.CreateDelegate(typeof(System.Action), target, m_BeginMethod);
                ret = true;
            }
            if (!string.IsNullOrEmpty(m_TickMethod))
            {
                m_TickDelegate = (System.Action)System.Delegate.CreateDelegate(typeof(System.Action), target, m_TickMethod);
                ret = true;
            }
            if (!string.IsNullOrEmpty(m_EndMethod))
            {
                m_EndDelegate = (System.Action)System.Delegate.CreateDelegate(typeof(System.Action), target, m_EndMethod);
                ret = true;
            }
            return ret;
        }

        public string GetName()
        {
            return m_StateName;
        }

        public bool KeepInStack()
        {
            return m_KeepInStack;
        }

        public void OnBegin()
        {
#if UNITY_EDITOR
            Debug.Log(string.Format("Begin State: {0}", m_StateName));
#endif
            if (m_BeginDelegate != null)
            {
                m_BeginDelegate();
            }
        }


        public void OnEnd()
        {
#if UNITY_EDITOR
            Debug.Log(string.Format("End State: {0}", m_StateName));
#endif
            if (m_EndDelegate != null)
            {
                m_EndDelegate();
            }
        }

        public void OnTick()
        {
            if (m_TickDelegate != null)
            {
                m_TickDelegate();
            }
        }
    }


    [System.Serializable]
    public class FiniteStateTransition
    {
        public string m_FromState;
        public string m_ToState;
        public string m_ConditionMethod;

        private ValueDelegate<bool> m_CondtionDelegate;


        public bool Init<T>(T target) where T : MonoBehaviour
        {
            if (target == null)
                return false;
            if (!string.IsNullOrEmpty(m_ConditionMethod))
            {
                m_CondtionDelegate = (ValueDelegate<bool>)System.Delegate.CreateDelegate(typeof(ValueDelegate<bool>), target, m_ConditionMethod);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsSuccess()
        {
            if (m_CondtionDelegate != null)
            {
                return m_CondtionDelegate();
            }
            else
            {
                return false;
            }
        }
    }

    // 通过委托实现的状态
    public class StateDelegate<T> : IFiniteState
    {
        public T m_StateData;
        public bool m_KeepStack;
        public event System.Action<T> OnBeginDelegate;
        public event System.Action<T> OnTickDelegate;
        public event System.Action<T> OnEndDelegate;
        private string m_Name;

        public StateDelegate(string name)
        {
            m_Name = name;
        }

        public StateDelegate(string name, T stateData)
        {
            m_Name = name;
            m_StateData = stateData;
        }

        public string GetName()
        {
            return m_Name;
        }

        public bool KeepInStack()
        {
            return m_KeepStack;
        }

        public void OnBegin()
        {
            if (OnBeginDelegate != null)
            {
                OnBeginDelegate(m_StateData);
            }
        }

        public void OnTick()
        {
            if (OnTickDelegate != null)
            {
                OnTickDelegate(m_StateData);
            }
        }

        public void OnEnd()
        {
            if (OnEndDelegate != null)
            {
                OnEndDelegate(m_StateData);
            }
        }

    }
}