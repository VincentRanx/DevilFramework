using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public static class AIUtility 
	{
        public static int GetRandomIndex(int[] index, int i)
        {
            if (index == null)
                return i;
            i = index.Length - i - 1;
            if (i > 0)
            {
                var r = Mathf.RoundToInt(Random.value * i);
                var tmp = index[r];
                index[r] = index[i];
                index[i] = tmp;
            }
            return index[i];
        }

    }

    public class AIManager
    {
        private static AIManager sInstance;
        public static AIManager Instance { get { if (sInstance == null) sInstance = new AIManager(); return sInstance; } }

        List<BehaviourTreeRunner> mActiveTree = new List<BehaviourTreeRunner>();

        private AIManager() { }

        public BehaviourTreeRunner FindBehaviourTree(string name)
        {
            return GlobalUtil.Find(mActiveTree, (x) => x.name == name);
        }

        public void Add(BehaviourTreeRunner btree)
        {
            mActiveTree.Add(btree);
        }

        public void Remove(BehaviourTreeRunner btree)
        {
            mActiveTree.Remove(btree);
        }
    }
}