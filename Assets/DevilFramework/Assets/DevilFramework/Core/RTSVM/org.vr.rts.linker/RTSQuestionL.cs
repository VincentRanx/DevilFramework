namespace org.vr.rts.linker
{

    public class RTSQuestionL : RTSLinker
    {

        private IRTSLinker mCondtionL;
        private IRTSLinker mTrueL;
        private IRTSLinker mFalseL;

        private bool mOnFalse;

        public RTSQuestionL()
            : base(IRTSDefine.Linker.QUESTION)
        {

        }

        override public bool isPriority(IRTSLinker linker)
        {
            if (mFalseL == null)
                return false;
            else
                return base.isPriority(linker);
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (linker.getId() == IRTSDefine.Linker.COLON)
            {
                if (mOnFalse)
                    return linker;
                mOnFalse = true;
                return this;
            }
            else if (mOnFalse)
            {
                IRTSLinker ret = mFalseL;
                if (ret != null)
                    ret.setSuper(null);
                mFalseL = linker;
                linker.setSuper(this);
                return ret;
            }
            else
            {
                IRTSLinker ret = mTrueL;
                if (ret != null)
                    ret.setSuper(null);
                mTrueL = linker;
                linker.setSuper(this);
                return ret;
            }
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            if (mCondtionL == null)
            {
                mCondtionL = linker;
                return true;
            }
            else
                return false;
        }

        override public IRTSRunner createRunner()
        {
            org.vr.rts.runner.RTSIfElseR ql = new org.vr.rts.runner.RTSIfElseR(mCondtionL, mTrueL, mFalseL);
            return ql;
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (mCondtionL == null || mFalseL == null || mTrueL == null)
                return IRTSDefine.Error.Compiling_DenyLinker;
            compileList.add(mCondtionL);
            compileList.add(mFalseL);
            compileList.add(mTrueL);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSQuestionL lin = new RTSQuestionL();
            lin.mSrc = src;
            return lin;
        }

        override public string ToString()
        {
            return org.vr.rts.util.RTSUtil.linkString(' ', mCondtionL, mSrc, mTrueL, ":", mFalseL);
        }
    }
}
