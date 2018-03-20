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
        int mNodeId;
        public int NodeId { get { return mNodeId; } }
        int mChildLen;
        public int ChildLength { get { return mChildLen; } }
        BTNodeBase[] mChildren;

        int mConditionLen;
        public int DecoratorLength { get { return mConditionLen; } }
        IBTCondition[] mDecorators;

        int mServiceLen;
        public int ServiceLength { get { return mServiceLen; } }
        IBTService[] mServices;

        public EBTTaskState State { get; protected set; }

        //public BTNodeBase(int id, int conditionLen, int childLength, int serviceLen)
        //{
        //    mNodeId = id;
        //    InitDecoratorSize(conditionLen, childLength, serviceLen);
        //}

        public BTNodeBase(int id)
        {
            mNodeId = id;
        }

        public void InitDecoratorSize(int conditionLen, int childLen, int serviceLen)
        {
            mConditionLen = conditionLen;
            mChildLen = childLen;
            mServiceLen = serviceLen;
            if (childLen > 0)
            {
                mChildren = new BTNodeBase[childLen];
            }
            if (serviceLen > 0)
            {
                mServices = new IBTService[serviceLen];
            }
            if (conditionLen > 0)
            {
                mDecorators = new IBTCondition[conditionLen];
            }
        }

        public BTNodeBase ParentNode { get; private set; }

        public BTNodeBase ChildAt(int index)
        {
            return mChildren[index];
        }

        public void SetChild(int index, BTNodeBase node)
        {
            if (node != null)
            {
                node.ParentNode = this;
                mChildren[index] = node;
            }
        }

        public IBTCondition GetCondition(int index)
        {
            return mDecorators[index];
        }

        public void SetCondition(int index, IBTCondition decorator)
        {
            mDecorators[index] = decorator;
        }

        public IBTService GetService(int index)
        {
            return mServices[index];
        }

        public void SetService(int index, IBTService service)
        {
            mServices[index] = service;
        }

        public bool IsOnCondition(BehaviourTreeRunner runner)
        {
            for (int i = 0; i < DecoratorLength; i++)
            {
                IBTCondition decor = GetCondition(i);
                if (decor != null && !decor.IsSuccess(runner))
                {
                    return false;
                }
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
                State = EBTTaskState.faild;
            }
        }

        public void Visit(BehaviourTreeRunner runner)
        {
            if (IsOnCondition(runner))
                OnVisit(runner);
            else
                State = EBTTaskState.faild;
        }

        protected abstract void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime);

        protected abstract void OnVisit(BehaviourTreeRunner behaviourTree);

        public abstract BTNodeBase ChildForVisit { get; }

        public abstract void ReturnWithState(EBTTaskState state);

#if UNITY_EDITOR
        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, mNodeId);
        }
#endif
    }
}