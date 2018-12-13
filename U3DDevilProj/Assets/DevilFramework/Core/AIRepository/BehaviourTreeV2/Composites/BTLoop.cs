using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "循环 (LOOP)", IconPath = "Assets/DevilFramework/Gizmos/AI Icons/loop.png", HotKey = KeyCode.L)]
    public class BTLoop : BTControllerAsset
    {
        public enum EResult
        {
            ResultFromLastTask,
            PresentState,
            True,
            False,
            ResultFromChild,
        }

        public enum ELoopMode
        {
            UntilFailed,
            UntilSuccess,
            Always,
        }

        // 循环条件
        public ELoopMode m_LoopCondition;
        // 循环结束返回结果
        public EResult m_LoopResult;
        // 作为任务结果的子节点索引
        public int m_ResultFromChildIndex;
        public bool m_Random;

        int mVisitIndex;
        EBTState mLastState;

        public override string DisplayContent
        {
            get
            {
                return string.Format("{0} => {1}", m_LoopCondition, m_LoopResult);
            }
        }

        public override IBTNode GetNextChildTask()
        {
            if ( mVisitIndex < mChildren.Length)
                return mChildren[mVisitIndex];
            else
                return null;
        }

        public override EBTState OnReturn(EBTState state)
        {
            mLastState = state;
            if (m_LoopCondition == ELoopMode.UntilSuccess && state == EBTState.success)
                return GetResult(true);
            if (m_LoopCondition == ELoopMode.UntilFailed && state == EBTState.failed)
                return GetResult(true);
            mVisitIndex++;
            if (mVisitIndex >= mChildren.Length)
                mVisitIndex = 0;
            return EBTState.running;
        }

        public override EBTState OnStart()
        {
            mVisitIndex = 0;
            mLastState = EBTState.inactive;
            return mChildren.Length > 0 ? EBTState.running : EBTState.failed;
        }

        public override EBTState OnAbort()
        {
            return GetResult(false);
        }

        EBTState GetResult(bool success)
        {
            if (m_LoopResult == EResult.False)
                return EBTState.failed;
            else if (m_LoopResult == EResult.True)
                return EBTState.success;
            else if (m_LoopResult == EResult.ResultFromLastTask)
                return mLastState;
            else if (m_LoopResult == EResult.ResultFromChild && m_ResultFromChildIndex >= 0 && m_ResultFromChildIndex < mChildren.Length)
                return mChildren[m_ResultFromChildIndex].State;
            else
                return success ? EBTState.success : EBTState.failed;
        }
    }
}