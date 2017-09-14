namespace org.vr.rts.runner
{

    public class RTSInnerFuncR : IRTSRunner
    {

        private object[] mArgs;
        private bool mLoaded;
        private IRTSType mReturnType;
        private object mValue;
        private IRTSRunner mBody;
        private IRTSFunction mFunc;

        public RTSInnerFuncR(object[] args, IRTSFunction func)
        {
            mArgs = args;
            mReturnType = func.returnType();
            mFunc = func;
            IRTSLinker body = func.getBody();
            mBody = body == null ? null : body.createRunner();
        }

        public IRTSDefine.Stack applyStack()
        {
            return IRTSDefine.Stack.ACTION_RETURN;
        }

        public bool isConst()
        {
            return false;
        }

        public void loadedOnThread()
        {
            setValue(null);
            mLoaded = false;
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            setValue(value);
            return true;
        }

        private void setValue(object v)
        {
            mValue = mReturnType == null ? v : mReturnType.castValue(v);
        }

        public object getOutput()
        {
            return mValue;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (!mLoaded)
            {
                mLoaded = true;
                if (mBody != null && mArgs != null && mArgs.Length > 0)
                {
                    for (int i = 0; i < mArgs.Length; i++)
                    {
                        string lin = mFunc.getArgDef(i);
                        if (lin != null)
                            stack.addVar(lin, mArgs[i]);
                    }
                }
                if (stack.getThread().loadRunner(mBody))
                {
                    return 0;
                }
            }
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }

    }
}