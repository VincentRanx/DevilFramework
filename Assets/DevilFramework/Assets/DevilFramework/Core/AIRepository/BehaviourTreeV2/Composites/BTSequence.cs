using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [BTComposite(Title = "序列", Detail = "SEQUENCE", IconPath = "Assets/DevilFramework/Editor/Icons/sequence.png")]
    public class BTSequence : BTNodeBase
    {
        int mVisitIndex;
        bool mAbort;

        public BTSequence(int id) : base(id)
        {

        }

        public override BTNodeBase ChildForVisit
        {
            get
            {
                if (!mAbort && State == EBTTaskState.running && mVisitIndex < ChildLength)
                {
                    return ChildAt(mVisitIndex);
                }
                else
                {
                    return null;
                }
            }
        }

        protected override void OnReturnWithState(BehaviourTreeRunner btree, EBTTaskState state)
        {
            if(state == EBTTaskState.faild || !IsOnCondition(btree))
            {
                this.State = EBTTaskState.faild;
            }
            else
            {
                mVisitIndex++;
                if (mVisitIndex >= ChildLength)
                    this.State = EBTTaskState.success;
            }
        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            mVisitIndex = 0;
            mAbort = false;
            this.State = mVisitIndex < ChildLength ? EBTTaskState.running : EBTTaskState.success;
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            if (mAbort)
            {
                State = EBTTaskState.faild;
            }
            else
            {
                mVisitIndex++;
                if (mVisitIndex >= ChildLength)
                    this.State = EBTTaskState.success;
            }
        }

        protected override void OnAbort(BehaviourTreeRunner btree)
        {
            mAbort = true;
        }
    }
}