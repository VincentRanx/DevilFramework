namespace org.vr.rts
{

    /**
     * ´æ´¢×é¼þ
     * 
     * @author Administrator
     * 
     */
    public interface IRTSStack
    {

        int getId();

        IRTSStack getSuper();

        IRTSStack makeChild(int id);

        void onRemoved();

        object getVar(string varName);

        bool containsVar(string varName);

        void removeVar(string varName);

        void addVar(string varName, object var);

        void clearVars();

        IRTSThread getThread();

    }
}