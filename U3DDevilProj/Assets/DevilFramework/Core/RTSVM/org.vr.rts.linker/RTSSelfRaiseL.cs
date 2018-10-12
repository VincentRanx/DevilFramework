namespace org.vr.rts.linker
{

    //++ --
    public class RTSSelfRaiseL : RTSLinker
    {

        private IRTSLinker mVar;
        private bool mVarAtLeft;

        public RTSSelfRaiseL(IRTSDefine.Linker id)
            : base(id)
        {

        }

        override public IRTSLinker appendRightChild(IRTSLinker oper)
        {
            IRTSLinker ret = mVar;
            if (ret != null)
                ret.setSuper(null);
            mVarAtLeft = false;
            mVar = oper;
            oper.setSuper(this);
            return ret;
        }

        override public bool appendLeftChild(IRTSLinker oper)
        {

            mVar = oper;
            oper.setSuper(this);
            mVarAtLeft = true;
            return true;
        }

        override public bool isStructure()
        {
            return false;
        }

        override public IRTSRunner createRunner()
        {
            org.vr.rts.runner.RTSSelfRaiseR cal = new org.vr.rts.runner.RTSSelfRaiseR(mId, mVar.createRunner(),
                    mVarAtLeft);
            return cal;
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            if (mVar == null)
            {
                return IRTSDefine.Error.Compiling_DenyLinker;
            }
            compileList.add(mVar);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSSelfRaiseL oper = new RTSSelfRaiseL(mId);
            oper.mSrc = src;
            return oper;
        }

        override public string ToString()
        {
            if (mVarAtLeft)
                return mVar + mSrc;
            else
                return mSrc + mVar;
        }
    }
}
