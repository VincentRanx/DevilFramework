using Devil.Utility;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "开关？(G)", HotKey = KeyCode.G)]
    public class BTGateway : BTConditionAsset
    {
        [BTVariableReference(typeof(bool), EVarType.Variable)]
        public string m_UseBlackboard;
        IBlackboardValue<bool> mBlackboardValue;

        public bool m_IsTrue;
        
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
                    return m_IsTrue == mBlackboardValue.Value;
                else
                    return m_IsTrue;
            }
        }
    }
}