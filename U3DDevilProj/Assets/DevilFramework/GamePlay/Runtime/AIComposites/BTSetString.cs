using Devil.AI;
using UnityEngine;

namespace Devil.GamePlay
{
    [BTComposite(Title = "设置 string 值 (S)", HotKey = KeyCode.S, IconPath = "Assets/DevilFramework/Gizmos/AI Icons/blackboard.png", color = "#004040")]
    public class BTSetString : BTTaskAsset
    {
        [BTVariableReference(typeof(string), EVarType.Variable)]
        public string m_ToSet;
        IBlackboardValue<string> mToSet;

        public string m_Value;

        public override string DisplayContent { get { return string.Format("\"{0}\" -> {1}", m_Value, m_ToSet); } }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            mToSet = binder.Blackboard.Value<string>(m_ToSet);
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }

        public override EBTState OnStart()
        {
            if (mToSet != null)
                mToSet.Set(m_Value);
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