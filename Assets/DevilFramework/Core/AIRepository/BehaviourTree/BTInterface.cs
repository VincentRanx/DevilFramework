using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevilTeam.AI
{
    public enum EBTState
    {
        // 失败
        failed = 0,
        // 运行中
        running,
        // 成功
        success,
    }

    public enum EBTNode
    {
        empty = 0,
        sequence = 1,
        selector = 2,
        queue = 3,
        custom_control = 4,
        behaviour = 5,
        condition = 6,
        patrol = 7,
    }

    // 行为树节点
    public interface IBTNode
    {
        EBTNode NodeType { get; }

        // 优先级
        int Priority { get; set; }

        // 父节点
        IBTControlNode ParentNode { get; set; }

        // 当前执行的叶子节点
        IBTNode LeafNode { get; }

        // 访问该节点, 该方法作为初始化方法使用
        void OnVisit();

        // 更新状态
        EBTState OnTick();

        object Output { get; }

    }

    // 行为树控制节点
    public interface IBTControlNode : IBTNode
    {
        // PS: 在控制节点中的 OnTick 方法总是在子节点的 OnTick 方法被执行后再执行， 如果要处理在子节点之前的任务，可以在 OnVisit 方法中实现

        // 子节点个数
        int ChildrenCount { get; }

        // 获取子节点
        IBTNode GetChildAt(int index);

        // 添加子节点
        void AddChild(IBTNode node);

        // 根据子节点运行状态返回当前结点状态
        void ReturnState(EBTState state);

        // 是否覆盖当前运行的行为树叶子节点状态
        bool OverrideState { get; }

    }

    public enum ESortType
    {
        none,
        asc,
        desc,
        random,
    }

    public delegate EBTState BehaviourTick(BTCustomData userData);
    public delegate bool ConditionTick(BTCustomData userData);

    [System.Serializable]
    public class BTCustomData
    {
        public int m_Priority;

        public int m_IntData;

        public float m_FloatData;

        public string m_StringData;

        public ESortType m_SortType;

        public object m_InputData;

        public object m_OutputData;
    }

}
