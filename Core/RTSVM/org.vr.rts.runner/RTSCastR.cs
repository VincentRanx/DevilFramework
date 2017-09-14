namespace org.vr.rts.runner
{

    public class RTSCastR : IRTSRunner
    {

        private IRTSType mCastType;
        private IRTSRunner mChild;
        private bool mChildLoaded;
        private object mValue;

        public RTSCastR(IRTSType castType, IRTSRunner child)
        {
            mCastType = castType;
            mChild = child;
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
            mChildLoaded = false;
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
            if (!mChildLoaded)
            {
                mChildLoaded = true;
                if (stack.getThread().loadRunner(mChild))
                    return 0;
            }
            object v = mChild == null ? null : mChild.getOutput();
            mValue = mCastType == null ? v : mCastType.castValue(v);
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }

    }
}