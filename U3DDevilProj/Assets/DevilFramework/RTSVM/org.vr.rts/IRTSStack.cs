namespace org.vr.rts
{

    /**
     * ´æ´¢×é¼þ
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