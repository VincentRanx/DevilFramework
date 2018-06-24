namespace org.vr.rts
{

    /**
     * �ű�ִ���߳����
     * 
     * @author Administrator
     *
     */
    public interface IRTSThread
    {

        bool loadRunner(IRTSRunner cal);

        bool isFinished();

        void resetOutput();

        object getOutput();

        IRTSEngine getEngine();

        void run(IRTSEngine engine);

        void catchError(IRTSDefine.Error error, object msg);

        void sleep(long millies);

    }
}