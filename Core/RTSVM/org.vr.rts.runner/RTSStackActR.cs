namespace org.vr.rts.runner
{

    public class RTSStackActR : IRTSRunner
    {

        private IRTSDefine.Stack mType;
        private IRTSLinker mChildL;
        private IRTSRunner mChild;
        private bool mLoaded;

        public RTSStackActR(IRTSDefine.Stack type, IRTSLinker child)
        {
            mType = type;
            mChildL = child;
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
            mLoaded = false;
            mChild = null;
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        public object getOutput()
        {
            return mChild == null ? org.vr.rts.modify.RTSVoid.VOID : mChild.getOutput();
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (!mLoaded)
            {
                mLoaded = true;
                mChild = mChildL == null ? null : mChildL.createRunner();
                if (stack.getThread().loadRunner(mChild))
                    return 0;
            }
            return mType;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}
