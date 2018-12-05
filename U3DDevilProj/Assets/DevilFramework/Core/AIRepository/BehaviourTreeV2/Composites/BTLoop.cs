using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "循环 (LOOP)", IconPath = "Assets/DevilFramework/Gizmos/AI Icons/loop.png", HotKey = KeyCode.L)]
    public class BTLoop : BTControllerAsset
    {
        int mVisitIndex;

        public override IBTNode GetNextChildTask()
        {
            if (State == EBTState.running && mVisitIndex < mChildren.Length)
            {
                return mChildren[mVisitIndex];
            }
            else
            {
                return null;
            }
        }

        public override EBTState OnReturn(EBTState state)
        {
            mVisitIndex++;
            if (mVisitIndex >= mChildren.Length)
                mVisitIndex = 0;
            return EBTState.running;
        }

        public override EBTState OnStart()
        {
            mVisitIndex = 0;
            return mChildren.Length > 0 ? EBTState.running : EBTState.failed;
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }
    }
}