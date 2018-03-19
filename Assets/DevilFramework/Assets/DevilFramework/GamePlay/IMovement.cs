using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public enum MotionLayer
    {
        base_movement = 0, // 基本动作，如走、跳、游泳等
        additive_movement = 1, // 额外动作，如攻击、技能等
        decractor_movement = 2, // 装饰动作，如BUF
    }

    public enum MotionBlend
    {
        override_movement, // 覆盖
        additive_movement, // 叠加
    }

    public interface IMovement : ITick
    {
        /// <summary>
        /// 动作 ID
        /// </summary>
        int MotionId { get; }

        /// <summary>
        /// 优先级
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 动作层
        /// </summary>
        MotionLayer MotionLayer { get; }

        /// <summary>
        /// 混合方式
        /// </summary>
        MotionBlend BlendType { get; }

        /// <summary>
        /// 玩家
        /// </summary>
        IGamePlayer Player { get; }

        /// <summary>
        /// 监听的按键集合
        /// </summary>
        InputMask InteractInputs { get; }
        
        /// <summary>
        /// 动作是否进行中
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 是否可打断
        /// </summary>
        bool Interruptable { get; }

        /// <summary>
        /// 打断动作
        /// </summary>
        void Interrupt();

        /// <summary>
        /// 添加输入
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="isPress"></param>
        /// <param name="isRelease"></param>
        /// <returns> 分发输入到下一层动作 </returns>
        bool AddInput(InputMask mask, bool isPress, bool isRelease);

        /// <summary>
        /// 添加移动分量
        /// </summary>
        /// <param name="moveDir"></param>
        /// <returns> 分发输入到下一层动作 </returns>
        bool AddMove(Vector3 moveDir);
    }
}