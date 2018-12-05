using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    [CreateAssetMenu(fileName = "Motion Group", menuName = "Game/Motion Group")]
    public class MotionGroupAsset : MotionAsset
    {
        public enum EDrivenType
        {
            Selector,
            Sequence,
        }

        [SerializeField]
        EDrivenType m_DrivenType = EDrivenType.Sequence;
        [SerializeField]
        private MotionAsset[] m_MotionGroup = new MotionAsset[0];
        public MotionAsset[] MotionGroup { get { return m_MotionGroup; } }

        public override IPlayerMotion CreateMovement(IGamePlayer player)
        {
            if (m_DrivenType == EDrivenType.Selector)
            {
                var motion = MotionSelectorImpl.Create<MotionSelectorImpl>(this, player);
                motion.Prepare();
                return motion;
            }
            else
            {
                var motion = MotionSequenceImpl.Create<MotionSequenceImpl>(this, player);
                motion.Prepare();
                return motion;
            }
        }
    }

    public class MotionSelectorImpl : MotionImpl<MotionGroupAsset>
    {
        private List<IPlayerMotion> mMotions;
        private bool mReady;
        IPlayerMotion mActiveMotion;
        
        public void Prepare()
        {
            mReady = false;
            if (Player == null)
                return;
            mMotions = new List<IPlayerMotion>(Asset.MotionGroup.Length);
            for (int i = 0; i < Asset.MotionGroup.Length; i++)
            {
                var mot = Asset.MotionGroup[i];
                if (mot == null)
                    continue;
                var motion = mot.CreateMovement(Player);
                if (motion != null)
                    mMotions.Add(motion);
            }
            mActiveMotion = mMotions.Count > 0 ? mMotions[0] : null;
            mReady = true;
        }

        public override void OnEnable()
        {
            if (mActiveMotion != null)
                mActiveMotion.OnEnable();
        }

        public override void OnDisable()
        {
            if (mActiveMotion != null)
                mActiveMotion.OnDisable();
        }

        public override bool IsActive { get { return mActiveMotion == null ? false : mActiveMotion.IsActive; } }
        public override bool IsInterruptable { get { return mActiveMotion == null ? true : mActiveMotion.IsInterruptable; } }

        public override void AddInput(int flag, object data)
        {
            if (mActiveMotion != null)
                mActiveMotion.AddInput(flag, data);
        }

        public override bool CanUseInput(int flag, object data)
        {
            return mActiveMotion != null && mActiveMotion.CanUseInput(flag, data);
        }

        public override void Interrupt()
        {
            if (mActiveMotion != null)
                mActiveMotion.Interrupt();
        }

        public override void OnTick(float deltaTime)
        {
            if (mActiveMotion != null)
                mActiveMotion.OnTick(deltaTime);
        }

        public bool SwitchMotionTo(int motionId)
        {
            if (!mReady)
                return false;
            return SwitchMotion(GlobalUtil.FindIndex(mMotions, (x) => x.MotionId == motionId));
        }

        public bool SwitchMotionTo(string motionName)
        {
            if (!mReady)
                return false;
            return SwitchMotion(GlobalUtil.FindIndex(mMotions, (x) => x.MotionName == motionName));
        }

        public bool SwitchMotion(int motionIndex)
        {
            var motion = motionIndex >= 0 && motionIndex < mMotions.Count ? mMotions[motionIndex] : null;
            if (motion == null)
                return false;
            if (motion == mActiveMotion)
                return true;
            if (mActiveMotion.IsActive && !mActiveMotion.IsInterruptable)
                return false;
            if(mActiveMotion.IsActive)
                mActiveMotion.Interrupt();
            mActiveMotion.OnDisable();
            mActiveMotion = mMotions[motionIndex];
            mActiveMotion.OnEnable();
#if UNITY_EDITOR
            RTLog.LogFormat(LogCat.Game, "{0} switch Motion[{1}] to \"{2}\"", Player.Name, MotionName, mActiveMotion.MotionName);
#endif
            return true;
        }
    }

    public class MotionSequenceImpl : MotionImpl<MotionGroupAsset>
    {
        private List<IPlayerMotion> mMotions;
        private bool mReady;
        private int mToActMotion;
        private IPlayerMotion mActiveMotion;

        public void Prepare()
        {
            mReady = false;
            if (Player == null)
                return;
            mMotions = new List<IPlayerMotion>(Asset.MotionGroup.Length);
            for (int i = 0; i < Asset.MotionGroup.Length; i++)
            {
                var mot = Asset.MotionGroup[i];
                if (mot == null)
                    continue;
                var motion = mot.CreateMovement(Player);
                if (motion != null)
                    mMotions.Add(motion);
            }
            mToActMotion = -1;
            mReady = true;
        }

        public override bool IsActive { get { return mActiveMotion != null && mActiveMotion.IsActive; } }

        public override bool IsInterruptable { get { return mActiveMotion == null ? true : mActiveMotion.IsInterruptable; } }

        public override void OnEnable()
        {
            for (int i = 0; i < mMotions.Count; i++)
            {
                mMotions[i].OnEnable();
            }
        }

        public override void OnDisable()
        {
            for (int i = 0; i < mMotions.Count; i++)
            {
                mMotions[i].OnDisable();
            }
            mActiveMotion = null;
            mToActMotion = -1;
        }

        public override void AddInput(int flag, object data)
        {
            if (mToActMotion != -1)
            {
                var act = mMotions[mToActMotion];
                if (mActiveMotion != null && mActiveMotion != act)
                    mActiveMotion.Interrupt();
                mActiveMotion = act;
                mToActMotion = -1;
            }
            if (mActiveMotion != null)
            {
                mActiveMotion.AddInput(flag, data);
            }
        }

        public override bool CanUseInput(int flag, object data)
        {
            mToActMotion = -1;
            if (mActiveMotion != null)
            {
                if (mActiveMotion.CanUseInput(flag, data))
                    return true;
                else if (!mActiveMotion.IsInterruptable)
                    return false;
            }
            for (int i = 0; i < mMotions.Count; i++)
            {
                if (mMotions[i].CanUseInput(flag, data))
                {
                    mToActMotion = i;
                    return true;
                }
            }
            return false;
        }

        public override void Interrupt()
        {
            if (mActiveMotion != null)
                mActiveMotion.Interrupt();
        }

        public override void OnTick(float deltaTime)
        {
            if(mActiveMotion != null && mActiveMotion.IsActive)
            {
                mActiveMotion.OnTick(deltaTime);
            }
        }
    }
}