namespace org.vr.rts.modify
{

    public class RTSGeneral : RTSObjectType
    {

        private static RTSGeneral _type = new RTSGeneral();
        public static RTSGeneral TYPE { get { return _type; } }

        protected RTSGeneral() : base()
        {
        }

        override public object add(object a, object b)
        {
            return RTSString.TYPE.add(a, b);
        }

        override public object sub(object a, object b)
        {
            return null;
        }

        override public object mul(object a, object b)
        {
            return null;
        }

        override public object div(object a, object b)
        {
            return null;
        }

        override public object mod(object a, object b)
        {
            return null;
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
            return target;
        }

        override public int rtsCompare(object a, object b)
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

        override public string typeName()
        {
            return "RTSGeneral";
        }
        
    }
}