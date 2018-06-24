namespace org.vr.rts.linker
{

    public abstract class RTSBinaryL : RTSLinker
    {

        protected IRTSLinker mLeft;
        protected IRTSLinker mRight;

        public RTSBinaryL(IRTSDefine.Linker id)
            : base(id)
        {

        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            if (mLeft == null)
            {
                mLeft = linker;
                linker.setSuper(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            IRTSLinker ret = mRight;
            if (ret != null)
                ret.setSuper(null);
            mRight = linker;
            linker.setSuper(this);
            return ret;
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (mLeft == null || mRight == null)
                return IRTSDefine.Error.Compiling_DenyLinker;
            compileList.add(mLeft);
            compileList.add(mRight);
            return 0;
        }

        override public string ToString()
        {
            string s = org.vr.rts.util.RTSUtil.linkString(' ',
                    mLeft == null ? null : mLeft.ToString(), mSrc,
                    mRight == null ? null : mRight.ToString());
            return s;
        }

        public override bool isPriority(IRTSLinker linker)
        {
            if (mRight == null && !linker.isStructure())
                return false;
            else
                return base.isPriority(linker);
        }

        virtual public IRTSRunner createRunnerWith(IRTSLinker left, IRTSLinker right)
        {
            return null;
        }
    }
}