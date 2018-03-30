using UnityEngine;

namespace Devil.AI
{
    public class BehaviourLooper
    {
        BTNodeBase mRoot;
        public float NodeRuntime { get { return RuntimeNode == null ? 0 : RuntimeNode.NodeRuntime; } }
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

        //public void ResetTreeState()
        //{
        //    ResetTreeStateRecursive(mRoot);
        //    IsComplate = mRoot == null;
        //}

        //void ResetTreeStateRecursive(BTNodeBase root)
        //{
        //    if (root == null)
        //        return;
        //    root.Reset();
        //    for(int i = 0; i < root.ChildLength; i++)
        //    {
        //        ResetTreeStateRecursive(root.ChildAt(i));
        //    }
        //}

        public void Abort(BehaviourTreeRunner btree)
        {
            BTNodeBase node = RuntimeNode;
            while(node != null)
            {
                node.Abort(btree);
                if (node == mRoot)
                    break;
                node = node.ParentNode;
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
                RuntimeNode.Visit(runner);
            }
            else
            {
                RuntimeNode.Tick(runner, Time.deltaTime);
            }
            VisitChildren(runner, RuntimeNode);
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
        void VisitChildren(BehaviourTreeRunner runner, BTNodeBase root)
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
                    {
                        RuntimeNode = tmp;
                        return;
                    }
                    tmp.Cleanup(runner);
                    parent = tmp == mRoot ? null : tmp.ParentNode;
                    if (parent != null)
                    {
                        RuntimeNode = parent;
                        parent.ReturnWithState(runner, tmp.State);
                    }
                    tmp = parent;
                }
                else
                {
                    RuntimeNode = child;
                    child.Visit(runner);
                    tmp = child;
                }
            }
            RuntimeNode = null;
        }
    }
}