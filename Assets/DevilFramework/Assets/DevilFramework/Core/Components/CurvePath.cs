using Devil.Utility;
using UnityEngine;

namespace Devil
{
    public class CurvePath : MonoBehaviour
    {
        public ECurveType m_Type;

        public Vector3[] m_Points;

        private TCRSpline mPath;

        public void RecalculateLater()
        {
            mPath = null;
        }

#if UNITY_EDITOR
        private float normalSpeed = 1;
        private void OnDrawGizmosSelected()
        {
            if (mPath == null)
            {
                mPath = TCRSpline.GetCRSpline(m_Points);
            }
            if (mPath != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                mPath.DrawGizmos(m_Points.Length * 10, Color.green, ref normalSpeed);
            }
        }
#endif
    }
}