using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BTTask : BTNodeBase
    {
        BTTaskBase mTask;

        public BTTask(int id, BTTaskBase task) : base(id)
        {
            mTask = task;
        }

        public override BTNodeBase ChildForVisit
        {
            get { return null; }
        }

        protected override void OnReturnWithState(EBTTaskState state)
        {
        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            if (mTask != null)
            {
                State = mTask.OnTaskStart(behaviourTree);
            }
            else
            {
                State = EBTTaskState.faild;
            }
        }

        public override void InitData(BehaviourTreeRunner btree, string jsonData)
        {
            if (mTask != null)
                mTask.OnInitData(btree, jsonData);
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            if (mTask != null)
                State = mTask.OnTaskTick(behaviourTree, deltaTime);
        }

        public override bool AbortAndReturnSuccess(BehaviourTreeRunner behaviourTree)
        {
            if (mTask != null)
                return mTask.OnTaskAbortAndReturnSuccess(behaviourTree);
            else
                return false;
        }
    }

    public class BTConstTask : BTNodeBase
    {
        EBTTaskState mState;

        public BTConstTask(EBTTaskState state) : base(0) { mState = state; }

        public override BTNodeBase ChildForVisit
        {
            get { return null; }
        }

        protected override void OnReturnWithState(EBTTaskState state)
        {
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            State = mState;
        }

    }
}