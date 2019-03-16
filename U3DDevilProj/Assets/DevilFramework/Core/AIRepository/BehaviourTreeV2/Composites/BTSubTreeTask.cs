using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "子行为树 (S)", color = "#4b4bff", Detail =
        "以一棵行为树作为任务", HotKey = KeyCode.S, IconPath = "Assets/DevilFramework/Gizmos/AI Icons/BehaviourTree Icon.png")]
    public class BTSubTreeTask : BTTaskAsset
    {
        public static string NameSubtree(Object node)
        {
            return node.GetInstanceID().ToString("x");
        }

        public BehaviourTreeRunner.EResetMode m_ResetMode = BehaviourTreeRunner.EResetMode.ResetWhenLoopEnd;
        public bool m_SubTreeStandalone;
        public BehaviourTreeAsset m_BehaviourTree;
        BehaviourTreeRunner.AssetBinder mBinder;

#if UNITY_EDITOR
        public override string DisplayContent
        {
            get
            {
                if (m_BehaviourTree != null)
                    return string.Format("\"{0}\"", m_BehaviourTree.name);
                else
                    return null;
            }
        }
#endif

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            if (m_BehaviourTree != null)
            {
                mBinder = BehaviourTreeRunner.AssetBinder.NewSubBinder(binder);
                if (mBinder != null)
                {
                    mBinder.Name = NameSubtree(this);
                }
            }
        }

        public override EBTState OnAbort()
        {
            if (mBinder != null && !m_SubTreeStandalone && mBinder.IsAvailable && !mBinder.Looper.IsComplate)
                mBinder.Looper.Abort();
            return EBTState.failed;
        }

        public override EBTState OnStart()
        {
            if(mBinder != null && mBinder.IsAvailable)
            {
                mBinder.BindAsset(m_BehaviourTree, m_ResetMode);
                if (m_ResetMode == BehaviourTreeRunner.EResetMode.ResetWhenBegin)
                    mBinder.Looper.Reset();
                return EBTState.running;
            }
            else
            {
                return EBTState.failed;
            }
        }

        public override void OnStop()
        {
            if (m_ResetMode == BehaviourTreeRunner.EResetMode.AlwaysReset && mBinder != null && mBinder.RuntimeTree != null)
            {
                mBinder.StopService();
                mBinder.Looper.Reset();
            }
        }

        public override EBTState OnUpdate(float deltaTime)
        {
            if(mBinder != null && mBinder.RuntimeTree != null)
            {
                mBinder.UpdateLooper(deltaTime);
                mBinder.UpdateService(deltaTime);
                return mBinder.Looper.State;
            }
            else
            {
                return EBTState.failed;
            }
        }

        private void OnDisable()
        {
            if (mBinder != null)
            {
                mBinder.Dispose();
                mBinder = null;
            }
        }
    }
}