using Devil.GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    [RequireComponent(typeof(Canvas))]
    public class Panel3_2D : MonoBehaviour
    {
        [SortingLayerField]
        public int m_SortingLayer;

        public RectTransform m_Content;

        bool mIsPosSet;
        Vector3 mPos;

        Canvas mCanvas;
        public Canvas GetCanvas()
        {
            if (mCanvas == null)
                mCanvas = GetComponent<Canvas>();
            return mCanvas;
        }

        public Vector3 position { get { return mPos; } set { mPos = value; mIsPosSet = true; } }

        private void OnEnable()
        {
            var can = GetCanvas();
            if (PanelManager.Instance != null)
            {
                can.renderMode = RenderMode.ScreenSpaceCamera;
                can.worldCamera = PanelManager.Instance.UICamera;
                can.sortingLayerID = m_SortingLayer;
            }
            if (m_Content != null)
            {
                if (!mIsPosSet)
                {
                    mPos = m_Content.position;
                    mIsPosSet = true;
                }
                UpdatePosition();
            }
        }

        void UpdatePosition()
        {
            var can = GetCanvas();
            var pos = RectTransformUtility.WorldToScreenPoint(Camera.main, mPos);
            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Content.parent as RectTransform, pos, can.worldCamera, out local))
                m_Content.localPosition = local;
        }

        protected virtual void LateUpdate()
        {
            if (m_Content != null)
                UpdatePosition();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var can = GetCanvas();
            can.sortingLayerID = m_SortingLayer;
        }
#endif
    }
}