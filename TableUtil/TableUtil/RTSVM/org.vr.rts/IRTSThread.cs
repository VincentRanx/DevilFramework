namespace org.vr.rts
{

    /**
     * 脚本执行线程组件
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