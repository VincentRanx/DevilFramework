using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Collections;

namespace Devil.Utility
{
    public static class StringUtil
    {
        static object mLock = new object();
        public const int UPER_2_LOWER = 'a' - 'A';

        static ObjectBuffer<StringBuilder> mBuilders = new ObjectBuffer<StringBuilder>(5, () => new StringBuilder(128));
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
            StringBuilder builder = mBuilders.GetAnyTarget();
            builder.Remove(0, builder.Length);
            return builder;
        }

        public static StringBuilder GetBuilder(string data)
        {
            StringBuilder builder = mBuilders.GetAnyTarget();
            builder.Remove(0, builder.Length);
            builder.Append(data);
            return builder;
        }

        public static string ReleaseBuilder(StringBuilder builder)
        {
            string ret = builder.ToString();
            mBuilders.SaveBuffer(builder);
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
            for(int i=str.Length - 1; i >= 0; i--)
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
                try
                {
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
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                return ret;
            }
        }

        public static string Hash(string str, int minsize = 6)
        {
            string value = ToHash(str).ToString("x");
            int len = value.Length;
            if(len < minsize)
            {
                StringBuilder buf = GetBuilder();
                for(int i= minsize - len; i > 0; i--)
                {
                    buf.Append('0');
                }
                buf.Append(value);
                value = ReleaseBuilder(buf);
            }
            return value;
        }

        public static int ToHash(string str)
        {
            int hash = 0;
            int len = str == null ? 0 : str.Length;
            for(int i = 0; i < len; i++)
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
                hash = hash * 31 + GetCharIgnoreCase(str, i) ;
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
            for(int i = 0; i < len; i++)
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
            for(int i = len - 1; i >= 0; i--)
            {
                if (GetCharIgnoreCase(str, off + i) != GetCharIgnoreCase(pattern, i))
                    return false;
            }
            return true;
        }

        public static string Concat(params object[] strs)
        {
            if (strs == null || strs.Length == 0)
                return "";
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

        public static string Gather(IEnumerable iter, string seperator = ",")
        {
            if (iter == null)
                return "";
            StringBuilder buf = GetBuilder();
            if (string.IsNullOrEmpty(seperator))
            {
                foreach (var v in iter)
                {
                    buf.Append(v);
                }
            }
            else
            {
                bool first = true;
                foreach (var v in iter)
                {
                    if (first)
                        first = false;
                    else
                        buf.Append(seperator);
                    buf.Append(v);
                }
            }
            return ReleaseBuilder(buf);
        }

        public static string FormatTime(float seconds)
        {
            string s = "{0:00}:{1:00}:{2:00}";
            int sec = Mathf.CeilToInt(seconds);
            int minute = sec / 60;
            sec = sec % 60;
            int hour = minute / 60;
            minute = minute % 60;
            
            return string.Format(s, hour, minute, sec);
        }
        
        public static string Replace(string format, params object[] matches)
        {
            StringBuilder buf = GetBuilder(format);
            int len = matches == null ? 0 : matches.Length;
            for (int i = 0; i < len; i += 2)
            {
                var key = matches[i] == null ? null : matches[i].ToString();
                if (string.IsNullOrEmpty(key))
                    continue;
                var v = matches[i + 1] == null ? "" : matches[i + 1].ToString();
                buf.Replace(key, v);
            }
            return ReleaseBuilder(buf);
        }

        public static string WrapString(string text, int length, string replaceStr)
        {
            int len = text.Length;
            if (len <= length)
                return text;
            int n = replaceStr.Length;
            string s = text.Substring(0, Mathf.Min(length, len) - n);
            s += replaceStr;
            return s;
        }

        public static string WrapRichText(string text, int length, string replaceStr)
        {
            MatchCollection mats = Regex.Matches(text, @"<quad=[a-zA-Z0-9_]+>|<size=\d+>|<color=[\w\W]+>|</color>|</size>|</?b>|</?i>");
            StringBuilder buf = GetBuilder();
            int num = 0;
            int p = 0;
            Match m = mats == null && p < mats.Count ? mats[p++] : null;
            int cp = 0;
            while(num < length && cp < text.Length)
            {
                if(m == null || m.Index > cp)
                {
                    buf.Append(text[cp++]);
                    num++;
                }
                else
                {
                    for (int i = m.Index; i < m.Length + m.Index; i++)
                    {
                        buf.Append(text[i]);
                    }
                    cp = m.Length + m.Index;
                    m = p < mats.Count ? mats[p++] : null;
                }
            }
            if (cp < text.Length)
                buf.Append(replaceStr);
            return ReleaseBuilder(buf);
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
            List<string> tmp2 = new List<string>() ;
            for(int i = 0; i < matrix.Length; i++)
            {
                tmp2.Clear();
                if (!ParseArray(tmp[i], tmp2, bracketl, bracketr, split))
                    return null;
                var arr = new string[tmp2.Count];
                for(int j = 0; j < arr.Length; j++)
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

        /// <summary>
        /// 将字符串(x,y,z)转换为 Vector3
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Vector3 ParseVector3(string s)
        {
            string t = s.Trim();
            float[] arr;
            if (!ParseFloatArray(t, out arr, '(', ')', ','))
                return default(Vector3);
            Vector3 v = Vector3.zero;
            if (arr.Length > 0)
                v.x = arr[0];
            if (arr.Length > 1)
                v.y = arr[1];
            if (arr.Length > 2)
                v.z = arr[2];
            return v;
        }
        public static Vector2 ParseVector2(string s)
        {
            string t = s.Trim();
            float[] arr;
            if (!ParseFloatArray(t, out arr, '(', ')', ','))
                return default(Vector2);
            Vector2 v = Vector2.zero;
            if (arr.Length > 0)
                v.x = arr[0];
            if (arr.Length > 1)
                v.y = arr[1];
            return v;
        }
        public static Vector4 ParseVector4(string s)
        {
            string t = s.Trim();
            float[] arr;
            if (!ParseFloatArray(t, out arr, '(', ')', ','))
                return default(Vector4);
            Vector4 v = Vector4.zero;
            if (arr.Length > 0)
                v.x = arr[0];
            if (arr.Length > 1)
                v.y = arr[1];
            if (arr.Length > 2)
                v.z = arr[2];
            if (arr.Length > 3)
                v.w = arr[4];
            return v;
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

        public static string ChineseNumber(int num, bool upcase)
        {
            num = Mathf.Clamp(num, 0, 99999);
            string number;
            if (upcase)
                number = "零壹贰叁肆五陆柒捌玖拾佰仟萬";
            else
                number = "〇一二三四五六七八九十百千万";
            if (num == 0)
                return number[0].ToString();
            StringBuilder s = GetBuilder();
            if (num < 20)
            {
                int n = num / 10;
                if (n > 0)
                    s.Append(number[10]);
                n = num % 10;
                if (num < 10 || n > 0)
                    s.Append(number[n]);
            }
            else
            {
                bool zero = true;
                string unit = "";
                int uindex = 9;
                while (num > 0)
                {
                    int n = num % 10;
                    num /= 10;
                    if (n > 0)
                    {
                        s.Insert(0, unit).Insert(0, number[n]);
                    }
                    else if (num > 0 && !zero)
                    {
                        s.Insert(0, number[0]);
                    }
                    if (num > 0)
                        unit = number[++uindex].ToString();
                    zero = n == 0;
                }
            }
            return ReleaseBuilder(s);
        }

    }
    
}