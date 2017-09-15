namespace org.vr.rts.runner
{

    public class RTSArithmeticR : RTSBinaryR
    {

        private IRTSDefine.Linker mId;

        public RTSArithmeticR(IRTSDefine.Linker id, IRTSLinker left, IRTSLinker right)
            : base(left, right)
        {

            mId = id;
        }

        override protected void onFinalRun(IRTSStack stack, object left, object right)
        {
            IRTSType tp;
            object t = left != null ? left : right;
            if (t != null)
            {
                tp = stack.getThread().getEngine().getRTSType(t.GetType());
            }
            else
            {
                tp = org.vr.rts.modify.RTSGeneral.TYPE;
            }
            switch (mId)
            {
                case IRTSDefine.Linker.ADD:
                    mValue = tp.add(left, right);
                    break;
                case IRTSDefine.Linker.SUB:
                    mValue = tp.sub(left, right);
                    break;
                case IRTSDefine.Linker.MUL:
                    mValue = tp.mul(left, right);
                    break;
                case IRTSDefine.Linker.DIV:
                    mValue = tp.div(left, right);
                    break;
                case IRTSDefine.Linker.MOD:
                    mValue = tp.mod(left, right);
                    break;
                case IRTSDefine.Linker.BITAND:
                    mValue = tp.and(left, right);
                    break;
                case IRTSDefine.Linker.BITOR:
                    mValue = tp.or(left, right);
                    break;
                case IRTSDefine.Linker.XOR:
                    mValue = tp.xor(left, right);
                    break;
                default:
                    break;
            }
        }
    }
}