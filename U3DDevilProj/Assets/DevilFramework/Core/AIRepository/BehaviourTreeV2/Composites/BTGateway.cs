using Devil.Utility;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "开关？(G)", HotKey = KeyCode.G)]
    public class BTGateway : BTConditionAsset
    {
        [BTVariableReference]
        public string m_UseBlackboard;

        public bool m_IsTrue;

        IBlackboardValue<bool> mBlackboardValue;

        public override string DisplayName
        {
            get
            {
                return StringUtil.Concat(m_IsTrue ? "打开 " : "关闭 ", m_UseBlackboard);
            }
        }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            mBlackboardValue = !string.IsNullOrEmpty(m_UseBlackboard) ? binder.Runner.Blackboard.Value<bool>(m_UseBlackboard) : null;
        }

        public override bool IsSuccess
        {
            get
            {
                if (mBlackboardValue != null)
                    return m_IsTrue == mBlackboardValue.Value && mBlackboardValue.Value;
                else
                    return m_IsTrue;
            }
        }
    }
}