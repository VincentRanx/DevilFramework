using Devil;
using System;
using UnityEngine;

namespace Devil.Utility
{
    public delegate T ObjectGenerater<T>();
    public delegate T ObjectGenerater<K, T>(K selector);

    public class ObjectBuffer<T> where T : class
    {
        object mLock;
        int mLen;
        T[] mBuffer;
        public ObjectGenerater<T> Creater { get; set; }
        public Action<T> Destroier { get; set; }

        public ObjectBuffer(int initCapacity = 10, ObjectGenerater<T> creater = null, Action<T> destroier = null)
        {
            mBuffer = new T[initCapacity];
            mLen = 0;
            mLock = new object();
            Creater = creater;
            Destroier = destroier;
        }

        void MallocSize(int requiredLen)
        {
            int len = Mathf.Max(requiredLen, mBuffer.Length << 1);
            T[] tmp = new T[len];
            Array.Copy(mBuffer, tmp, mLen);
            mBuffer = tmp;
        }

        public void SaveBuffer(T target)
        {
            lock (mLock)
            {
                if (mLen >= mBuffer.Length)
                {
                    MallocSize(mLen + 10);
                }
                mBuffer[mLen++] = target;
            }
        }

        public T GetAnyTarget()
        {
            lock (mLock)
            {
                if (mLen > 0)
                {
                    T tmp = mBuffer[--mLen];
                    mBuffer[mLen] = null;
                    return tmp;
                }
                if (Creater != null)
                {
                    return Creater();
                }
                else
                {
                    return null;
                }
            }
        }

        public int CacheLength { get { return mLen; } }
        public T this[int index] { get { return mBuffer[index]; } }

        public void Clear()
        {
            lock (mLock)
            {
                for (int i = 0; i < mLen; i++)
                {
                    if (Destroier != null)
                        Destroier(mBuffer[i]);
                    mBuffer[i] = null;
                }
                mLen = 0;
            }
        }
    }

    public class ObjectPool<T> where T : class
    {
        object mLock;
        int[] mLen;
        T[][] mBuffer;
        public ObjectGenerater<int, T> Creater { get; set; }
        public Action<int, T> Destroier { get; set; }

        public ObjectPool(int groups, int initCapacity = 10, ObjectGenerater<int, T> creater = null, Action<int, T> destroier = null)
        {
            mBuffer = new T[groups][];
            mLen = new int[groups];
            for (int i = 0; i < groups; i++)
            {
                mBuffer[i] = new T[initCapacity];
                mLen[i] = 0;
            }
            mLock = new object();
            Creater = creater;
            Destroier = destroier;
        }

        void MallocSize(int group, int requiredLen)
        {
            int len = Mathf.Max(requiredLen, mBuffer[group].Length << 1);
            T[] tmp = new T[len];
            Array.Copy(mBuffer[group], tmp, mLen[group]);
            mBuffer[group] = tmp;
        }

        public void SaveBuffer(int group, T target)
        {
            lock (mLock)
            {
                if (mLen[group] >= mBuffer[group].Length)
                {
                    MallocSize(group, mLen[group] + 10);
                }
                int len = mLen[group];
                mBuffer[group][len] = target;
                mLen[group] = len + 1;
            }
        }

        public T GetAnyTarget(int group)
        {
            lock (mLock)
            {
                if (mLen[group] > 0)
                {
                    int len = mLen[group] - 1;
                    T tmp = mBuffer[group][len];
                    mBuffer[group][len] = null;
                    mLen[group] = len;
                    return tmp;
                }
                if (Creater != null)
                {
                    return Creater(group);
                }
                else
                {
                    return null;
                }
            }
        }

        public void Clear(int group)
        {
            lock (mLock)
            {
                for (int i = 0; i < mLen[group]; i++)
                {
                    if (Destroier != null)
                        Destroier(group, mBuffer[group][i]);
                    mBuffer[group][i] = null;
                }
                mLen[group] = 0;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < mBuffer.Length; i++)
            {
                Clear(i);
            }
        }
    }
}