using System.Collections;
using System.Collections.Generic;

namespace Devil.Utility
{
    public class BytesBuffer: IEnumerable<byte>
    {
        static ObjectPool<BytesBuffer> mPool = new ObjectPool<BytesBuffer>(32);
        public static BytesBuffer Get(int size)
        {
            var buffer = mPool.Get((x) => x.MaxSize >= size);
            if (buffer == null)
                buffer = new BytesBuffer(size < 256 ? 256 : size);
            buffer.Length = size;
            return buffer;
        }

        public static void Release(BytesBuffer buffer)
        {
            mPool.Add(buffer);
        }

        public static void ClearBuffer()
        {
            mPool.Clear();
        }

        byte[] mData;
        int MaxSize { get { return mData.Length; } }
        public int Length { get; private set; }

        public int Count { get { return Length; } }

        public bool IsReadOnly { get { return false; } }

        public byte this[int index]
        {
            get
            {
                if (index >= Length || index < 0)
                    throw new System.IndexOutOfRangeException();
                return mData[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new System.IndexOutOfRangeException();
                mData[index] = value;
            }
        }

        private BytesBuffer(int size)
        {
            mData = new byte[size];
        }
        
        public void CopyTo(byte[] array, int arrayIndex)
        {
            System.Array.Copy(mData, 0, array, arrayIndex, Length);
        }

        public void CopyTo(int sourceIndex, byte[] array, int destinationIndex, int length)
        {
            System.Array.Copy(mData, sourceIndex, array, destinationIndex, length);
        }

        public void CopyTo(BytesBuffer buffer, int arrayIndex)
        {
            System.Array.Copy(mData, 0, buffer.mData, arrayIndex, Length);
        }

        public void CopyTo(int sourceIndex, BytesBuffer buffer, int destinationIndex, int length)
        {
            System.Array.Copy(mData, sourceIndex, buffer.mData, destinationIndex, length);
        }
        
        public IEnumerator<byte> GetEnumerator()
        {
            return new ByteEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ByteEnumerator(this);
        }

        public class ByteEnumerator :IEnumerator<byte>
        {
            int ptr;
            BytesBuffer buffer;
            public ByteEnumerator(BytesBuffer buffer)
            {
                this.buffer = buffer;
                ptr = -1;
            }

            public byte Current { get { return buffer.mData[ptr]; } }

            object IEnumerator.Current { get { return buffer.mData[ptr]; } }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                ptr++;
                return ptr < buffer.Length && ptr >= 0;
            }

            public void Reset()
            {
                ptr = -1;
            }
        }

    }
}