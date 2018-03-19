
namespace org.vr.rts.advance
{
    public class RTSWaitForValueR<T> : IRTSRunner
    {
        bool mWait;
        IRTSRuntime mRuntime;
        public int mThreadId;
        public T value;

        public RTSWaitForValueR(IRTSRuntime runtime)
        {
            mRuntime = runtime;
        }

        public void StopWait()
        {
            mWait = false;
        }

        public IRTSDefine.Stack applyStack()
        {
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }

        public object getOutput()
        {
            return value;
        }

        public bool isConst()
        {
            return false;
        }

        public void loadedOnThread()
        {
            mWait = true;
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (mRuntime != null)
                mRuntime.Yield();
            if (mWait)
                return IRTSDefine.Stack.ACTION_HOLD;
            else
                return 0;
        }
    }
}