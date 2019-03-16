using UnityEngine;

namespace Devil.GamePlay
{ 
    // response process: input -> decorator -> additive -> base
    //public enum EMotionLayer
    //{
    //    base_motion = 0, // 基本动作，如走、跳、游泳等
    //    additive_motion = 1, // 额外动作，如攻击、技能等
    //    passive_motion = 2, // 被动行为
    //}

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
    
    public abstract class ActorMovement : MonoBehaviour
    {
        public int m_Priority;
        [SerializeField]
        private bool m_Interruptable = true;
        //[SerializeField]
        //private EMotionLayer m_MotionLayer = EMotionLayer.base_motion;
        [SerializeField]
        private EMotionBlend m_BlendType = EMotionBlend.additive_movement;
        //[SerializeField]
        //[MaskField]
        //private EMotionFallback m_FallbackFlags = 0;
        
        public EMotionBlend BlendType { get { return m_BlendType; } }

        //public EMotionFallback FallbackFlags { get { return m_FallbackFlags; } }

        public abstract void SetActor(ActorController actor);
        public bool IsActive { get; protected set; }
        public bool IsInterruptable { get { return m_Interruptable; } }
        public abstract void AddInput(InputData data);
        public abstract bool CanUseInput(InputData data);
        public abstract void Interrupt();
        public abstract void ActorUpdate(float deltaTime);
    }
}