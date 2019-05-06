using Devil.GamePlay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
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

        protected virtual void Awake()
        {
            var sub = GetComponentsInChildren<ISubPanel>(true);
            if (sub != null)
            {
                var can = GetCanvas();
                for (int i = 0; i < sub.Length; i++)
                {
                    sub[i].SetRootCanvas(can);
                }
            }
        }

        protected virtual void Start() { }

        protected virtual void OnEnable()
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
                MoveToWorldPos(m_Content, mPos);

            }
        }

        protected virtual void OnDisable() { }

        protected virtual void OnDestroy() { }

        protected virtual void LateUpdate()
        {
            if (m_Content != null)
                MoveToWorldPos(m_Content, mPos);
        }

        public bool MoveToWorldPos(Transform trans, Vector3 worldPos)
        {
            return MoveToWorldPos(GetCanvas(), trans, worldPos);
        }

        public static bool MoveToWorldPos(Canvas canvas, Transform trans, Vector3 worldPos)
        {
            var pos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);
            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(trans.parent as RectTransform, pos, canvas.worldCamera, out local))
            {
                trans.localPosition = local;
                return true;
            }
            return false;
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