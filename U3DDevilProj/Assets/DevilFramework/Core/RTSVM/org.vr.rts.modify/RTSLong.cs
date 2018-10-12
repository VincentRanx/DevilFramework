namespace org.vr.rts.modify
{

    public class RTSLong : RTSObjectType
    {


        public static RTSLong _type = new RTSLong();
        public static RTSLong TYPE { get { return _type; } }

        public static long valueOf(object obj)
        {
            if (obj == null)
                return 0;
            else if (obj is int)
                return ((int)obj);
            else if (obj is float)
                return (long)((float)obj);
            else if (obj is long)
                return ((long)obj);
            else if (obj is double)
                return (long)((double)obj);
            else if (obj is byte)
                return ((byte)obj);
            else if (obj is char)
                return ((char)obj);
            else if (obj is bool)
                return ((bool)obj) ? -1 : 0;
            else
            {
                long l;
                if (long.TryParse(obj.ToString(), out l))
                    return l;
                else
                    return obj.GetHashCode();
            }
        }

        private RTSLong() : base()
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
            return valueOf(a) & valueOf(b);
        }

        override public object or(object a, object b)
        {
            return valueOf(a) | valueOf(b);
        }

        override public object xor(object a, object b)
        {
            return valueOf(a) ^ valueOf(b);
        }

        override public object nigative(object target)
        {
            return ~valueOf(target);
        }

        override public object castValue(object target)
        {
            return valueOf(target);
        }

        override public int rtsCompare(object a, object b)
        {
            long l = valueOf(a);
            long r = valueOf(b);
            return l < r ? -1 : (l > r ? 1 : 0);
        }

        override public string typeName()
        {
            return "RTSLong";
        }

    }
}