using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TableCore
{
    public delegate bool Filter<T>(T target);

    public static class Utils
    {
        static StringBuilder mBuilder = new StringBuilder();

        public static bool EqualAsString(object a, object b)
        {
            string sa = a == null ? "" : a.ToString();
            string sb = b == null ? "" : b.ToString();
            return sa == sb;
        }

        public static bool EqualIgnoreCase(string a, string b)
        {
            if ((a == null) ^ (b == null))
                return false;
            return a.ToLower() == b.ToLower();
        }
        
        public static int BinsearchIndex(IList<IIdentified> list, int id, int start, int end)
        {
            int l = 0;
            int r = end - 1;
            int c;
            int cs;
            while (l <= r)
            {
                c = (l + r) >> 1;
                cs = list[c].Id - id;
                if (cs == 0)
                    return c;
                else if (cs > 0)
                    r = c - 1;
                else
                    l = c + 1;
            }
            return -1;
        }

        public static int GetCharIndexIgnoreCase(string str, char lowerChar)
        {
            if (str == null)
                return -1;
            int delta = 'a' - 'A';
            for (int i = str.Length - 1; i >= 0; i--)
            {
                char v = str[i];
                if (v >= 'A' && v <= 'Z')
                    v = (char)(v + delta);
                if (v == lowerChar)
                    return i;
            }
            return -1;
        }

        public static string GetCellName(int row, int col)
        {
            mBuilder.Remove(0, mBuilder.Length);
            if (col < 0)
            {
                mBuilder.Append("[ROW]");
            }
            else
            {
                int n = col % 26;
                int pre = col / 26;
                mBuilder.Append((char)(n + 'A'));
                while (pre != 0)
                {
                    n = pre % 26;
                    pre = pre / 26;
                    mBuilder.Insert(0, (char)(n + 'A'));
                }
            }
            mBuilder.Append(row + 1);
            return mBuilder.ToString();
        }

        public static bool GetCell(string cellName, out int row, out int col)
        {
            mBuilder.Remove(0, mBuilder.Length);
            int r = 0;
            int c = 0;
            mBuilder.Append(cellName);
            int p = 0;
            while (p < mBuilder.Length)
            {
                char d = mBuilder[p++];
                if (d >= 'a' && d <= 'z' || d >= 'A' && d <= 'Z')
                {
                    c *= 26;
                    c += (int)d;
                }
                else
                {
                    string str = mBuilder.ToString(p - 1, mBuilder.Length - p + 1);
                    if(int.TryParse(str, out r))
                    {
                        row = r - 1;
                        col = c;
                        return true;
                    }
                    break;
                }
            }
            row = 0;
            col = 0;
            return false;
        }

        public static string GetCell(DataTable table, int row, int col)
        {
            object o = table.Rows[row][col];
            return o == null ? "" : o.ToString();
        }

        public static bool FindCell(Filter<string> cellFileter, DataTable table, out int row, out int col)
        {
            if (table == null || cellFileter == null)
            {
                row = -1;
                col = -1;
                return false;
            }
            int rows = table.Rows.Count;
            int cols = table.Columns.Count;
            for (int i = 0; i < rows; i++)
            {
                var rowdata = table.Rows[i];
                for (int j = 0; j < cols; j++)
                {
                    object o = rowdata[j];
                    if (cellFileter(o == null ? null : o.ToString()))
                    {
                        row = i;
                        col = j;
                        return true;
                    }
                }
            }
            row = -1;
            col = -1;
            return false;
        }
        
        public static string GetString(object data)
        {
            return data == null ? "" : data.ToString();
        }

        public static StringBuilder AppendRepeat(this StringBuilder builder, string str, int num)
        {
            for(int i = 0; i < num; i++)
            {
                builder.Append(str);
            }
            return builder;
        }

        public static StringBuilder AppendWithTab(this StringBuilder builder, string str, int tabNum)
        {
            AppendRepeat(builder, "\t", tabNum);
            builder.Append(str);
            return builder;
        }

        public static bool IsAbsolutePath(string path)
        {
            return path != null && Regex.IsMatch(path, @"^[c-zC-Z]:\[\w\W]+");
        }

        public static string GetRelativePath(string relativePath)
        {

            string path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.Combine(path, relativePath);
            return path;
        }

        public static string ReadFile(string fileName)
        {
            if (File.Exists(fileName))
                return File.ReadAllText(fileName);
            else
                return "";
        }

        public static void WriteFile(string fileName, string data)
        {
            if(!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(data))
                File.WriteAllText(fileName, data);
        }

        public static string ReadRelativeFile(string relativePath)
        {
            return ReadFile(GetRelativePath(relativePath));
        }

        public static void WriteRelativeFile(string relativePath, string data)
        {
            WriteFile(GetRelativePath(relativePath), data);
        }

        public static void Sort<T>(this IList<T> array, Comparison<T> compare)
        {
            for (int i = 0; i < array.Count; i++)
            {
                for (int j = i + 1; j < array.Count; j++)
                {
                    T a = array[i];
                    T b = array[j];
                    if (compare(a, b) > 0)
                    {
                        array[i] = b;
                        array[j] = a;
                    }
                }
            }
        }

        public static object NewInstance(string typeName)
        {
            try
            {
                Type tp = Type.GetType(typeName);
                if (tp != null)
                {
                    return Activator.CreateInstance(tp);
                    //ConstructorInfo cons = tp.GetConstructor(new Type[0]);
                    //Formater = cons.Invoke(null) as IDataFormater;
                    //Formater.Init(element);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
