namespace org.vr.rts
{

    public interface IRTSRuntime
    {
        void Yield();

        void AddFunction(string funcName, IRTSFunction func);

        bool LoadRunner(IRTSRunner runner, int threadId);

        bool Execute(string cmd, int threadId);

        object ExecuteImmediate(string cmd);

        object ExecuteFunction(string funcName, object[] args, bool immediate);

    }
}