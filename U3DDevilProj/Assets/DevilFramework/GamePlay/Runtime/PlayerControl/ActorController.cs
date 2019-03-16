using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Devil.GamePlay
{
    [System.Serializable]
    public struct InputData
    {
        public int flag;
        public int index;
        public object value;
        public float value1;
        public Vector3 value3;

        public InputData (int flag)
        {
            this.flag = flag;
            index = 0;
            value1 = 0;
            value3 = Vector3.zero;
            value = null;
        }

        public InputData(int flag, int index)
        {
            this.flag = flag;
            this.index = index;
            value1 = 0;
            value3 = Vector3.zero;
            value = null;
        }

        public InputData(int flag, float value)
        {
            this.flag = flag;
            index = 0;
            this.value1 = value;
            value3 = Vector3.zero;
            this.value = null;
        }

        public InputData(int flag, object data)
        {
            this.flag = flag;
            index = 0;
            value1 = 0;
            value3 = Vector3.zero;
            this.value = data;
        }
    }

    public class ActorController : MonoBehaviour
    {
        public interface IFallbackMovement
        {
            bool isGrounded { get; }
            bool isSwiming { get; }
        }

        #region private
        private ActorMovement[] mBaseMotions;
        private ActorMovement[] mAdditiveMotions;
        // 被动技能
        private ActorMovement[] mPassiveMotions;

        //private ActorMovement mLandedFallback;
        //private ActorMovement mFallingFallback;
        //private ActorMovement mSwingFallback;

        [SerializeField]
        GameObject m_AdditiveMotions;
        [SerializeField]
        GameObject m_PassiveMotions;

        private Animator mAnim;
        private Rigidbody mRig;
        private CharacterController mChar;
        Vector3 _velocity;
        IFallbackMovement mStFallback;

        public bool isGrounded { get; private set; }
        
        private bool ProcessPassiveMotion(InputData data)
        {
            bool interact = false;
            for (int i = 0; i < mPassiveMotions.Length; i++)
            {
                var motion = mPassiveMotions[i];
                if (motion.CanUseInput(data))
                {
                    motion.AddInput(data);
                    interact = true;
                }
            }
            return interact;
        }

        private bool ProcessCurrentMotion(InputData data)
        {
            if (CurrentAdditiveMotion != null)
            {
                if (CurrentAdditiveMotion.CanUseInput(data))
                {
                    CurrentAdditiveMotion.AddInput(data);
                    return true;
                }
                else if (CurrentAdditiveMotion.BlendType == EMotionBlend.override_movement)
                {
                    return true;
                }
            }
            if (CurrentBaseMotion != null && CurrentBaseMotion.CanUseInput(data))
            {
                CurrentBaseMotion.AddInput(data);
                return true;
            }
            return false;
        }

        private bool ProcessAdditiveMotion(InputData data)
        {
            bool overrideAdd = CurrentAdditiveMotion == null || CurrentAdditiveMotion.IsInterruptable;
            if (!overrideAdd)
                return false;
            bool overrideBase = CurrentBaseMotion == null || CurrentBaseMotion.IsInterruptable;
            int priority = CurrentAdditiveMotion == null ? int.MinValue : CurrentAdditiveMotion.m_Priority;
            ActorMovement motion;
            for (int i = 0; i < mAdditiveMotions.Length; i++)
            {
                motion = mAdditiveMotions[i];
                if (motion.m_Priority < priority)
                    return false;
                if (!motion.CanUseInput(data))
                    continue;
                if ((motion.BlendType == EMotionBlend.override_movement && overrideAdd && overrideBase)
                    || (motion.BlendType == EMotionBlend.additive_movement && overrideAdd))
                {
                    if (CurrentAdditiveMotion != null)
                        CurrentAdditiveMotion.Interrupt();
                    if (motion.BlendType == EMotionBlend.override_movement && CurrentBaseMotion != null)
                    {
                        CurrentBaseMotion.Interrupt();
                        CurrentBaseMotion = null;
                    }
                    CurrentAdditiveMotion = motion;
                    motion.AddInput(data);
                    return true;
                }
            }
            return false;
        }

        private bool ProcessBaseMotion(InputData data)
        {
            if (CurrentBaseMotion != null && !CurrentBaseMotion.IsInterruptable)
                return false;
            int priority = CurrentBaseMotion == null ? int.MinValue : CurrentBaseMotion.m_Priority;
            ActorMovement motion;
            for (int i = 0; i < mBaseMotions.Length; i++)
            {
                motion = mBaseMotions[i];
                if (motion.m_Priority < priority)
                    return false;
                if (!motion.CanUseInput(data))
                    continue;
                if (CurrentBaseMotion != null)
                    CurrentBaseMotion.Interrupt();
                CurrentBaseMotion = motion;
                motion.AddInput(data);
                return true;
            }
            return false;
        }

        protected virtual void Awake()
        {
            mRig = GetComponent<Rigidbody>();
            mChar = GetComponent<CharacterController>();
            mAnim = GetComponent<Animator>();
        }

        protected virtual void Start()
        {
            InitMovements();
        }
        
        protected virtual void Update()
        {
            //bool noneexecution = isAlive;
            var deltaTime = Time.deltaTime;
            for (int i = 0; i < mPassiveMotions.Length; i++)
            {
                var p = mPassiveMotions[i];
                if (p.IsActive)
                {
                    p.ActorUpdate(deltaTime);
                    if (p.BlendType == EMotionBlend.override_movement)
                    {
                        //noneexecution = false;
                        if (CurrentAdditiveMotion != null)
                        {
                            CurrentAdditiveMotion.Interrupt();
                            CurrentAdditiveMotion = null;
                        }
                        if (CurrentBaseMotion != null)
                        {
                            CurrentBaseMotion.Interrupt();
                            CurrentBaseMotion = null;
                        }
                    }
                }
            }
            if (CurrentAdditiveMotion != null)
            {
                if(!isAlive)
                {
                    CurrentAdditiveMotion.Interrupt();
                    CurrentAdditiveMotion = null;
                }
                else if (CurrentAdditiveMotion.IsActive)
                {
                    CurrentAdditiveMotion.ActorUpdate(deltaTime);
                    //noneexecution = false;
                }
                else
                {
                    CurrentAdditiveMotion = null;
                }
            }
            if (CurrentBaseMotion != null)
            {
                if(!isAlive)
                {
                    CurrentBaseMotion.Interrupt();
                    CurrentBaseMotion = null;
                }
                else if (CurrentBaseMotion.IsActive)
                {
                    CurrentBaseMotion.ActorUpdate(deltaTime);
                    //noneexecution = false;
                }
                else
                {
                    CurrentBaseMotion = null;
                }
            }
          
        }

        protected virtual void LateUpdate()
        {
            if (mStFallback != null)
            {
                isGrounded = mStFallback.isGrounded;
            }
        }

        [ContextMenu("Find Movements")]
        protected void InitMovements()
        {
            mBaseMotions = GetComponents<ActorMovement>();
            mAdditiveMotions = m_AdditiveMotions == null ? new ActorMovement[0] : m_AdditiveMotions.GetComponents<ActorMovement>();
            mPassiveMotions = m_PassiveMotions == null ? new ActorMovement[0] : m_PassiveMotions.GetComponents<ActorMovement>();
            System.Comparison<ActorMovement> compare = (x, y) => x.m_Priority >= y.m_Priority ? -1 : 1;
            GlobalUtil.Sort(mBaseMotions, compare);
            GlobalUtil.Sort(mAdditiveMotions, compare);
            GlobalUtil.Sort(mPassiveMotions, compare);
            //var node = lst.Last;
            for (int i = 0; i < mBaseMotions.Length; i++)
            {
                var mov = mBaseMotions[i];
                mov.SetActor(this);
                if (mov is IFallbackMovement)
                    mStFallback = (IFallbackMovement)mov;
            }
            for (int i = 0; i < mAdditiveMotions.Length; i++)
            {
                mAdditiveMotions[i].SetActor(this);
            }
            for (int i = 0; i < mPassiveMotions.Length; i++)
            {
                mPassiveMotions[i].SetActor(this);
            }
        }

        public ActorMovement FindMovement(FilterDelegate<ActorMovement> filter)
        {
            var ret = GlobalUtil.Find(mBaseMotions, filter);
            if (ret == null)
                ret = GlobalUtil.Find(mAdditiveMotions, filter);
            if (ret == null)
                ret = GlobalUtil.Find(mPassiveMotions, filter);
            return ret;
        }

        #endregion

        #region public methods

        public virtual Vector3 position
        {
            get { return transform.position; }
            set
            {
                if (isActiveAndEnabled && mRig != null && !mRig.isKinematic)
                    mRig.MovePosition(value);
                else
                    transform.position = value;
                //if (mAgent != null)
                //    mAgent.Warp(value);
            }
        }
        public virtual Quaternion rotation { get { return transform.rotation; } set { transform.rotation = value; } }
        public virtual Vector3 velocity
        {
            get
            {
                if (mRig != null && !mRig.isKinematic)
                    _velocity = mRig.velocity;
                return _velocity; ;
            }
            set
            {
                _velocity = value;
                if (mRig != null && !mRig.isKinematic)
                    mRig.velocity = value;
            }
        }

        public bool isDestroied { get { return this == null; } }

        public virtual bool isAlive { get { return true; } }

        public ActorMovement CurrentBaseMotion { get; private set; }

        public ActorMovement CurrentAdditiveMotion { get; private set; }

        public Animator AttachedAnimator
        {
            get
            {
                if (mAnim == null)
                    mAnim = GetComponent<Animator>();
                return mAnim;
            }
        }

        public Rigidbody AttachedRigidbody
        {
            get
            {
                if (mRig == null)
                    mRig = GetComponent<Rigidbody>();
                return mRig;
            }
        }

        public CharacterController AttachedCharacter { get { return mChar; } }
        
        public bool AddInput(InputData data)
        {
            if (!isAlive)
                return false;
            // 被动劫持动作响应
            if (ProcessPassiveMotion(data))
                return true;
            // 优先响应当前动作
            if (ProcessCurrentMotion(data))
                return true;
            // 响应叠加动作
            if (ProcessAdditiveMotion(data))
                return true;
            // 响应基础层动作
            return ProcessBaseMotion(data);
        }

        #endregion
    }
}