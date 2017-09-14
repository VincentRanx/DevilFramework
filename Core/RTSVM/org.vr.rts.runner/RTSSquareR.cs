namespace org.vr.rts.runner
{

    public class RTSSquareR : IRTSRunner
    {

        private IRTSLinker mVar;
        private IRTSRunner mVarR;
        private IRTSLinker mIndex;
        private IRTSRunner mIndexR;
        private org.vr.rts.util.RTSList<IRTSLinker> mArray;
        private IRTSRunner[] mArrayR;
        private bool mVarLoaded;
        private bool mIndexLoaded;
        private int mListCur;
        private object mValue;

        public RTSSquareR(IRTSLinker var, IRTSLinker index)
        {
            mVar = var;
            mIndex = index;
        }

        public RTSSquareR(org.vr.rts.util.RTSList<IRTSLinker> list)
        {
            mArray = list;
        }

        public IRTSDefine.Stack applyStack()
        {
            return 0;
        }

        public bool isConst()
        {
            return false;
        }

        public void loadedOnThread()
        {
            mVarLoaded = false;
            mIndexLoaded = false;
            mListCur = 0;
            if (mArray != null)
                mArrayR = new IRTSRunner[mArray.length()];
        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return false;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (!mVarLoaded)
            {
                mVarLoaded = true;
                mVarR = mVar == null ? null : mVar.createRunner();
                if (stack.getThread().loadRunner(mVarR))
                    return 0;
            }
            if (!mIndexLoaded)
            {
                mIndexLoaded = true;
                mIndexR = mIndex == null ? null : mIndex.createRunner();
                if (stack.getThread().loadRunner(mIndexR))
                    return 0;
            }
            while (mArray != null && mListCur < mArray.length())
            {
                IRTSLinker l = mArray.get(mListCur);
                mArrayR[mListCur] = l == null ? null : l.createRunner();
                if (stack.getThread().loadRunner(mArrayR[mListCur++]))
                    return 0;
            }
            if (mArrayR != null)
            {
                //org.vr.rts.typedef.RTSarray arr = new org.vr.rts.typedef.RTSarray(mArrayR.Length);
                object[] arr = new object[mArrayR.Length];
                for (int i = 0; i < mArrayR.Length; i++)
                {
                    object v = mArrayR[i] == null ? null : mArrayR[i].getOutput();
                    //arr.set(i, v);
                    arr[i] = v;
                }
                mValue = arr;
            }
            else
            {
                object obj = mVarR == null ? null : mVarR.getOutput();
                object index = mIndexR == null ? null : mIndexR.getOutput();
                int n = org.vr.rts.modify.RTSInteger.valueOf(index);
                if (obj != null && obj is System.Collections.IList)
                {
                    mValue = ((System.Collections.IList)obj)[n];
                }
                else
                {
                    mValue = n == 0 ? obj : null;
                }
            }
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            object obj = mVarR == null ? null : mVarR.getOutput();
            object index = mIndexR == null ? null : mIndexR.getOutput();
            int n = org.vr.rts.modify.RTSInteger.valueOf(index);
            if (obj != null && obj is System.Collections.IList)
            {
                ((System.Collections.IList)obj)[n] = value;
                mValue = value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public object getOutput()
        {
            return mValue;
        }
    }
}
