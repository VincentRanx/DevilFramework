namespace org.vr.rts.util
{

    public class RTSList<T>
    {

        private T[] values;
        private int len;
        private int capacity;
        private int mallocUnit;

        public RTSList()
        {
            mallocUnit = 5;
        }

        public RTSList(int capacity)
        {
            mallocUnit = 5;
            this.capacity = capacity > 0 ? capacity : 5;
            if (capacity < mallocUnit)
                values = new T[capacity];
        }

        public RTSList(int capacity, int mallocSize)
        {
            this.mallocUnit = mallocSize > 0 ? mallocSize : 5;
            this.capacity = capacity > 0 ? capacity : this.mallocUnit;
            if (capacity < mallocSize)
                values = new T[capacity];
        }

        // newSize must greater than current size
        private void mallocSize(int newSize)
        {
            if (values == null)
            {
                values = new T[newSize > capacity ? newSize : capacity];
            }
            else
            {
                T[] list = new T[newSize];
                for (int i = 0; i < len; i++)
                {
                    list[i] = values[i];
                }
                values = list;
                list = null;
            }
        }

        public void clear()
        {
            for (int i = 0; i < len; i++)
            {
                values[i] = default(T);
            }
            len = 0;
        }

        public void set(int index, T value)
        {
            if (index < len)
                values[index] = value;
        }

        public void add(T value)
        {
            if (values == null || len >= values.Length)
                mallocSize(len + mallocUnit + (len << 2));
            values[len++] = value;
        }

        public void setOrAdd(int index, T value)
        {
            if (values == null || index >= values.Length)
            {
                mallocSize(values.Length + mallocUnit + (len << 2));
            }
            values[index] = value;
            if (index >= len)
                len = index + 1;
        }

        public void addList(RTSList<T> list)
        {
            if (list != null)
            {
                for (int i = 0; i < list.length(); i++)
                {
                    add(list.get(i));
                }
            }
        }

        public T removeLast()
        {
            if (len > 0)
            {
                T v = values[--len];
                values[len] = default(T);
                return v;
            }
            else
            {
                return default(T);
            }
        }

        public void removeFrom(int startIndex)
        {
            if (len > startIndex)
            {
                for (int i = startIndex; i < len; i++)
                {
                    values[i] = default(T);
                }
                len = startIndex;
            }
        }

        public T getLast()
        {
            if (len > 0)
                return values[len - 1];
            else
                return default(T);
        }

        public T get(int index)
        {
            if (index < len)
                return values[index];
            else
                return default(T);
        }

        public int length()
        {
            return len;
        }
    }
}