using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public class GamePlayerBase : MonoBehaviour, IGamePlayer
    {
        [SerializeField]
        private MotionAsset[] m_Motions = new MotionAsset[0];

        private bool mIsReady;
        private IPlayerMotion[] mBaseMotions;
        private IPlayerMotion[] mAdditiveMotions;
        private IPlayerMotion[] mPassiveMotions;
        private IPlayerMotion mLandedFallback;
        private IPlayerMotion mFallingFallback;
        private IPlayerMotion mSwingFallback;

        public virtual Vector3 position { get { return transform.position; } set { transform.position = value; } }
        public virtual Quaternion rotation { get { return transform.rotation; } set { transform.rotation = value; } }
        public virtual Vector3 velocity { get; set; }

        public Vector3 forward { get { return transform.forward; } }
        public Vector3 up { get { return transform.up; } }
        public Vector3 right { get { return transform.right; } }
        public virtual bool isGrounded { get { return true; } }
        public virtual bool isStoped
        {
            get
            {
                if (!isGrounded)
                    return false;
                var v = velocity;
                v.y = 0;
                return v.sqrMagnitude <= 0.00001f;
            }
        }
        public virtual bool isAlive { get { return true; } }

        public IPlayerMotion CurrentBaseMotion { get; private set; }

        public IPlayerMotion CurrentAdditiveMotion { get; private set; }

        public virtual int Identify { get { return GetInstanceID(); } }

        public virtual string Name { get { return name; } }

        protected virtual MotionAsset[] GetMotions()
        {
            return m_Motions;
        }

        private Animator mAnim;
        public Animator AttachedAnimator { get { return mAnim; } }
        private Rigidbody mRig;
        public Rigidbody AttachedRigidbody { get { return mRig; } }
        CharacterController mCtrl;
        public CharacterController AttachedController { get { return mCtrl; } }

        public IPlayerMotion FindMotion(int motionId)
        {
            var motion = GlobalUtil.Find(mBaseMotions, (x) => x.MotionId == motionId);
            if (motion != null)
                return motion;
            motion = GlobalUtil.Find(mAdditiveMotions, (x) => x.MotionId == motionId);
            return motion;
        }

        public IPlayerMotion FindMotion(string motionName)
        {
            var motion = GlobalUtil.Find(mBaseMotions, (x) => x.MotionName == motionName);
            if (motion != null)
                return motion;
            motion = GlobalUtil.Find(mAdditiveMotions, (x) => x.MotionName == motionName);
            return motion;
        }

        protected virtual void Awake()
        {
            mAnim = GetComponent<Animator>();
            mCtrl = GetComponent<CharacterController>();
            mRig = GetComponent<Rigidbody>();
            InitMotions(GetMotions());
        }

        protected virtual void InitMotions(MotionAsset[] assets)
        {
            LinkedList<IPlayerMotion> lst = new LinkedList<IPlayerMotion>();
            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var mov = asset == null ? null : asset.CreateMovement(this);
                if (mov != null)
                    lst.AddLast(mov);
            }
            GlobalUtil.Sort(lst, (x, y) => x.Priority - y.Priority);
            List<IPlayerMotion> basecat = new List<IPlayerMotion>();
            List<IPlayerMotion> addcat = new List<IPlayerMotion>();
            List<IPlayerMotion> passcat = new List<IPlayerMotion>();
            var node = lst.Last;
            while (node != null)
            {
                var mov = node.Value;
                node = node.Previous;
#if UNITY_EDITOR
                //RTLog.LogFormat(LogCat.Game, "Instantiate Motion \"{0}\" @ player \"{1}\"", mov.MotionName, name);
#endif
                if (mov.MotionLayer == EMotionLayer.additive_motion)
                    addcat.Add(mov);
                else if (mov.MotionLayer == EMotionLayer.base_motion)
                    basecat.Add(mov);
                else if (mov.MotionLayer == EMotionLayer.passive_motion)
                    passcat.Add(mov);
                if ((mov.FallbackFlags & EMotionFallback.landed_fallback) != 0)
                    mLandedFallback = mov;
                if ((mov.FallbackFlags & EMotionFallback.falling_fallback) != 0)
                    mFallingFallback = mov;
                if ((mov.FallbackFlags & EMotionFallback.swing_fallback) != 0)
                    mSwingFallback = mov;
            }
            mBaseMotions = basecat.ToArray();
            mAdditiveMotions = addcat.ToArray();
            mPassiveMotions = passcat.ToArray();
            mIsReady = true;
        }

        protected virtual void OnEnable()
        {
            for (int i = 0; i < mBaseMotions.Length; i++)
            {
                mBaseMotions[i].OnEnable();
            }
            for (int i = 0; i < mAdditiveMotions.Length; i++)
            {
                mAdditiveMotions[i].OnEnable();
            }
        }

        protected virtual void OnDisable()
        {
            for (int i = 0; i < mBaseMotions.Length; i++)
            {
                mBaseMotions[i].OnDisable();
            }
            for (int i = 0; i < mAdditiveMotions.Length; i++)
            {
                mAdditiveMotions[i].OnDisable();
            }
        }

        private bool ProcessPassiveMotion(int flag, object data)
        {
            bool interact = false;
            for (int i = 0; i < mPassiveMotions.Length; i++)
            {
                var motion = mPassiveMotions[i];
                if (motion.CanUseInput(flag, data))
                {
                    motion.AddInput(flag, data);
                    interact = true;
                }
            }
            return interact;
        }

        private bool ProcessCurrentMotion(int flag, object data)
        {
            if (CurrentAdditiveMotion != null)
            {
                if (CurrentAdditiveMotion.CanUseInput(flag, data))
                {
                    CurrentAdditiveMotion.AddInput(flag, data);
                    return true;
                }
                else if (CurrentAdditiveMotion.BlendType == EMotionBlend.override_movement)
                {
                    return true;
                }
            }
            if (CurrentBaseMotion != null && CurrentBaseMotion.CanUseInput(flag, data))
            {
                CurrentBaseMotion.AddInput(flag, data);
                return true;
            }
            return false;
        }

        private bool ProcessAdditiveMotion(int flag, object data)
        {
            bool overrideAdd = CurrentAdditiveMotion == null || CurrentAdditiveMotion.IsInterruptable;
            if (!overrideAdd)
                return false;
            bool overrideBase = CurrentBaseMotion == null || CurrentBaseMotion.IsInterruptable;
            int priority = CurrentAdditiveMotion == null ? int.MinValue : CurrentAdditiveMotion.Priority;
            IPlayerMotion motion;
            for (int i = 0; i < mAdditiveMotions.Length; i++)
            {
                motion = mAdditiveMotions[i];
                if (motion.Priority < priority)
                    return false;
                if (!motion.CanUseInput(flag, data))
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
                    motion.AddInput(flag, data);
                    return true;
                }
            }
            return false;
        }

        private bool ProcessBaseMotion(int flag, object data)
        {
            if (CurrentBaseMotion != null && !CurrentBaseMotion.IsInterruptable)
                return false;
            int priority = CurrentBaseMotion == null ? int.MinValue : CurrentBaseMotion.Priority;
            IPlayerMotion motion;
            for (int i = 0; i < mBaseMotions.Length; i++)
            {
                motion = mBaseMotions[i];
                if (motion.Priority < priority)
                    return false;
                if (!motion.CanUseInput(flag, data))
                    continue;
                if (CurrentBaseMotion != null)
                    CurrentBaseMotion.Interrupt();
                CurrentBaseMotion = motion;
                motion.AddInput(flag, data);
                return true;
            }
            return false;
        }

        public void AddInput(int flag, object data)
        {
            if (!mIsReady || !isAlive)
                return;
            // 被动劫持动作响应
            if (ProcessPassiveMotion(flag, data))
                return;
            // 优先响应当前动作
            if (ProcessCurrentMotion(flag, data))
                return;
            // 响应叠加动作
            if (ProcessAdditiveMotion(flag, data))
                return;
            // 响应基础层动作
            ProcessBaseMotion(flag, data);
        }

        protected virtual void Update()
        {
            bool noneexecution = isAlive;
            for (int i = 0; i < mPassiveMotions.Length; i++)
            {
                var p = mPassiveMotions[i];
                if (p.IsActive)
                {
                    p.OnTick(Time.deltaTime);
                    if (p.BlendType == EMotionBlend.override_movement)
                    {
                        noneexecution = false;
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
                    noneexecution = false;
                    CurrentAdditiveMotion.OnTick(Time.deltaTime);
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
                    noneexecution = false;
                    CurrentBaseMotion.OnTick(Time.deltaTime);
                }
                else
                {
                    CurrentBaseMotion = null;
                }
            }
            if (noneexecution)
            {
                IPlayerMotion motion;
                if (isGrounded)
                    motion = mLandedFallback;
                else
                    motion = mFallingFallback;
                if (motion != null)
                    motion.OnTick(Time.deltaTime);
            }
        }
    }
}