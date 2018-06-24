namespace org.vr.rts.modify
{

    public class RTSFloat : IRTSType
    {

        private static RTSFloat _type = new RTSFloat();
        public static RTSFloat TYPE { get { return _type; } }

        public static float valueOf(object obj)
        {
            if (obj == null)
                return 0;
            else if (obj is int)
                return ((int)obj);
            else if (obj is float)
                return ((float)obj);
            else if (obj is long)
                return ((long)obj);
            else if (obj is double)
                return (float)((double)obj);
            else if (obj is byte)
                return ((byte)obj);
            else if (obj is char)
                return ((char)obj);
            else if (obj is bool)
                return ((bool)obj) ? -1f : 0f;
            else
                return obj.GetHashCode();
        }

        private RTSFloat()
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
            return RTSBool.TYPE.and(a, b);
        }

        public object or(object a, object b)
        {
            return RTSBool.TYPE.or(a, b);
        }

        public object xor(object a, object b)
        {
            return RTSBool.TYPE.xor(a, b);
        }

        public object nigative(object target)
        {
            return RTSBool.TYPE.nigative(target);
        }

        public object castValue(object target)
        {
            return valueOf(target);
        }

        public int rtsCompare(object a, object b)
        {
            float l = valueOf(a);
            float r = valueOf(b);
            return l < r ? -1 : (l > r ? 1 : 0);
        }

        public string typeName()
        {
            return "RTSFloat";
        }

    }
}