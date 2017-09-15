namespace org.vr.rts.util
{
    public class RTSUtil
    {

        public static string keyOfFunc(string funcName, int argCount)
        {
            if (argCount >= 0)
            {
                return RTSUtil.linkString('-', funcName, argCount);
            }
            else
            {
                return funcName;
            }
        }

        public static string propertyName(IRTSDefine.Property property)
        {
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            if ((property & IRTSDefine.Property.CONST) != 0)
                buffer.Append("const ");
            if ((property & IRTSDefine.Property.GLOBAL) != 0)
                buffer.Append("static ");
            string ret = buffer.ToString();
            return ret;
        }

        public static bool isString(string text)
        {
            if (text == null)
            {
                return false;
            }
            int len = text.Length;
            if (len < 2)
            {
                return false;
            }
            return text[0] == '\"' && text[len - 1] == '\"';
        }

        public static bool isGoodName(string text)
        {
            if (text == null || text.Length == 0)
            {
                return false;
            }
            char c0 = text[0];
            if (RTSCfg.SUPPORT_CHINESE_NAME)
                return c0 < '0' || c0 > '9';
            else
                return (c0 == '_') || (c0 <= 'z' && c0 >= 'a')
                    || (c0 <= 'Z' && c0 >= 'A');
        }

        public static string linkString(char linker, params object[] values)
        {
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            if (values == null)
                return "";
            bool sp = false;
            for (int i = 0; i < values.Length; i++)
            {
                if (null != values[i])
                {
                    if (sp)
                    {
                        buffer.Append(linker);
                        sp = false;
                    }
                    buffer.Append(values[i]);
                    sp = linker != 0;
                }
            }
            string ret = buffer.ToString();
            return ret;
        }

        public static string linkString(string linker, bool skipNull,
                params object[] values)
        {
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            if (values == null)
                return "";
            bool first = true;
            for (int i = 0; i < values.Length; i++)
            {
                if (skipNull && values[i] == null)
                    continue;
                if (!first && linker != null)
                {
                    buffer.Append(linker);
                }
                else
                {
                    first = false;
                }
                if (null != values[i])
                    buffer.Append(values[i]);
            }
            string ret = buffer.ToString();
            return ret;
        }

        public static bool isNullOrEmpty(string str)
        {
            return str == null || str.Length == 0;
        }

        public static int max(int a, int b)
        {
            return a > b ? a : b;
        }

        public static int min(int a, int b)
        {
            return a > b ? b : a;
        }

        public static int clamp(int value, int left, int right)
        {
            if (left == right)
            {
                return left;
            }
            else if (left < right)
            {
                int ret = max(value, left);
                return min(ret, right);
            }
            else
            {
                int ret = max(value, right);
                return min(ret, left);
            }
        }

        public static string getEnumDescript(System.Type enumType, int value, string linker = "|")
        {
            if (enumType == null || !enumType.IsEnum)
                return "";
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            System.Array names = System.Enum.GetNames(enumType);
            System.Array values = System.Enum.GetValues(enumType);
            bool first = true;
            for (int i = 0; i < names.Length; i++)
            {
                try
                {
                    int n = (int)values.GetValue(i);
                    if ((n & value) != 0)
                    {
                        if (!first)
                            buffer.Append(linker);
                        else
                            first = false;
                        buffer.Append(names.GetValue(i));
                    }
                }
                finally
                {
                }
            }
            string ret = buffer.ToString();
            return ret;
        }

    }
}