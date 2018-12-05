using Devil.Utility;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "等待 (W)", HotKey = KeyCode.W)]
    public class BTWaitTask : BTTaskAsset
    {
        public float m_MinTime = 1;
        public float m_MaxTime = 2;

        float mTime;

        public override string DisplayContent
        {
            get
            {
                if (m_MinTime < 0)
                    m_MinTime = 0;
                if (m_MaxTime < m_MinTime)
                    m_MaxTime = m_MinTime;
                if (m_MinTime <= m_MaxTime - 0.1f)
                    return StringUtil.Concat("等待 ", m_MinTime, "~", m_MaxTime, " 秒钟");
                else
                    return StringUtil.Concat("等待 ", m_MinTime, " 秒钟");
            }
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }

        public override EBTState OnStart()
        {
            if (m_MaxTime >= m_MinTime + 0.1f)
                mTime = Random.Range(m_MinTime, m_MaxTime);
            else
                mTime = m_MinTime;
            return EBTState.running;
        }

        public override void OnStop()
        {
        }

        public override EBTState OnUpdate(float deltaTime)
        {
            mTime -= deltaTime;
            return mTime <= 0 ? EBTState.success : EBTState.running;
        }
    }
}