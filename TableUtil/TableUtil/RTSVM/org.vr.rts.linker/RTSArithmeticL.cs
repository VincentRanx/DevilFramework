namespace org.vr.rts.linker
{

    public class RTSArithmeticL : RTSBinaryL
    {

        public RTSArithmeticL(IRTSDefine.Linker id)
            : base(id)
        {

        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (mId == IRTSDefine.Linker.ADD || mId == IRTSDefine.Linker.SUB)
            {
                if (mRight == null)
                    return IRTSDefine.Error.Compiling_DenyLinker;
                if (mLeft != null)
                    compileList.add(mLeft);
                compileList.add(mRight);
                return 0;
            }
            else
            {
                return base.onCompile(compileList);
            }
        }

        override public IRTSRunner createRunner()
        {
            return new org.vr.rts.runner.RTSArithmeticR(mId, mLeft, mRight);
        }

        override public IRTSRunner createRunnerWith(IRTSLinker left, IRTSLinker right)
        {
            return new org.vr.rts.runner.RTSArithmeticR(mId, left, right);
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSArithmeticL add = new RTSArithmeticL(mId);
            add.mSrc = src;
            return add;
        }

    }
}