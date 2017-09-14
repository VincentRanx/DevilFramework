namespace org.vr.rts
{

    public interface IRTSType
    {

        object add(object a, object b);

        object sub(object a, object b);

        object mul(object a, object b);

        object div(object a, object b);

        object mod(object a, object b);

        object and(object a, object b);

        object or(object a, object b);

        object xor(object a, object b);

        object nigative(object target);

        object castValue(object target);

        int rtsCompare(object a, object b);

        string typeName();
    }
}