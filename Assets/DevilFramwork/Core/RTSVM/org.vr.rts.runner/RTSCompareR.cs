namespace org.vr.rts.runner
{

    public class RTSCompareR : RTSBinaryR
    {

        private IRTSDefine.Linker mId;

        public RTSCompareR(IRTSDefine.Linker id, IRTSLinker left, IRTSLinker right)
            : base(left, right)
        {

            mId = id;
        }

        override protected void onFinalRun(IRTSStack stack, object left, object right)
        {
            IRTSType tp = null;
            object t = left != null ? left : right;
            if (t != null)
            {
                tp = stack.getThread().getEngine().getRTSType(t.GetType());
            }
            if (tp == null)
                tp = org.vr.rts.modify.RTSGeneral.TYPE;
            int cp = tp.rtsCompare(left, right);
            setFinalV(cp);
        }

        private void setFinalV(int cp)
        {
            switch (mId)
            {
                case IRTSDefine.Linker.MORE:
                    mValue = cp > 0;
                    break;
                case IRTSDefine.Linker.MOREQU:
                    mValue = cp >= 0;
                    break;
                case IRTSDefine.Linker.LESS:
                    mValue = cp < 0;
                    break;
                case IRTSDefine.Linker.LESSEQU:
                    mValue = cp <= 0;
                    break;
                case IRTSDefine.Linker.EQUAL:
                    mValue = cp == 0;
                    break;
                case IRTSDefine.Linker.NOTEQU:
                    mValue = cp != 0;
                    break;
                default:
                    mValue = false;
                    break;
            }
        }
    }
}