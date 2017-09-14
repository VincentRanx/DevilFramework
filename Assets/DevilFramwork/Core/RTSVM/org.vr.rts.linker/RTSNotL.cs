namespace org.vr.rts.linker
{

    public class RTSNotL : RTSLinker
    {

        private IRTSLinker mChildL;

        public RTSNotL()
            : base(IRTSDefine.Linker.NOT)
        {

        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            IRTSLinker ret = mChildL;
            if (ret != null)
                ret.setSuper(null);
            mChildL = linker;
            linker.setSuper(this);
            return ret;
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            return false;
        }

        override public IRTSRunner createRunner()
        {
            IRTSRunner r = new org.vr.rts.runner.RTSNotR(mChildL.createRunner());
            return r;
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (mChildL == null)
                return IRTSDefine.Error.Compiling_DenyLinker;
            compileList.add(mChildL);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSNotL lin = new RTSNotL();
            lin.mSrc = src;
            return lin;
        }

        override public string ToString()
        {
            return org.vr.rts.util.RTSUtil.linkString(' ', mSrc, mChildL);
        }
    }
}