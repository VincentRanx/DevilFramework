namespace org.vr.rts.runner
{

    public class RTSAndR : RTSBinaryR
    {

        public RTSAndR(IRTSLinker left, IRTSLinker right)
            : base(left, right)
        {

        }

        override protected void onFinalRun(IRTSStack stack, object left, object right)
        {
            mValue = (org.vr.rts.modify.RTSBool.valueOf(left) && org.vr.rts.modify.RTSBool.valueOf(right));
        }

        override protected bool skipRight()
        {
            object l = mLeft == null ? null : mLeft.getOutput();
            return !org.vr.rts.modify.RTSBool.valueOf(l);
        }
    }
}