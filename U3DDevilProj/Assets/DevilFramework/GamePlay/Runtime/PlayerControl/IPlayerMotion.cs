namespace Devil.GamePlay
{
    // response process: input -> decorator -> additive -> base
    public enum EMotionLayer
    {
        base_motion = 0, // 基本动作，如走、跳、游泳等
        additive_motion = 1, // 额外动作，如攻击、技能等
        passive_motion = 2, // 被动行为
    }

    public enum EMotionBlend
    {
        override_movement, // 覆盖
        additive_movement, // 叠加
    }

    public enum EMotionFallback
    {
        landed_fallback = 1, // 站立默认动作
        falling_fallback = 2, // 自由落体默认动作
        swing_fallback = 4, // 游泳默认动作
    }
    
    public interface IPlayerMotion : ITick
    {
        /// <summary>
        /// 动作 ID
        /// </summary>
        int MotionId { get; }

        /// <summary>
        /// 动作名
        /// </summary>
        string MotionName { get; }

        /// <summary>
        /// 优先级（值越大，优先级越高）
        /// </summary>
        int Priority { get; }

        void OnEnable();

        void OnDisable();

        /// <summary>
        /// 默认动作标签
        /// </summary>
        EMotionFallback FallbackFlags { get; }

        /// <summary>
        /// 动作层
        /// </summary>
        EMotionLayer MotionLayer { get; }

        /// <summary>
        /// 混合方式
        /// </summary>
        EMotionBlend BlendType { get; }

        /// <summary>
        /// 玩家
        /// </summary>
        IGamePlayer Player { get; }
        
        /// <summary>
        /// 动作是否进行中
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 是否可被打断
        /// </summary>
        bool IsInterruptable { get; }

        /// <summary>
        /// 打断当前动作
        /// </summary>
        void Interrupt();
        
        /// <summary>
        /// 响应输入
        /// </summary>
        /// <returns></returns>
        bool CanUseInput(int flag, object data);

        /// <summary>
        /// 响应输入信号
        /// </summary>
        void AddInput(int flag, object data);
    }
}