using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    [DefaultExecutionOrder(1000)]
    [ExecuteInEditMode]
    public class SkeletonReference : MonoBehaviour
    {
        public enum EBakeState
        {
            None = 0,
            BakePosition = 1,
            BakeRotation = 2,
            BakePosAndRot = 3,
        }

        [System.Serializable]
        public class BoneBinder
        {
            public string bonePath;
            public Transform bone;
            public Transform bindTo;

            public BoneBinder() { }
        }

        [HideInInspector]
        [SerializeField]
        private Transform m_LocalRoot;
        public Transform LocalRoot {
            get { return m_LocalRoot == null ? transform : m_LocalRoot; }
#if UNITY_EDITOR
            // 只允许编辑器修改
            set { m_LocalRoot = value; }
#endif
        }

        [HideInInspector]
        [SerializeField]
        private Transform m_ReferenceRoot;
        [SerializeField]
        private bool m_ReverseBind;

        public EBakeState m_BakeSelfState = EBakeState.BakePosition;

        [HideInInspector]
        [SerializeField]
        List<BoneBinder> m_Binders = new List<BoneBinder>();
        public List<BoneBinder> Binders { get { return m_Binders; } }

        [HideInInspector]
        public int m_BakeSelfToBoneIndex;
        
        public Transform ReferenceRoot
        {
            get { return m_ReferenceRoot; }
            set
            {
                if (m_ReferenceRoot != value)
                {
                    m_ReferenceRoot = value;
                    UpdateBinders();
                }
            }
        }

        public bool ReverseBind
        {
            get { return m_ReverseBind; }
            set { m_ReverseBind = value; }
        }
        
        public void BakePose(bool reverseBind, float weight)
        {
            if (m_ReferenceRoot == null)
            {
                m_ReferenceRoot = null;
                return;
            }
            var local = LocalRoot;
            if (reverseBind)
            {
                m_ReferenceRoot.position = Vector3.Lerp(m_ReferenceRoot.position, local.position, weight);
                m_ReferenceRoot.rotation = Quaternion.Slerp(m_ReferenceRoot.rotation, local.rotation, weight);
                for (int i = 0; i < m_Binders.Count; i++)
                {
                    var bind = m_Binders[i];
                    if (bind.bone == null || bind.bindTo == null)
                        continue;
                    bind.bindTo.position = Vector3.Lerp(bind.bindTo.position, bind.bone.position, weight);
                    bind.bindTo.rotation = Quaternion.Slerp(bind.bindTo.rotation, bind.bone.rotation, weight);
                }
            }
            else
            {
                if (m_BakeSelfToBoneIndex < m_Binders.Count && m_BakeSelfToBoneIndex >= 0 && m_BakeSelfState != 0)
                {
                    var bone = m_Binders[m_BakeSelfToBoneIndex];
                    if (bone.bone != transform && bone.bindTo != null)
                    {
                        if ((m_BakeSelfState & EBakeState.BakePosition) != 0)
                            transform.position = Vector3.Lerp(transform.position, bone.bindTo.position, weight);
                        if ((m_BakeSelfState & EBakeState.BakeRotation) != 0)
                            transform.rotation = Quaternion.Slerp(transform.rotation, bone.bindTo.rotation, weight);
                    }
                }
                local.position = Vector3.Lerp(local.position, m_ReferenceRoot.position, weight);
                local.rotation = Quaternion.Slerp(local.rotation, m_ReferenceRoot.rotation, weight);
                for (int i = 0; i < m_Binders.Count; i++)
                {
                    var bind = m_Binders[i];
                    if (bind.bone == null || bind.bindTo == null)
                        continue;
                    bind.bone.position = Vector3.Lerp(bind.bone.position, bind.bindTo.position, weight);
                    bind.bone.rotation = Quaternion.Slerp(bind.bone.rotation, bind.bindTo.rotation, weight);
                }
            }
        }

        public void BakePose(bool reverseBind)
        {
            if (m_ReferenceRoot == null)
            {
                m_ReferenceRoot = null;
                return;
            }
            var local = LocalRoot;
            if (reverseBind)
            {
                m_ReferenceRoot.position = local.position;
                m_ReferenceRoot.rotation = local.rotation;
                for (int i = 0; i < m_Binders.Count; i++)
                {
                    var bind = m_Binders[i];
                    if (bind.bone == null || bind.bindTo == null)
                        continue;
                    bind.bindTo.position = bind.bone.position;
                    bind.bindTo.rotation = bind.bone.rotation;
                }
            }
            else
            {
                if (m_BakeSelfToBoneIndex < m_Binders.Count && m_BakeSelfToBoneIndex >= 0 && m_BakeSelfState != 0)
                {
                    var bone = m_Binders[m_BakeSelfToBoneIndex];
                    if (bone.bone != transform && bone.bindTo != null)
                    {
                        if ((m_BakeSelfState & EBakeState.BakePosition) != 0)
                            transform.position = bone.bindTo.position;
                        if ((m_BakeSelfState & EBakeState.BakeRotation) != 0)
                            transform.rotation = bone.bindTo.rotation;
                    }
                }
                local.position = m_ReferenceRoot.position;
                local.rotation = m_ReferenceRoot.rotation;
                for (int i = 0; i < m_Binders.Count; i++)
                {
                    var bind = m_Binders[i];
                    if (bind.bone == null || bind.bindTo == null)
                        continue;
                    bind.bone.position = bind.bindTo.position;
                    bind.bone.rotation = bind.bindTo.rotation;
                }
            }
        }
        
        protected virtual string GetBindBoneName(string boneName)
        {
            return boneName;
        }
        
        void UpdateBinders()
        {
            var target = ReferenceRoot;
            if (target == null)
                return;
            for (int i = m_Binders.Count - 1; i>= 0;i--)
            {
                var bone = m_Binders[i];
                bone.bindTo = target.Find(bone.bonePath);
            }
        }
        
        private void LateUpdate()
        {
            BakePose(m_ReverseBind);
        }

#if UNITY_EDITOR
        
        [ContextMenu("Bake Pose")]
        void BakePose()
        {
            BakePose(m_ReverseBind);
        }
#endif
    }
}