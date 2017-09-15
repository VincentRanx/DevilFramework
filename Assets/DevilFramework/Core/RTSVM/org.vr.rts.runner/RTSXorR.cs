namespace org.vr.rts.runner
{

    public class RTSXorR : RTSBinaryR
    {

        public RTSXorR(IRTSLinker left, IRTSLinker right)
            : base(left, right)
        {
        }

        override protected void onFinalRun(IRTSStack stack, object left, object right)
        {
            object t = left != null ? left : right;
            IRTSType tp = stack.getThread().getEngine().getRTSType(t == null ? null : t.GetType());
            mValue = tp.xor(left, right);
        }
    }
}
