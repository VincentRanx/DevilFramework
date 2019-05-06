using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "TEST (T)", color = "#2681ba", HotKey = KeyCode.T,
        IconPath = "Assets/DevilFramework/Gizmos/AI Icons/Condition Icon.png")]
    public class BTCheckCondition : BTTaskAsset
    {
        public ELogic m_Logic;
        public bool m_ReverseResult;
        
        public override string DisplayContent
        {
            get
            {
                return m_ReverseResult ? "已反转" : null;
            }
        }

        public override EBTState OnAbort()
        {
            return m_ReverseResult ? EBTState.success : EBTState.failed;
        }
        
        public override EBTState OnStart()
        {
            return IsOnCondition ? EBTState.success : EBTState.failed;
        }

        public override void OnStop()
        {
        }

        public override EBTState OnUpdate(float deltaTime)
        {
            return EBTState.success;
        }
    }
}