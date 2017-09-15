namespace org.vr.rts.util
{

    public class RTSConverter
    {

        public const string TRUE = "true";
        public const string FALSE = "false";

        public static object getCompileValue(string src)
        {
            if (src == null)
                return null;
            int len = src.Length;
            char endC = len > 0 ? src[len - 1] : '\0';
            if (len >= 2 && src[0] == '\"' && endC == '\"')
                return src.Substring(1, len - 2);
            if (endC == 'l' || endC == 'L')
                return tryParseLong(src);
            bool hex = len > 2 && src[0] == '0' && (src[1] == 'x' || src[1] == 'X');
            if (!hex)
            {
                if (endC == 'f' || endC == 'F')
                    return tryParseFloat(src);
                if (endC == 'd' || endC == 'D')
                    return tryParseDouble(src);
                if (src.IndexOf('.') >= 0)
                    return tryParseFloat(src);
            }
            return tryParseInteger(src);
        }

        public static object tryParseInteger(string src)
        {
            if (src == null || src.Length == 0)
                return null;
            int n = 0;
            int off = 0;
            int end = src.Length;
            bool hex = src.Length > 2 && src[0] == '0' && (src[1] == 'x' || src[1] == 'X');
            if (end <= off)
                return null;
            if (hex)
            {
                off = 2;
                int d;
                for (int i = off; i < end; i++)
                {
                    char c = src[i];
                    if (c >= '0' && c <= '9')
                        d = (c - '0');
                    else if (c >= 'a' && c <= 'z')
                        d = (c - 'a' + 10);
                    else if (c >= 'A' && c <= 'Z')
                        d = (c - 'A' + 10);
                    else
                        return null;
                    n <<= 4;
                    n |= d;
                }
            }
            else
            {
                int d;
                for (int i = off; i < end; i++)
                {
                    char c = src[i];
                    if (c >= '0' && c <= '9')
                        d = (c - '0');
                    else
                        return null;
                    n *= 10;
                    n += d;
                }
            }
            return n;
        }

        public static object tryParseLong(string src)
        {
            if (src == null || src.Length == 0)
                return null;
            long n = 0;
            int off = 0;
            int end = src.Length;
            bool hex = src.Length > 2 && src[0] == '0' && (src[1] == 'x' || src[1] == 'X');
            char e = src[end - 1];
            if (e == 'l' || e == 'L')
                end--;
            if (end <= off)
                return null;
            if (hex)
            {
                off = 2;
                long d;
                for (int i = off; i < end; i++)
                {
                    char c = src[i];
                    if (c >= '0' && c <= '9')
                        d = (c - '0');
                    else if (c >= 'a' && c <= 'z')
                        d = (c - 'a' + 10);
                    else if (c >= 'A' && c <= 'Z')
                        d = (c - 'A' + 10);
                    else
                        return null;
                    n <<= 4;
                    n |= d;
                }
            }
            else
            {
                long d;
                for (int i = off; i < end; i++)
                {
                    char c = src[i];
                    if (c >= '0' && c <= '9')
                        d = (c - '0');
                    else
                        return null;
                    n *= 10;
                    n += d;
                }
            }
            return n;
        }

        public static object tryParseFloat(string src)
        {
            if (src == null || src.Length == 0)
                return null;
            float f = 0f;
            bool dot = false;
            float d = 0.1f;
            int off = 0;
            int end = src.Length;
            char e = src[end - 1];
            if (e == 'f' || e == 'F')
                end--;
            if (end <= off)
                return null;
            for (int i = off; i < end; i++)
            {
                char c = src[i];
                if (c == '.')
                {
                    if (dot)
                        return null;
                    dot = true;
                    continue;
                }
                if (c >= '0' && c <= '9')
                {
                    float t = (float)(c - '0');
                    if (dot)
                    {
                        t *= d;
                        f += t;
                        d *= 0.1f;
                    }
                    else
                    {
                        f *= 10f;
                        f += t;
                    }
                }
                else
                    return null;
            }
            return f;
        }

        public static object tryParseDouble(string src)
        {
            if (src == null || src.Length == 0)
                return null;
            double f = 0d;
            bool dot = false;
            float d = 0.1f;
            int off = 0;
            int end = src.Length;
            char e = src[end - 1];
            if (e == 'd' || e == 'D')
                end--;
            if (end <= off)
                return null;
            for (int i = off; i < end; i++)
            {
                char c = src[i];
                if (c == '.')
                {
                    if (dot)
                        return null;
                    dot = true;
                    continue;
                }
                if (c >= '0' && c <= '9')
                {
                    double t = (double)(c - '0');
                    if (dot)
                    {
                        t *= d;
                        f += t;
                        d *= 0.1f;
                    }
                    else
                    {
                        f *= 10f;
                        f += t;
                    }
                }
                else
                    return null;
            }
            return f;
        }

        public static object tryParseBool(string src)
        {
            if (TRUE.Equals(src))
                return true;
            else if (FALSE.Equals(src))
                return false;
            else
                return null;
        }

        public static char[] toHexChars(byte[] bytes, int len)
        {
            if (bytes == null)
                return null;
            int LEN = len < bytes.Length && len >= 0 ? len : bytes.Length;
            char[] chars = new char[LEN << 1];
            int v, c, j;
            for (int i = 0; i < LEN; i++)
            {
                v = bytes[i];
                c = (v >> 4) & 0xf;
                j = i << 1;
                chars[j] = (char)(c >= 10 ? (c - 10 + 'a') : (c + '0'));
                c = v & 0xf;
                chars[j + 1] = (char)(c >= 10 ? (c - 10 + 'a') : (c + '0'));
            }
            return chars;
        }

        public static string toHexString(byte[] bytes, int len, int sizePerLine)
        {
            if (bytes == null)
                return null;
            int LEN = len < bytes.Length && len >= 0 ? len : bytes.Length;
            int LLEN = sizePerLine << 1;
            int row = 0;
            if (LLEN > 0)
            {
                row = (LEN << 1) / LLEN;
                if ((LEN << 1) % LLEN != 0)
                    row++;
            }
            char[] chars = new char[(LEN << 1) + row - 1];
            int v, c, j;
            int r = 0;
            for (int i = 0; i < LEN; i++)
            {
                v = bytes[i];
                c = (v >> 4) & 0xf;
                j = (i << 1);
                chars[j + r] = (char)(c >= 10 ? (c - 10 + 'A') : (c + '0'));
                c = v & 0xf;
                chars[j + r + 1] = (char)(c >= 10 ? (c - 10 + 'A') : (c + '0'));
                if ((LLEN > 0) && ((j + 2) % LLEN == 0) && (i < LEN - 1))
                {
                    chars[j + r++ + 2] = '\n';
                }
            }
            return new string(chars);
        }
    }
}