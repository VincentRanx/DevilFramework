using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace TableCore
{
    public static class StringUtil
    {
        static object mLock = new object();
        public const int UPER_2_LOWER = 'a' - 'A';

        //public static ObjectBuffer<StringBuilder> BuilderBuffer { get { return mBuilders; } }

        static List<string> mTempList = new List<string>(32);
        static StringBuilder mTempBuffer = new StringBuilder(512);

        static StringBuilder UseBuffer()
        {
            mTempBuffer.Remove(0, mTempBuffer.Length);
            return mTempBuffer;
        }

        static StringBuilder UseBuffer(string data)
        {
            mTempBuffer.Remove(0, mTempBuffer.Length);
            mTempBuffer.Append(data);
            return mTempBuffer;
        }

        public static StringBuilder GetBuilder()
        {
            StringBuilder builder = new StringBuilder();
            return builder;
        }

        public static StringBuilder GetBuilder(string data)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(data);
            return builder;
        }

        public static string ReleaseBuilder(StringBuilder builder)
        {
            string ret = builder.ToString();
            return ret;
        }

        //public const string EMPTY = "\"\"";
        //public const string HEX_CHARS = "0123456789ABCDEFabcdef";
        private const string INVALID_CHAR_SET = ",<.>/?;:'\"[{]}\\|`~!@#$%^&*()-=+ \r\n\t";
        // to全角
        public static string ToSBC(string text)
        {
            char[] c = text.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (INVALID_CHAR_SET.IndexOf(c[i]) > -1)
                {
                    if (32 == c[i])
                    {
                        c[i] = (char)12288;
                    }
                    else if (c[i] < 127)
                    {
                        c[i] = (char)(c[i] + 65248);
                    }
                }
            }

            return new string(c);
        }

        public static bool ContainInvalidChar(string text)
        {
            char[] c = text.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (INVALID_CHAR_SET.IndexOf(c[i]) > -1)
                {
                    return true;
                }
            }

            return false;
        }

        public static int LengthOfUTF8(string str)
        {
            int length = 0;
            char[] characters = str.ToCharArray();
            foreach (char c in characters)
            {
                int cInt = (int)c;
                if (cInt < 256)
                {
                    length++;
                }
                else
                {
                    length += 2;
                }
            }
            return length;
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

        public static int GetCharIndex(string str, char w)
        {
            if (str == null)
                return -1;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                char v = str[i];
                if (v == w)
                    return i;
            }
            return -1;
        }

        public static string GetHex(byte[] bytes)
        {
            lock (mLock)
            {
                StringBuilder builder = UseBuffer();
                for (int i = 0; i < bytes.Length; i++)
                {
                    uint b = bytes[i];
                    if (i > 0 && i % 2 == 0)
                        builder.Append('-');
                    if (b < 0x10)
                        builder.Append('0');
                    builder.Append(b.ToString("x"));
                }
                return builder.ToString();
            }
        }

        public static string ToMD5(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            lock (mLock)
            {
                StringBuilder buffer = UseBuffer(str);
                string ret = null;
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                buffer.Remove(0, buffer.Length);
                buffer.Append(BitConverter.ToString(hashBytes));
                int delta = 'A' - 'a';
                for (int i = buffer.Length - 1; i >= 0; i--)
                {
                    char c = buffer[i];
                    if (c == '-')
                        buffer.Remove(i, 1);
                    else if (c >= 'a' && c <= 'a')
                        buffer[i] = (char)(c + delta);
                }
                ret = buffer.ToString();
                return ret;
            }
        }

        public static int ToHash(string str)
        {
            int hash = 0;
            int len = str == null ? 0 : str.Length;
            for (int i = 0; i < len; i++)
            {
                hash = hash * 31 + str[i];
            }
            return hash;
        }

        public static int IgnoreCaseToHash(string str)
        {
            int hash = 0;
            int len = str == null ? 0 : str.Length;
            for (int i = 0; i < len; i++)
            {
                hash = hash * 31 + GetCharIgnoreCase(str, i);
            }
            return hash;
        }

        public static char GetCharIgnoreCase(string str, int index)
        {
            char c = str[index];
            if (c >= 'A' && c <= 'Z')
                return (char)(c + UPER_2_LOWER);
            else
                return c;
        }

        public static bool StartWithIgnoreCase(string str, string pattern)
        {
            int len = pattern.Length;
            if (str.Length < len)
                return false;
            for (int i = 0; i < len; i++)
            {
                if (GetCharIgnoreCase(str, i) != GetCharIgnoreCase(pattern, i))
                    return false;
            }
            return true;
        }

        public static bool EndWithIgnoreCase(this string str, string pattern)
        {
            int len = pattern.Length;
            int off = str.Length - len;
            if (off < 0)
                return false;
            for (int i = len - 1; i >= 0; i--)
            {
                if (GetCharIgnoreCase(str, off + i) != GetCharIgnoreCase(pattern, i))
                    return false;
            }
            return true;
        }

        public static string Concat(params string[] strs)
        {
            lock (mLock)
            {
                StringBuilder builder = UseBuffer();
                builder.Remove(0, builder.Length);
                int len = strs == null ? 0 : strs.Length;
                for (int i = 0; i < len; i++)
                {
                    builder.Append(strs[i]);
                }
                return builder.ToString();
            }
        }
        
        public static string Replace(string format, params string[] matches)
        {
            StringBuilder buf = GetBuilder(format);
            for (int i = 0; i < matches.Length; i += 2)
            {
                buf.Replace(matches[i], matches[i + 1]);
            }
            return ReleaseBuilder(buf);
        }


        public static string WrapString(string text, int length, string replaceStr)
        {
            int len = text.Length;
            if (len <= length)
                return text;
            int n = replaceStr.Length;
            string s = text.Substring(0, Math.Min(length, len) - n);
            s += replaceStr;
            return s;
        }
        
        public static string WrapLinePerChar(string text, int charCount)
        {
            lock (mLock)
            {
                char l = '\n';
                StringBuilder str = UseBuffer(text);
                for (int i = charCount; i < str.Length; i += charCount)
                {
                    str.Insert(i++, l);
                }
                return str.ToString();
            }
        }

        public static string LinkString(bool skipEmpty, string linker, params object[] values)
        {
            if (values == null)
                return "";
            lock (mLock)
            {
                bool first = true;
                StringBuilder str = UseBuffer();
                for (int i = 0; i < values.Length; i++)
                {
                    if (skipEmpty && (values[i] == null))
                        continue;
                    if (!first)
                        str.Append(linker);
                    first = false;
                    str.Append(values[i] == null ? "" : values[i].ToString());
                }
                return str.ToString();
            }
        }

        public static string LinkArray<T>(bool skipEmpty, string linker, IList<T> values)
        {
            if (values == null)
                return "";
            lock (mLock)
            {
                bool first = true;
                StringBuilder str = UseBuffer();
                for (int i = 0; i < values.Count; i++)
                {
                    if (skipEmpty && (values[i] == null))
                        continue;
                    if (!first)
                        str.Append(linker);
                    first = false;
                    str.Append(values[i] == null ? "" : values[i].ToString());
                }
                return str.ToString();
            }
        }

        public static bool ParseArray(string str, ICollection<string> arr, char bracketl = '[', char bracketr = ']', char split = ',')
        {
            lock (mLock)
            {
                int p = 0;
                int offset = 0;
                StringBuilder buf = UseBuffer(str);
                if (bracketl == '\0' && bracketr == '\0')
                {
                    for (int i = 0; i < buf.Length; i++)
                    {
                        char c = buf[i];
                        if (c == split)
                        {
                            arr.Add(buf.ToString(offset, i - offset));
                            offset = i + 1;
                        }
                    }
                    if (offset < buf.Length)
                        arr.Add(buf.ToString(offset, buf.Length - offset));
                    return true;
                }
                for (int i = 0; i < buf.Length; i++)
                {
                    char c = buf[i];
                    if (c == bracketl)
                    {
                        p++;
                        if (p == 1)
                            offset = i + 1;
                    }
                    else if (c == bracketr)
                    {
                        p--;
                        if (p == 0)
                        {
                            if (offset < i)
                                arr.Add(buf.ToString(offset, i - offset));
                            offset = i + 1;
                        }
                    }
                    else if (c == split && p == 1)
                    {
                        arr.Add(buf.ToString(offset, i - offset));
                        offset = i + 1;
                    }
                    else if (p < 1)
                    {
                        return false;
                    }
                }
                return p == 0;
            }
        }

        public static string[][] ParseMatrix(string s, char bracketl = '[', char bracketr = ']', char split = ',')
        {
            if (string.IsNullOrEmpty(s))
                return new string[0][];
            List<string> tmp = new List<string>();
            string[][] matrix;
            if (!ParseArray(s, tmp, bracketl, bracketr, split))
                return null;
            matrix = new string[tmp.Count][];
            List<string> tmp2 = new List<string>();
            for (int i = 0; i < matrix.Length; i++)
            {
                tmp2.Clear();
                if (!ParseArray(tmp[i], tmp2, bracketl, bracketr, split))
                    return null;
                var arr = new string[tmp2.Count];
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = tmp2[j];
                }
                matrix[i] = arr;
            }
            return matrix;
        }

        public static bool ParseArray(string s, out int[] result, char bracketl = '[', char bracketr = ']', char split = ',')
        {
            List<string> arr = new List<string>(5);
            arr.Clear();
            if (!ParseArray(s, arr, bracketl, bracketr, split))
            {
                result = null;
                return false;
            }
            result = new int[arr.Count];
            for (int i = 0; i < result.Length; i++)
            {
                if (!int.TryParse(arr[i].Trim(), out result[i]))
                    return false;
            }
            return true;
        }

        public static bool ParseFloatArray(string s, out float[] result, char bracketl = '[', char bracketr = ']', char split = ',')
        {
            List<string> arr = new List<string>(5);
            if (!ParseArray(s, arr, bracketl, bracketr, split))
            {
                result = null;
                return false;
            }
            result = new float[arr.Count];
            for (int i = 0; i < result.Length; i++)
            {
                if (!float.TryParse(arr[i].Trim(), out result[i]))
                    return false;
            }
            return true;
        }
        
        public static string KV2Json(bool autoNextLine, params object[] keyValues)
        {
            StringBuilder str = GetBuilder();
            str.Append('{');
            bool first = true;
            for (int i = 0; i < keyValues.Length - 1; i += 2)
            {
                object key = keyValues[i];
                if (key == null)
                    continue;
                if (!first)
                {
                    str.Append(',');
                    if (autoNextLine)
                        str.Append('\n');
                }
                else
                    first = false;
                object v = keyValues[i + 1];
                str.Append('\"').Append(key.ToString()).Append("\":\"").Append(v == null ? "" : v.ToString()).Append("\"");
            }
            str.Append('}');
            return ReleaseBuilder(str);
        }

        public static string Dictionary2Json<K, T>(Dictionary<K, T> dic, bool autoNextLine = false)
        {
            StringBuilder str = GetBuilder();
            str.Append('{');
            bool first = true;
            foreach (K key in dic.Keys)
            {
                if (!first)
                {
                    str.Append(',');
                    if (autoNextLine)
                        str.Append("\n");
                }
                else
                {
                    first = false;
                }
                str.Append("\"").Append(key.ToString()).Append("\":\"").Append(dic[key].ToString()).Append("\"");
            }
            str.Append("}");
            return ReleaseBuilder(str);
        }
        
    }
}