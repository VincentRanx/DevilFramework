using UnityEngine;

namespace Devil.Utility
{
    [System.Serializable]
    public class BezierCurve
    {
        [System.Serializable]
        public struct PController
        {
            public Vector3 point;
            public Vector3 leftHandle;
            public Vector3 rightHandle;
            public float weight;
        }

        [SerializeField]
        private PController[] m_Points;

        [SerializeField]
        private float m_TotalWeight;

        public BezierCurve()
        {
            m_Points = new PController[0];
            m_TotalWeight = 0;
        }

        // TODO
    }
}