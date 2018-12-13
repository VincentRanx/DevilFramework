using Devil.Utility;

namespace org.vr.rts.modify
{

    public abstract class RTSObjectType : RTSType
    {
        public override object getProperty(IRTSEngine engine, object target, string propertyname)
        {

            object value;
            if (!Ref.TryGetFieldOrProperty(target, propertyname, out value))
                value = base.getProperty(engine, target, propertyname);
            return value;
        }

        public override object setProperty(IRTSEngine engine, object target, string propertyname, object value)
        {
            if (Ref.SetFieldOrProperty(target, propertyname, value))
                return value;
            else
                return base.setProperty(engine, target, propertyname, value);
        }

        public override object function(IRTSEngine engine, object target, string funcname, object[] args)
        {
            object ret;
            if (Ref.CallMethod(target, funcname, args, out ret))
                return ret;
            else
                return base.function(engine, target, funcname, args);
        }
        
    }
}