
#if ( UNITY_IPHONE || UNITY_ANDROID ) && !UNITY_EDITOR
#define MOBILE
#endif

using Devil.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devil.UI
{
    [AddComponentMenu("UISupport/Dragable Object")]
    public class DragableObject2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        public delegate void DragDelegate(GameObject go);

        Camera mCam;
        bool mPressed;
        bool mMoved;

        public Graphic m_DragGraphic;
        public Color m_NormalColor = Color.white;
        public Color m_OnDragColor = Color.gray;
        public Color m_DisableColor = Color.gray * 0.7f;

        [Header("拖拽对象")]
        public Transform m_DragTarget;

        [Header("当丢开始拖拽时更新对象开始位置")]
        public bool m_BeStartPosWhereDropStart;

        [Header("当丢下对象时更新对象开始位置")]
        public bool m_BeStartPosWhereDrop;

        [Header("当没有丢下对象时恢复到初始位置")]
        public bool m_ResetToBegin;

        [Header("恢复初始位置时间")]
        [Range(0, 2)]
        public float m_ResetDuration = 0.2f;

        public event DragDelegate OnDragBegin = (x)=> { };
        public event DragDelegate OnDragEnd = (x) => { };

        float mPixDistance;
        bool mBeginReset;
        float mResetTimer;
        float mResetDividDuration;
        bool mDrop;
        bool mOnDragUpdate;
        Vector3 mStartPos;

        Vector2 mPos;
        Vector2 mScreenPos;

        Vector2 mTouchPos;
        Vector2 mLocalTouchPos;

        Vector2 mTarget;

        Vector2 TouchPos
        {
            get
            {
#if MOBILE
            if (Input.touchCount > 0)
                mTouchPos = Input.touches[0].position;
#else
                mTouchPos = Input.mousePosition;
#endif
                return mTouchPos;
            }
        }

        void ResetPos()
        {
            mPos = m_DragTarget.localPosition;
            mScreenPos = TouchPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_DragTarget.parent as RectTransform, mScreenPos, mCam, out mLocalTouchPos);
            mTarget = mPos;
            mPixDistance = PixelDistance * 5f;
        }

        float PixelDistance
        {
            get
            {
                Vector2 p0, p1;
                RectTransform par = m_DragTarget.parent as RectTransform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(par, Vector2.zero, mCam, out p0);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(par, new Vector2(1,0), mCam, out p1);
                return Vector2.Distance(p0, p1);
            }
        }

        void OnEnable()
        {
            mMoved = false;
            if (!mCam)
                mCam = ComponentUtil.ActiveCameraForLayer(gameObject.layer);
            if (!m_DragTarget)
                m_DragTarget = transform;
            if (m_DragGraphic)
                m_DragGraphic.color = m_NormalColor;
            mStartPos = m_DragTarget.localPosition;
            mOnDragUpdate = false;
            ResetPos();
        }

        void OnDisable()
        {
            mPressed = false;
            mMoved = false;
            mBeginReset = false;
            mOnDragUpdate = false;
            mResetTimer = 0;
            if (m_DragGraphic)
                m_DragGraphic.color = m_DisableColor;
            if (m_ResetToBegin)
                m_DragTarget.localPosition = mStartPos;
        }

        void SetDragSatate(bool drag)
        {
            if (drag ^ mOnDragUpdate)
            {
                mOnDragUpdate = drag;
                if (isActiveAndEnabled && m_DragGraphic)
                {
                    StopCoroutine("ChangeColor");
                    StartCoroutine("ChangeColor", drag ? m_OnDragColor : m_NormalColor);
                }
            }
        }

        System.Collections.IEnumerator ChangeColor(Color color)
        {
            float t = 0;
            while(t  < 1)
            {
                yield return null;
                t += Time.unscaledDeltaTime * 4f;
                m_DragGraphic.color = Color.Lerp(m_DragGraphic.color, color, t);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            mPressed = false;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            mPressed = true;
            ResetPos();
            enabled = true;
            mBeginReset = false;
            if (m_BeStartPosWhereDropStart)
            {
                mStartPos = transform.localPosition;
            }
            OnDragBegin(gameObject);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Vector2 screen = TouchPos;
            Vector2 p;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_DragTarget.parent as RectTransform, screen, mCam, out p);
            mTarget = mPos + p - (Vector2)mLocalTouchPos;
        }

        void SetTargetPos()
        {
            Vector3 p = mTarget;
            p.z = m_DragTarget.localPosition.z;
            m_DragTarget.localPosition = p;
        }

        void DoEndDrag()
        {
            OnDragEnd(m_DragTarget.gameObject);
            if (mDrop && m_BeStartPosWhereDrop)
            {
                mStartPos = m_DragTarget.localPosition;
            }
            if (!mDrop && m_ResetToBegin)
            {
                mBeginReset = true;
                mResetTimer = 0;
                mResetDividDuration = m_ResetDuration > 0 ? (1 / m_ResetDuration) : 0;
            }
            mMoved = false;
        }

        void DoResetToStart()
        {
            mResetTimer += Time.unscaledDeltaTime;
            float f = mResetDividDuration > 0 ? mResetTimer * mResetDividDuration : 1;
            Vector3 p = Vector3.Lerp(m_DragTarget.localPosition, mStartPos, f);
            m_DragTarget.localPosition = p;
            if (f >= 1)
            {
                mBeginReset = false;
            }
        }

        void Update()
        {
#if MOBILE
        if (Input.touchCount == 0)
            mPressed = false;
#endif
            if (m_DragTarget)
            {
                if (mPressed)
                {
                    SetDragSatate(true);
                    if (!mMoved)
                        mMoved = Vector2.Distance(mTarget, (Vector2)m_DragTarget.localPosition) >= mPixDistance;
                    if (mMoved)
                        SetTargetPos();
                }
                else if (mMoved)
                {
                    SetDragSatate(true);
                    if(mTarget != (Vector2)m_DragTarget.localPosition)
                    {
                        SetTargetPos();
                    }
                    else
                    {
                        DoEndDrag();   
                    }
                }
                else if (mBeginReset)
                {
                    SetDragSatate(false);
                    DoResetToStart();
                }
                else
                {
                    SetDragSatate(false);
                }
                mDrop = false;
            }
        }

        public void DropTarget()
        {
            mDrop = true;
        }

        public void ResetToBegin()
        {
            if(!mBeginReset)
            {
                mBeginReset = true;
                mResetTimer = 0;
                mResetDividDuration = m_ResetDuration > 0 ? (1 / m_ResetDuration) : 0;
            }
        }

        public void CancelResetToBegin()
        {
            mBeginReset = false;
        }

        private void OnValidate()
        {
                if (!m_DragTarget)
                    m_DragTarget = transform;
                if (m_DragGraphic)
                    m_DragGraphic.color = enabled ? m_NormalColor : m_DisableColor;
        }
    }
}