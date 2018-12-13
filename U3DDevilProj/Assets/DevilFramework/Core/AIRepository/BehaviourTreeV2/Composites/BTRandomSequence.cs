using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "随机序列 (R)", Detail = "随机执行子节点，\n直到任一任务失败", 
        IconPath = "Assets/DevilFramework/Gizmos/AI Icons/sequence.png", HotKey = KeyCode.R)]
    public class BTRandomSequence : BTControllerAsset
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
            if (state == EBTState.failed)
                return EBTState.failed;
            mExecI++;
            return mExecI < mExecIndex.Length ? EBTState.running : EBTState.success;
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
                return EBTState.success;
            }
        }
    }
}