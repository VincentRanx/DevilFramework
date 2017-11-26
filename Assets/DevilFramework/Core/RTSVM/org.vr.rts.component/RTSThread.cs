using org.vr.rts.modify;
using org.vr.rts.util;

namespace org.vr.rts.component
{

    public class RTSThread : IRTSThread
    {
        private int mStackSize;
        private IRTSStack mStack;
        private IRTSEngine mEngine;
        private object mOutput;
        private RTSList<IRTSRunner> mRunners;
        private long mSleepTo;
        private long mTicks;
        public int Pid { get; private set; }

        public RTSThread( int pid, int capacity)
        {
            Pid = pid;
            mRunners = new RTSList<IRTSRunner>(capacity);
            mStack = RTSStack.Factory.getStack(this, -1);
            mOutput = RTSVoid.VOID;
            mStackSize = RTSCfg.CALL_STACK_DEPTH;
        }

        public void setStackSize(int size)
        {
            if (size > 0)
                mStackSize = size;
        }

        public bool isFinished()
        {
            return mRunners.length() == 0;
        }

        public void Interrupt()
        {
            mRunners.clear();
            while (mStack.getId() >= 0)
            {
                mStack.onRemoved();
                mStack = (RTSStack)mStack.getSuper();
            }
        }

        public void resetOutput()
        {
            mOutput = RTSVoid.VOID;
        }

        public object getOutput()
        {
            return mOutput;
        }

        public IRTSEngine getEngine()
        {
            return mEngine;
        }

        public bool isSleeping()
        {
            if (mSleepTo > 0)
            {
                long l = System.DateTime.Now.Ticks;
                if (l < mSleepTo)
                    return true;
                mSleepTo = 0;
            }
            return false;
        }

        public void run(IRTSEngine engine)
        {
            mTicks = System.DateTime.Now.Ticks;
            int len = mRunners.length();
            if (len > 0)
            {
                mEngine = engine;
                while (mStack.getId() >= len)
                {
                    mStack.onRemoved();
                    mStack = mStack.getSuper();
                }
                IRTSRunner cal = mRunners.getLast();
                IRTSDefine.Stack ret = cal.run(mStack);
                mOutput = cal.getOutput();
                if (ret != IRTSDefine.Stack.ACTION_HOLD)
                {
                    if (ret != 0)
                    {
                        int off = mRunners.length();
                        for (int i = off - 1; i >= 0; i--)
                        {
                            if ((mRunners.get(i).applyStack() & ret) != 0)
                                break;
                            off = i;
                        }
                        mRunners.removeFrom(off);
                        IRTSRunner cal2 = mRunners.getLast();
                        if (cal2 != null && cal2.onReturnAndSkip(ret, mOutput))
                        {
                            mRunners.removeLast();
                        }
                    }
                    else if (len == mRunners.length())
                    {
                        mRunners.removeLast();
                    }
                }
                mEngine = null;
                len = mRunners.length();
                while (mStack.getId() >= len)
                {
                    mStack.onRemoved();
                    mStack = (RTSStack)mStack.getSuper();
                }
            }
        }

        public bool loadRunner(IRTSRunner cal)
        {
            if (cal == null)
                return false;
            if (cal.isConst())
            {
                mOutput = cal.getOutput();
                return false;
            }
            int len = mRunners.length();
            if (len >= mStackSize)
            {
                return false;
            }
            mRunners.add(cal);
            IRTSDefine.Stack sco = cal.applyStack();
            if ((sco & IRTSDefine.Stack.ACTION_RETURN) != 0)
            {
                IRTSStack scope = mStack.makeChild(len);
                if (scope != null)
                    mStack = scope;
            }
            cal.loadedOnThread();
            return true;
        }

        public void catchError(IRTSDefine.Error error, object msg)
        {
            if (mEngine != null)
            {
                IRTSLog log = mEngine.getLogger();
                if (log != null)
                    log.logError(RTSUtil.getEnumDescript(typeof(IRTSDefine.Error), (int)error) + ":" + msg);
                mRunners.clear();
            }
            while (mStack.getId() >= 0)
            {
                IRTSStack stack = mStack;
                mStack = stack.getSuper();
                stack.onRemoved();
            }
        }

        public void sleep(long millies)
        {
            long l = mTicks + millies * 10000;
            if (l > mSleepTo)
                mSleepTo = l;
        }
    }
}
