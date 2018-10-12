using System;

namespace TableCore.Exceptions
{
    public class ValueNotMatchTypeException : Exception
    {
        public static string GetMessage(string data, GTType gttype, string cell)
        {
            return string.Format("\"{0}\" 不能匹配类型 \"{1}\" \n单元格: {2}", data, gttype.Name, cell);
        }

        public static string GenMessage(string data, GTType gttype, int row, int col)
        {
            return GetMessage(data, gttype, Utils.GetCellName(row, col));
        }

        public ValueNotMatchTypeException(string data, GTType gttype, int row, int col) : base(GenMessage(data, gttype, row, col))
        {

        }

        public ValueNotMatchTypeException(string data, GTType gttype, string cell):base(GetMessage(data, gttype, cell))
        {

        }

    }
}
