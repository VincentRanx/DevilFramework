namespace org.vr.rts
{

    public interface IRTSPlugin
    {

        object pluginFunction(object[] args);
    }

    public delegate object RTSPluginDelegate(object[] args);

    public delegate object RTSPluginAction(IRTSStack stack);
}