using Devil.Utility;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public class LoopPose : MonoBehaviour
    {
        [SerializeField]
        Transform m_LoopTrans;

        [SerializeField]
        bool m_FreeXAxis;

        [SerializeField]
        bool m_FreeYAxis;

        [SerializeField]
        bool m_FreeZAxis;

        Vector3 mStartPos;
        Transform mLocalTrans;
        bool mAllFree;

        private void Start()
        {
            mAllFree = m_FreeXAxis && m_FreeYAxis && m_FreeZAxis;
            mLocalTrans = transform.parent;
            if (!m_LoopTrans && transform.childCount > 0)
                m_LoopTrans = transform.GetChild(0);
            if (!m_LoopTrans || m_LoopTrans.parent != transform)
            {
                enabled = false;
#if UNITY_EDITOR
                RTLog.LogError(LogCat.Game, "'LoopPose' 需要设置一个子节点作为 'Loop Trans'");
#endif
                return;
            }
            Matrix4x4 m = mLocalTrans ? mLocalTrans.worldToLocalMatrix : Matrix4x4.identity;
            mStartPos = m.MultiplyPoint(m_LoopTrans.position);
        }

        private void LateUpdate()
        {
            if (!mAllFree)
            {
                Matrix4x4 m = mLocalTrans ? mLocalTrans.worldToLocalMatrix : Matrix4x4.identity;
                Vector3 pos = m.MultiplyPoint(m_LoopTrans.position);
                Vector3 delta = pos - mStartPos;
                if (m_FreeXAxis)
                    delta.x = 0;
                if (m_FreeYAxis)
                    delta.y = 0;
                if (m_FreeZAxis)
                    delta.z = 0;
                transform.localPosition -= delta;
            }
        }
    }
}