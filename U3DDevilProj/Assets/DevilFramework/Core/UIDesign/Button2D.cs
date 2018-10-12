using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

namespace Devil.UI
{
    [AddComponentMenu("UISupport/Button (new)")]
    [ExecuteInEditMode]
    public class Button2D : MonoBehaviour, ISubmitHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IDragHandler
    {
        public delegate void ButtonEvent(Button2D button, int count);
        public delegate void HoldEvent(Button2D button);

        #region field
        private static int sSyncId;
        
        [SerializeField]
        private Graphic m_TargetGraphic;
        private Image m_TargetImg;

        public AudioSource m_ClickSound;

        [SerializeField]
        private bool m_SyncButton;

        [Tooltip("点击区域半径")]
        [SerializeField]
        [Range(0, 50)]
        private float m_TouchRadius = 3;//点击区域半径

        [Tooltip("按住操作开始时间(单位:s)")]
        [SerializeField]
        [Range(0f, 10f)]
        private float m_HoldStartTime = 0.5f;//按住操作开始时间（s）

        [Tooltip("双击时间间隔(单位:s)")]
        [SerializeField]
        [Range(0f, 2f)]
        private float m_DoubleClickTime;//按住操作开始时间（s）

        [SerializeField]
        [Range(0f, 10f)]
        private float m_DelayTime = 0f;//完成点击后的持续时间

        public Color m_NormalColor = Color.white;
        private Sprite m_NormalSprite;
        public Color m_PressColor = Color.gray;
        public Sprite m_PressSprite;

        public Color m_HoldColor = Color.gray;
        public Sprite m_HoldSprite;
        public Color m_DisableColor = Color.gray;
        public Sprite m_DisableSprite;

        public ButtonEvent OnClick { get; set; }
        public HoldEvent OnHoldBegin { get; set; }
        public HoldEvent OnHoldEnd { get; set; }
        [SerializeField]
        private Button.ButtonClickedEvent _onClick = new Button.ButtonClickedEvent();
        public Button.ButtonClickedEvent onClick
        {
            get
            {
                if (_onClick == null)
                    _onClick = new Button.ButtonClickedEvent();
                return _onClick;
            }
        }

        private float mDownTime;
        private int mPointerId = -1;
        private Vector2 mDownPos;
        private bool mHolding;
        private bool mPressed;

        private float mClickTime;
        private int mClickCount;

        private float mReleaseTime;//点击或者按住结束时间
        private bool mReleased;

        #endregion

