using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "清除黑板数据 (C)", HotKey = KeyCode.C)]
    public class BTClearBlackboard : BTTaskAsset
    {
        [BTVariableReference]
        public string m_VariableName;
        BTBlackboard mBlackboard;
        int mIndex;

        public override string DisplayContent
        {
            get
            {
                if (!string.IsNullOrEmpty(m_VariableName))
                    return string.Format("Delete \"{0}\"", m_VariableName);
                else
                    return null;
            }
        }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            mBlackboard = binder.Blackboard;
            mIndex = mBlackboard.GetIndex(m_VariableName);
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }

        public override EBTState OnStart()
        {
            return EBTState.running;
        }

        public override EBTState OnUpdate(float deltaTime)
        {
            if (mIndex != -1)
                mBlackboard.ClearAt(mIndex);
            return EBTState.success;
        }
    }
}