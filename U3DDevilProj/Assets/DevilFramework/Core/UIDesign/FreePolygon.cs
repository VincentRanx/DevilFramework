using Devil.Utility;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Devil.UI
{
    public class FreePolygon : MaskableGraphic
    {
        [SerializeField]
        Sprite m_Sprite;
        [SerializeField]
        Color m_WireColor = Color.white;
        [SerializeField]
        float m_WireSize = 1;

        [HideInInspector]
        [SerializeField] Vector2[] m_Points = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
        public Vector2[] Points { get { return m_Points; } }

        UIVertex[] tempVerts;
        readonly UIVertex[] wireQuad = new UIVertex[4];
        readonly Vector2[] uvs = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

        public override Texture mainTexture
        {
            get
            {
                if (m_Sprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return m_Sprite.texture;
            }
        }

        public Vector3 Lerp(Rect rect, Vector2 uv)
        {
            Vector3 v = new Vector3();
            v.x = Mathf.Lerp(rect.xMin, rect.xMax, uv.x);
            v.y = Mathf.Lerp(rect.yMin, rect.yMax, uv.y);
            return v;
        }

        public Vector2 GetPoint(Rect rect, Vector3 localpoint)
        {
            Vector2 v;
            v.x = (localpoint.x - rect.xMin) / rect.width;
            v.y = (localpoint.y - rect.yMin) / rect.height;
            return v;
        }

        public void RemovePoint(int index)
        {
            if (index < m_Points.Length && m_Points.Length > 3)
            {
                Vector2[] points = new Vector2[m_Points.Length - 1];
                if (index > 0)
                    System.Array.Copy(m_Points, points, index);
                if (index < points.Length)
                    System.Array.Copy(m_Points, index + 1, points, index, points.Length - index);
                m_Points = points;
                SetVerticesDirty();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public void AddPoint(Vector2 point)
        {
            Vector2[] points = new Vector2[m_Points.Length + 1];
            System.Array.Copy(m_Points, points, m_Points.Length);
            points[m_Points.Length] = point;
            m_Points = points;
            SetVerticesDirty();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        Vector2 LerpUV(Vector4 uvrect, Vector2 uv)
        {
            Vector2 v;
            v.x = Mathf.Lerp(uvrect.x, uvrect.z, uv.x);
            v.y = Mathf.Lerp(uvrect.y, uvrect.w, uv.y);
            return v;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (m_Points == null)
                return;
            Rect rect = GetPixelAdjustedRect();
            Vector4 uvrect = m_Sprite == null ? new Vector4(0, 0, 1, 1) : DataUtility.GetOuterUV(m_Sprite);

            if (tempVerts == null || tempVerts.Length != m_Points.Length)
                tempVerts = new UIVertex[m_Points.Length];
            for(int i = 0; i < tempVerts.Length; i++)
            {
                UIVertex vert = new UIVertex();
                vert.position = Lerp(rect, m_Points[i]);
                vert.color = color;
                vert.uv0 = LerpUV(uvrect, m_Points[i]);
                tempVerts[i] = vert;
                vh.AddVert(vert);
            }
            for(int i = 2; i < tempVerts.Length; i ++)
            {
                vh.AddTriangle(0, i - 1, i);
            }

            if(m_WireSize > 0)
            {
                Vector2 border;
                border.x = (m_WireSize / rect.width) * (uvrect.z - uvrect.x);
                border.y = (m_WireSize / rect.height) * (uvrect.w - uvrect.y);
                Quaternion rot = Quaternion.AngleAxis(90f, Vector3.forward);
                Vector3 p0, p1;
                for(int i = 0; i < tempVerts.Length; i++)
                {
                    p0 = tempVerts[i].position;
                    p1 = tempVerts[(i + 1) % tempVerts.Length].position;
                    Vector3 dir = rot * ((p1 - p0).normalized * m_WireSize * 0.5f);
                    UIVertex vert;
                    Vector4 borderuv;
                    int n = i % 4;
                    if (n == 0)
                        borderuv = new Vector4(uvrect.x, uvrect.y, uvrect.x + border.x, uvrect.w);
                    else if (n == 1)
                        borderuv = new Vector4(uvrect.x, uvrect.w - border.y, uvrect.z, uvrect.w);
                    else if (n == 2)
                        borderuv = new Vector4(uvrect.z - border.x, uvrect.y, uvrect.z, uvrect.w);
                    else
                        borderuv = new Vector4(uvrect.x, uvrect.y, uvrect.z, uvrect.y + border.y);

                    vert = new UIVertex();
                    vert.position = p0 - dir;
                    vert.uv0 = LerpUV(borderuv, uvs[0]);
                    vert.color = m_WireColor;
                    wireQuad[0] = vert;

                    vert = new UIVertex();
                    vert.position = p0 + dir;
                    vert.uv0 = LerpUV(borderuv, uvs[1]);
                    vert.color = m_WireColor;
                    wireQuad[1] = vert;

                    vert = new UIVertex();
                    vert.position = p1 + dir;
                    vert.uv0 = LerpUV(borderuv, uvs[2]);
                    vert.color = m_WireColor;
                    wireQuad[2] = vert;

                    vert = new UIVertex();
                    vert.position = p1 - dir;
                    vert.uv0 = LerpUV(borderuv, uvs[3]);
                    vert.color = m_WireColor;
                    wireQuad[3] = vert;
                    vh.AddUIVertexQuad(wireQuad);
                }
            }
        }
        
    }
}