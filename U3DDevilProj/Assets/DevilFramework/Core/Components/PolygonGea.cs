using Devil.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Devil.UI
{
    public class PolygonGea : MaskableGraphic
    {
        [SerializeField]
        Color m_WireColor = Color.white;
        [SerializeField]
        float m_OutWireSize = 3;
        [SerializeField]
        float m_InnerWireSize = 1;

        [Range(3, 20)]
        [SerializeField]
        int m_Sides = 3;
        [SerializeField]
        string m_InitSize = ""; // 初始大小
        [Range(-180, 180)]
        [SerializeField]
        float m_RotateAngle;
        
        UIVertex[] tempVerts;
        float[] mLength;
        UIVertex[] wireQuad = new UIVertex[4];

        protected override void Awake()
        {
            base.Awake();
            InitSize();
        }

        public float GetSideLength(int sideIndex)
        {
            if (mLength != null && sideIndex < mLength.Length)
                return mLength[sideIndex];
            else
                return 1;
        }

        public void SetSideLength(int sideIndex, float length)
        {
            if(sideIndex < m_Sides && sideIndex >= 0)
            {
                length = Mathf.Clamp01(length);
                if(mLength == null || mLength.Length != m_Sides)
                {
                    var len = new float[m_Sides];
                    for(int i = 0; i < m_Sides; i++)
                    {
                        if (mLength != null && i < mLength.Length)
                            len[i] = mLength[i];
                        else
                            len[i] = 1;
                    }
                    mLength = len;
                }
                mLength[sideIndex] = length;
                SetVerticesDirty();
            }
        }

        void InitSize()
        {
            if(string.IsNullOrEmpty(m_InitSize))
            {
                mLength = new float[m_Sides];
                for (int i = 0; i < m_Sides; i++)
                {
                    mLength[i] = 1;
                }
                return;
            }
            float[] arr = StringUtil.ParseFloatArray(m_InitSize, ';');
            if(arr != null)
            {
                float[] len = new float[m_Sides];
                float v = 1;
                for(int i = 0; i < m_Sides; i++)
                {
                    if (i < arr.Length)
                        v = arr[i];
                    len[i] = v;
                }
                mLength = len;
            }
            else
            {
                mLength = new float[m_Sides];
                for(int i = 0; i < m_Sides; i++)
                {
                    mLength[i] = 1;
                }
            }
        }

        Vector3 Lerp(Rect rect, Vector3 uv)
        {
            Vector3 v = new Vector3();
            v.x = Mathf.Lerp(rect.xMin, rect.xMax, uv.x);
            v.y = Mathf.Lerp(rect.yMin, rect.yMax, uv.y);
            return v;
        }
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                InitSize();
#endif
            vh.Clear();
            if (tempVerts == null || tempVerts.Length != m_Sides + 2)
                tempVerts = new UIVertex[m_Sides + 2];
            float div = 1f / m_Sides;
            Quaternion roter = Quaternion.AngleAxis(360f * div, Vector3.forward);
            Vector3 p = Quaternion.AngleAxis(m_RotateAngle, Vector3.forward) * new Vector3(0, 0.5f);
            Vector3 cent = new Vector3(0.5f, 0.5f);
            Rect rect = GetPixelAdjustedRect();
            UIVertex cenvert = new UIVertex();
            cenvert.position = Lerp(rect, cent);
            cenvert.uv0 = new Vector2(0.5f, 0);
            cenvert.color = color;
            tempVerts[0] = cenvert;
            vh.AddVert(cenvert);
            for(int i = 0; i <= m_Sides; i++)
            {
                UIVertex vert = new UIVertex();
                vert.position = Lerp(rect, cent + p * GetSideLength(i % m_Sides));
                vert.uv0 = new Vector2(i % 2, 1);
                vert.color = color;
                tempVerts[i + 1] = vert;
                vh.AddVert(vert);
                p = roter * p;
            }
            for (int i = 0; i < m_Sides; i++)
            {
                vh.AddTriangle(0, 1 + i, 2 + i);
            }

            FillInnerWire(vh);
            FillOutWire(vh);
        }

        void FillInnerWire(VertexHelper vh)
        {
            if(m_InnerWireSize > 0)
            {
                Quaternion nor = Quaternion.AngleAxis(90, Vector3.forward);
                for(int i = 0; i < m_Sides; i++)
                {
                    Vector3 dir = tempVerts[i + 1].position - tempVerts[0].position;
                    dir = nor * (dir.normalized * m_OutWireSize * 0.5f);
                    UIVertex vert = new UIVertex();
                    vert.position = tempVerts[i + 1].position + dir;
                    vert.color = m_WireColor;
                    vert.uv0 = new Vector2(0, 1);
                    wireQuad[0] = vert;

                    vert = new UIVertex();
                    vert.position = tempVerts[0].position + dir;
                    vert.color = m_WireColor;
                    vert.uv0 = Vector2.one;
                    wireQuad[1] = vert;

                    vert = new UIVertex();
                    vert.position = tempVerts[0].position - dir;
                    vert.color = m_WireColor;
                    vert.uv0 = new Vector2(1, 0);
                    wireQuad[2] = vert;

                    vert = new UIVertex();
                    vert.position = tempVerts[i + 1].position - dir;
                    vert.color = m_WireColor;
                    vert.uv0 = Vector2.zero;
                    wireQuad[3] = vert;
                    vh.AddUIVertexQuad(wireQuad);
                }
            }
        }

        void FillOutWire(VertexHelper vh)
        {
            if (m_OutWireSize > 0)
            {
                Quaternion nor = Quaternion.AngleAxis(90, Vector3.forward);
                for (int i = 0; i < m_Sides; i++)
                {
                    Vector3 dir = tempVerts[i + 2].position - tempVerts[i + 1].position;
                    dir = nor * (dir.normalized * m_OutWireSize * 0.5f);
                    UIVertex vert = new UIVertex();
                    vert.position = tempVerts[i + 1].position + dir;
                    vert.color = m_WireColor;
                    vert.uv0 = new Vector2(0, 1);
                    wireQuad[0] = vert;

                    vert = new UIVertex();
                    vert.position = tempVerts[i + 2].position + dir;
                    vert.color = m_WireColor;
                    vert.uv0 = Vector2.one;
                    wireQuad[1] = vert;

                    vert = new UIVertex();
                    vert.position = tempVerts[i + 2].position - dir;
                    vert.color = m_WireColor;
                    vert.uv0 = new Vector2(1, 0);
                    wireQuad[2] = vert;

                    vert = new UIVertex();
                    vert.position = tempVerts[i + 1].position - dir;
                    vert.color = m_WireColor;
                    vert.uv0 = Vector2.zero;
                    wireQuad[3] = vert;
                    vh.AddUIVertexQuad(wireQuad);
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Application.isPlaying)
            {
                mLength = null;
                InitSize();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Rect rect = rectTransform.rect;
            Vector3 dir = Quaternion.AngleAxis(m_RotateAngle, Vector3.forward) * Vector3.up * rect.height * 0.5f;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rect.center, dir);
        }
#endif
    }
}