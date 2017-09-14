namespace org.vr.rts.advance
{

    public class AsyncR : IRTSRunner
    {
        private IRTSRuntime mRuntime;
        private IRTSLinker mChild;
        private IRTSType mCastType;
        private object mResult;

        public AsyncR(IRTSRuntime runtime, IRTSLinker child)
        {
            mRuntime = runtime;
            mChild = child;
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
            return mResult;
        }

        public bool isConst()
        {
            return false;
        }

        public void loadedOnThread()
        {
            
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if(mRuntime != null && mChild != null)
            {
                bool ret = mRuntime.LoadRunner(mChild.createRunner(), -1);
                if (mCastType != null)
                    mResult = mCastType.castValue(ret);
                else
                    mResult = ret;
            }
            return 0;
        }
    }
}