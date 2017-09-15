namespace org.vr.rts.linker
{

    public class RTSLogicL : RTSBinaryL
    {

        public RTSLogicL(IRTSDefine.Linker id)
            : base(id)
        {

        }

        override public IRTSRunner createRunner()
        {
            switch (mId)
            {
                case IRTSDefine.Linker.AND:
                    return new org.vr.rts.runner.RTSAndR(mLeft, mRight);
                case IRTSDefine.Linker.OR:
                    return new org.vr.rts.runner.RTSOrR(mLeft, mRight);
                case IRTSDefine.Linker.XOR:
                    return new org.vr.rts.runner.RTSXorR(mLeft, mRight);
                default:
                    return null;
            }
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSLogicL lin = new RTSLogicL(mId);
            lin.mSrc = src;
            return lin;
        }
    }
}
