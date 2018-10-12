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
#if UNITY_EDITOR
                ResetState(mRoot);
#endif
                IsComplate = false;
                RuntimeNode = mRoot;
                RuntimeNode.StartLoop(runner);
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
                    tmp.StopLoop(runner);
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
                    child.StartLoop(runner);
                    tmp = child;
                }
            }
            RuntimeNode = null;
        }

#if UNITY_EDITOR
        void ResetState(BTNodeBase root)
        {
            if (root != null)
            {
                root.ResetState();
                for(int i = 0; i < root.ChildLength; i++)
                {
                    ResetState(root.ChildAt(i));
                }
            }
        }

#endif
    }
}