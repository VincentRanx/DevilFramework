using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "黑板已设置？(B)", Detail = "检查黑板参数是否设置?",
        IconPath = "Assets/DevilFramework/Gizmos/AI Icons/blackboard.png",
        HotKey = KeyCode.B)]
    public class BTIsBlackboardSet : BTConditionAsset
    {
        public bool m_ReverseResult;

        [BTVariableReference]
        public string m_BlackboardProperty;

        BTBlackboard mBlackboard;
        int mPropertyIndex;
        
        public override string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(m_BlackboardProperty))
                    return null;
                else if(m_ReverseResult)
                    return string.Format("非\"{0}\"?", m_BlackboardProperty);
                else
                    return string.Format("\"{0}\"?", m_BlackboardProperty);
            }
        }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            mBlackboard = binder.Blackboard;
            mPropertyIndex = string.IsNullOrEmpty(m_BlackboardProperty) ? -1 : mBlackboard.GetIndex(m_BlackboardProperty);
        }

        public override bool IsSuccess { get { return mBlackboard.IsSet(mPropertyIndex) ^ m_ReverseResult; } }

    }
}