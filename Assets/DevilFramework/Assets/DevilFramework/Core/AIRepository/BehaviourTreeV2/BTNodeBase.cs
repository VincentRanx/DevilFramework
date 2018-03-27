using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Devil.AI
{

    public enum EBTLife
    {
        died,
        alive,
    }

    public enum EBTTaskState
    {
        inactive,
        success,
        faild,
        running,
    }

    public abstract class BTNodeBase
    {
        public int NodeId { get; private set; }
        public int ChildLength { get; private set; }
        BTNodeBase[] mChildren;
        public int ConditionLength { get; private set; }
        BTConditionBase[] mConditions;
        public int ServiceLength { get; private set; }
        BTServiceBase[] mServices;
        private bool mVisited;
        private uint mNotFlags;

        public EBTTaskState State { get; protected set; }

#if UNITY_EDITOR
        public bool[] ConditionBuffer;
        public bool BreakToggle { get; set; }
#endif

        public BTNodeBase(int id)
        {
            NodeId = id;
        }

        public virtual void InitDecoratorSize(int conditionLen, int childLen, int serviceLen)
        {
            ConditionLength = conditionLen;
            ChildLength = childLen;
            ServiceLength = serviceLen;
            if (childLen > 0)
            {
                mChildren = new BTNodeBase[childLen];
            }
            if (serviceLen > 0)
            {
                mServices = new BTServiceBase[serviceLen];
            }
            if (conditionLen > 0)
            {
                mConditions = new BTConditionBase[conditionLen];
            }
#if UNITY_EDITOR
            ConditionBuffer = new bool[conditionLen];
#endif
        }

        public virtual void InitData(BehaviourTreeRunner btree, string jsonData)
        {

        }

        public BTNodeBase ParentNode { get; private set; }

        public BTNodeBase ChildAt(int index)
        {
            return mChildren[index];
        }

        public virtual void SetChild(int index, BTNodeBase node)
        {
            if (node != null)
            {
                node.ParentNode = this;
                mChildren[index] = node;
            }
        }

        public BTConditionBase GetCondition(int index)
        {
            return mConditions[index];
        }

        public virtual void SetCondition(int index, BTConditionBase condition)
        {
            mConditions[index] = condition;
        }

        public BTServiceBase GetService(int index)
        {
            return mServices[index];
        }

        public virtual void SetService(int index, BTServiceBase service)
        {
            mServices[index] = service;
        }

        public void SetNotFlag(int conditionIndex, bool flag)
        {
            if (flag)
                mNotFlags |= (1u << conditionIndex);
            else
                mNotFlags &= ~(1u << conditionIndex);
        }

        public bool GetNotFlag(int conditionIndex)
        {
            return (mNotFlags & (1u << conditionIndex)) != 0;
        }

        public bool IsRunnable(BehaviourTreeRunner runner)
        {
            for (int i = 0; i < ConditionLength; i++)
            {
                BTConditionBase decor = GetCondition(i);
                if (decor != null && !(decor.IsTaskRunnable(runner) ^ GetNotFlag(i)))
                {
#if UNITY_EDITOR
                    ConditionBuffer[i] = false;
                    for (int k = i + 1; k < ConditionLength; k++)
                    {
                        decor = GetCondition(k);
                        ConditionBuffer[k] = decor == null ? false : decor.IsTaskRunnable(runner);
                    }
#endif
                    return false;
                }
#if UNITY_EDITOR
                ConditionBuffer[i] = true;
#endif
            }

            return true;
        }

        public bool IsOnCondition(BehaviourTreeRunner runner)
        {
            for (int i = 0; i < ConditionLength; i++)
            {
                BTConditionBase decor = GetCondition(i);
                if (decor != null && !(decor.IsTaskOnCondition(runner) ^ GetNotFlag(i)))
                {
#if UNITY_EDITOR
                    ConditionBuffer[i] = false;
                    for (int k = i + 1; k < ConditionLength; k++)
                    {
                        decor = GetCondition(k);
                        ConditionBuffer[k] = decor == null ? false : decor.IsTaskRunnable(runner);
                    }
#endif
                    return false;
                }
#if UNITY_EDITOR
                ConditionBuffer[i] = true;
#endif
            }
            return true;
        }

        public virtual void Reset()
        {
            State = EBTTaskState.inactive;
        }

        public void Tick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            if (IsOnCondition(behaviourTree))
            {
                OnTick(behaviourTree, deltaTime);
            }
            else
            {
                State = AbortAndReturnSuccess(behaviourTree) ? EBTTaskState.success : EBTTaskState.faild;
            }
        }

        public void Visit(BehaviourTreeRunner btree)
        {
#if UNITY_EDITOR
            if (BreakToggle)
            {
                Debug.Break();
            }
#endif
            if (IsRunnable(btree))
            {
                for (int i = 0; i < ServiceLength; i++)
                {
                    BTServiceBase serv = mServices[i];
                    if (serv != null)
                        btree.StartService(serv);
                }
                mVisited = true;
                OnVisit(btree);
            }
            else
            {
                State = EBTTaskState.faild;
            }
        }

        public void Cleanup(BehaviourTreeRunner btree)
        {
            if (mVisited)
            {
                mVisited = false;
                for (int i = ServiceLength - 1; i >= 0; i--)
                {
                    BTServiceBase serv = mServices[i];
                    if (serv != null)
                        btree.StopService(serv);
                }
                OnCleanup(btree);
            }
        }

        public void ReturnWithState(EBTTaskState state)
        {
            OnReturnWithState(state);
        }

        public abstract BTNodeBase ChildForVisit { get; }

        public virtual bool AbortAndReturnSuccess(BehaviourTreeRunner behaviourTree)
        {
            return false;
        }

        protected virtual void OnCleanup(BehaviourTreeRunner btree) { }

        protected abstract void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime);

        protected abstract void OnVisit(BehaviourTreeRunner behaviourTree);

        protected abstract void OnReturnWithState(EBTTaskState state);

#if UNITY_EDITOR
        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, NodeId);
        }
#endif
    }
}