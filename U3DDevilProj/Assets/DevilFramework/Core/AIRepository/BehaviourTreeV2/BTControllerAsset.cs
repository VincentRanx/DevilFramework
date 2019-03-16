using UnityEngine;

namespace Devil.AI
{
    public interface IBTController
    {
        EBTState OnStart();
        void OnStop();
        EBTState OnAbort();
        EBTState OnReturn(EBTState state);
    }
    
    public abstract class BTControllerAsset : BTNodeAsset , IBTController
    {
        public bool m_SuccessForAbort;
        private EBTState mState;
        public override EBTState State { get { return mState; } }
        public override bool IsController { get { return true; } }

        protected IBTNode[] mChildren;
        public override bool EnableChild { get { return true; } }
        public override void OnPrepare(BehaviourTreeRunner.AssetBinder asset, BTNode node)
        {
            base.OnPrepare(asset, node);
            mChildren = new IBTNode[node.ChildrenCount];
            for(int i= 0; i < mChildren.Length; i++)
            {
                var child = node.ChildAt(i);
                mChildren[i] = child == null ? null : child.Asset as IBTNode;
            }
        }
        
        public override void Abort()
        {
            mState = OnAbort();
            if (mState <= EBTState.running || m_SuccessForAbort)
                mState = m_SuccessForAbort ? EBTState.success : EBTState.failed;
        }

        public override void Start()
        {
#if UNITY_EDITOR
            if (EditorBreakToggle)
                Debug.Break();
#endif
            mState = OnStart();
            StartDecorator();
        }

        public override void Stop()
        {
            StopDecorator();
            OnStop();
        }
        
        public override void OnTick(float deltaTime) { }

        public override void ReturnState(EBTState state)
        {
            mState = OnReturn(state);
        }
        
        public abstract EBTState OnStart();
        public virtual void OnStop() { }
        public abstract EBTState OnAbort();
        public abstract EBTState OnReturn(EBTState state);
    }

}