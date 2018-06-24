namespace org.vr.rts
{

    /**
     * �洢���
     * 
     * @author Administrator
     * 
     */
    public interface IRTSStack : IRTSVarPool
    {

        int getId();

        IRTSStack getSuper();

        IRTSStack makeChild(int id);

        void onRemoved();

        IRTSThread getThread();

    }
}