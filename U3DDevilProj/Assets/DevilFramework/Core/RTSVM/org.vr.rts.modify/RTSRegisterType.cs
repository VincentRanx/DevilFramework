using org.vr.rts.component;

namespace org.vr.rts.modify
{

    public class RTSRegisterType : RTSObjectType
    {

        public static RTSRegisterType _type = new RTSRegisterType();
        public static RTSRegisterType TYPE { get { return _type; } }

        public static RTSRegister valueOf(object obj)
        {
            return obj as RTSRegister;
        }

        private RTSRegisterType() : base()
        {

        }

        override public object add(object a, object b)
        {
            RTSRegister regA = a as RTSRegister;
            if (a != null && regA == null)
                return null;
            RTSRegister reg = b as RTSRegister;
            if(reg != null && reg.Vars != null)
            {
                if(regA == null)
                {
                    regA = new RTSRegister();
                }
                foreach(var v in reg.Vars.Keys)
                {
                    regA.addVar(v, reg.Vars[v]);
                }
            }
            return regA;
        }

        override public object sub(object a, object b)
        {
            RTSRegister regA = a as RTSRegister;
            if (regA == null)
                return null;
            RTSRegister reg = b as RTSRegister;
            if (reg != null && reg.Vars != null)
            {
                foreach (var v in reg.Vars.Keys)
                {
                    regA.removeVar(v);
                }
            }
            return regA;
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

        override public object castValue(object target)
        {
            return valueOf(target);
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
            return "RTSRegister";
        }

    }
}