using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Devil.UI
{
    public class EmojiAnimator : MonoBehaviour
    {
        interface IPlayer
        {
            Sprite sprite { get; set; }
        }

        public class TightPlayer : IPlayer
        {
            TightImage mImg;

            public TightPlayer(TightImage img)
            {
                mImg = img;
            }

            public Sprite sprite { get { return mImg.sprite; } set { mImg.sprite = value; } }
        }

        public class ImagePlayer : IPlayer
        {
            Image mImg;
            public ImagePlayer(Image img)
            {
                mImg = img;
            }
            public Sprite sprite { get { return mImg.sprite; } set { mImg.sprite = value; } }
        }

        public enum ETimeType
        {
            scaledTime,
            realTime,
        }

        [SerializeField]
        SpriteAtlas m_Atlas;

        [SerializeField]
        EmojiAnims m_AnimClips;

        [SerializeField]
        ETimeType m_TimeType;

        [SerializeField]
        string m_StartAnim;

        // 播放机
        IPlayer mPlayer;

        EmojiAnims.Anim mCurrentAnim;
        Sprite[] mSprites;
        float mTime;
        int mFrame;

        IPlayer GetPlayer()
        {
            TightImage timg = GetComponent<TightImage>();
            if (timg != null)
                return new TightPlayer(timg);
            Image img = GetComponent<Image>();
            if (img != null)
                return new ImagePlayer(img);
            return null;
        }

        public bool IsValid { get { return mPlayer != null && m_Atlas != null && m_AnimClips != null; } }

        public SpriteAtlas Atlas
        {
            get { return m_Atlas; }
            set
            {
                if (m_Atlas != value)
                {
                    m_Atlas = value;
                    mSprites = null;
                    mCurrentAnim = null;
                }
            }
        }

        public EmojiAnims AnimClips
        {
            get { return m_AnimClips; }
            set
            {
                if (m_AnimClips != value)
                {
                    m_AnimClips = value;
                    mSprites = null;
                    mCurrentAnim = null;
                }
            }
        }

        public ETimeType TimeType { get { return m_TimeType; } set { m_TimeType = value; } }

        public void Play(string animName)
        {
            if (!IsValid)
                return;
            var anim = m_AnimClips.GetAnim(animName);
            if (anim == null || !anim.IsVaild)
                return;
            mSprites = new Sprite[anim.m_Frames.Length];
            for(int i = 0; i < mSprites.Length; i++)
            {
                mSprites[i] = m_Atlas.GetSprite(anim.m_Frames[i].m_Sprite);
            }
            mCurrentAnim = anim;
            mTime = 0;
            mFrame = 0;
            var spr = mSprites[mFrame];
            if (spr != null)
                mPlayer.sprite = spr;
        }

        private void Awake()
        {
            mPlayer = GetPlayer();
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(m_StartAnim))
                Play(m_StartAnim);
        }

        private void OnDestroy()
        {
            mPlayer = null;
            mSprites = null;
            mCurrentAnim = null;
        }

        private void Update()
        {
            if(mCurrentAnim != null)
            {
                mTime += m_TimeType == ETimeType.realTime ? Time.unscaledDeltaTime : Time.deltaTime;
                int f = mCurrentAnim.GetFrameIndex(mTime);
                if(f != mFrame)
                {
                    mFrame = f;
                    mPlayer.sprite = mSprites[mFrame];
                }
            }
        }
    }
}