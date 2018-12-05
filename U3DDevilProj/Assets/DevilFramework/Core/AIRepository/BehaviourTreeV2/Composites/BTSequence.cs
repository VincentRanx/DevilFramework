using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "序列 (SEQUENCE)",Detail = "依次执行子节点，\n直到任何一个任务执行失败", IconPath = "Assets/DevilFramework/Gizmos/AI Icons/sequence.png", HotKey = KeyCode.S)]
    public class BTSequence : BTControllerAsset
    {
        int mIndex;

        public override IBTNode GetNextChildTask()
        {
            return mChildren[mIndex];
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }

        public override EBTState OnReturn(EBTState state)
        {
            if (state == EBTState.failed)
                return EBTState.failed;
            mIndex++;
            if (mIndex < mChildren.Length)
                return EBTState.running;
            else
                return EBTState.success;
        }

        public override EBTState OnStart()
        {
            mIndex = 0;
            return mIndex < mChildren.Length ? EBTState.running : EBTState.success;
        }
        
    }
}