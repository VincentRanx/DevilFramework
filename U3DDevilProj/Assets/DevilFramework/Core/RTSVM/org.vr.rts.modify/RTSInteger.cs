namespace org.vr.rts.modify
{

    public class RTSInteger : RTSObjectType
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
            {
                int n;
                if (int.TryParse(obj.ToString(), out n))
                    return n;
                else
                    return obj.GetHashCode();
            }
        }

        private RTSInteger() : base()
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
            return valueOf(a) - valueOf(b);
        }

        override public string typeName()
        {
            return "RTSInteger";
        }

    }
}