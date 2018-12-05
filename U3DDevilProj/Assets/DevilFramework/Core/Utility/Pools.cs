using System.Collections.Generic;

namespace Devil.Utility
{
    public class Pools
    {
        static Pools sInstance;
        static Pools Instance
        {
            get
            {
                if (sInstance == null)
                    sInstance = new Pools();
                return sInstance;
            }
        }

        Dictionary<System.Type, IPool> mPools;

        private Pools()
        {
            mPools = new Dictionary<System.Type, IPool>();
        }

        ObjectPool<T> DetectPool<T>() where T : class
        {
            var tp = typeof(T);
            IPool pool;
            if (!mPools.TryGetValue(tp, out pool))
            {
                var p = new ObjectPool<T>(32);
                mPools[tp] = p;
                return p;
            }
            return (ObjectPool<T>)pool;
        }

        public static ObjectPool<T> GetPool<T>() where T : class
        {
            return Instance.DetectPool<T>();
        }

        public static T Get<T>() where T : class
        {
            var p = GetPool<T>();
            return p.Get();
        }

        public static void Add<T>(T data) where T : class
        {
            var p = GetPool<T>();
            p.Add(data);
        }

        public static void AddToPool(object data)
        {
            if (data == null)
                return;
            var tp = data.GetType();
            IPool pool;
            if (Instance.mPools.TryGetValue(tp, out pool))
                pool.AddData(data);
        }
    }
}