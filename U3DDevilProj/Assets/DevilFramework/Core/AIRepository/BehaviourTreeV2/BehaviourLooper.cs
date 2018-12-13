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
        IBTNode mRuntime;
        
        public BehaviourLooper(BehaviourTreeRunner runner)
        {
            Runner = runner;
            IsComplate = true;
        }

        public BehaviourLooper CreateSubLooper()
        {
            var looper = new BehaviourLooper(Runner);
            looper.Parent = this;
            return looper;
        }

        public void SetBehaviuor(IBTNode root)
        {
            mRoot = root;
            IsComplate = Runner != null && mRoot == null;
            mRuntime = null;
        }

        public void Reset()
        {
            var t = mRuntime;
            while(t != null)
            {
                t.Stop();
                if (t == mRoot)
                    break;
                t = t.Parent;
            }
            IsComplate = mRoot == null;
            mRuntime = null;
            State = EBTState.inactive;
#if UNITY_EDITOR
            EditorClearAccess();
#endif
        }
        
        public void Abort()
        {
            if(mRuntime == null)
            {
                State = EBTState.inactive;
                return;
            }
            var node = mRuntime;
            while(node != null)
            {
                node.Abort();
                if (node == mRoot)
                    break;
                node = node.Parent;
            }
            State = mRoot == null ? EBTState.failed : mRoot.State;
            mRuntime = null;
            IsComplate = true;
#if UNITY_EDITOR
            EditorClearAccess();
#endif
        }

        public void Update(float deltaTime)
        {
            if (IsComplate)
                return;
            if (mRuntime == null)
            {
                mRuntime = mRoot;
                mRuntime.Start();
#if UNITY_EDITOR
                EditorAccess(mRuntime);
#endif
            }
            mRuntime = Visit(mRuntime);
            if (mRuntime == null)
                IsComplate = true;
            else
                mRuntime.OnTick(deltaTime);
            State = mRoot.State;
        }

        // 返回 running 状态的节点
        IBTNode Visit(IBTNode root)
        {
            var tmp = root;
            while(tmp != null)
            {
                if (tmp.State == EBTState.running && !tmp.IsOnCondition)
                    tmp.Abort();
                if (tmp.IsController)
                {
                    var child = tmp.State == EBTState.running ? tmp.GetNextChildTask() : null;
                    if(child != null)
                    {
                        child.Start();
                        tmp = child;
#if UNITY_EDITOR
                        EditorAccess(child);
#endif
                    }
                    else
                    {
                        tmp.Stop();
                        var parent = tmp == mRoot ? null : tmp.Parent;
                        if (parent != null)
                            parent.ReturnState(tmp.State);
                        tmp = parent;
                    }
                }
                else
                {
                    if (tmp.State == EBTState.running)
                        return tmp;
                    tmp.Stop();
                    var parent = tmp == mRoot ? null : tmp.Parent;
                    if (parent != null)
                        parent.ReturnState(tmp.State);
                    tmp = parent;
                }
            }
            return null;
        }
        
        public void Dispose()
        {
            Parent = null;
            IsComplate = true;
            mRuntime = null;
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