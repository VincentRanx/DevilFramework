using org.vr.rts.runner;

namespace org.vr.rts.linker
{

    public class RTSExecL : RTSLinker
    {

        private IRTSDefine.Linker mRId;
        private IRTSLinker mTempL;
        private org.vr.rts.util.RTSList<IRTSLinker> mChildrenL;
        private bool mClosed;

        public RTSExecL()
            : base(0)
        {

            mRId = 0;
        }

        public RTSExecL(IRTSDefine.Linker id, IRTSDefine.Linker rightId)
            : base(id)
        {

            mRId = rightId;
        }

        override public bool isPriority(IRTSLinker linker)
        {
            return mClosed && mId != 0;
        }

        override public bool isStructure()
        {
            return true;
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (linker.getId() == mRId)
            {
                mClosed = true;
                return this;
            }
            else if (linker.getId() == IRTSDefine.Linker.SEMICOLON)
            {
                if (mTempL != null)
                {
                    if (mChildrenL == null)
                        mChildrenL = new org.vr.rts.util.RTSList<IRTSLinker>(2, 5);
                    mChildrenL.add(mTempL);
                    mTempL = null;
                }
                return this;
            }
            else
            {
                if (mTempL != null && mTempL.isStructure())
                {
                    if (mChildrenL == null)
                        mChildrenL = new org.vr.rts.util.RTSList<IRTSLinker>(2, 5);
                    mChildrenL.add(mTempL);
                    mTempL = null;
                }
                IRTSLinker ret = mTempL;
                if (ret != null)
                {
                    ret.setSuper(null);
                }
                mTempL = linker;
                linker.setSuper(this);
                return ret;
            }
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            return false;
        }

        override public IRTSRunner createRunner()
        {
            if (mChildrenL != null)
            {
                return new RTSExecR(mChildrenL);
            }
            else
            {
                return null;
            }
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (!mClosed && mId != 0)
                return IRTSDefine.Error.Compiling_DenyLinker;
            if (mTempL != null)
            {
                if (mChildrenL == null)
                    mChildrenL = new org.vr.rts.util.RTSList<IRTSLinker>(1);
                mChildrenL.add(mTempL);
                mTempL = null;
            }
            if (mChildrenL != null)
            {
                for (int i = 0; i < mChildrenL.length(); i++)
                {
                    IRTSLinker l = mChildrenL.get(i);
                    if (l != null)
                        compileList.add(l);
                }
            }
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSExecL linker = new RTSExecL(mId, mRId);
            linker.mSrc = src;
            return linker;
        }

        override public string ToString()
        {
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            buf.Append('{');
            if (mChildrenL != null && mChildrenL.length() > 0)
            {
                buf.Append('\n');
                for (int i = 0; i < mChildrenL.length(); i++)
                {
                    IRTSLinker lin = mChildrenL.get(i);
                    if (lin != null)
                    {
                        string s = lin.ToString();
                        int off = 0;
                        for (int j = 0; j < s.Length; j++)
                        {
                            if (s[j] == '\n')
                            {
                                buf.Append('\t').Append(s.Substring(off, j - off))
                                        .Append('\n');
                                off = j + 1;
                            }
                            else if (j == s.Length - 1)
                            {
                                buf.Append('\t').Append(s.Substring(off))
                                        .Append(';').Append('\n');

                            }
                        }
                    }
                }
            }
            buf.Append('}');
            return buf.ToString();
        }
    }
}
