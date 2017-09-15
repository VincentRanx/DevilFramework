namespace org.vr.rts.modify
{

    public class RTSGeneral : IRTSType
    {

        private static RTSGeneral _type = new RTSGeneral();
        public static RTSGeneral TYPE { get { return _type; } }

        protected RTSGeneral()
        {
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
            return target;
        }

        public int rtsCompare(object a, object b)
        {
            if (a == null && b == null)
                return 0;
            else if (a == null)
                return -1;
            else if (b == null)
                return 1;
            else if (a.Equals(b))
                return 0;
            else
                return a.GetHashCode() - b.GetHashCode();
        }

        public string typeName()
        {
            return "RTSGeneral";
        }

    }
}