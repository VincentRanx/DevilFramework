namespace org.vr.rts.runner
{

    public class RTSPluginFuncR : IRTSRunner
    {

        private object[] mArgs;
        private IRTSType mReturnType;
        private IRTSPlugin mPlugin;

        private object mValue;
        private IRTSRunner mChild;

        public RTSPluginFuncR(IRTSType returnType, object[] args, IRTSPlugin plugin)
        {
            mArgs = args;
            mReturnType = returnType;
            mPlugin = plugin;
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
            mChild = null;
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
            if (mChild == null && mPlugin != null)
            {
                object o = mPlugin.pluginFunction(mArgs);
                mChild = o as IRTSRunner;
                if (stack.getThread().loadRunner(mChild))
                {
                    return 0;
                }
                else
                {
                    mChild = null;
                    setValue(o);
                    return 0;
                }
            }
            if (mChild != null)
            {
                object o = mChild.getOutput();
                mChild = o as IRTSRunner;
                if (stack.getThread().loadRunner(mChild))
                {
                    return 0;
                }
                else
                {
                    mChild = null;
                    setValue(o);
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
