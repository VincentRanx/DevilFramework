using System.Collections.Generic;

namespace Devil.Utility
{
    public class ObjectPool<T> where T : class
    {
        static LinkedList<T> mBuffer = new LinkedList<T>();

        public static T Get(ObjectGenerater<T> ctor = null)
        {
            if(mBuffer.Count > 0)
            {
                var v = mBuffer.First.Value;
                mBuffer.RemoveFirst();
                return v;
            }
            else if(ctor != null)
            {
                return ctor();
            }
            else
            {
                return null;
            }
        }

        public static void Cache(T v)
        {
            if (v != null)
                mBuffer.AddLast(v);
        }

        public static void Release(ObjectCleaner<T> cleaner = null)
        {
            if(cleaner == null)
            {
                mBuffer.Clear();
            }
            else
            {
                var node = mBuffer.First;
                while(node != null)
                {
                    var v = node.Value;
                    var next = node.Next;
                    if (cleaner(v))
                        mBuffer.Remove(node);
                    node = next;
                }
            }
        }
    }
}