namespace org.vr.rts.modify
{

    public class RTSInteger : IRTSType
    {

        private static RTSInteger _type = new RTSInteger();
        public static RTSInteger TYPE { get { return _type; } }

        public static int valueOf(object obj)
        {
            if (obj == null)
                return 0;
            else if (obj is int)
                return ((int)obj);
            else if (obj is float)
                return (int)((float)obj);
            else if (obj is long)
                return (int)((long)obj);
            else if (obj is double)
                return (int)((double)obj);
            else if (obj is byte)
                return ((byte)obj);
            else if (obj is char)
                return ((char)obj);
            else if (obj is bool)
                return ((bool)obj) ? -1 : 0;
            else
                return obj.GetHashCode();
        }

        private RTSInteger()
        {

        }

        public object add(object a, object b)
        {
            return valueOf(a) + valueOf(b);
        }

        public object sub(object a, object b)
        {
            return valueOf(a) - valueOf(b);
        }

        public object mul(object a, object b)
        {
            return valueOf(a) * valueOf(b);
        }

        public object div(object a, object b)
        {
            return valueOf(a) / valueOf(b);
        }

        public object mod(object a, object b)
        {
            return valueOf(a) % valueOf(b);
        }

        public object and(object a, object b)
        {
            return valueOf(a) & valueOf(b);
        }

        public object or(object a, object b)
        {
            return valueOf(a) | valueOf(b);
        }

        public object xor(object a, object b)
        {
            return valueOf(a) ^ valueOf(b);
        }

        public object nigative(object target)
        {
            return ~valueOf(target);
        }

        public object castValue(object target)
        {
            return valueOf(target);
        }

        public int rtsCompare(object a, object b)
        {
            return valueOf(a) - valueOf(b);
        }

        public string typeName()
        {
            return "RTSInteger";
        }

    }
}