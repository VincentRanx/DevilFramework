using System.IO;

namespace Devil.ContentProvider
{
    public abstract class AssetStreamBase : Stream
    {
        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return true; } }

        public override bool CanWrite { get { return false; } }

        public override void Flush()
        {
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
        }
    }
}