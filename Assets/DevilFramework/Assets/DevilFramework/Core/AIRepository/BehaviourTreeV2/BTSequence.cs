using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [BehaviourTree(FrameStyle = "flow node 1", DisplayName = "SEQUENCE", SubTitle = 
@"顺序执行子节点，直到第一个失败的
任务结束，类似于 AND 运算。", SortOrder = -3)]
    public class BTSequence : BTNodeBase
    {
        int mVisitIndex;

        public BTSequence(int id) : base(id)
        {

        }

        public override BTNodeBase ChildForVisit
        {
            get
            {
                if (State == EBTTaskState.running && mVisitIndex < ChildLength)
                {
                    return ChildAt(mVisitIndex);
                }
                else
                {
                    return null;
                }
            }
        }

        public override void ReturnWithState(EBTTaskState state)
        {
            if(state == EBTTaskState.success)
            {
                mVisitIndex++;
                if (mVisitIndex >= ChildLength)
                    this.State = EBTTaskState.success;
            }
            else
            {
                this.State = EBTTaskState.faild;
            }
        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            mVisitIndex = 0;
            this.State = mVisitIndex < ChildLength ? EBTTaskState.running : EBTTaskState.success;
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            mVisitIndex++;
            if (mVisitIndex >= ChildLength)
                this.State = EBTTaskState.success;
        }
    }
}