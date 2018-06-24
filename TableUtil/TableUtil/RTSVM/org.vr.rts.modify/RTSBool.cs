namespace org.vr.rts.modify
{

    public class RTSBool : IRTSType
    {

        private static RTSBool _type = new RTSBool();
        public static RTSBool TYPE { get { return _type; } }

        private RTSBool()
        {
        }

        public static bool valueOf(object target)
        {
            if (target == null)
                return false;
            else if (target is bool)
                return (bool)target;
            else if (target is int)
                return ((int)target) != 0;
            else if (target is long)
                return ((long)target) != 0;
            else if (target is byte)
                return ((byte)target) != 0;
            else if (target is char)
                return ((char)target) != 0;
            else
                return false;
        }

        public object castValue(object target)
        {
            return valueOf(target);
        }

        public object add(object a, object b)
        {
            return RTSString.TYPE.add(a, b);
        }

        public object sub(object a, object b)
        {
            return null;
        }

        public object mul(object a, object b)
        {
            return null;
        }

        public object div(object a, object b)
        {
            return null;
        }

        public object mod(object a, object b)
        {
            return null;
        }

        public object and(object a, object b)
        {
            return valueOf(a) && valueOf(b);
        }

        public object or(object a, object b)
        {
            return valueOf(a) || valueOf(b);
        }

        public object xor(object a, object b)
        {
            return valueOf(a) ^ valueOf(b);
        }

        public object nigative(object target)
        {
            return !valueOf(target);
        }

        public int rtsCompare(object a, object b)
        {
            return RTSInteger.TYPE.rtsCompare(a, b);
        }

        public string typeName()
        {
            return "RTSBool";
        }
    }
}