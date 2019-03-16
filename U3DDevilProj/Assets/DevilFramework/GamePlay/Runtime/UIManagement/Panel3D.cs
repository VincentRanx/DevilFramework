using UnityEngine;

namespace Devil.GamePlay
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    public class Panel3D : BindableMono
    {
        public float m_Width = 1f;
        [Range(0, 1f)]
        public float m_DynamicRange = 0.5f;

        [Range(0, 1f)]
        public float m_ScaleByDistance = 0;

        float mScaler;
        Canvas mCanvas;
        public Canvas GetCanvas()
        {
            if (mCanvas == null)
                mCanvas = GetComponent<Canvas>();
            return mCanvas;
        }

        protected virtual void OnEnable()
        {
            ScaleSize();
            var can = GetCanvas();
            can.renderMode = RenderMode.WorldSpace;
            can.worldCamera = Camera.main;
        }

        void ScaleSize()
        {
            var can = transform as RectTransform;
            if(can != null)
            {
                mScaler = m_Width / Mathf.Max(1, can.sizeDelta.x);
                transform.localScale = mScaler * Vector3.one;
            }
        }

        public Vector3 position { get { return transform.position; } set { transform.position = value; } }

        protected virtual void OnDisable()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void LateUpdate()
        {
#if UNITY_EDITOR
            ScaleSize();
#endif
            AlignToCamera();
        }

        void AlignToCamera()
        {
            var can = GetCanvas();
            var cam = can.worldCamera;
            if (cam == null)
                return;
            var ctrans = cam.transform;
            var pos = ctrans.worldToLocalMatrix.MultiplyPoint(transform.position);
            var dir = pos.normalized;
            if (Vector3.Dot(dir, Vector3.forward) <= 0)
                return;
            var rot = Quaternion.FromToRotation(Vector3.forward, dir);
            rot = Quaternion.Slerp(rot, Quaternion.LookRotation(Vector3.forward, Vector3.up), m_DynamicRange);
            transform.rotation = rot * ctrans.rotation;
            transform.localScale = Vector3.one * mScaler * (1 + Vector3.Distance(transform.position, ctrans.position) * 0.05f * m_ScaleByDistance);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            ScaleSize();
        }
#endif
    }
}