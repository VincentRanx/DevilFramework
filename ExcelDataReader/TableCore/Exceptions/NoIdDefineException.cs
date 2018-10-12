using System;

namespace TableCore.Exceptions
{
    public class NoIdDefineException : Exception
    {
        public NoIdDefineException(string table) : base(string.Format("表 \"{0}\" 没有定义 ID 或者 ID 类型非 int.", table))
        {

        }
    }
}
