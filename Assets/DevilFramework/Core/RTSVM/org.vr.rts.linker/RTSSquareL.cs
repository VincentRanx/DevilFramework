namespace org.vr.rts.linker
{

    public class RTSSquareL : RTSLinker
    {

        private IRTSLinker mVar;
        private IRTSLinker mChild;
        private bool mClosed;

        public RTSSquareL()
            : base(IRTSDefine.Linker.BRACKET_SQUARE)
        {

        }

        override public bool isPriority(IRTSLinker linker)
        {
            if (!mClosed)
                return false;
            else
                return base.isPriority(linker);
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (linker.getId() == IRTSDefine.Linker.BRACKET_SQUARE2)
            {
                mClosed = true;
                return this;
            }
            IRTSLinker ret = mChild;
            if (ret != null)
            {
                ret.setSuper(null);
            }
            mChild = linker;
            linker.setSuper(this);
            return ret;
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            if (mVar == null)
            {
                mVar = linker;
                linker.setSuper(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        override public IRTSRunner createRunner()
        {
            if (mVar != null)
            {
                return new org.vr.rts.runner.RTSSquareR(mVar, mChild);
            }
            else
            {
                org.vr.rts.util.RTSList<IRTSLinker> lst;
                if (mChild == null)
                {
                    lst = null;
                }
                else if (mChild.getId() == IRTSDefine.Linker.COMMA)
                {
                    lst = ((RTSCommaL)mChild).getChildren();
                }
                else
                {
                    lst = new org.vr.rts.util.RTSList<IRTSLinker>(1);
                    lst.add(mChild);
                }
                return new org.vr.rts.runner.RTSSquareR(lst);
            }
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (!mClosed)
            {
                return IRTSDefine.Error.Compiling_DenyLinker;
            }
            if (mVar != null)
                compileList.add(mVar);
            if (mChild != null)
                compileList.add(mChild);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSSquareL oper = new RTSSquareL();
            oper.mSrc = src;
            return oper;
        }

        override public string ToString()
        {
            return org.vr.rts.util.RTSUtil.linkString(' ', mVar, "[", mChild, "]");
        }
    }
}
