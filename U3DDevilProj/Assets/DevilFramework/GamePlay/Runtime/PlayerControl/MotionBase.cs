namespace Devil.GamePlay
{
    public abstract class MotionBase<TMotion> : IPlayerMotion where TMotion : MotionBase<TMotion>, new()
    {
        public MotionAsset Asset { get; private set; }

        public int MotionId { get { return Asset.MotionId; } }

        public string MotionName { get { return Asset.MotionName; } }

        public int Priority { get { return Asset.Priority; } }

        public EMotionLayer MotionLayer { get { return Asset.MotionLayer; } }

        public EMotionBlend BlendType { get { return Asset.BlendType; } }

        public EMotionFallback FallbackFlags { get; private set; }

        public IGamePlayer Player { get; private set; }

        protected MotionBase()
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

        public static TMotion Create(MotionAsset asset, IGamePlayer player)
        {
            TMotion v = new TMotion();
            v.Asset = asset;
            v.Player = player;
            v.FallbackFlags = asset.FallbackFlags;
            return v;
        }
    }
}