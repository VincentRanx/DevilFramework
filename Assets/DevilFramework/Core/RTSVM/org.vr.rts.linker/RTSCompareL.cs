namespace org.vr.rts.linker
{

    public class RTSCompareL : RTSBinaryL
    {

        public RTSCompareL(IRTSDefine.Linker id)
            : base(id)
        {

        }

        override public IRTSRunner createRunner()
        {
            return new org.vr.rts.runner.RTSCompareR(mId, mLeft, mRight);
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSCompareL lin = new RTSCompareL(mId);
            lin.mSrc = src;
            return lin;
        }

    }
}
