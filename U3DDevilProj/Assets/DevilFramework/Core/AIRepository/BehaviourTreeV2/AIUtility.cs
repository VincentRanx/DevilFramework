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
}