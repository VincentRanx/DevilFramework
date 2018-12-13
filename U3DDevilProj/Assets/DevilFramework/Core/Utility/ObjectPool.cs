using UnityEngine;

namespace Devil.Utility
{
    public interface IPool : System.IDisposable
    {
        object GetData();
        bool AddData(object data);
    }

    public interface IPoolWorker<T>
    {
        T NewData();
        void CleanData(T data);
    }

    public class ObjectPool<T> : IPool where T : class
    {
        object mLock = new object();

        public delegate T ObjectConstructor();
        public delegate void ObjectCleaner(T target);

        private T[] mCache;
        private int mLen;

        IPoolWorker<T> mWorker;
        public IPoolWorker<T> Worker
        {
            get { return mWorker; }
            set
            {
                if(mWorker != value)
                {
                    mWorker = value;
                    if(mWorker != null)
                    {
                        mConstructor = mWorker.NewData;
                        mCleaner = mWorker.CleanData;
                    }
                }
            }
        }

        ObjectConstructor mConstructor;
        ObjectCleaner mCleaner;
        public void SetConstructor(ObjectConstructor constructor)
        {
            mConstructor = constructor;
            if (mConstructor == null && mWorker != null)
                mConstructor = mWorker.NewData;
        }

        public void SetCleaner(ObjectCleaner cleaner)
        {
            mCleaner = cleaner;
            if (mCleaner == null && mWorker != null)
                mCleaner = mWorker.CleanData;
        }

        public int Length { get { return mLen; } }
        public int Capacity { get { return mCache.Length; } }
        public System.Type GetPoolType()
        {
            return typeof(T);
        }

        public ObjectPool(int capacity = 256)
        {
            mCache = new T[Mathf.Max(16, capacity)];
            mLen = 0;
        }

        public ObjectPool(int capacity, ObjectConstructor creator, ObjectCleaner cleaner = null)
        {
            mConstructor = creator;
            mCleaner = cleaner;
            mCache = new T[Mathf.Max(32, capacity)];
            mLen = 0;
        }
        
        public void Resize(int capacity)
        {
            lock (mLock)
            {
                var len = Mathf.Max(32, capacity);
                if (len != mCache.Length)
                {
                    var newcache = new T[len];
                    if (mCleaner != null)
                    {
                        while (mLen > len)
                        {
                            mCleaner(mCache[--mLen]);
                            mCache[mLen] = null;
                        }
                    }
                    else
                    {
                        while (mLen > len)
                        {
                            mCache[--mLen] = null;
                        }
                    }
                    System.Array.Copy(mCache, newcache, mLen);
                    mCache = newcache;
                }
            }
        }
        
        public bool AddData(object data)
        {
            if (data is T)
                return Add((T)data);
            else
                return false;
        }

        public void RemoveFromPool(object data)
        {
            if (data is T)
                Remove((T)data);
        }

        public bool Add(T data)
        {
            lock (mLock)
            {
                if (data == null)
                {
                    return false;
                }
                else if (mLen < mCache.Length)
                {
                    mCache[mLen++] = data;
                    return true;
                }
                else
                {
                    if (mCleaner != null)
                        mCleaner(data);
                    return false;
                }
            }
        }

        public void Remove(T data)
        {
            lock (mLock)
            {
                for (int i = 0; i < mLen; i++)
                {
                    if (mCache[i] == data)
                    {
                        if (i < mLen - 1)
                        {
                            mCache[i] = mCache[--mLen];
                            mCache[mLen] = null;
                        }
                        else
                        {
                            mCache[--mLen] = null;
                        }
                        break;
                    }
                }
            }
        }

        public object GetData() { return Get(); }

        public T Get()
        {
            lock (mLock)
            {
                if (mLen > 1)
                {
                    var ret = mCache[0];
                    mCache[0] = mCache[--mLen];
                    mCache[mLen] = null;
                    return ret;
                }
                else if (mLen > 0)
                {
                    var ret = mCache[0];
                    mCache[--mLen] = null;
                    return ret;
                }
                else if (mConstructor != null)
                {
                    return mConstructor();
                }
                else
                {
                    return null;
                }
            }
        }

        public T Get(FilterDelegate<T> condition)
        {
            if (condition == null)
                return Get();
            lock (mLock)
            {
                for (int i = 0; i < mLen; i++)
                {
                    var ret = mCache[i];
                    if (condition(ret))
                    {
                        mCache[i] = mCache[--mLen];
                        mCache[mLen] = null;
                        return ret;
                    }
                }
                return null;
            }
        }

        public void Clear()
        {
            lock (mLock)
            {
                if (mCleaner != null)
                {
                    while (mLen > 0)
                    {
                        mCleaner(mCache[--mLen]);
                        mCache[mLen] = null;
                    }
                }
                else
                {
                    while (mLen > 0)
                    {
                        mCache[--mLen] = null;
                    }
                }
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }
}