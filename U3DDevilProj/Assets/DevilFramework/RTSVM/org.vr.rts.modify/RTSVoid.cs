namespace org.vr.rts.modify
{

    public class RTSVoid : RTSObjectType
    {
        private static RTSVoid _void = new RTSVoid();
        public static RTSVoid VOID { get { return _void; } }

        public static IRTSType TYPE { get { return _void; } }

        public RTSVoid() : base()
        {
        }

        override public object castValue(object target)
        {
            return VOID;
        }

        override public string ToString()
        {
            return "";
        }

        override public object add(object a, object b)
        {
            return null;
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
            return null;
        }

        override public object or(object a, object b)
        {
            return null;
        }

        override public object xor(object a, object b)
        {
            return null;
        }

        override public object nigative(object target)
        {
            return null;
        }

        override public int rtsCompare(object a, object b)
        {
            return 0;
        }

        override public string typeName()
        {
            return "RTSVoid";
        }
    }
}