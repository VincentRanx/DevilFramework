using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil
{
    public class CubeAreaCollider : ComponentGroup<BoxCollider>
    {
        [SerializeField]
        Vector3 m_Center;

        [SerializeField]
        Vector3 m_Size;

        [SerializeField]
        Vector3 m_Boldness;

        [HideInInspector]
        [SerializeField]
        BoxCollider[] m_Colliders;

        [HideInInspector]
        [SerializeField]
        bool m_Inited;

        protected override void OnPreInit()
        {
            m_ComponentCount = 6;
            m_Boldness.x = Mathf.Abs(m_Boldness.x);
            m_Boldness.y = Mathf.Abs(m_Boldness.y);
            m_Boldness.z = Mathf.Abs(m_Boldness.z);
        }

        protected override void OnPostInit()
        {
            m_Colliders = m_Components;

            Vector3 msize = m_Size + m_Boldness;
            msize *= 0.5f;
            Vector3 center, size;

            center = new Vector3(0, -msize.y, 0) + m_Center;
            size = new Vector3(m_Size.x + m_Boldness.x * 2, m_Boldness.y, m_Size.z + m_Boldness.z * 2);
            m_Colliders[0].center = center;
            m_Colliders[0].size = size;

            center = new Vector3(0, msize.y, 0) + m_Center;
            m_Colliders[1].center = center;
            m_Colliders[1].size = size;

            center = new Vector3(-msize.x, 0, 0) + m_Center;
            size = new Vector3(m_Boldness.x, m_Size.y, m_Size.z + m_Boldness.z * 2);
            m_Colliders[2].center = center;
            m_Colliders[2].size = size;

            center = new Vector3(msize.x, 0, 0) + m_Center;
            m_Colliders[3].center = center;
            m_Colliders[3].size = size;

            center = new Vector3(0, 0, -msize.z) + m_Center;
            size = new Vector3(m_Size.x, m_Size.y, m_Boldness.z);
            m_Colliders[4].center = center;
            m_Colliders[4].size = size;

            center = new Vector3(0, 0, msize.z) + m_Center;
            m_Colliders[5].center = center;
            m_Colliders[5].size = size;

            m_Inited = true;
        }


#if UNITY_EDITOR

        private void OnValidate()
        {
            m_Inited = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!m_Inited)
                InitComponents();
            Gizmos.matrix = transform.localToWorldMatrix;
            Color color = Color.green;
            color.a = 0.5f;
            Gizmos.color = color;
            for(int i = 0; i < m_ComponentCount; i++)
            {
                Gizmos.DrawCube(m_Colliders[i].center, m_Colliders[i].size);
            }

            color.a = 1;
            Gizmos.color = color;
            Gizmos.DrawWireCube(m_Center, m_Size);
        }
#endif
    }
}