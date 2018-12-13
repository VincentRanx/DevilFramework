using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devil.UI
{
    [ExecuteInEditMode]
    public class ContentSizeJudgeLayout : UIBehaviour, ILayoutSelfController
    {
        public enum FitMode
        {
            Unconstrained,
            MinSize,
            PreferredSize
        }

        [SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;
        public FitMode horizontalFit
        {
            get { return m_HorizontalFit; }
            set
            {
                if (m_HorizontalFit != value)
                {
                    m_HorizontalFit = value;
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
                if (m_VerticalFit != value)
                {
                    m_VerticalFit = value;
                    SetDirty();
                }
            }
        }
        [SerializeField]
        protected RectTransform m_Brother;
        [SerializeField]
        protected Vector4 m_BrotherSpace; // left, bottom, top, right
        // 扩展父节点
        public bool m_ExpendParentWidth;
        public bool m_ExpendParentHeight;
        public Vector2 m_MinParentSize = new Vector2(100, 200);
        public Vector2 m_ParentSpace = new Vector2(10, 10);

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

        private DrivenRectTransformTracker m_Tracker;

        protected ContentSizeJudgeLayout()
        { }

        protected override void OnEnable()
        {
            base.OnEnable();
            AlignBrother();
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
            AlignBrother();
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
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(m_Rect, axis));
            else
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(m_Rect, axis));
        }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            FixAnchors();
            HandleSelfFittingAlongAxis(0);
        }

        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
            AlignBrother();
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        void FixAnchors()
        {
            if (!m_ExpendParentHeight && !m_ExpendParentWidth)
                return;
            RectTransform parent = transform.parent as RectTransform;
            var self = rectTransform;
            if (parent != null)
            {
                var amin = parent.anchorMin;
                var amax = parent.anchorMax;
                var smin = self.anchorMin;
                var smax = self.anchorMax;
                if (m_ExpendParentHeight)
                {
                    var v = (amin.y + amax.y) * 0.5f;
                    amin.y = v;
                    amax.y = v;
                    v = (smin.y + smax.y) * 0.5f;
                    smin.y = v;
                    smax.y = v;
                }
                if (m_ExpendParentWidth)
                {
                    var h = (amin.x + amax.x) * 0.5f;
                    amin.x = h;
                    amax.x = h;
                    h = (smin.x + smax.x) * 0.5f;
                    smin.x = h;
                    smax.x = h;
                }
            }
        }

        void AlignBrother()
        {
            Vector2 size = rectTransform.rect.size + new Vector2(m_BrotherSpace.x + m_BrotherSpace.z, m_BrotherSpace.y + m_BrotherSpace.w);
            if (m_Brother != null)
            {
                Vector3 location = rectTransform.localPosition;
                Rect rect = rectTransform.rect;
                Rect prect = (rectTransform.parent as RectTransform).rect;
                Vector2 p = new Vector2((location.x - prect.xMin) / prect.width, (location.y - prect.yMin) / prect.height);
                m_Brother.pivot = rectTransform.pivot;
                m_Brother.anchorMin = p;
                m_Brother.anchorMax = p;
                m_Brother.localPosition = rectTransform.localPosition;
                m_Brother.offsetMin = new Vector2(rect.xMin - m_BrotherSpace.x, rect.yMin - m_BrotherSpace.y);
                m_Brother.offsetMax = new Vector2(rect.xMax + m_BrotherSpace.z, rect.yMax + m_BrotherSpace.w);
            }
            if (m_ExpendParentWidth || m_ExpendParentHeight)
            {
                RectTransform parent = transform.parent as RectTransform;
                var self = rectTransform;
                if(parent != null)
                {
                    if (m_ExpendParentWidth)
                        parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(m_MinParentSize.x, size.x + m_ParentSpace.x * 2));
                    if (m_ExpendParentHeight)
                        parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(m_MinParentSize.y, size.y + m_ParentSpace.y * 2));
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

#endif
    }
}