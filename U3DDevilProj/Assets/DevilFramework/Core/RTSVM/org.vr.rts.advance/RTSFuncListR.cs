using org.vr.rts.util;

namespace org.vr.rts.advance
{

    public class RTSFuncListR : IRTSRunner
    {

        private string[] mFuncNames;
        private object[] mArgs;
        private int mArgC;
        private IRTSType mCast;

        private IRTSFunction mFunc;
        private IRTSRunner mFuncR;
        private int mFuncCur;
        private object mValue;

        public RTSFuncListR(IRTSType castType, string[] funcNames, int argc, object[] args)
        {
            mFuncNames = funcNames;
            mArgC = argc;
            mArgs = args;
            mCast = castType;
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
            mFuncR = null;
            mFunc = null;
            mFuncCur = 0;
            mValue = null;
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
            while (mFuncNames != null && mFuncCur < mFuncNames.Length)
            {
                mFunc = stack.getThread().getEngine().getFunction(mFuncNames[mFuncCur++], mArgC);
                mFuncR = mFunc == null ? null : mFunc.createRunner(mArgs);
                if (stack.getThread().loadRunner(mFuncR))
                    return 0;
            }
            mValue = mFuncR == null ? null : mFuncR.getOutput();
            if (mCast != null)
                mValue = mCast.castValue(mValue);
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}
