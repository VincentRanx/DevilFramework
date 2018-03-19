using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BTSequence : BTNodeBase
    {
        int mVisitIndex;

        public BTSequence(int id, int decoratorLen, int childLength, int serviceLen) : base(id, decoratorLen, childLength, serviceLen)
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