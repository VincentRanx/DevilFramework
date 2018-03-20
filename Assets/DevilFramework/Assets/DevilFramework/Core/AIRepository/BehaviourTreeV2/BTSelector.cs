using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [BehaviourTree(FrameStyle = "flow node 1", DisplayName = "SELECTOR")]
    public class BTSelector : BTNodeBase
    {
        int mVisitIndex;

        public BTSelector(int id) : base(id)
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
            if(state == EBTTaskState.faild)
            {
                mVisitIndex++;
                if (mVisitIndex >= ChildLength)
                    this.State = EBTTaskState.faild;
            }
            else
            {
                this.State = EBTTaskState.success;
            }
        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            mVisitIndex = 0;
            this.State = mVisitIndex < ChildLength ? EBTTaskState.running : EBTTaskState.faild;
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            if (mVisitIndex >= ChildLength)
                this.State = EBTTaskState.faild;
        }
    }
}