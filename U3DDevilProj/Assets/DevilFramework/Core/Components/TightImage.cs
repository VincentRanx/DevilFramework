using Devil.Effects;
using UnityEngine;
using UnityEngine.UI;

namespace Devil.UI
{
    public class TightImage : MaskableGraphic
    {
        readonly UIVertex[] tempQuad = new UIVertex[4];

        [SerializeField]
        [Range(0, 5)]
        float m_BlurStep = 1;
        public float BlurStep
        {
            get { return m_BlurStep; }
            set
            {
                var v = Mathf.Clamp(m_BlurStep, 0, 5);
                if (v != m_BlurStep)
                {
                    m_BlurStep = v;
                    ReleasePostTexture();
                    SetMaterialDirty();
                }
            }
        }

        [SerializeField]
        [Range(0, 10)]
        int m_BlurIters = 0;
        public int BlurIters
        {
            get { return m_BlurIters; }
            set
            {
                var v = Mathf.Clamp(value, 0, 10);
                if (m_BlurIters != v)
                {
                    m_BlurIters = v;
                    ReleasePostTexture();
                    SetMaterialDirty();
                }
            }
        }

        // 后期特效贴图
        RenderTexture mPostTexture;
        public RenderTexture postTex { get { return mPostTexture; } }
        int mPostTexId;

        public void ReleasePostTexture()
        {
            if (mPostTexture != null)
            {
                RenderTexture.ReleaseTemporary(mPostTexture);
                mPostTexture = null;
            }
        }

        RenderTexture GetPostTexture(Texture source)
        {
            int id = source.GetInstanceID();
            if (mPostTexture != null && id != mPostTexId)
                ReleasePostTexture();
            if (mPostTexture == null)
            {
                mPostTexId = id;
                mPostTexture = RenderTexture.GetTemporary(source.width, source.height);
                GraphicHelper.Blur(source, mPostTexture, m_BlurStep, m_BlurIters);
            }
            return mPostTexture;
        }

        [SerializeField] Sprite m_Sprite;
        public Sprite sprite
        {
            get { return m_Sprite; }
            set
            {
                if (m_Sprite != value)
                {
                    m_Sprite = value;
                    ReleasePostTexture();
                    SetAllDirty();
                }
            }
        }

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
                var tex = m_Sprite.texture;
                if (m_BlurIters > 0 && m_BlurStep > 0)
                {
                    var posttex = GetPostTexture(tex);
                    return posttex;
                }
                return tex;
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
            if (m_Sprite == null)
            {
                FillQuad(vh);
                return;
            }
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Bounds b = m_Sprite.bounds;
            Vector2 mul;
            mul.x = 1 / b.size.x;
            mul.y = 1 / b.size.y;
            Vector2[] verts = m_Sprite.vertices;
            Vector2[] uvs = m_Sprite.uv;
            ushort[] triangles = m_Sprite.triangles;
            Vector2 inuv = Vector2.zero;
            for (int i = 0; i < verts.Length; i++)
            {
                UIVertex vert = new UIVertex();
                vert.uv0 = uvs[i];
                inuv.x = (verts[i].x - b.min.x) * mul.x;
                inuv.y = (verts[i].y - b.min.y) * mul.y;
                vert.position = Lerp(rect, inuv);
                vert.color = color;
                vh.AddVert(vert);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                vh.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
            }
        }

        void FillQuad(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Color col = new Color(color.r, color.g, color.b, color.a * 0.5f);
            UIVertex vert;

            vert = new UIVertex();
            vert.position = rect.min;
            vert.color = col;
            vert.uv0 = new Vector2(0, 0);
            tempQuad[0] = vert;

            vert = new UIVertex();
            vert.position = new Vector3(rect.xMin, rect.yMax);
            vert.color = col;
            vert.uv0 = new Vector2(0, 1);
            tempQuad[1] = vert;

            vert = new UIVertex();
            vert.position = new Vector3(rect.xMax, rect.yMax);
            vert.color = col;
            vert.uv0 = new Vector2(1, 1);
            tempQuad[2] = vert;

            vert = new UIVertex();
            vert.position = new Vector3(rect.xMax, rect.yMin);
            vert.color = col;
            vert.uv0 = new Vector2(1, 0);
            tempQuad[3] = vert;

            vh.AddUIVertexQuad(tempQuad);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ReleasePostTexture();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Application.isPlaying)
                ReleasePostTexture();
        }
#endif
    }
}