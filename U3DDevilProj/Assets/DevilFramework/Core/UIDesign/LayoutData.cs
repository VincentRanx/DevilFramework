using UnityEngine;
using UnityEngine.EventSystems;

namespace Devil.UI
{
    public class LayoutData : UIBehaviour
    {
        public struct LayoutInfo
        {
            public int id;
            public object data;

            public static LayoutInfo NULL
            {
                get
                {
                    LayoutInfo info;
                    info.id = 0;
                    info.data = 0;
                    return info;
                }
            }
        }

        // 就位时间
        [Range(0, 3f)]
        [SerializeField]
        float m_PositionDuration;
        RectTransform mTrans;
        public RectTransform rectTransform
        {
            get
            {
                if (mTrans == null)
                    mTrans = transform as RectTransform;
                return mTrans;
            }
        }

        Vector3 mFinalPos;
        bool mPosDelayed;
        Vector3 mPosStart;
        float mPosTime;
        float mPosTimeScale;

        public virtual void SetLocalPosition(Vector3 localPos)
        {
#if UNITY_EDITOR
            if(!Application.isPlaying)
            {
                transform.localPosition = localPos;
                return;
            }
#endif
            mFinalPos = localPos;
            mPosDelayed = m_PositionDuration >= 0.1f;
            if (!mPosDelayed)
            {
                transform.localPosition = localPos;
            }
            else
            {
                mPosStart = transform.localPosition;
                mPosTimeScale = 1 / m_PositionDuration;
                mPosTime = 0;
            }
        }

        public virtual void OnBindData(LayoutInfo info)
        {
            //if (!gameObject.activeSelf)
            //    gameObject.SetActive(true);
        }

        public virtual void OnUnbindData()
        {
            //if (gameObject.activeSelf)
            //    gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            if (mPosDelayed)
            {
                mPosTime += Time.unscaledDeltaTime * mPosTimeScale;
                OnProcessPos(Mathf.Clamp01(mPosTime));
                if (mPosTime >= 1)
                    mPosDelayed = false;
            }
        }

        protected virtual void OnProcessPos(float percentage)
        {
            transform.localPosition = Vector3.Lerp(mPosStart, mFinalPos, -percentage * (percentage - 2));
        }
    }
}