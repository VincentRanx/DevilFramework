using UnityEngine;

namespace Devil.AI
{
    public class BehaviourLooper
    {
        BTNodeBase mRoot;
        public float NodeRuntime { get; private set; }
        public bool IsComplate { get; private set; }
        public BTNodeBase RuntimeNode { get; private set; }
        public EBTTaskState State { get; private set; }

        public BehaviourLooper(BTNodeBase rootNode)
        {
            mRoot = rootNode;
            IsComplate = mRoot == null;
        }

        public void Reset()
        {
            IsComplate = mRoot == null;
            RuntimeNode = null;
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
            {
                IsComplate = true;
                return;
            }
            if (RuntimeNode == null)
            {
                IsComplate = false;
#if UNITY_EDITOR
                runner.NotifyBehaviourTreeBegin();
#endif
                RuntimeNode = mRoot;
                NodeRuntime = 0;
                RuntimeNode.Visit(runner);
            }
            else
            {
                RuntimeNode.Tick(runner, Time.deltaTime);
                NodeRuntime += Time.deltaTime;
            }
            RuntimeNode = VisitChildren(runner, RuntimeNode);
            State = mRoot.State;
            if(RuntimeNode == null)
            {
                IsComplate = true;
            }
#if UNITY_EDITOR
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
                    tmp.Cleanup(runner);
                    parent = tmp == mRoot ? null : tmp.ParentNode;
                    if (parent != null)
                        parent.ReturnWithState(tmp.State);
                    tmp = parent;
                }
                else
                {
                    NodeRuntime = 0;
                    child.Visit(runner);
                    tmp = child;
                }
            }
            return null;
        }
    }
}