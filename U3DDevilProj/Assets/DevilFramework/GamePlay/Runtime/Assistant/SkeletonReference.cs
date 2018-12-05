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
        public struct BoneBinder
        {
            public Transform bone;
            public Transform bindTo;

            public BoneBinder(Transform bone, Transform bindTo)
            {
                this.bone = bone;
                this.bindTo = bindTo;
            }
        }

        [SerializeField]
        private Transform m_LocalRoot;
        // 防止设置错误， mLocalRoot 保存修正的localroot
        public Transform LocalRoot { get { return m_LocalRoot == null || !m_LocalRoot.IsChildOf(transform) ? transform : m_LocalRoot; } }
        [SerializeField]
        private Transform m_ReferenceRoot;
        [SerializeField]
        private bool m_ReverseBind;

        [SerializeField]
        private EBakeState m_BakeSelfState = EBakeState.BakePosition;
        [SerializeField]
        private Transform m_BakeSelfToTarget;

        [SerializeField]
        List<BoneBinder> m_Binders = new List<BoneBinder>();

        private Transform mSelfBakeTarget;

        Stack<BoneBinder> mStack;

        public Transform ReferenceRoot
        {
            get { return m_ReferenceRoot; }
            set
            {
                if (m_ReferenceRoot != value)
                {
                    m_ReferenceRoot = value;
                    GetBinders();
                }
            }
        }

        public bool ReverseBind
        {
            get { return m_ReverseBind; }
            set { m_ReverseBind = value; }
        }

        public void BakePose(bool reverseBind)
        {
            if (m_ReferenceRoot == null)
            {
                m_Binders.Clear();
                m_ReferenceRoot = null;
                return;
            }
            if (reverseBind)
            {
                for (int i = 0; i < m_Binders.Count; i++)
                {
                    var bind = m_Binders[i];
                    bind.bindTo.position = bind.bone.position;
                    bind.bindTo.rotation = bind.bone.rotation;
                }
            }
            else
            {
                if (mSelfBakeTarget != null)
                {
                    if ((m_BakeSelfState & EBakeState.BakePosition) != 0)
                        transform.position = mSelfBakeTarget.position;
                    if ((m_BakeSelfState & EBakeState.BakeRotation) != 0)
                        transform.rotation = mSelfBakeTarget.rotation;
                }
                for (int i = 0; i < m_Binders.Count; i++)
                {
                    var bind = m_Binders[i];
                    bind.bone.position = bind.bindTo.position;
                    bind.bone.rotation = bind.bindTo.rotation;
                }
            }
        }
        
        protected virtual string GetBindBoneName(string boneName)
        {
            return boneName;
        }

        [ContextMenu("Get Binders")]
        void GetBinders()
        {
            if (mStack == null)
                mStack = new Stack<BoneBinder>();
            else
                mStack.Clear();
            m_Binders.Clear();
            if (m_ReferenceRoot == null)
                return;
            var root = LocalRoot;
            mStack.Push(new BoneBinder(root, m_ReferenceRoot));
            Transform bone;
            Transform to;
            while (mStack.Count > 0)
            {
                int p = m_Binders.Count;
                while (mStack.Count > 0)
                {
                    m_Binders.Add(mStack.Pop());
                }
                for (int i = p; i < m_Binders.Count; i++)
                {
                    var bind = m_Binders[i];
                    if (bind.bone == m_BakeSelfToTarget)
                        mSelfBakeTarget = bind.bindTo;
                    for (int j = 0; j < bind.bone.childCount; j++)
                    {
                        bone = bind.bone.GetChild(j);
                        to = bind.bindTo.Find(GetBindBoneName(bone.name));
                        if (to != null)
                            mStack.Push(new BoneBinder(bone, to));
                    }
                }
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        
        private void LateUpdate()
        {
            BakePose(m_ReverseBind);
        }

#if UNITY_EDITOR

        [ContextMenu("Bake Pose")]
        void BakePose()
        {
            GetBinders();
            BakePose(m_ReverseBind);
        }

        private void OnValidate()
        {
            bool getbind = true;
            if (m_BakeSelfState == EBakeState.None && m_BakeSelfToTarget != null)
            {
                m_BakeSelfToTarget = null;
                getbind = false;
            }
            if (m_LocalRoot != null && !m_LocalRoot.IsChildOf(transform))
            {
                m_LocalRoot = null;
                getbind = false;
            }
            if (m_BakeSelfToTarget != null && !m_BakeSelfToTarget.IsChildOf(transform))
            {
                m_BakeSelfToTarget = null;
                getbind = false;
            }
            if (getbind)
                GetBinders();
        }
#endif
    }
}