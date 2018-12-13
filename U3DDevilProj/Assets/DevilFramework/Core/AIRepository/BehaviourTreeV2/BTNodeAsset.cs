using UnityEngine;
using System.Collections.Generic;
using Devil.Utility;

namespace Devil.AI
{

    public enum EBTState
    {
        inactive,
        running,
        success,
        failed,
    }

    public interface IBTNode : ITick, IIdentified
    {
        IBTNode Parent { get; }
        bool IsOnCondition { get; }
        bool IsController { get; }
        EBTState State { get; }
        void Abort();
        void Start();
        IBTNode GetNextChildTask();
        void Stop();
        void ReturnState(EBTState state);
    }

    [System.Serializable]
    public abstract class BTNodeAsset : BTAsset, IBTNode
    {
        [HideInInspector]
        public string m_Comment = "";
        [HideInInspector]
        [SerializeField]
        List<int> m_ConditionIds = new List<int>();
        public int ConditionCount { get { return m_ConditionIds.Count; } }
        public int GetConditionId(int index)
        {
            return m_ConditionIds[index];
        }

        [SerializeField]
        ELogic m_ConditionLogic;

        public int Identify { get; private set; }
        protected List<ICondition> mConditions = new List<ICondition>();
        public virtual string DisplayContent { get { return null; } }

        public IBTNode Parent { get; private set; }

        public virtual bool IsOnCondition
        {
            get
            {
                if (m_ConditionLogic == ELogic.Or && mConditions.Count > 0)
                {
                    for (int i = 0; i < mConditions.Count; i++)
                    {
                        if (mConditions[i].IsSuccess)
                            return true;
                    }
                    return false;
                }
                else
                {
                    for (int i = 0; i < mConditions.Count; i++)
                    {
                        if (!mConditions[i].IsSuccess)
                            return false;
                    }
                    return true;
                }
            }
        }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            Identify = node.Identify;
            var p = node.Parent; // TreeAsset.GetNodeById(node.parentId);
            Parent = p == null ? null : p.Asset as IBTNode;
            mConditions.Clear();
            for (int i = 0; i < m_ConditionIds.Count; i++)
            {
                var cond = TreeAsset.GetNodeById(m_ConditionIds[i]);
                if (cond != null)
                    mConditions.Add(cond.Asset as ICondition);
            }
        }

        public abstract void Abort();
        public abstract void Start();
        public abstract IBTNode GetNextChildTask();
        public abstract void Stop();
        public abstract void ReturnState(EBTState state);
        public abstract void OnTick(float deltaTime);
        public abstract EBTState State { get; }
        public abstract bool IsController { get; }

#if UNITY_EDITOR
        public bool EditorConditionResult(int index)
        {
            return index >= 0 && index < mConditions.Count ? mConditions[index].IsSuccess : false;
        }
        public List<int> EditorConditionIds { get { return m_ConditionIds; } }
        public bool EditorBreakToggle { get; set; }
        public void EditorRemoveCondition(int id)
        {
            m_ConditionIds.Remove(id);
        }
        public void EditorSwitchCondition(BTNode condition, int offset)
        {
            if (condition == null || m_ConditionIds.Count < 2 || offset == 0)
                return;
            var index = GlobalUtil.FindIndex(m_ConditionIds, (x) => x == condition.Identify);
            if (index == -1)
                return;
            var to = Mathf.Clamp(index + offset, 0, m_ConditionIds.Count - 1);
            var a = m_ConditionIds[index];
            m_ConditionIds[index] = m_ConditionIds[to];
            m_ConditionIds[to] = a;
        }
        public void EditorSetCondition(int index, int id)
        {
            if (index >= 0 && index < m_ConditionIds.Count)
                m_ConditionIds[index] = id;
        }
#endif
    }
}