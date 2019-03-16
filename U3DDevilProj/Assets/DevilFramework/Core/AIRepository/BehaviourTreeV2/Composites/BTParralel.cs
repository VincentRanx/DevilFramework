using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "并行任务 (P)", Detail = "同时执行子任务，并且选择\n第一个任务作为任务结果", color = "#8e4c06",
        HotKey = KeyCode.P, IconPath = "Assets/DevilFramework/Gizmos/AI Icons/parralel.png")]
    public class BTParralel : BTTaskAsset
    {
        public enum EResult
        {
            Success,
            MainTaskResult,
            AnySubTask,
            AllSubTask,
        }

        public int m_MainTask = 0;
        public EResult m_ResultFrom;
        
        public bool m_WaitAllTaskDone = false;
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

        EBTState GetResult()
        {
            if (m_ResultFrom == EResult.Success)
            {
                return EBTState.success;
            }
            else if (m_ResultFrom == EResult.MainTaskResult)
            {
                return mLoopers[m_MainTask].State;
            }
            else if (m_ResultFrom == EResult.AnySubTask)
            {
                for (int i = 0; i < mLoopers.Length; i++)
                {
                    if (mLoopers[i].State == EBTState.success)
                        return EBTState.success;
                }
                return EBTState.failed;
            }
            else if (m_ResultFrom == EResult.AllSubTask)
            {
                for (int i = 0; i < mLoopers.Length; i++)
                {
                    if (mLoopers[i].State == EBTState.failed)
                        return EBTState.failed;
                }
                return EBTState.success;
            }
            else
            {
                return EBTState.failed;
            }
        }

        public override EBTState OnAbort()
        {
            if (mLoopers == null || mLoopers.Length == 0)
                return EBTState.failed;
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (mLoopers[i].State == EBTState.running)
                    mLoopers[i].Abort();
            }
#if UNITY_EDITOR
            if (m_MainTask < 0 || m_MainTask >= mLoopers.Length)
                m_MainTask = 0;
#endif
            return GetResult();
        }

        public override EBTState OnStart()
        {
            if (mLoopers.Length == 0)
                return m_ResultFrom == EResult.Success || m_ResultFrom == EResult.AllSubTask ? EBTState.success : EBTState.failed;
            if (m_MainTask < 0 || m_MainTask >= mLoopers.Length)
                m_MainTask = 0;
            for (int i = 0; i < mLoopers.Length; i++)
            {
                mLoopers[i].Reset();
            }
            return EBTState.running;
        }

        public override void OnStop()
        {
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (!mLoopers[i].IsComplate)
                    mLoopers[i].Abort();
            }
        }

        public override EBTState OnUpdate(float deltaTime)
        {
            int exe = 0;
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (!mLoopers[i].IsComplate)
                {
                    exe++;
                    mLoopers[i].Update(deltaTime);
                }
            }
#if UNITY_EDITOR
            if (m_MainTask < 0 || m_MainTask >= mLoopers.Length)
                m_MainTask = 0;
#endif
            bool waitall = m_WaitAllTaskDone || m_ResultFrom == EResult.AnySubTask || m_ResultFrom == EResult.AllSubTask;
            return exe == 0 || !waitall ? GetResult() : EBTState.running;
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