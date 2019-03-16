using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public class ComponentGroup<T> : MonoBehaviour where T : Component
    {
        protected int m_ComponentCount;

        protected T[] m_Components;

        protected virtual void OnPreInit() { }

        protected virtual void OnPostInit() { }

        protected void DestroyForThis(Object obj)
        {
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        public void InitComponents()
        {
            OnPreInit();
            m_Components = GetComponents<T>();
            m_ComponentCount = Mathf.Max(0, m_ComponentCount);
            int len = m_Components == null ? 0 : m_Components.Length;
            if (m_ComponentCount != len)
            {
                while (len > m_ComponentCount)
                {
                    DestroyForThis(m_Components[--len]);
                }
                T[] components = new T[m_ComponentCount];
                int min = Mathf.Min(m_ComponentCount, len);
                if (min > 0)
                {
                    System.Array.Copy(m_Components, components, min);
                }
                while (min < m_ComponentCount)
                {
                    components[min++] = gameObject.AddComponent<T>();
                }
                m_Components = components;
            }
            OnPostInit();
        }

    }
}