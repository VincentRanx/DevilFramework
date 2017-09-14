using org.vr.rts.util;

namespace org.vr.rts.runner
{

    public class RTSFuncR : IRTSRunner
    {

        private string mFuncName;
        private int mArgC;
        private RTSList<IRTSRunner> mArgs;
        private IRTSType mCast;

        private IRTSFunction mFunc;
        private IRTSRunner mFuncR;
        private int mArgCur;
        private bool mLoaded;
        private object mValue;

        public RTSFuncR(IRTSType castType, string funcName, RTSList<IRTSRunner> args)
        {
            mFuncName = funcName;
            mArgs = args;
            mArgC = args == null ? 0 : args.length();
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
            mArgCur = 0;
            mLoaded = false;
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
            if (mFunc == null)
            {
                mFunc = stack.getThread().getEngine().getFunction(mFuncName, mArgC);
                if (mFunc == null)
                {
                    stack.getThread().catchError(IRTSDefine.Error.Runtime_NoFunctionDefine,
                            "Don't find function defined:" + RTSUtil.keyOfFunc(mFuncName, mArgC));
                    return 0;
                }
            }
            while (mArgs != null && mArgCur < mArgs.length())
            {
                IRTSRunner r = mArgs.get(mArgCur++);
                if (stack.getThread().loadRunner(r))
                    return 0;
            }
            if (!mLoaded)
            {
                mLoaded = true;
                object[] args = mArgC > 0 ? new object[mArgC] : null;
                if (args != null)
                {
                    for (int i = 0; i < mArgC; i++)
                    {
                        IRTSRunner r = mArgs.get(i);
                        if (r != null)
                            args[i] = r.getOutput();
                    }
                }
                mFuncR = mFunc.createRunner(args);
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
