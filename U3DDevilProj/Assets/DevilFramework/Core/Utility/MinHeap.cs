using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.Utility
{
    // 最小堆
    public class MinHeap<T> : ICollection<T>
    {
        private T[] mDatas;
        private Comparison<T> mComparison;
        private int mLength;

        public MinHeap(Comparison<T> comparison = null)
        {
            mDatas = new T[256];
            if (comparison == null)
                mComparison = (x, y) => x.GetHashCode() - y.GetHashCode();
            else
                mComparison = comparison;
        }

        public MinHeap(int capacity, Comparison<T> comparison = null)
        {
            mDatas = new T[Mathf.Max(16, capacity)];
            if (comparison == null)
                mComparison = (x, y) => x.GetHashCode() - y.GetHashCode();
            else
                mComparison = comparison;
        }
        
        public int Length { get { return mLength; } }
        public T this[int index] { get { return mDatas[index]; } }

        public void Add(T value)
        {
            if(mLength == mDatas.Length)
            {
                var newdata = new T[mLength << 1];
                Array.Copy(mDatas, newdata, mLength);
                mDatas = newdata;
            }

            int child = mLength;
            mDatas[mLength++] = value;
            T tmp;
            int parent;
            while(child > 0)
            {
                parent = (child - 1) >> 1;
                if (mComparison(mDatas[parent], mDatas[child]) <= 0)
                    break;
                tmp = mDatas[parent];
                mDatas[parent] = mDatas[child];
                mDatas[child] = tmp;
                child = parent;
            }
        }

        public T TopValule { get { return mDatas[0]; } }

        public int Count { get { return mLength; } }

        public bool IsReadOnly { get { return false; } }

        public void Clear()
        {
            var def = default(T);
            for(int i = 0; i < mLength; i++)
            {
                mDatas[i] = def;
            }
            mLength = 0;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < mLength; i++)
            {
                if (mComparison(mDatas[i], item) == 0)
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(mDatas, 0, array, arrayIndex, mLength);
        }

        public T RemoveTop()
        {
            return RemoveAt(0);
        }

        public T RemoveAt(int index)
        {
            var def = default(T);
            if (index >= mLength)
                return def;
            var ret = mDatas[index];
            mDatas[index] = def;
            var tmp = mDatas[--mLength];
            mDatas[mLength] = def;
            int child = (index << 1) + 1;
            int parent;
            while (child < mLength)
            {
                if (child < mLength - 1 && mComparison(mDatas[child], mDatas[child + 1]) > 0)
                    child++;
                parent = (child - 1) >> 1;
                mDatas[parent] = mDatas[child];
                mDatas[child] = def;
                child = (child << 1) + 1;
            }
            child = (child - 1) >> 1;
            mDatas[child] = tmp;
            while (child > 0)
            {
                parent = (child - 1) >> 1;
                if (mComparison(mDatas[parent], mDatas[child]) <= 0)
                    break;
                tmp = mDatas[parent];
                mDatas[parent] = mDatas[child];
                mDatas[child] = tmp;
                child = parent;
            }
            return ret;
        }

        public bool Remove(T item)
        {
            int rev = -1;
            for(int i= 0; i < mLength; i++)
            {
                if(mComparison(mDatas[i] , item) == 0)
                {
                    rev = i;
                    break;
                }
            }
            if (rev == -1)
                return false;
            RemoveAt(rev);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private class Enumerator : IEnumerator<T>
        {
            private int mPtr;
            MinHeap<T> mHeap;
            public Enumerator(MinHeap<T> heap)
            {
                mHeap = heap;
                mPtr = -1;
            }

            public T Current { get { return mPtr < mHeap.mLength && mPtr >= 0 ? mHeap.mDatas[mPtr] : default(T); } }

            object IEnumerator.Current { get { return mPtr < mHeap.mLength && mPtr >= 0 ? mHeap.mDatas[mPtr] : default(T); } }

            public void Dispose()
            {
                mHeap = null;
            }

            public bool MoveNext()
            {
                if (++mPtr < mHeap.mLength)
                    return true;
                else
                    return false;
            }

            public void Reset()
            {
                mPtr = -1;
            }
        }
    }
}