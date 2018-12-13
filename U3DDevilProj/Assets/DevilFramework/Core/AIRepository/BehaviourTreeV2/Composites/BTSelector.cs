using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "选择 (SELECTOR)",Detail = "依次执行子节点，\n直到任何一个任务执行成功", 
        IconPath = "Assets/DevilFramework/Gizmos/AI Icons/selector.png", HotKey = KeyCode.S)]
    public class BTSelector : BTControllerAsset
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
            if (state == EBTState.success)
                return EBTState.success;
            mIndex++;
            if (mIndex < mChildren.Length)
                return EBTState.running;
            else
                return EBTState.failed;
        }

        public override EBTState OnStart()
        {
            mIndex = 0;
            return mIndex < mChildren.Length ? EBTState.running : EBTState.failed;
        }
        
    }
}