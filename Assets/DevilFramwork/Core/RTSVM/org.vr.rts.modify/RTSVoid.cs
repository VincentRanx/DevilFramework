namespace org.vr.rts.modify
{

    public class RTSVoid : IRTSType
    {
        private static RTSVoid _void = new RTSVoid();
        public static RTSVoid VOID { get { return _void; } }

        public static IRTSType TYPE { get { return _void; } }

        public RTSVoid()
        {
        }

        public object castValue(object target)
        {
            return VOID;
        }

        override public string ToString()
        {
            return "";
        }

        public object add(object a, object b)
        {
            return null;
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
            return null;
        }

        public object or(object a, object b)
        {
            return null;
        }

        public object xor(object a, object b)
        {
            return null;
        }

        public object nigative(object target)
        {
            return null;
        }

        public int rtsCompare(object a, object b)
        {
            return 0;
        }

        public string typeName()
        {
            return "RTSVoid";
        }
    }
}