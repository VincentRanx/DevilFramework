namespace org.vr.rts.runner
{

    public class RTSForR : IRTSRunner
    {

        private int mCur;
        private IRTSRunner[] mLins;// condition,body,end
        private IRTSRunner[] mInit;
        private int mInitLen;

        public RTSForR(IRTSLinker initL, IRTSLinker condition, IRTSLinker endL, IRTSLinker bodyL)
        {
            mLins = new IRTSRunner[3];
            mLins[0] = condition == null ? null : condition.createRunner();
            mLins[1] = bodyL == null ? null : bodyL.createRunner();
            mLins[2] = endL == null ? null : endL.createRunner();
            if (initL == null)
            {
                mInit = null;
            }
            else if (initL.getId() == IRTSDefine.Linker.COMMA)
            {
                org.vr.rts.util.RTSList<IRTSLinker> lst = ((org.vr.rts.linker.RTSCommaL)initL).getChildren();
                if (lst == null || lst.length() == 0)
                {
                    mInit = null;
                }
                else
                {
                    mInit = new IRTSRunner[lst.length()];
                    for (int i = 0; i < mInit.Length; i++)
                    {
                        IRTSLinker l = lst.get(i);
                        mInit[i] = l == null ? null : l.createRunner();
                    }
                }
            }
            else
            {
                mInit = new IRTSRunner[1];
                mInit[0] = initL.createRunner();
            }
        }

        public IRTSDefine.Stack applyStack()
        {
            return IRTSDefine.Stack.ACTION_CONTINUE | IRTSDefine.Stack.ACTION_BREAK;
        }

        public bool isConst()
        {
            return false;
        }

        public void loadedOnThread()
        {
            mCur = 0;
            mInitLen = 0;
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            if (returnTppe == IRTSDefine.Stack.ACTION_CONTINUE)
            {
                mCur = 2;
                return false;
            }
            else
            {
                return true;
            }
        }

        public object getOutput()
        {
            return org.vr.rts.modify.RTSVoid.VOID;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (mInit != null && mInitLen < mInit.Length)
            {
                if (stack.getThread().loadRunner(mInit[mInitLen++]))
                    return 0;
            }
            if (mCur == 0)
            {
                if (stack.getThread().loadRunner(mLins[mCur++]))
                    return 0;
            }
            if (mCur == 1)
            {
                object o = mLins[0] == null ? null : mLins[0].getOutput();
                if (!org.vr.rts.modify.RTSBool.valueOf(o))
                    return 0;
                if (stack.getThread().loadRunner(mLins[mCur++]))
                    return 0;
            }
            if (mCur == 2)
            {
                if (stack.getThread().loadRunner(mLins[mCur++]))
                    return 0;
            }
            mCur = 0;
            return IRTSDefine.Stack.ACTION_HOLD;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}