        #region Events Handler

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {

        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (mPressed && eventData.pointerId == mPointerId)
            {
                mPressed = false;
                if (mHolding)
                {
                    mHolding = false;
                    NotifyHoldEnd();
                }
                else
                {
                    OnBtnClick();
                }
                SetNormalColor();
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (RefusedForSync)
                return;
            if (Time.realtimeSinceStartup - mReleaseTime < m_DelayTime)
                return;
            if (!mPressed)
            {
                GetSyncLock();
                mDownTime = Time.realtimeSinceStartup;
                mDownPos = eventData.position;
                mHolding = false;
                mPressed = true;
                mReleaseTime = 0;
                mPointerId = eventData.pointerId;
                SetColorFromPressToHold();
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (mPressed && mPointerId == eventData.pointerId)
            {
                mPressed = false;
                if (mHolding)
                {
                    mHolding = false;
                    NotifyHoldEnd();
                }
                SetNormalColor();
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (mPressed && eventData.pointerId == mPointerId)
            {
                mPressed = false;
                Vector2 p = eventData.position;
                if (Vector2.Distance(p, mDownPos) > m_TouchRadius)
                {
                    if (mHolding)
                    {
                        mHolding = false;
                        NotifyHoldEnd();
                    }
                    SetNormalColor();
                }
            }
        }

        #endregion

        #region Unity Editor
#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!m_TargetGraphic)
                m_TargetGraphic = GetComponent<Graphic>();
            if (m_TargetGraphic)
                m_TargetGraphic.color = m_NormalColor;
            if(!(m_TargetGraphic is Image))
            {
                m_NormalSprite = null;
                m_PressSprite = null;
                m_HoldSprite = null;
                m_DisableSprite = null;
            }
        }
#endif
        #endregion

        #region MONO
        protected virtual void Awake()
        {
            if (!m_TargetGraphic)
                m_TargetGraphic = GetComponent<Graphic>();
            m_TargetImg = m_TargetGraphic as Image;
            if (m_TargetImg)
            {
                m_NormalSprite = m_TargetImg.sprite;
            }
        }

        protected virtual void Update()
        {
            float t = Time.realtimeSinceStartup;
            if (mPressed && !mHolding && m_HoldStartTime > 0)
            {
                if (t - mDownTime > m_HoldStartTime)
                {
                    mHolding = true;
                    NotifyHoldBegin();
                }
            }

            if (mClickCount > 0 && t - mClickTime > m_DoubleClickTime)
            {
                NotifyClick(mClickCount);
                mClickCount = 0;
            }

            if (mReleased && mReleaseTime > 0 && t - mReleaseTime > m_DelayTime)
            {
                ReleaseSyncLock();
            }
        }

        protected virtual void OnEnable()
        {
            if (m_TargetGraphic)
                m_TargetGraphic.color = m_NormalColor;
            if (m_TargetImg && m_NormalSprite)
                m_TargetImg.sprite = m_NormalSprite;
        }

        protected virtual void OnDisable()
        {
            ((IPointerExitHandler)this).OnPointerExit(null);
            ReleaseSyncLock();
            if (m_TargetGraphic)
                m_TargetGraphic.color = m_DisableColor;
            if (m_TargetImg && m_DisableSprite)
                m_TargetImg.sprite = m_DisableSprite;
        }

        private bool RefusedForSync { get { return m_SyncButton && sSyncId != 0 && GetInstanceID() != sSyncId; } }
        private bool GetSyncLock()
        {
            if (m_SyncButton && sSyncId == 0)
            {
                sSyncId = GetInstanceID();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ReleaseSyncLock()
        {
            if (sSyncId == GetInstanceID())
            {
                sSyncId = 0;
                mReleased = false;
            }
        }

        private void SetColorFromPressToHold()
        {
            StopAllCoroutines();
            if (m_TargetGraphic)
            {
                m_TargetGraphic.color = m_PressColor;
                if (m_TargetImg && m_PressSprite)
                    m_TargetImg.sprite = m_PressSprite;
                if (m_HoldStartTime > 0)
                {
                    StartCoroutine(ChangeColor(m_HoldColor, m_HoldStartTime, m_HoldSprite));
                }
            }
        }

        private void SetNormalColor()
        {
            StopAllCoroutines();
            StartCoroutine(ChangeColor(enabled ? m_NormalColor : m_DisableColor, Mathf.Max(0.1f, m_DoubleClickTime * 0.6f), enabled ? m_NormalSprite : m_DisableSprite));
        }

        private IEnumerator ChangeColor(Color color, float delay, Sprite spr)
        {
            float t = 0;
            if (!m_TargetGraphic)
                yield break;
            if (delay <= 0)
            {
                m_TargetGraphic.color = color;
                yield break;
            }
            Color c0 = m_TargetGraphic.color;
            while (t < delay)
            {
                yield return null;
                if (!m_TargetGraphic)
                    break;
                t += Time.unscaledDeltaTime;
                Color c = Color.Lerp(c0, color, Mathf.Clamp01(t / delay));
                m_TargetGraphic.color = c;
            }
            if (m_TargetImg && spr)
                m_TargetImg.sprite = spr;
        }

        #endregion

        #region Button Events

        private void OnBtnClick()
        {
            if (m_ClickSound)
                m_ClickSound.Play();
            if (m_DoubleClickTime > 0)
            {
                mClickCount++;
                mClickTime = Time.realtimeSinceStartup;
            }
            else
            {
                //else
                //    AudioManager.instance.PlayClickAudioSource();
                NotifyClick(1);
            }
        }

        private void NotifyHoldBegin()
        {
            if (OnHoldBegin != null)
                OnHoldBegin(this);
        }

        private void NotifyHoldEnd()
        {
            mReleaseTime = Time.realtimeSinceStartup;
            mReleased = true;
            if (OnHoldEnd != null)
            {
                OnHoldEnd(this);
            }
        }

        private void NotifyClick(int count)
        {
            mReleaseTime = Time.realtimeSinceStartup;
            mReleased = true;
            if (OnClick != null)
            {
                OnClick(this, count);
            }
            if (_onClick != null)
            {
                _onClick.Invoke();
            }

        }

        #endregion

    }
}