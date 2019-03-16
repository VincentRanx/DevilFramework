using System.Collections.Generic;

namespace Devil.AI
{
    public class BehaviourLooper : System.IDisposable
    {
        public BehaviourLooper Parent { get; private set; }
        public bool IsComplate { get; private set; }
        public EBTState State { get; private set; }
        public BehaviourTreeRunner Runner { get; private set; }
        IBTNode mRoot;
        Stack<IBTNode> mExecuting;
        
        public BehaviourLooper(BehaviourTreeRunner runner)
        {
            Runner = runner;
            IsComplate = true;
            mExecuting = new Stack<IBTNode>(8);
        }

        public BehaviourLooper CreateSubLooper()
        {
            var looper = new BehaviourLooper(Runner);
            looper.Parent = this;
            return looper;
        }

        public void SetBehaviuor(IBTNode root)
        {
            Abort();
            mRoot = root;
            IsComplate = Runner != null && mRoot == null;
        }

        public void Reset()
        {
            while (mExecuting.Count > 0)
            {
                var node = mExecuting.Pop();
                node.Stop();
            }
            IsComplate = mRoot == null;
            State = EBTState.inactive;
#if UNITY_EDITOR
            EditorClearAccess();
#endif
        }

        public void Abort()
        {
            while (mExecuting.Count > 0)
            {
                var node = mExecuting.Pop();
                node.Abort();
                node.Stop();
            }
            State = mRoot == null ? EBTState.failed : mRoot.State;
            IsComplate = true;
#if UNITY_EDITOR
            EditorClearAccess();
#endif
        }

        public void Update(float deltaTime)
        {
            if (IsComplate)
                return;
            if (mExecuting.Count == 0)
            {
                State = EBTState.running;
#if UNITY_EDITOR
                EditorAccess(mRoot);
#endif
                if (mRoot.IsOnCondition)
                {
                    mRoot.Start();
                    mExecuting.Push(mRoot);
                }
                else
                {
                    mRoot.Abort();
                    State = mRoot.State;
                    IsComplate = true;
                    return;
                }
            }
            while (mExecuting.Count > 0)
            {
                var current = mExecuting.Peek();
                if (current.State == EBTState.running && !current.IsOnCondition)
                    current.Abort();
                if (current.State == EBTState.running)
                {
                    if (current.IsController)
                    {
                        var child = current.GetNextChildTask();
#if UNITY_EDITOR
                        if (child != null)
                            EditorAccess(child);
#endif
                        if (child == null)
                        {
                            current.ReturnState(EBTState.failed);
                        }
                        else if (child.IsOnCondition)
                        {
                            child.Start();
                            mExecuting.Push(child);
                        }
                        else
                        {
                            child.Abort();
                            current.ReturnState(child.State);
                        }
                    }
                    else
                    {
                        current.OnTick(deltaTime);
                        return;
                    }
                }
                else
                {
                    var child = mExecuting.Pop();
                    child.Stop();
                    current = mExecuting.Count > 0 ? mExecuting.Peek() : null;
                    if (current != null)
                        current.ReturnState(child.State);
                }
            }
            State = mRoot.State;
            IsComplate = true;
        }

        public void Dispose()
        {
            Abort();
            Parent = null;
            mRoot = null;
            Runner = null;
        }

#if UNITY_EDITOR
        // 执行过的节点
        HashSet<IBTNode> mEditorAccessed = new HashSet<IBTNode>();
        HashSet<IBTNode> mEditorAccessedDeep = new HashSet<IBTNode>();
        public HashSet<IBTNode> EditorAccessed { get { return mEditorAccessedDeep; } }
        void EditorAccess(IBTNode node)
        {
            mEditorAccessed.Add(node);
            var p = this;
            while(p != null)
            {
                p.mEditorAccessedDeep.Add(node);
                if (p == mRoot)
                    break;
                p = p.Parent;
            }
        }

        void EditorClearAccess()
        {
            foreach (var t in mEditorAccessed)
            {
                var p = this;
                while (p != null)
                {
                    p.mEditorAccessedDeep.Remove(t);
                    p = p.Parent;
                }
            }
            mEditorAccessed.Clear();
        }
#endif
    }
}