namespace DevilTeam.AI
{
    public enum BTState
    {
        // 失败
        failed = 0,
        // 运行中
        running,
        // 成功
        success,
    }

    // 行为树节点
    public interface IBTNode
    {
        // 父节点
        IBTControlNode ParentNode { get; }

        // 当前执行的叶子节点
        IBTNode LeafNode { get; }

        // 访问该节点, 该方法作为初始化方法使用
        void OnVisit();

        // 更新状态
        BTState OnTick();
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
        void ReturnState(BTState state);

    }

    public class BehaviourTree
    {
        // 根节点
        private IBTControlNode m_Root;
        public IBTControlNode RootNode { get { return m_Root; } }

        private IBTControlNode m_CurrentNode;

        // 访问所有节点，可以作为初始化节点使用
        public void Visit()
        {
            m_CurrentNode = m_Root;
            m_CurrentNode.OnVisit();
        }

        public void OnTick()
        {
            if(m_CurrentNode == null)
            {
                return;
            }
            BTState state;
            IBTControlNode ctrl;
            do
            {
                IBTNode leaf = m_CurrentNode.LeafNode;
                ctrl = leaf.ParentNode;
                state = leaf.OnTick();
                while (state != BTState.running && ctrl != null)
                {
                    ctrl.ReturnState(state);
                    state = ctrl.OnTick();
                    if (state != BTState.running)
                    {
                        ctrl = ctrl.ParentNode;
                    }
                }
                m_CurrentNode = ctrl;
            } while (state != BTState.running && m_CurrentNode != null);
        }
    }
}
