namespace org.vr.rts.linker
{

    public class RTSForL : RTSLinker
    {

        private int mCur;
        private IRTSLinker[] mChildren;// init,condition,end,loop
        private bool mCArg;

        public RTSForL()
            : base(IRTSDefine.Linker.FOR)
        {

            mChildren = new IRTSLinker[4];
        }

        override public bool isPriority(IRTSLinker oper)
        {
            if (mChildren[3] == null)
                return false;
            else if (mChildren[3].getId() == IRTSDefine.Linker.BRACKET_FLOWER)
                return true;
            else
                return base.isPriority(oper);
        }

        override public IRTSLinker appendRightChild(IRTSLinker oper)
        {
            IRTSDefine.Linker id = oper.getId();
            if (mCArg)
            {
                if (id == IRTSDefine.Linker.BRACKET2)
                {
                    mCArg = false;
                    mCur++;
                    return this;
                }
                else if (id == IRTSDefine.Linker.SEMICOLON)
                {
                    if (mCur < 2)
                    {
                        mCur++;
                        return this;
                    }
                    else
                    {
                        return oper;
                    }
                }
                else
                {
                    IRTSLinker ret = mChildren[mCur];
                    if (ret != null)
                        ret.setSuper(null);
                    mChildren[mCur] = oper;
                    oper.setSuper(this);
                    return ret;
                }
            }
            else if (mCur == 0)
            {
                if (id != IRTSDefine.Linker.BRACKET)
                    return oper;
                mCArg = true;
                return this;
            }
            else
            {
                if (mCur != 3)
                    return oper;
                IRTSLinker ret = mChildren[mCur];
                if (ret != null)
                    ret.setSuper(null);
                mChildren[mCur] = oper;
                oper.setSuper(this);
                return ret;
            }
        }

        override public bool appendLeftChild(IRTSLinker oper)
        {
            return false;
        }

        override public bool isStructure()
        {
            return true;
        }

        override public IRTSRunner createRunner()
        {
            if (mChildren[0] == null && mChildren[1] == null)
                return null;
            org.vr.rts.runner.RTSForR r = new org.vr.rts.runner.RTSForR(mChildren[0], mChildren[1], mChildren[2],
                    mChildren[3]);
            return r;
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (mChildren[3] == null)
            {
                return IRTSDefine.Error.Compiling_DenyLinker;
            }
            for (int i = 0; i < mChildren.Length; i++)
            {
                if (mChildren[i] != null)
                    compileList.add(mChildren[i]);
            }
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSForL oper = new RTSForL();
            oper.mSrc = src;
            return oper;
        }

        override public string ToString()
        {
            return org.vr.rts.util.RTSUtil.linkString(' ', mSrc, "(", mChildren[0], ";",
                    mChildren[1], ";", mChildren[2], ")", mChildren[3]);
        }
    }
}