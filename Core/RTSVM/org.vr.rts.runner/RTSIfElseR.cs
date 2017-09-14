namespace org.vr.rts.runner
{

    public class RTSIfElseR : IRTSRunner
    {

        private IRTSLinker mConditionL;
        private IRTSLinker mTrueL;
        private IRTSLinker mFalseL;

        private IRTSRunner mConditionR;
        private IRTSRunner mResultR;

        private int mCur;

        public RTSIfElseR(IRTSLinker condition, IRTSLinker trueL, IRTSLinker falseL)
        {
            mConditionL = condition;
            mTrueL = trueL;
            mFalseL = falseL;
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
            mCur = 0;
            mConditionR = null;
            mResultR = null;
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        public object getOutput()
        {
            return mResultR == null ? null : mResultR.getOutput();
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (mCur == 0)
            {
                mCur++;
                mConditionR = mConditionL == null ? null : mConditionL.createRunner();
                if (stack.getThread().loadRunner(mConditionR))
                    return 0;
            }
            if (mCur == 1)
            {
                mCur++;
                object o = mConditionR == null ? null : mConditionR.getOutput();
                bool c = org.vr.rts.modify.RTSBool.valueOf(o);
                IRTSLinker l = c ? mTrueL : mFalseL;
                mResultR = l == null ? null : l.createRunner();
                if (stack.getThread().loadRunner(mResultR))
                    return 0;
            }
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}
