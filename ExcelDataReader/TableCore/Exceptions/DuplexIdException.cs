using System;

namespace TableCore.Exceptions
{
    public class DuplexIdException : Exception
    {
        public DuplexIdException(string id) : base(string.Format("ID \"{0}\" 重复。\nTable: {1}/{2}",
            id, GTStatus.Instance.FileName, GTStatus.Instance.UsedTableName))
        {

        }
    }

}
