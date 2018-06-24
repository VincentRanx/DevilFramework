namespace org.vr.rts.linker
{

    public class RTSEvaluateL : RTSBinaryL
    {

        private RTSBinaryL mPreLin;

        public RTSEvaluateL(RTSBinaryL preLin)
            : base(IRTSDefine.Linker.EVALUATE)
        {

            mPreLin = preLin;
        }

        override public IRTSRunner createRunner()
        {
            return new org.vr.rts.runner.RTSEvaluateR(mLeft, mRight, mPreLin);
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSEvaluateL lin = new RTSEvaluateL(mPreLin);
            lin.mSrc = src;
            return lin;
        }
    }
}
