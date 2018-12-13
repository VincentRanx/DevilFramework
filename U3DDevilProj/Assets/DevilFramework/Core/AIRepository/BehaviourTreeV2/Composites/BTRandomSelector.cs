using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "随机选择 (R)", Detail = "随机执行子节点，\n直到任一任务成功", 
        IconPath = "Assets/DevilFramework/Gizmos/AI Icons/selector.png", HotKey = KeyCode.R)]
    public class BTRandomSelector : BTControllerAsset
    {
        int mExecI;
        int[] mExecIndex;

        public override void OnPrepare(BehaviourTreeRunner.AssetBinder asset, BTNode node)
        {
            base.OnPrepare(asset, node);
            mExecIndex = new int[mChildren.Length];
            for(int i = 0; i < mExecIndex.Length; i++)
            {
                mExecIndex[i] = i;
            }
        }

        public override IBTNode GetNextChildTask()
        {
            var i = AIUtility.GetRandomIndex(mExecIndex, mExecI);
            return mChildren[i];
        }

        public override EBTState OnAbort()
        {
            return EBTState.failed;
        }

        public override EBTState OnReturn(EBTState state)
        {
            if (state == EBTState.success)
                return EBTState.success;
            mExecI++;
            return mExecI < mExecIndex.Length ? EBTState.running : EBTState.failed;
        }

        public override EBTState OnStart()
        {
            mExecI = 0;
            if (mExecIndex.Length > 0)
            {
                return EBTState.running;
            }
            else
            {
                return EBTState.failed;
            }
        }
    }
}