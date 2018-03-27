using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public abstract class BTConditionBase
    {
        public int Id { get; private set; }

        public BTConditionBase(int id)
        {
            Id = id;
        }

        public abstract void OnInitData(BehaviourTreeRunner btree, string jsonData);

        public abstract bool IsTaskRunnable(BehaviourTreeRunner btree);

        public abstract bool IsTaskOnCondition(BehaviourTreeRunner btree);
    }
}