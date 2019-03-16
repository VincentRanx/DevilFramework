using UnityEngine;

namespace Devil.AI
{
    public enum EGoalState
    {
        Inactive, // 未开始
        Active, // 进行中
        Wait, // 进行等待中
        Finished, // 结束
    }

    [System.Serializable]
	public abstract class AIGoal : ScriptableObject
	{
        [SerializeField]
        EGoalState m_GoalState;
        public EGoalState State
        {
            get { return m_GoalState; }
            set
            {
                if (m_GoalState != value)
                {
                    m_GoalState = value;
                    if (value == EGoalState.Active)
                        OnStart();
                    else if (value == EGoalState.Finished)
                        OnFinished();
                    else if (value == EGoalState.Inactive)
                        OnReset();
                }
            }
        }

        [SerializeField]
        int m_ResultCode;
        public int Result
        {
            get { return m_ResultCode; }
            set
            {
                if (m_ResultCode != value)
                {
                    m_ResultCode = value;
                    if (m_GoalState == EGoalState.Active)
                        OnResult(m_ResultCode);
                }
            }
        }

        // 评估可行性， <= 0 不可行
        public abstract float EvaluateFeasibility();

        protected abstract void OnResult(int result);

        protected abstract void OnReset();

        protected abstract void OnStart();

        protected abstract void OnUpdate(float deltaTime);

        protected abstract void OnFinished();

    }
}