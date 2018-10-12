using org.vr.rts.util;

namespace org.vr.rts.runner
{

    public class RTSDomainR : IRTSRunner
    {
        protected string mDomain;
        protected IRTSLinker mLeftL;
        IRTSRunner mLeftR;
        protected RTSList<IRTSLinker> mArgs;
        RTSList<IRTSRunner> mArgRs;
        bool mLeftRun;
        int mArgPtr;
        int mArgNum;
        bool mInvokeFunc;
        protected object mValue;
        IRTSRunner mOutput;
        bool mAsFunction;

        public RTSDomainR(IRTSLinker left, string name, RTSList<IRTSLinker> args)
        {
            mDomain = name;
            mLeftL = left;
            mArgs = args;
            mArgNum = args == null ? 0 : args.length();
            if (mArgNum > 0)
                mArgRs = new RTSList<IRTSRunner>(mArgNum);
            mAsFunction = true;
        }

        public RTSDomainR(IRTSLinker left, string name)
        {
            mDomain = name;
            mLeftL = left;
            mArgNum = 0;
            mAsFunction = false;
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
            mLeftR = null;
            if(mArgRs != null)
                mArgRs.clear();
            mLeftRun = mLeftL != null;
            mInvokeFunc = true;
            mArgPtr = 0;
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
            if(mLeftRun)
            {
                mLeftRun = false;
                mLeftR = mLeftL.createRunner();
                if (stack.getThread().loadRunner(mLeftR))
                    return 0;
            }
            while (mArgNum > mArgPtr)
            {
                IRTSRunner r = mArgs.get(mArgPtr++).createRunner();
                mArgRs.add(r);
                if (stack.getThread().loadRunner(r))
                    return 0;
            }
            if (mInvokeFunc)
            {
                mInvokeFunc = false;
                object domain = mLeftR == null ? null : mLeftR.getOutput();
                if (domain == null)
                    return 0;
                IRTSType tp = stack.getThread().getEngine().getRTSType(domain.GetType());
                if (!mAsFunction)
                {
                    mValue = tp.getProperty(stack.getThread().getEngine(), domain, mDomain);
                    mOutput = mValue as IRTSRunner;
                    if (stack.getThread().loadRunner(mOutput))
                        return 0;
                }
                else
                {
                    object[] args = new object[mArgNum];
                    for(int i = 0; i < mArgNum; i++)
                    {
                        IRTSRunner r = mArgRs.get(i);
                        args[i] = r == null ? null : r.getOutput();
                    }
                    mValue = tp.function(stack.getThread().getEngine(), domain, mDomain, args);
                    mOutput = mValue as IRTSRunner;
                    if (stack.getThread().loadRunner(mOutput))
                        return 0;
                }
            }
            if (mOutput != null)
            {
                mValue = mOutput.getOutput();
                mOutput = mValue as IRTSRunner;
                if (stack.getThread().loadRunner(mOutput))
                {
                    return 0;
                }
            }
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            object domain = mLeftR == null ? null : mLeftR.getOutput();
            if (domain == null)
                return false;
            IRTSType tp = stack.getThread().getEngine().getRTSType(domain.GetType());
            mValue = tp.setProperty(stack.getThread().getEngine(), domain, mDomain, value);
            return true;
        }

        virtual protected bool skipLeft()
        {
            return false;
        }

        virtual protected bool skipRight()
        {
            return false;
        }

    }
}