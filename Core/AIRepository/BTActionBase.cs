namespace DevilTeam.AI
{
    public class BTActionBase : IBTNode
    {
        private bool m_Reset;
        private bool m_OnCondition;

        private IBTControlNode m_Parent;

        public IBTControlNode ParentNode
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        public IBTNode LeafNode { get { return null; } }

        public BTState OnTick()
        {
            if (m_Reset)
            {
                m_Reset = false;
                m_OnCondition = OnCondition();
            }
            BTState state;
            if (m_OnCondition)
            {
                OnAction();
                state = IsFinished() ? BTState.success : BTState.running;
            }
            else
            {
                state = BTState.failed;
            }
            if (state != BTState.running)
                m_Reset = true;
            return state;
        }

        public void OnVisit()
        {
            m_Reset = true;
            m_OnCondition = false;
        }

        protected virtual bool OnCondition()
        {
            return true;
        }

        protected virtual bool IsFinished()
        {
            return true;
        }

        protected virtual void OnAction()
        {

        }
    }

}
