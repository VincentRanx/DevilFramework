using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    [System.Serializable]
    public class AIGoalGraph : ScriptableObject
    {
        [System.Serializable]
        public struct Result
        {
            public int goalId;
            public int goalResult;
        }

        [System.Serializable]
        public class Goal
        {
            public int m_Id;
            public ELogic m_ConditionLogic = ELogic.And;
        }

        public class Transition
        {
            public int m_GoalId;
        }

        [SerializeField]
        List<Goal> m_Goals = new List<Goal>();

    }
}