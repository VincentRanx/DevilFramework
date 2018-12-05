using UnityEngine;

namespace Devil.GamePlay
{
    public abstract class MotionAsset : ScriptableObject
    {
        [SerializeField]
        private int m_MotionId;
        [SerializeField]
        private int m_Priority;
        [SerializeField]
        private EMotionLayer m_MotionLayer = EMotionLayer.base_motion;
        [SerializeField]
        private EMotionBlend m_BlendType = EMotionBlend.override_movement;
        [SerializeField]
        [MaskField]
        private EMotionFallback m_FallbackFlags = 0;

        public int MotionId { get { return m_MotionId; } }
        public string MotionName { get { return name; } }
        public int Priority { get { return m_Priority; } }
        public EMotionLayer MotionLayer { get { return m_MotionLayer; } }
        public EMotionBlend BlendType { get { return m_BlendType; } }
        public EMotionFallback FallbackFlags { get { return m_FallbackFlags; } }

        public abstract IPlayerMotion CreateMovement(IGamePlayer player);
    }

    public abstract class MotionImpl<TAsset> : IPlayerMotion where TAsset : MotionAsset
    {
        public TAsset Asset { get; private set; }

        public int MotionId { get { return Asset.MotionId; } }

        public string MotionName { get { return Asset.MotionName; } }

        public int Priority { get { return Asset.Priority; } }

        public EMotionLayer MotionLayer { get { return Asset.MotionLayer; } }

        public EMotionBlend BlendType { get { return Asset.BlendType; } }

        public EMotionFallback FallbackFlags { get; private set; }

        public IGamePlayer Player { get; private set; }

        protected MotionImpl()
        {
        }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public abstract bool IsActive { get; }
        public abstract bool IsInterruptable { get; }
        public abstract void AddInput(int flag, object data);
        public abstract bool CanUseInput(int flag, object data);
        public abstract void Interrupt();
        public abstract void OnTick(float deltaTime);

        public static TMotion Create<TMotion>(TAsset asset, IGamePlayer player) where TMotion : MotionImpl<TAsset>, new()
        {
            TMotion v = new TMotion();
            v.Asset = asset;
            v.Player = player;
            v.FallbackFlags = asset.FallbackFlags;
            return v;
        }
    }
}