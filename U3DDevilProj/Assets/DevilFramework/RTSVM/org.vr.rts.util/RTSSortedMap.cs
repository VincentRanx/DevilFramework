namespace org.vr.rts.util
{

    public class RTSSortedMap<K, V>
        where K : class
        where V : class
    {

        private K[] mKeys;
        private V[] mValues;
        private int mLen;

        public RTSSortedMap(int capacity)
        {
            mKeys = new K[capacity];
            mValues = new V[capacity];
            mLen = 0;
        }

        public int lenth()
        {
            return mLen;
        }

        public K keyAt(int index)
        {
            return mKeys[index];
        }

        public V valueAt(int index)
        {
            return mValues[index];
        }

        // newSize must greater than current size
        private void mallocSize(int newSize)
        {
            K[] keys = new K[newSize];
            V[] values = new V[newSize];
            for (int i = 0; i < mLen; i++)
            {
                keys[i] = mKeys[i];
                values[i] = mValues[i];
            }
            mKeys = keys;
            mValues = values;
        }

        public int indexOfKey(K key)
        {
            for (int i = 0; i < mLen; i++)
            {
                if (mKeys[i] == key)
                    return i;
            }
            return -1;
        }

        public int indexOfValue(V value)
        {
            for (int i = 0; i < mLen; i++)
            {
                if (mValues[i] == value)
                    return i;
            }
            return -1;
        }

        public void removeAt(int index)
        {
            if (index < mLen)
            {
                mLen--;
                for (int i = index; i < mLen; i++)
                {
                    mKeys[i] = mKeys[i + 1];
                    mValues[i] = mValues[i + 1];
                }
                mKeys[mLen] = null;
                mValues[mLen] = null;
            }
        }

        public bool insertAt(int index, K key, V value)
        {
            if (index > mLen)
                return false;
            if (mLen == mKeys.Length)
                mallocSize(mKeys.Length + (mLen >> 1) + 1);
            for (int i = mLen; i > index; i--)
            {
                mKeys[i] = mKeys[i - 1];
                mValues[i] = mValues[i - 1];
            }
            mKeys[index] = key;
            mValues[index] = value;
            mLen++;
            return true;
        }

        public void add(K key, V value)
        {
            if (mLen == mKeys.Length)
                mallocSize(mKeys.Length + (mLen >> 1) + 1);
            mKeys[mLen] = key;
            mValues[mLen] = value;
            mLen++;
        }
    }
}