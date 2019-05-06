using Devil.Utility;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "等待 (W)", IconPath = "Assets/DevilFramework/Gizmos/AI Icons/loop.png", HotKey = KeyCode.W)]
    public class BTWaitTask : BTTaskAsset
    {
        public bool m_WaitForever = false;
        //public float m_MinTime = 1;
        //public float m_MaxTime = 2;
        [RangeField]
        public Vector2 m_TimeRange = new Vector2(1, 2);

        float mTime;

        public override string DisplayContent
        {
            get
            {
                if (m_TimeRange.x <= m_TimeRange.y - 0.1f)
                    return StringUtil.Concat("等待 ", m_TimeRange.x, "~", m_TimeRange.y, " 秒钟");
                else
                    return StringUtil.Concat("等待 ", m_TimeRange.x, " 秒钟");
            }
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }

        public override EBTState OnStart()
        {
            if (m_TimeRange.y >= m_TimeRange.x + 0.1f)
                mTime = Random.Range(m_TimeRange.x, m_TimeRange.y);
            else
                mTime = m_TimeRange.x;
            return EBTState.running;
        }

        public override void OnStop()
        {
        }

        public override EBTState OnUpdate(float deltaTime)
        {
            if (m_WaitForever)
                return EBTState.running;
            mTime -= deltaTime;
            return mTime <= 0 ? EBTState.success : EBTState.running;
        }
    }
}