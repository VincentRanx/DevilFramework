using Devil.Utility;
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
        static ObjectBuffer<EmojiButton> btnPool = new ObjectBuffer<EmojiButton>(32, () => new EmojiButton());

        public struct Emoji
        {
            public int charIndex;
            public string spriteName;
            public EmojiAnims.Anim anim;
            public bool raw;
            public float rotate;
            public Vector2 offset;
            public int btnId;

            public void GetFrame(float time, out string sprite, out float rotate)
            {
                if (anim == null)
                {
                    sprite = spriteName;
                    rotate = this.rotate;
                }
                else
                {
                    var f = anim.GetFrame(time);
                    spriteName = f.m_Sprite;
                    sprite = spriteName;
                    rotate = this.rotate + f.m_Rotate;
                }
            }
        }

        public class EmojiButton
        {
            public int emojiIndex;
            public int clickId;
            public Rect rect;
        }

        [SerializeField]
        SpriteAtlas m_Atlas;
        [SerializeField]
        EmojiAnims m_Anims;
        [SerializeField]
        bool m_SupportEmoji = true;

        int mTexId;
        List<Emoji> mEmojies = new List<Emoji>();

        Material mMatInst;
        readonly UIVertex[] m_TempVerts = new UIVertex[4];
        readonly string mEmojiPattern = @"<quad=[a-zA-Z0-9_]+[ a-z0-9\-=]*( /)?>";
        List<string> mEmojiProperties = new List<string>();

        List<EmojiButton> mEmojiBtns = new List<EmojiButton>();

        bool mAnimate;
        float mAnimDuration;
        float mTimer;
        float mTime;

        void ClearEmojiBtns()
        {
            for (int i = 0; i < mEmojiBtns.Count; i++)
            {
                var btn = mEmojiBtns[i];
                btnPool.SaveBuffer(btn);
            }
            mEmojiBtns.Clear();
        }

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

        bool ParseEmoji(Match mat, out Emoji emoji)
        {
            mEmojiProperties.Clear();
            if (!StringUtil.ParseArray(mat.Value, mEmojiProperties, '<', '>', ' ') || mEmojiProperties.Count < 1)
            {
                emoji = default(Emoji);
                return false;
            }
            Emoji tmp;
            tmp.charIndex = mat.Index;
            string src = mEmojiProperties[0].Substring(5);
            var anim = m_Anims == null ? null : m_Anims.GetAnim(src);
            tmp.anim = anim;
            int len = mEmojiProperties.Count;
            bool raw = len > 1 && mEmojiProperties[len - 1] == "/";
            tmp.raw = raw;
            if (raw)
                len--;
            tmp.spriteName = src;
            tmp.rotate = 0;
            tmp.btnId = 0;
            Vector2 offset = Vector2.zero;
            int n;
            float f;
            for (int i = 1; i < len; i++)
            {
                var pro = mEmojiProperties[i];
                if (string.IsNullOrEmpty(pro))
                    continue;
                if (pro.StartsWith("rotate=") && float.TryParse(pro.Substring(7), out f))
                    tmp.rotate = f;
                else if (pro.StartsWith("dx=") && float.TryParse(pro.Substring(3), out f))
                    offset.x = f;
                else if (pro.StartsWith("dy=") && float.TryParse(pro.Substring(3), out f))
                    offset.y = f;
                else if (pro.StartsWith("click=") && int.TryParse(pro.Substring(6), out n))
                    tmp.btnId = n;
            }
            tmp.offset = offset;
            emoji = tmp;
            return true;
        }

        void GetEmojies()
        {
            mAnimate = false;
            mAnimDuration = 10;
            mEmojies.Clear();
            ClearEmojiBtns();
            if (!supportRichText || !m_SupportEmoji || m_Atlas == null)
                return;
            mTexId = Shader.PropertyToID("_EmojiTex");
            MatchCollection matchs = Regex.Matches(m_Text, mEmojiPattern);
            for (int i = 0; i < matchs.Count; i++)
            {
                Match mat = matchs[i];
                Emoji emoji;
                if (!ParseEmoji(mat, out emoji))
                    continue;
                if (emoji.btnId != 0)
                {
                    var btn = btnPool.GetAnyTarget();
                    btn.rect = default(Rect);
                    btn.emojiIndex = mEmojies.Count;
                    btn.clickId = emoji.btnId;
                    emoji.btnId = mEmojiBtns.Count;
                    mEmojiBtns.Add(btn);
                }
                else
                {
                    emoji.btnId = -1;
                }
                mEmojies.Add(emoji);
                if (emoji.anim != null)
                {
                    mAnimate = true;
                    mAnimDuration = Mathf.Min(mAnimDuration, emoji.anim.m_Duration);
                }
            }
        }

        public bool SupportEmoji
        {
            get { return m_SupportEmoji; }
            set
            {
                if (m_SupportEmoji != value)
                {
                    m_SupportEmoji = value;
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
            if (mMatInst != null)
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
                if (mMatInst == null)
                    mMatInst = new Material(Shader.Find("DevilTeam/EmojiFont"));
                m_Material = mMatInst;
            }
        }

        Sprite GetSprite(string spr)
        {
            return m_Atlas == null ? null : m_Atlas.GetSprite(spr);
        }

        EmojiButton GetEmojiBtn(Emoji emoji, int emojiIndex)
        {
            if (emoji.btnId < 0 || emoji.btnId >= mEmojiBtns.Count)
                return null;
            var btn = mEmojiBtns[emoji.btnId];
            if (btn.emojiIndex == emojiIndex)
                return btn;
            else
                return null;
        }

        Rect CalculateTempVertsBounds()
        {
            Vector2 center = (m_TempVerts[0].position + m_TempVerts[1].position + m_TempVerts[2].position + m_TempVerts[3].position) * 0.25f;
            Vector2 s1 = m_TempVerts[0].position - m_TempVerts[2].position;
            Vector2 s2 = m_TempVerts[1].position - m_TempVerts[3].position;
            Vector2 size = new Vector2(Mathf.Max(Mathf.Abs(s1.x), Mathf.Abs(s2.x)),
                Mathf.Max(Mathf.Abs(s1.y), Mathf.Abs(s2.y)));
            return new Rect(center - size * 0.5f, size);
        }

        void RotateTempVerts(float angle)
        {
            Quaternion roter = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 center = (m_TempVerts[0].position + m_TempVerts[1].position + m_TempVerts[2].position + m_TempVerts[3].position) * 0.25f;
            m_TempVerts[0].position = center + roter * (m_TempVerts[0].position - center);
            m_TempVerts[1].position = center + roter * (m_TempVerts[1].position - center);
            m_TempVerts[2].position = center + roter * (m_TempVerts[2].position - center);
            m_TempVerts[3].position = center + roter * (m_TempVerts[3].position - center);
        }

        void MoveTempVerts(Vector3 offset)
        {
            m_TempVerts[0].position += offset;
            m_TempVerts[1].position += offset;
            m_TempVerts[2].position += offset;
            m_TempVerts[3].position += offset;
        }

        void ModifyTempVertsColor(Color color)
        {
            m_TempVerts[0].color = color;
            m_TempVerts[1].color = color;
            m_TempVerts[2].color = color;
            m_TempVerts[3].color = color;
        }

        void ModifyTempVertsUV(Sprite spr, Vector2 duv)
        {
            Vector4 uv = DataUtility.GetOuterUV(spr);
            m_TempVerts[0].uv0 = new Vector2(uv.x, uv.w) - duv;
            m_TempVerts[1].uv0 = new Vector2(uv.z, uv.w) - duv;
            m_TempVerts[2].uv0 = new Vector2(uv.z, uv.y) - duv;
            m_TempVerts[3].uv0 = new Vector2(uv.x, uv.y) - duv;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (!m_SupportEmoji)
            {
                base.OnPopulateMesh(toFill);
                return;
            }
            if (font == null)
                return;

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
                Vector2 duv = Vector2.one * 2;
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_TempVerts[tempVertsIndex] = verts[i];
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                    {
                        int es = 0;
                        while (emojIndex < mEmojies.Count && emoj.charIndex == (i >> 2) && mat != null)
                        {
                            es++;
                            string sprName;
                            float rot;
                            emoj.GetFrame(mTime, out sprName, out rot);
                            Sprite spr = GetSprite(sprName);
                            if (spr != null)
                            {
                                ModifyTempVertsUV(spr, duv);
                                if (Mathf.Abs(rot) > 0.1)
                                    RotateTempVerts(rot);
                                if (emoj.offset.sqrMagnitude > 0.1)
                                {
                                    var offset = (Vector3)emoj.offset;
                                    MoveTempVerts(offset);
                                }
                                if (emoj.raw)
                                {
                                    var col = Color.white;
                                    col.a = color.a;
                                    ModifyTempVertsColor(col);
                                }
                                var btn = GetEmojiBtn(emoj, emojIndex);
                                if (btn != null)
                                    btn.rect = CalculateTempVertsBounds();
                                mat.SetTexture(mTexId, spr.texture);
                                toFill.AddUIVertexQuad(m_TempVerts);
                            }
                            if (++emojIndex < mEmojies.Count)
                                emoj = mEmojies[emojIndex];
                        }
                        if (es == 0)
                        {
                            toFill.AddUIVertexQuad(m_TempVerts);
                        }
                    }
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
        }

        private void Update()
        {
            if (mAnimate)
            {
                mTimer += Time.deltaTime;
                mTime += Time.deltaTime;
                if (mTimer >= mAnimDuration)
                {
                    mTimer -= mAnimDuration;
                    SetVerticesDirty();
                }
            }
        }

        public int GetBtnId(Vector2 screenPoint)
        {
            var can = canvas;
            if (can == null)
                return 0;
            Vector2 point;
            var raypoint = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, can.worldCamera, out point);
            if (raypoint)
            {
                for (int i = 0; i < mEmojiBtns.Count; i++)
                {
                    var btn = mEmojiBtns[i];
                    if (btn.rect.Contains(point))
                    {
                        return btn.clickId;
                    }
                }
            }
            return 0;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            GetEmojies();
            GetMaterials();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.cyan * 0.5f;
            for (int i = 0; i < mEmojiBtns.Count; i++)
            {
                var btn = mEmojiBtns[i];
                GizmosUtil.DrawWiredCube(btn.rect.center, btn.rect.size);
            }
        }

#endif

    }

}