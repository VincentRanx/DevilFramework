using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devil.UI
{
    public class MinSizeLayout : UIBehaviour, ILayoutSelfController
    {
        public enum FitMode
        {
            Unconstrained,
            MinSize,
            PreferredSize,
        }

        public RectTransform m_Viewport;
        [SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;
        public FitMode horizontalFit
        {
            get { return m_HorizontalFit; }
            set
            {
                if (value != m_HorizontalFit)
                {
                    m_HorizontalFit = value;
                    SetDirty();
                }
            }
        }
        [SerializeField]
        float m_MinWidth = 0;
        public float MinWidth
        {
            get { return m_MinWidth; }
            set
            {
                if (m_MinWidth != value)
                {
                    m_MinWidth = value;
                    if (m_HorizontalFit == FitMode.MinSize)
                        SetDirty();
                }
            }
        }
        [SerializeField] protected FitMode m_VerticalFit = FitMode.Unconstrained;
        public FitMode verticalFit
        {
            get { return m_VerticalFit; }
            set
            {
                if (value != m_VerticalFit)
                {
                    m_VerticalFit = value;
                    SetDirty();
                }
            }
        }

        [System.NonSerialized] private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }
        [SerializeField]
        float m_MinHeight = 0;
        public float MinHeight
        {
            get { return m_MinHeight; }
            set
            {
                if (m_MinHeight != value)
                {
                    m_MinHeight = value;
                    if (m_VerticalFit == FitMode.MinSize)
                        SetDirty();
                }
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
            if (fitting == FitMode.Unconstrained)
            {
                // Keep a reference to the tracked transform, but don't control its properties:
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
                return;
            }

            m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            // Set size to min or preferred size
            if (fitting == FitMode.MinSize)
            {
                float min;
                if (m_Viewport != null)
                    min = axis == 0 ? m_Viewport.rect.width : m_Viewport.rect.height;
                else
                    min = axis == 0 ? m_MinWidth : m_MinHeight;
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, Mathf.Max(min, LayoutUtility.GetMinSize(m_Rect, axis)));
            }
            else
            {
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(m_Rect, axis));
            }
        }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

#endif
    }
}