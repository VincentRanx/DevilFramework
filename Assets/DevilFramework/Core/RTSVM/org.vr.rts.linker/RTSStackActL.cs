namespace org.vr.rts.linker
{

    public class RTSStackActL : RTSLinker
    {

        private IRTSDefine.Stack mType;
        protected IRTSLinker mChild;

        public RTSStackActL(IRTSDefine.Stack type)
            : base(IRTSDefine.Linker.STACK_ACT)
        {

            mType = type;
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (mType == IRTSDefine.Stack.ACTION_RETURN)
            {
                IRTSLinker lin = mChild;
                if (lin != null)
                    lin.setSuper(null);
                mChild = linker;
                linker.setSuper(this);
                return lin;
            }
            else
            {
                return linker;
            }
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            return false;
        }

        override public IRTSRunner createRunner()
        {
            org.vr.rts.runner.RTSStackActR r = new org.vr.rts.runner.RTSStackActR(mType, mChild);
            return r;
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (mSuper != null && !mSuper.isStructure())
            {
                return IRTSDefine.Error.Compiling_DenyLinker;
            }
            if (mChild != null)
            {
                compileList.add(mChild);
            }
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSStackActL lin = new RTSStackActL(mType);
            lin.mSrc = src;
            return lin;
        }

        override public string ToString()
        {
            return org.vr.rts.util.RTSUtil.linkString(' ', mSrc, mChild);
        }
    }
}
