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
        [TextArea(3, 7)]
        public string m_Comment = "";
        [HideInInspector]
        [SerializeField]
        List<int> m_ConditionIds = new List<int>();
        public int ConditionCount { get { return m_ConditionIds.Count; } }
        public int GetConditionId(int index)
        {
            return m_ConditionIds[index];
        }
       
        public int Identify { get; private set; }
        List<ICondition> mConditions = new List<ICondition>();
        public virtual string DisplayContent { get { return null; } }

        public IBTNode Parent { get; private set; }

        public bool IsOnCondition
        {
            get
            {
#if UNITY_EDITOR
                for (int i = 0; i < mConditions.Count; i++)
                {
                    editor_conditionCache[i] = mConditions[i].IsSuccess;
                }
#endif
                for (int i = 0; i < mConditions.Count; i++)
                {
                    if (!mConditions[i].IsSuccess)
                        return false;
                }
                return true;
            }
        }
        
        public override void OnPrepare(BehaviourTreeRunner.AssetBinder asset, BTNode node)
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
#if UNITY_EDITOR
            editor_conditionCache = new bool[m_ConditionIds.Count];
#endif
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

        public const string P_CONDITION = "m_ConditionIds";
        public const string P_SERVICE = "m_ServiceIds";
        [System.NonSerialized]
        public bool[] editor_conditionCache;

        public override void EditorNodeRemoved(ICollection<int> ids)
        {
            foreach(var id in ids)
            {
                m_ConditionIds.Remove(id);
            }
        }
        public override void EditorGetDependentIds(ICollection<int> ids)
        {
            if(m_ConditionIds != null)
            {
                foreach(var id in m_ConditionIds)
                {
                    ids.Add(id);
                }
            }
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
#endif
    }
}