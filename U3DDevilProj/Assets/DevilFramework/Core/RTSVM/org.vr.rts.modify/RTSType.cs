namespace org.vr.rts.modify
{

    public abstract class RTSType : IRTSType
    {
        public virtual object getProperty(IRTSEngine engine, object target, string propertyname)
        {
            IRTSFunction func = engine.getFunction(string.Format("{0}_get_{1}", typeName(), propertyname), 1);
            if (func != null)
                return func.createRunner(new object[] { target });
            else
                return null;
        }

        public virtual object setProperty(IRTSEngine engine, object target, string propertyname, object value)
        {
            IRTSFunction func = engine.getFunction(string.Format("{0}_set_{1}", typeName(), propertyname), 2);
            if (func != null)
                return func.createRunner(new object[] { target, value });
            else
                return null;
        }

        public virtual object function(IRTSEngine engine, object target, string funcname, object[] args)
        {
            object[] arg;
            if (args != null && args.Length > 0)
            {
                arg = new object[args.Length + 1];
                System.Array.Copy(args, 0, arg, 1, args.Length);
            }
            else
            {
                arg = new object[1];
            }
            arg[0] = target;
            IRTSFunction func = engine.getFunction(string.Format("{0}_{1}", typeName(), funcname), arg.Length);
            if (func != null)
                return func.createRunner(arg);
            else
                return null;
        }

        public abstract object add(object a, object b);
        public abstract object sub(object a, object b);
        public abstract object mul(object a, object b);
        public abstract object div(object a, object b);
        public abstract object mod(object a, object b);
        public abstract object and(object a, object b);
        public abstract object or(object a, object b);
        public abstract object xor(object a, object b);
        public abstract object nigative(object target);
        public abstract object castValue(object target);
        public abstract int rtsCompare(object a, object b);
        public abstract string typeName();
    }
}