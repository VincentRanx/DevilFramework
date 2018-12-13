
namespace org.vr.rts.modify
{

    public class RTSString : RTSObjectType
    {

        private static RTSString _type = new RTSString();
        public static RTSString TYPE { get { return _type; } }

        public static string stringOf(object obj)
        {
            return obj == null ? null : obj.ToString();
        }

        private RTSString() : base()
        {
        }

        override public object add(object a, object b)
        {
            if (a == null && b == null)
                return null;
            else if (a == null)
                return b.ToString();
            else if (b == null)
                return a.ToString();
            else
                return a.ToString() + b.ToString();
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
            return RTSBool.TYPE.add(a, b);
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
            return target == null ? null : target.ToString();
        }

        override public int rtsCompare(object a, object b)
        {
            string astr = stringOf(a);
            string bstr = stringOf(b);
            if (astr == null && bstr == null)
                return 0;
            else if (astr == null)
                return -1;
            else if (bstr == null)
                return 1;
            else
                return astr.CompareTo(bstr);
        }

        override public string typeName()
        {
            return "RTSString";
        }
        
    }
}