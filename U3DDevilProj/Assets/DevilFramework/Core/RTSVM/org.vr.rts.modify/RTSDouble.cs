namespace org.vr.rts.modify
{

    public class RTSDouble : RTSObjectType
    {

        private static RTSDouble _type = new RTSDouble();
        public static RTSDouble TYPE { get { return _type; } }

        public static double valueOf(object obj)
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
                return ((double)obj);
            else if (obj is byte)
                return ((byte)obj);
            else if (obj is char)
                return ((char)obj);
            else if (obj is bool)
                return ((bool)obj) ? -1d : 0d;
            else
            {
                double v;
                if (double.TryParse(obj.ToString(), out v))
                    return v;
                else
                    return obj.GetHashCode();
            }
        }

        private RTSDouble() : base()
        {

        }

        override public object add(object a, object b)
        {
            return valueOf(a) + valueOf(b);
        }

        override public object sub(object a, object b)
        {
            return valueOf(a) - valueOf(b);
        }

        override public object mul(object a, object b)
        {
            return valueOf(a) * valueOf(b);
        }

        override public object div(object a, object b)
        {
            return valueOf(a) / valueOf(b);
        }

        override public object mod(object a, object b)
        {
            return valueOf(a) % valueOf(b);
        }

        override public object and(object a, object b)
        {
            return RTSBool.TYPE.and(a, b);
        }

        override public object or(object a, object b)
        {
            return RTSBool.TYPE.or(a, b);
        }

        override public object xor(object a, object b)
        {
            return RTSBool.TYPE.xor(a, b);
        }

        override public object nigative(object target)
        {
            return RTSBool.TYPE.nigative(target);
        }

        override public object castValue(object target)
        {
            return valueOf(target);
        }

        override public int rtsCompare(object a, object b)
        {
            double l = valueOf(a);
            double r = valueOf(b);
            return l < r ? -1 : (l > r ? 1 : 0);
        }

        override public string typeName()
        {
            return "RTSDouble";
        }

    }
}