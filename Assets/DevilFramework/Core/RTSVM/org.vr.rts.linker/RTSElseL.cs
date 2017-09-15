namespace org.vr.rts.linker
{

    public class RTSElseL : RTSLinker
    {

        private IRTSLinker mBodyL;
        private bool mOver;

        public RTSElseL()
            : base(IRTSDefine.Linker.ELSE)
        {

        }

        override public bool isStructure()
        {
            return true;
        }

        override public bool isPriority(IRTSLinker linker)
        {
            return mOver;
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            IRTSDefine.Linker id = linker.getId();
            if (id == IRTSDefine.Linker.SEMICOLON)
            {
                if (mBodyL == null)
                    return linker;
                mOver = true;
                return this;
            }
            else
            {
                if (mOver)
                    return linker;
                IRTSLinker ret = mBodyL;
                if (ret != null)
                    ret.setSuper(null);
                mBodyL = linker;
                linker.setSuper(this);
                if (linker.isStructure())
                    mOver = true;
                return ret;
            }
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            return false;
        }

        override public IRTSRunner createRunner()
        {
            return mBodyL.createRunner();
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (mBodyL == null)
                return IRTSDefine.Error.Compiling_DenyLinker;
            if (mSuper == null || mSuper.getId() != IRTSDefine.Linker.IF)
                return IRTSDefine.Error.Compiling_DenyLinker;
            compileList.add(mBodyL);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSElseL l = new RTSElseL();
            l.mSrc = src;
            return l;
        }

        override public string ToString()
        {
            return org.vr.rts.util.RTSUtil.linkString(' ', mSrc, mBodyL);
        }
    }
}
