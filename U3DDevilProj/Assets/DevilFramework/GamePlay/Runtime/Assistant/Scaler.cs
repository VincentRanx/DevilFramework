using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public class Scaler : MonoBehaviour
    {

        public float m_Length = 1;
        public float m_HorizontalLength = 1f;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            float hand = UnityEditor.HandleUtility.GetHandleSize(transform.position);
            Gizmos.color = Color.green * 0.9f;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawSphere(transform.position, hand * 0.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * (m_HorizontalLength + 0.3f));
            Gizmos.DrawRay(transform.position + transform.up * m_Length, transform.forward * (m_HorizontalLength + 0.3f));
            Gizmos.DrawRay(transform.position, transform.up * m_Length);
            float dis = 0.1f * hand;
            float len0 = 0;
            float len = 0;
            float total = Mathf.Abs(m_Length);
            float sign = Mathf.Sign(m_Length);
            while (len < total)
            {
                len += dis;
                if (len > total)
                    len = total;
                Gizmos.DrawRay(transform.position + transform.up * len0 * sign + transform.forward * m_HorizontalLength, transform.up * (len - len0) * sign);
                len0 = len + dis * 0.5f;
                len = len0;
            }
        }
#endif

    }
}