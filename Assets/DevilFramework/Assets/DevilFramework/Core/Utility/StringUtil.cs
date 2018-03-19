using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Security.Cryptography;

namespace Devil.Utility
{
    public class StringUtil
    {
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

        public static string ToMD5(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            try
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
            catch (Exception e)
            {
                Debug.LogException( e);
                return null;
            }
        }

        public static void ReadDictionary(string str, Dictionary<string,string> dic)
        {
            string[] lines = str.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
                {
                    continue;
                }

                string key = null;
                int idx = line.IndexOf('=');
                if (idx >= 0)
                {
                    try
                    {
                        key = line.Substring(0, idx).Trim();
                        dic[key]= line.Substring(idx + 1);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException( ex);
                    }
                }
                else
                {
                    Debug.LogError("Parse Error: " + line);
                }
            }
        }

        public static string ParseDictionary(Dictionary<string, string> dic)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string key in dic.Keys)
            {
                builder.Append(key).Append('=').Append(dic[key]).Append('\n');
            }
            return builder.ToString();
        }

        public const string EMPTY = "\"\"";
        public const string HEX_CHARS = "0123456789ABCDEFabcdef";

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

        public static string WrapLinePerChar(string text, int charCount)
        {
            string l = "\n";
            StringBuilder str = new StringBuilder(text);
            for (int i = charCount; i < str.Length; i += charCount)
            {
                str.Insert(i++, l);
            }
            return str.ToString();
        }

        public static string LinkString(bool skipEmpty, string linker, params object[] values)
        {
            if (values == null)
                return "";
            bool first = true;
            StringBuilder str = new StringBuilder();
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

        public static string LinkArray<T>(bool skipEmpty, string linker, T[] values)
        {
            if (values == null)
                return "";
            bool first = true;
            StringBuilder str = new StringBuilder();
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

        public static bool ParseArray(string s, out int[] result)
        {
            if (string.IsNullOrEmpty(s))
            {
                result = null;
                return false;
            }
            string[] ss = s.Trim().Split(',');
            result = new int[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                if (!int.TryParse(ss[i].Trim(), out result[i]))
                    return false;
            }
            return true;
        }

        public static bool ParseFloatArray(string s, out float[] result)
        {
            if (string.IsNullOrEmpty(s))
            {
                result = null;
                return false;
            }
            string[] ss = s.Trim().Split(',');
            result = new float[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                if (!float.TryParse(ss[i].Trim(), out result[i]))
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
            int n = t.Length;
            t = t.Substring(1, n - 2);
            string[] ss = t.Split(',');
            Vector3 v = Vector3.zero;
            if (ss.Length > 0)
                v.x = float.Parse(ss[0]);
            if (ss.Length > 1)
                v.y = float.Parse(ss[1]);
            if (ss.Length > 2)
                v.z = float.Parse(ss[2]);
            return v;
        }
        public static Vector2 ParseVector2(string s)
        {
            string t = s.Trim();
            int n = t.Length;
            t = t.Substring(1, n - 2);
            string[] ss = t.Split(',');
            Vector2 v = Vector2.zero;
            if (ss.Length > 0)
                v.x = float.Parse(ss[0]);
            if (ss.Length > 1)
                v.y = float.Parse(ss[1]);
            return v;
        }
        public static Vector4 ParseVector4(string s)
        {
            string t = s.Trim();
            int n = t.Length;
            t = t.Substring(1, n - 2);
            string[] ss = t.Split(',');
            Vector4 v = Vector4.zero;
            if (ss.Length > 0)
                v.x = float.Parse(ss[0]);
            if (ss.Length > 1)
                v.y = float.Parse(ss[1]);
            if (ss.Length > 2)
                v.z = float.Parse(ss[2]);
            if (ss.Length > 3)
                v.w = float.Parse(ss[3]);
            return v;
        }
        
        public static string KV2Json(bool autoNextLine, params object[] keyValues)
        {
            StringBuilder str = new StringBuilder();
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
            return str.ToString();
        }

        public static string Dictionary2Json<K, T>(Dictionary<K, T> dic, bool autoNextLine = false)
        {
            StringBuilder str = new StringBuilder();
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
            return str.ToString();
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
            string s = "";
            if (num < 20)
            {
                int n = num / 10;
                if (n > 0)
                    s += number[10];
                n = num % 10;
                if (num < 10 || n > 0)
                    s += number[n];
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
                        s = number[n] + unit + s;
                    }
                    else if (num > 0 && !zero)
                    {
                        s = number[0] + s;
                    }
                    if (num > 0)
                        unit = number[++uindex].ToString();
                    zero = n == 0;
                }
            }
            return s;
        }

    }
    
}