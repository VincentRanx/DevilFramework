using DevilTeam.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace DevilTeam.UI
{
    [AddComponentMenu("UISupport/Line (2D)")]
    public class Line2D : MaskableGraphic
    {
        [SerializeField]
        private CoordType2D m_CoordType;

        [SerializeField]
        private Vector3[] m_Points;

        [SerializeField]
        private float m_Size = 5f;

        [Range(0,1)]
        [SerializeField]
        private float m_Smooth = 0;

        private Rect mTempRect;
        private TCRSpline mSmoothLine;
        readonly UIVertex[] mTempVerts = new UIVertex[4];

        private void GenerateSmoothLine()
        {
            if (m_Size > 0 && m_Smooth > 0 && m_Points != null && m_Points.Length > 2)
            {
                mSmoothLine = TCRSpline.GetCRSpline(m_Points);
            }
            else
            {
                mSmoothLine = null;
            }
        }

        protected override void Awake()
        {
            GenerateSmoothLine();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            GenerateSmoothLine();
            base.OnValidate();
        }
#endif

        public Vector2 Calculate2DPos(Vector3 p)
        {
            if (m_CoordType == CoordType2D.uv)
            {
                return new Vector2(Mathf.Lerp(mTempRect.min.x, mTempRect.max.x, p.x), Mathf.Lerp(mTempRect.min.y, mTempRect.max.y, p.y));
            }
            else
            {
                return mTempRect.min + (Vector2)p;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (m_Size <= 0 || m_Points == null || m_Points.Length < 2)
                return;
            Vector2 p1, p2;
            Vector2 dir;
            mTempRect = rectTransform.rect;
            p1 = Calculate2DPos(m_Points[0]);
            if (mSmoothLine == null)
            {
                for (int i = 1; i < m_Points.Length; i++)
                {
                    p2 = Calculate2DPos(m_Points[i]);
                    dir.x = p2.y - p1.y;
                    dir.y = p1.x - p2.x;
                    dir = dir.normalized * m_Size * 0.5f;

                    mTempVerts[0].color = color;
                    mTempVerts[0].position = p1 + dir;

                    mTempVerts[1].color = color;
                    mTempVerts[1].position = p2 + dir;

                    mTempVerts[2].color = color;
                    mTempVerts[2].position = p2 - dir;

                    mTempVerts[3].color = color;
                    mTempVerts[3].position = p1 - dir;

                    vh.AddUIVertexQuad(mTempVerts);

                    p1 = p2;
                }
            }
            else
            {
                float dis = 1f / Mathf.Min(500f, m_Points.Length * Mathf.Ceil(50f * m_Smooth));
                float t = 0;
                while(t < 1)
                {
                    t += dis;
                    p2 = Calculate2DPos(mSmoothLine.Lerp(Mathf.Min(1, t)));
                    dir.x = p2.y - p1.y;
                    dir.y = p1.x - p2.x;
                    dir = dir.normalized * m_Size * 0.5f;

                    mTempVerts[0].color = color;
                    mTempVerts[0].position = p1 + dir;

                    mTempVerts[1].color = color;
                    mTempVerts[1].position = p2 + dir;

                    mTempVerts[2].color = color;
                    mTempVerts[2].position = p2 - dir;

                    mTempVerts[3].color = color;
                    mTempVerts[3].position = p1 - dir;

                    vh.AddUIVertexQuad(mTempVerts);

                    p1 = p2;
                }
            }
        }
    }
}