using Devil.Utility;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title ="复制黑板数据 (C)", Detail = "在黑板中复制变量", color = "#004040", HotKey = KeyCode.C)]
	public class BTCopyValueTask : BTTaskAsset 
	{
        [BTVariableReference]
        public string m_CopyFrom;
        [BTVariableReference]
        public string m_CopyTo;

        BTBlackboard blackboard;
        int from;
        int to;

        public override string DisplayContent
        {
            get
            {
                if (!string.IsNullOrEmpty(m_CopyFrom) && !string.IsNullOrEmpty(m_CopyTo))
                    return StringUtil.Concat(m_CopyFrom, " -> \n", m_CopyTo);
                return base.DisplayContent;
            }
        }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            blackboard = binder.Blackboard;
            from = blackboard.GetIndex(m_CopyFrom);
            to = blackboard.GetIndex(m_CopyTo);
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }

        public override EBTState OnStart()
        {
            if (from == -1 || to == -1)
                return EBTState.failed;
            blackboard.CopyValue(from, to);
            return EBTState.success;
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