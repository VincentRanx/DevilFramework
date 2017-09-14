
#if ( UNITY_IPHONE || UNITY_ANDROID ) && !UNITY_EDITOR
#define MOBILE
#endif

using DevilTeam.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DevilTeam.UI
{
    [AddComponentMenu("UISupport/Dragable Object")]
    public class DragableObject2D : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        public delegate void DragDelegate(GameObject go);

        Vector2 mPos;
        Vector2 mScreenPos;

        Vector2 mTouchPos;
        Vector3 mLocalTouchPos;

        Vector2 mTarget;

        Camera mCam;
        bool mPressed;
        bool mMoved;

        public Transform m_DragTarget;
        [SerializeField]
        float m_FollowStrength = 0.5f;

        float mPixDistance;

        DragDelegate mDragEnd;

        public void AddDragEndIfNeeded(DragDelegate onDragEnd)
        {
            if (onDragEnd == null)
                return;
            if (mDragEnd != null)
            {
                System.Delegate[] list = mDragEnd.GetInvocationList();
                if (list != null && list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                    {
                        System.Delegate del = list[i];
                        if ((DragDelegate)del == onDragEnd)
                            return;
                    }
                }
            }
            mDragEnd += onDragEnd;
        }

        public void RemoveDragEnd(DragDelegate onDragEnd)
        {
            mDragEnd -= onDragEnd;
        }

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
            if (mCam)
            {
                mLocalTouchPos = mCam.ScreenToWorldPoint(mScreenPos);
                Transform par = m_DragTarget.parent;
                if (par)
                    mLocalTouchPos = par.worldToLocalMatrix.MultiplyPoint(mLocalTouchPos);
            }
            mTarget = mPos;
            mPixDistance = PixelDistance * 5f;
        }

        float PixelDistance
        {
            get
            {
                if (mCam)
                {
                    Vector3 p1 = mCam.ScreenToWorldPoint(new Vector3(0, 0, 0));
                    Vector3 p2 = mCam.ScreenToWorldPoint(new Vector3(1, 0, 0));
                    Transform par = m_DragTarget.parent;
                    Matrix4x4 m = par.worldToLocalMatrix;
                    if (par)
                    {
                        p1 = m.MultiplyPoint(p1);
                        p2 = m.MultiplyPoint(p2);
                    }
                    p1.z = 0;
                    p2.z = 0;
                    return Vector3.Distance(p1, p2);
                }
                else
                {
                    return 0f;
                }
            }
        }

        void Start()
        {

        }

        void OnEnable()
        {
            mMoved = false;
            if (!m_DragTarget)
                m_DragTarget = transform;
            if (!mCam)
            {
                mCam = ComponentUtil.ActiveCameraForLayer(gameObject.layer);
            }
            ResetPos();
        }

        void OnDisable()
        {
            mPressed = false;
            mMoved = false;
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
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Vector2 screen = TouchPos;
            Vector3 pos = mCam.ScreenToWorldPoint(screen);
            Transform par = m_DragTarget.parent;
            if (par)
                pos = par.worldToLocalMatrix.MultiplyPoint(pos);
            mTarget = mPos + (Vector2)pos - (Vector2)mLocalTouchPos;
        }

        void Update()
        {
#if MOBILE
        if (Input.touchCount == 0)
            mPressed = false;
#endif
            if (mCam && m_DragTarget)
            {
                if (mPressed)
                {

                    if (!mMoved)
                        mMoved = Vector2.Distance(mTarget, (Vector2)m_DragTarget.localPosition) >= mPixDistance;
                    if (mMoved)
                    {
                        Vector3 p = mTarget;
                        //p = UITools.CloseTo(m_DragTarget.localPosition, p, m_FollowStrength, 0.3f);
                        p.z = m_DragTarget.localPosition.z;
                        m_DragTarget.localPosition = p;
                    }
                }
                else if (mMoved && mTarget != (Vector2)transform.localPosition)
                {
                    Vector3 p = mTarget;
                    //p = UITools.CloseTo(m_DragTarget.localPosition, p, m_FollowStrength, 0.3f);
                    p.z = m_DragTarget.localPosition.z;
                    m_DragTarget.localPosition = p;
                }
                else
                {
                    if (mMoved && mDragEnd != null)
                    {
                        mDragEnd(m_DragTarget.gameObject);
                    }
                    mMoved = false;
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            m_FollowStrength = Mathf.Clamp(m_FollowStrength, 0.2f, 1f);
        }

#endif
    }
}