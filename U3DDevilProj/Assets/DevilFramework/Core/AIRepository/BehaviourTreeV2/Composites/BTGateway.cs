using Devil.Utility;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "开关？(G)", HotKey = KeyCode.G)]
    public class BTGateway : BTConditionAsset
    {
        [BTBlackboardProperty]
        public string m_UseBlackboard;

        public bool m_IsTrue;

        BTBlackboardGetter<bool> mBlackboardValue;

        public override string DisplayName
        {
            get
            {
                return StringUtil.Concat(m_IsTrue ? "打开 " : "关闭 ", m_UseBlackboard);
            }
        }

        public override int Mask
        {
            get
            {
                return 0;
            }
        }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            mBlackboardValue = string.IsNullOrEmpty(m_UseBlackboard) ? binder.Runner.Blackboard.Getter<bool>(m_UseBlackboard) : null;
        }

        public override bool IsSuccess
        {
            get
            {
                if (mBlackboardValue != null)
                    return m_IsTrue == mBlackboardValue.IsSet && mBlackboardValue.Value;
                else
                    return m_IsTrue;
            }
        }
    }
}