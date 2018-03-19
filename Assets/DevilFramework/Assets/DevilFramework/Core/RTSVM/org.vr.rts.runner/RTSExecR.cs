using org.vr.rts.modify;
using org.vr.rts.util;

namespace org.vr.rts.runner
{

    public class RTSExecR : IRTSRunner
    {

        private int mCur;
        private RTSList<IRTSLinker> mRunners;
        private object mValue;
        private IRTSRunner mLastRunner;

        public RTSExecR(org.vr.rts.util.RTSList<IRTSLinker> runners)
        {
            mRunners = runners;
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
            mValue = RTSVoid.VOID;
            mLastRunner = null;
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            mValue = value;
            return true;
        }

        public object getOutput()
        {
            return mValue;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            while (mRunners != null && mCur < mRunners.length())
            {
                IRTSLinker l = mRunners.get(mCur++);
                IRTSRunner r = l == null ? null : l.createRunner();
                if (r != null)
                    mLastRunner = r;
                if (stack.getThread().loadRunner(r))
                {
                    return 0;
                }
            }
            if (mLastRunner != null)
                mValue = mLastRunner.getOutput();
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}
