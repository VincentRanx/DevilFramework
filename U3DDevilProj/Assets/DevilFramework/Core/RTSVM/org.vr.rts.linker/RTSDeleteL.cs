using org.vr.rts.util;

namespace org.vr.rts.linker
{

    public class RTSDeleteL : RTSLinker
    {

        private org.vr.rts.util.RTSList<RTSVariableL> mVars;
        private IRTSLinker mCur;

        public RTSDeleteL()
            : base(IRTSDefine.Linker.DELETE)
        {
            mVars = new org.vr.rts.util.RTSList<RTSVariableL>(3);
        }

        //override public bool isPriority(IRTSLinker linker)
        //{
        //    return linker.isStructure()
        //            && linker.getId() != IRTSDefine.Linker.SEMICOLON;
        //}

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (linker.getId() == IRTSDefine.Linker.COMMA)
            {
                if (!(mCur is RTSVariableL))
                    return linker;
                mVars.add((RTSVariableL)mCur);
                mCur = null;
                return this;
            }
            else
            {
                IRTSLinker ret = mCur;
                if (ret != null)
                    ret.setSuper(null);
                mCur = linker;
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
            return new org.vr.rts.runner.RTSDeleteR(mVars);
        }

        override public IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList)
        {
            if (mSuper != null && !mSuper.isStructure())
                return IRTSDefine.Error.Compiling_DenyLinker;
            if (mCur != null)
            {
                if (!(mCur is RTSVariableL))
                    return IRTSDefine.Error.Compiling_DenyLinker;
                mVars.add((RTSVariableL)mCur);
                mCur = null;
            }
            for (int i = mVars.length() - 1; i >= 0; i--)
            {
                compileList.add(mVars.get(i));
            }
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSDeleteL dL = new RTSDeleteL();
            dL.mSrc = src;
            return dL;
        }

        override public string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(mSrc).Append(' ');
            for (int i = 0; i < mVars.length(); i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append(mVars.get(i));
            }
            return sb.ToString();
        }
    }
}