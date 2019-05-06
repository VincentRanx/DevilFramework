using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class AIManager : MonoBehaviour
    {
        static List<BehaviourTreeRunner> mActiveTree = new List<BehaviourTreeRunner>();

        public static BehaviourTreeRunner FindBehaviourTree(string name)
        {
            return GlobalUtil.Find(mActiveTree, (x) => x.name == name);
        }

        public static void Add(BehaviourTreeRunner btree)
        {
            mActiveTree.Add(btree);
        }

        public static void Remove(BehaviourTreeRunner btree)
        {
            mActiveTree.Remove(btree);
        }
    }
}