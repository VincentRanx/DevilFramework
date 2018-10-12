using Devil.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devil.UI
{
    [ExecuteInEditMode]
    public class RelativeFitter : UIBehaviour, ILayoutSelfController
    {
        public enum EEdge
        {
            left, 
            right,
            up,
            down,
        }

        [System.Serializable]
        public class EdgeData
        {
            public RectTransform m_Target;
            public EEdge m_Edge;
            public float m_Offset;

            public EdgeData()
            {

            }

            public EdgeData(EEdge edge)
            {
                m_Edge = edge;
            }

            public float GetEdgePosition(RectTransform parent)
            {
                if (parent == null)
                    return m_Offset;
                RectTransform target;
                if (m_Target == null)
                    target = parent;
                else
                    target = m_Target;
                var matrix = target == parent ? Matrix4x4.identity : (parent.worldToLocalMatrix * target.localToWorldMatrix);
                Vector3 pos;
                switch (m_Edge)
                {
                    case EEdge.left:
                        pos = matrix.MultiplyPoint(new Vector3(target.rect.xMin + m_Offset, 0, 0));
                        return pos.x;
                    case EEdge.right:
                        pos = matrix.MultiplyPoint(new Vector3(target.rect.xMax + m_Offset, 0, 0));
                        return pos.x;
                    case EEdge.up:
                        pos = matrix.MultiplyPoint(new Vector3(0, target.rect.yMax + m_Offset, 0));
                        return pos.y;
                    case EEdge.down:
                        pos = matrix.MultiplyPoint(new Vector3(0, target.rect.yMin + m_Offset, 0));
                        return pos.y;
                    default:
                        return m_Offset;
                }
            }
        }

        [SerializeField]
        EdgeData m_LeftEdge = new EdgeData(EEdge.left);

        [SerializeField]
        EdgeData m_RightEdge = new EdgeData(EEdge.right);

        [SerializeField]
        EdgeData m_UpEdge = new EdgeData(EEdge.up);

        [SerializeField]
        EdgeData m_DownEdge = new EdgeData(EEdge.down);

        [System.NonSerialized]
        private RectTransform m_Rect;

        bool mDirty;

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
        
        void UpdateRect()
        {
            m_Tracker.Clear();
            Vector4 edge; // left, right, up, down
            var parent = rectTransform.parent as RectTransform;
            var rect = _2DUtil.CalculateRelativeRect(parent, rectTransform);
            if (m_LeftEdge != null)
                edge.x = m_LeftEdge.GetEdgePosition(parent);
            else
                edge.x = rect.xMin;
            if (m_RightEdge != null)
                edge.y = m_RightEdge.GetEdgePosition(parent);
            else
                edge.y = rect.xMax;
            if (m_UpEdge != null)
                edge.z = m_UpEdge.GetEdgePosition(parent);
            else
                edge.z = rect.yMax;
            if (m_DownEdge != null)
                edge.w = m_DownEdge.GetEdgePosition(parent);
            else
                edge.w = rect.yMin;
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.SizeDelta | DrivenTransformProperties.AnchoredPosition);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            //rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.offsetMin = new Vector2(edge.x, edge.w) - parent.rect.min;
            rectTransform.offsetMax = new Vector2(edge.y, edge.z) - parent.rect.max;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mDirty = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_Tracker.Clear();
            //LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        private void Update()
        {
            if (mDirty)
            {
                mDirty = false;
                UpdateRect();
            }
        }

        public void SetLayoutHorizontal()
        {
            
        }

        public void SetLayoutVertical()
        {
            
        }

        [ContextMenu("Set Layout Drity")]
        public void SetDirty()
        {
            mDirty = true;
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateRect();
        }

        void DrawEdgeGizmos(EdgeData edge)
        {
            Rect rect = edge.m_Target.rect;
            Gizmos.color = Color.green * 0.5f;
            Gizmos.matrix = edge.m_Target.localToWorldMatrix;
            Gizmos.DrawCube(rect.center, rect.size);
            Gizmos.color = Color.yellow;
            switch (edge.m_Edge)
            {
                case EEdge.left:
                    Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMin, rect.yMax));
                    break;
                case EEdge.right:
                    Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMin), new Vector3(rect.xMax, rect.yMax));
                    break;
                case EEdge.up:
                    Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax));
                    break;
                case EEdge.down:
                    Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMin), new Vector3(rect.xMax, rect.yMin));
                    break;
                default:
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_LeftEdge != null && m_LeftEdge.m_Target != null)
                DrawEdgeGizmos(m_LeftEdge);

            if (m_RightEdge != null && m_RightEdge.m_Target != null)
                DrawEdgeGizmos(m_RightEdge);

            if (m_UpEdge != null && m_UpEdge.m_Target != null)
                DrawEdgeGizmos(m_UpEdge);

            if (m_DownEdge != null && m_DownEdge.m_Target != null)
                DrawEdgeGizmos(m_DownEdge);
        }
#endif
    }
}