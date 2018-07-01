using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Devil
{
    public class RichText : Text
    {
        public struct Emoji
        {
            public int charIndex;
            public Sprite sprite;
        }

        [SerializeField]
        SpriteAtlas m_Atlas;
        int mTexId;
        List<Emoji> mEmojies = new List<Emoji>();

        Material mMatInst;
        readonly UIVertex[] m_TempVerts = new UIVertex[4];

        public override string text
        {
            get
            {
                return m_Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (string.IsNullOrEmpty(m_Text))
                        return;
                    m_Text = "";
                    GetEmojies();
                    SetVerticesDirty();
                }
                else if (m_Text != value)
                {
                    m_Text = value;
                    GetEmojies();
                    SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            GetEmojies();
            GetMaterials();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if(mMatInst != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                    Destroy(mMatInst);

#if UNITY_EDITOR
                else
                    DestroyImmediate(mMatInst);
#endif
                mMatInst = null;
            }
        }

        void GetMaterials()
        {
            if (m_Material == null)
            {
                if(mMatInst == null)
                    mMatInst = new Material(Shader.Find("DevilTeam/EmojiFont"));
                m_Material = mMatInst;
            }
        }

        void GetEmojies()
        {
            mEmojies.Clear();
            if (!supportRichText || m_Atlas == null)
                return;
            mTexId = Shader.PropertyToID("_EmojiTex");
            MatchCollection matchs = Regex.Matches(m_Text, "<quad=[a-z|A-Z|0-9|_]+>");
            for (int i = 0; i < matchs.Count; i++)
            {
                Match mat = matchs[i];
                Emoji emoj;
                emoj.charIndex = mat.Index;
                emoj.sprite = m_Atlas.GetSprite(mat.Value.Substring(6, mat.Value.Length - 7));
                mEmojies.Add(emoj);
            }
        }
        
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (font == null)
                return;
            
            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;
            
            Material mat = materialForRendering;

            int emojIndex = 0;
            Emoji emoj = mEmojies.Count > 0 ? mEmojies[emojIndex] : default(Emoji);

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            //Last 4 verts are always a new line... (\n)
            int vertCount = verts.Count - 4;

            Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
            roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
            toFill.Clear();
            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_TempVerts[tempVertsIndex] = verts[i];
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                    m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                    if (tempVertsIndex == 3)
                        toFill.AddUIVertexQuad(m_TempVerts);
                }
            }
            else
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_TempVerts[tempVertsIndex] = verts[i];
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                    {
                        if(emojIndex < mEmojies.Count && emoj.charIndex == (i >> 2) && emoj.sprite != null && mat != null)
                        {
                            Vector4 uv = DataUtility.GetOuterUV(emoj.sprite);
                            m_TempVerts[0].uv1 = new Vector2(uv.x, uv.w);
                            m_TempVerts[1].uv1 = new Vector2(uv.z, uv.w);
                            m_TempVerts[2].uv1 = new Vector2(uv.z, uv.y);
                            m_TempVerts[3].uv1 = new Vector2(uv.x, uv.y);
                            mat.SetTexture(mTexId, emoj.sprite.texture);
                            if (++emojIndex < mEmojies.Count)
                                emoj = mEmojies[emojIndex];
                        }
                        toFill.AddUIVertexQuad(m_TempVerts);
                    }
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            GetEmojies();
            GetMaterials();
        }
#endif

    }

}