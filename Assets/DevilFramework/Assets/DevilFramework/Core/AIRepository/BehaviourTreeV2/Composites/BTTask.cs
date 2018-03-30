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

        protected override void OnReturnWithState(BehaviourTreeRunner btree, EBTTaskState state)
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
        
        protected override void OnAbort(BehaviourTreeRunner btree)
        {
            if (mTask != null)
                mTask.OnAbort(btree);
        }
    }
    
}