using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BehaviourLooper
    {
        BTNodeBase mRoot;
        BTNodeBase mRuntimeNode;
        float mRuntimeNodeTime;
        public float NodeRuntime { get { return mRuntimeNodeTime; } }
        public bool IsComplate { get; private set; }
        public BTNodeBase RuntimeNode { get { return mRuntimeNode; } }

        public BehaviourLooper(BTNodeBase rootNode)
        {
            mRoot = rootNode;
        }

        public void Reset()
        {
            IsComplate = false;
            mRuntimeNode = null;
        }

        public void ResetTreeState()
        {
            ResetTreeStateRecursive(mRoot);
            IsComplate = false;
        }

        void ResetTreeStateRecursive(BTNodeBase root)
        {
            if (root == null)
                return;
            root.Reset();
            for(int i = 0; i < root.ChildLength; i++)
            {
                ResetTreeStateRecursive(root.ChildAt(i));
            }
        }

        public void Update(BehaviourTreeRunner runner, float deltaTime)
        {
            if (mRoot == null)
                return;
            if (mRuntimeNode == null)
            {
                IsComplate = false;
#if DEBUG_AI
                runner.NotifyBehaviourTreeBegin();
#endif
                mRuntimeNode = mRoot;
                mRuntimeNodeTime = 0;
                mRuntimeNode.Visit(runner);
            }
            else
            {
                mRuntimeNode.Tick(runner, Time.deltaTime);
                mRuntimeNodeTime += Time.deltaTime;
            }
            mRuntimeNode = VisitChildren(runner, mRuntimeNode);
            if(mRuntimeNode == null)
            {
                IsComplate = true;
            }
#if DEBUG_AI
            runner.NotifyBehaviourTreeFrame();
            if (IsComplate)
            {
                runner.NotifyBehaviourTreeEnd();
            }
#endif
        }

        // 返回 running 状态的节点
        BTNodeBase VisitChildren(BehaviourTreeRunner runner, BTNodeBase root)
        {
            BTNodeBase tmp = root;
            BTNodeBase child;
            BTNodeBase parent;
            while (tmp != null)
            {
                child = tmp.ChildForVisit;
                if (child == null)
                {
                    if (tmp.State == EBTTaskState.running)
                        return tmp;
                    parent = tmp == mRoot ? null : tmp.ParentNode;
                    if (parent != null)
                        parent.ReturnWithState(tmp.State);
                    tmp = parent;
                }
                else
                {
                    mRuntimeNodeTime = 0;
                    child.Visit(runner);
                    tmp = child;
                }
            }
            return null;
        }
    }
}