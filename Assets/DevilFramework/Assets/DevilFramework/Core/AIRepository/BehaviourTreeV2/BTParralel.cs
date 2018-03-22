using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [BehaviourTree(FrameStyle = "flow node 3", DisplayName = "PARRALEL", SubTitle = "并行执行子节点，相互不干扰,类似于并发", SortOrder = -1)]
    public class BTParralel: BTNodeBase
    {
        BehaviourLooper[] mLoopers;

        public BTParralel(int id) : base(id)
        {
        }

        public override BTNodeBase ChildForVisit
        {
            get
            {
                return null;
            }
        }

        public override void InitDecoratorSize(int conditionLen, int childLen, int serviceLen)
        {
            base.InitDecoratorSize(conditionLen, childLen, serviceLen);
            mLoopers = new BehaviourLooper[childLen];
        }

        public override void SetChild(int index, BTNodeBase node)
        {
            base.SetChild(index, node);
            if(node != null)
                mLoopers[index] = new BehaviourLooper(node);
        }

        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < mLoopers.Length; i++)
            {
                mLoopers[i].ResetTreeState();
            }
        }

        public override void ReturnWithState(EBTTaskState state)
        {

        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            State = ChildLength > 0 ? EBTTaskState.running : EBTTaskState.success;
            for (int i = 0; i < mLoopers.Length; i++)
            {
                if (mLoopers[i] != null)
                    mLoopers[i].Reset();
            }
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            int num = 0;
            for(int i = 0; i < mLoopers.Length; i++)
            {
                if (mLoopers[i] == null || mLoopers[i].IsComplate)
                    continue;
                mLoopers[i].Update(behaviourTree, deltaTime);
                num++;
            }
            if (num == 0)
                State = EBTTaskState.success;
        }

        public override bool AbortWithSucces()
        {
            return true;
        }
    }
}