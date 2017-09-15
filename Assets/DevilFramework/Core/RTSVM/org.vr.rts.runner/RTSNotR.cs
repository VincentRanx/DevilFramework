namespace org.vr.rts.runner
{

    public class RTSNotR : IRTSRunner
    {

        private IRTSRunner mChild;
        private object mValue;
        private bool mLoaded;

        public RTSNotR(IRTSRunner runner)
        {
            mChild = runner;
        }

        public IRTSDefine.Stack applyStack()
        {
            return 0;
        }

        public bool isConst()
        {
            return false;
        }

        public void loadedOnThread()
        {
            mValue = false;
            mLoaded = false;
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        public object getOutput()
        {
            return mValue;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (!mLoaded)
            {
                mLoaded = true;
                if (stack.getThread().loadRunner(mChild))
                    return 0;
            }
            object o = mChild == null ? null : mChild.getOutput();
            mValue = !org.vr.rts.modify.RTSBool.valueOf(o);
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}
