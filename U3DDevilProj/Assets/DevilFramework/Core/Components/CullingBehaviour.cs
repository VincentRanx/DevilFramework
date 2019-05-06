using System.Collections.Generic;
using UnityEngine;

namespace Devil
{
    public delegate void CullEvent(ICulling target, bool visible, int distanceLv);

    [DefaultExecutionOrder(1000)]
    public class CullingBehaviour : MonoBehaviour
	{

        public class CullTarget
        {
            public ICulling target;
            public bool visible;
            public int cullLv;

            public CullTarget() { }

            public CullTarget(ICulling target)
            {
                this.target = target;
            }
        }

        public static event System.Action<CullingBehaviour> OnCurrentCullerChanged;

        static List<CullingBehaviour> sCullers = new List<CullingBehaviour>();
        public static CullingBehaviour CurrentCulling { get { return sCullers.Count > 0 ? sCullers[sCullers.Count - 1] : null; } }

        [SerializeField]
        private int m_CullingCapacity = 512;

        [SerializeField]
        private Vector4 m_DistanceLv = new Vector4(50, 100, 200, 500);

        public bool m_AutoFindCamera = true;

        // culling api
        private int mCapacity;
        private CullingGroup mCullingGroup;
        private BoundingSphere[] mCullingSpheres;
        private CullTarget[] mCullingTarget;
        private int mCullLength;

        private float[] mDistanceLvs;

        public event CullEvent OnCulling;
        
        private void OnEnable()
        {
            if (m_AutoFindCamera)
            {
                var cam = GetComponent<Camera>();
                if (cam == null)
                    cam = Camera.main;
                SetCamera(cam);
            }
            sCullers.Add(this);
            if (OnCurrentCullerChanged != null)
                OnCurrentCullerChanged(this);
        }

        private void OnDisable()
        {
            if (m_AutoFindCamera)
                SetCamera(null);
            sCullers.Remove(this);
            if (OnCurrentCullerChanged != null)
                OnCurrentCullerChanged(CurrentCulling);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < mCullLength; i++)
            {
                if (mCullingTarget[i].target != null)
                    mCullingSpheres[i] = mCullingTarget[i].target.Bounding;
            }
        }

        private void OnDestroy()
        {
            if(mCullingGroup != null)
            {
                mCullingGroup.Dispose();
                mCullingGroup = null;
            }
            mCullLength = 0;
            mCapacity = 0;
            mCullingSpheres = null;
            mCullingTarget = null;
        }

        private void InitCapacity(int cap)
        {
            cap = Mathf.Max(32, cap);
            if (mCapacity != cap)
            {
                var len = mCapacity;
                mCapacity = cap;
                var sphs = mCullingSpheres;
                var tars = mCullingTarget;
                mCullingSpheres = new BoundingSphere[mCapacity];
                mCullingTarget = new CullTarget[mCapacity];
                if(len > 0)
                {
                    len = Mathf.Min(len, cap);
                    System.Array.Copy(sphs, mCullingSpheres, len);
                    System.Array.Copy(tars, mCullingTarget, len);
                }
            }
        }

        public void SetCamera(Camera cam)
        {
            if (cam != null)
            {
                if (mCullingSpheres == null)
                    InitCapacity(m_CullingCapacity);
                if (mCullingGroup == null)
                {
                    if (mDistanceLvs == null)
                        mDistanceLvs = new float[4];
                    mDistanceLvs[0] = m_DistanceLv.x;
                    mDistanceLvs[1] = m_DistanceLv.y;
                    mDistanceLvs[2] = m_DistanceLv.z;
                    mDistanceLvs[3] = m_DistanceLv.w;
                    mCullingGroup = new CullingGroup();
                    mCullingGroup.SetBoundingDistances(mDistanceLvs);
                    mCullingGroup.SetDistanceReferencePoint(transform);
                    mCullingGroup.SetBoundingSpheres(mCullingSpheres);
                    mCullingGroup.SetBoundingSphereCount(mCullLength);
                    mCullingGroup.onStateChanged = OnCullStateChanged;
                }
                mCullingGroup.targetCamera = cam;
                mCullingGroup.enabled = true;
            }
            else if(mCullingGroup != null)
            {
                mCullingGroup.enabled = false;
            }
        }

        private void OnCullStateChanged(CullingGroupEvent sphere)
        {
            var t = mCullingTarget[sphere.index];
            if (t.target == null)
                return;
            t.visible = sphere.isVisible;
            t.cullLv = sphere.currentDistance;
            t.target.OnCulling(sphere.isVisible, sphere.currentDistance);
            if (OnCulling != null)
                OnCulling(t.target, t.visible, t.cullLv);
        }

        public void AddCullingTarget(ICulling target)
        {
            if (mCullingSpheres == null)
                InitCapacity(m_CullingCapacity);
            for (int i = 0; i < mCullLength; i++)
            {
                if (mCullingTarget[i].target == null)
                {
                    mCullingTarget[i].target = target;
                    mCullingSpheres[i] = target.Bounding;
                    return;
                }
            }
            mCullingTarget[mCullLength] = new CullTarget(target);
            mCullingSpheres[mCullLength] = target.Bounding;
            mCullLength++;
            if (mCullingGroup != null)
            {
                mCullingGroup.SetBoundingSphereCount(mCullLength);
            }
        }

        public void RemoveCullingTarget(ICulling target)
        {
            for (int i = mCullLength - 1; i >= 0; i--)
            {
                if (mCullingTarget[i].target == target)
                {
                    mCullingTarget[i].target = null;
                    if (mCullLength == i + 1)
                        mCullLength--;
                    return;
                }
            }
        }

#if UNITY_EDITOR

        Color[] colors = new Color[] { Color.green,
            Color.Lerp(Color.green, Color.red, 0.33f),
            Color.Lerp(Color.green, Color.red, 0.66f),
            Color.red };

        private void OnValidate()
        {
            InitCapacity(m_CullingCapacity);
            if (mCullingGroup != null)
            {
                mCullingGroup.SetBoundingSpheres(mCullingSpheres);
                mCullingGroup.SetBoundingSphereCount(mCullLength);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "Cull Icon.png");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = colors[0];
            Gizmos.DrawWireSphere(transform.position, m_DistanceLv.x);
            Gizmos.color = colors[1];
            Gizmos.DrawWireSphere(transform.position, m_DistanceLv.y);
            Gizmos.color = colors[2];
            Gizmos.DrawWireSphere(transform.position, m_DistanceLv.z);
            Gizmos.color = colors[3];
            Gizmos.DrawWireSphere(transform.position, m_DistanceLv.w);

            for (int i = 0; i < mCullLength; i++)
            {
                if (mCullingTarget[i].target == null)
                    continue;
                Gizmos.color = mCullingTarget[i].visible ? colors[mCullingTarget[i].cullLv] : Color.gray;
                var bound = mCullingTarget[i].target.Bounding;
                Gizmos.DrawWireSphere(bound.position, bound.radius);
            }
        }
#endif
    }
}