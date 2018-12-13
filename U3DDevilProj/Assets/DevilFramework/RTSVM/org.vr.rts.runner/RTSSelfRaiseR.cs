namespace org.vr.rts.runner
{

    public class RTSSelfRaiseR : IRTSRunner
    {

        private IRTSDefine.Linker mOperId;
        private IRTSRunner mVar;
        private bool mLeft;
        private bool mVarLoaded;
        private object mValue;

        public RTSSelfRaiseR(IRTSDefine.Linker operId, IRTSRunner var, bool varAtLeft)
        {
            mOperId = operId;
            mVar = var;
            mLeft = varAtLeft;
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
            mVarLoaded = false;
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return false;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (!mVarLoaded)
            {
                mVarLoaded = true;
                if (stack.getThread().loadRunner(mVar))
                    return 0;
            }
            object obj = mVar.getOutput();
            IRTSType tp = stack.getThread().getEngine().getRTSType(obj == null ? null : obj.GetType());
            object newObj;
            if (mOperId == IRTSDefine.Linker.SELFADD)
                newObj = tp.add(obj, 1);
            else if (mOperId == IRTSDefine.Linker.SELFSUB)
                newObj = tp.sub(obj, 1);
            else
                newObj = null;
            mVar.evaluate(stack, newObj);
            mValue = mLeft ? obj : newObj;
            return 0;
        }

        public object getOutput()
        {
            return mValue;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}
