using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "并行任务 (P)", Detail = "同时执行子任务，并且选择\n第一个任务作为任务结果", color = "#8e4c06",
        HotKey = KeyCode.P, IconPath = "Assets/DevilFramework/Gizmos/AI Icons/parralel.png")]
    public class BTParralel : BTTaskAsset
    {
        public BehaviourTreeRunner.EResetMode m_LoopMode = BehaviourTreeRunner.EResetMode.ResetWhenLoopEnd;
        BehaviourLooper[] mLoopers;

        public override bool EnableChild { get { return true; } }

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder binder, BTNode node)
        {
            base.OnPrepare(binder, node);
            mLoopers = new BehaviourLooper[node.ChildrenCount];
            for (int i = 0; i < mLoopers.Length; i++)
            {
                mLoopers[i] = binder.Looper.CreateSubLooper();
                mLoopers[i].SetBehaviuor(node.ChildAt(i).Asset as IBTNode);
            }
        }

        public override EBTState OnAbort()
        {
            if (mLoopers.Length == 0)
                return EBTState.failed;
            //if (m_LoopMode == BehaviourTreeRunner.EResetMode.AlwaysReset)
            //{
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (mLoopers[i].State == EBTState.running)
                    mLoopers[i].Abort();
            }
            //}
            return mLoopers[0].State;
        }

        public override EBTState OnStart()
        {
            if (mLoopers.Length == 0)
                return EBTState.failed;
            if (m_LoopMode == BehaviourTreeRunner.EResetMode.AlwaysReset)
            {
                for (int i = 0; i < mLoopers.Length; i++)
                {
                    mLoopers[i].Reset();
                }
            }
            return EBTState.running;
        }

        public override void OnStop()
        {
            if (m_LoopMode == BehaviourTreeRunner.EResetMode.AlwaysReset)
            {
                for (int i = 1; i < mLoopers.Length; i++)
                {
                    if (mLoopers[i].State == EBTState.running)
                        mLoopers[i].Abort();
                }
            }
        }

        public override EBTState OnUpdate(float deltaTime)
        {
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (m_LoopMode != BehaviourTreeRunner.EResetMode.NeverReset && mLoopers[i].IsComplate)
                    mLoopers[i].Reset();
                mLoopers[i].Update(deltaTime);
            }
            return mLoopers[0].State;
        }

        private void OnDisable()
        {
            if(mLoopers != null)
            {
                for(int i= 0; i < mLoopers.Length; i++)
                {
                    mLoopers[i].Dispose();
                }
                mLoopers = null;
            }
        }
    }
}