using System;

namespace TableCore.Exceptions
{
    public class TypeNotDefinedException : Exception
    {
        public TypeNotDefinedException(string type) : base(string.Format("没有定义类型 \"{0}\"", type))
        {

        }
    }
}
