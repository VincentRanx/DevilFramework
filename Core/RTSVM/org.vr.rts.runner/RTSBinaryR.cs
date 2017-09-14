namespace org.vr.rts.runner
{

    public abstract class RTSBinaryR : IRTSRunner
    {

        private IRTSLinker mLeftL;
        private IRTSLinker mRightL;
        protected IRTSRunner mLeft;
        protected IRTSRunner mRight;
        protected object mValue;
        private int mChildCount;

        public RTSBinaryR(IRTSLinker left, IRTSLinker right)
        {
            mLeftL = left;
            mRightL = right;
        }

        public bool isConst()
        {
            return false;
        }

        public IRTSDefine.Stack applyStack()
        {
            return 0;
        }

        public void loadedOnThread()
        {
            mValue = null;
            mChildCount = 0;
            mLeft = null;
            mRight = null;
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
            if (mChildCount == 0)
            {
                mChildCount++;
                if (!skipLeft())
                {
                    mLeft = mLeftL == null ? null : mLeftL.createRunner();
                    if (stack.getThread().loadRunner(mLeft))
                        return 0;
                }
            }
            if (mChildCount == 1)
            {
                mChildCount++;
                if (!skipRight())
                {
                    mRight = mRightL == null ? null : mRightL.createRunner();
                    if (stack.getThread().loadRunner(mRight))
                        return 0;
                }
            }
            if (mChildCount == 2)
            {
                mChildCount++;
                onFinalRun(stack, mLeft == null ? null : mLeft.getOutput(), mRight == null ? null : mRight.getOutput());
            }
            return 0;
        }

        public IRTSRunner getLeft()
        {
            return mLeft;
        }

        public IRTSRunner getRight()
        {
            return mRight;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }

        virtual protected bool skipLeft()
        {
            return false;
        }

        virtual protected bool skipRight()
        {
            return false;
        }

        protected abstract void onFinalRun(IRTSStack stack, object left, object right);

    }
}