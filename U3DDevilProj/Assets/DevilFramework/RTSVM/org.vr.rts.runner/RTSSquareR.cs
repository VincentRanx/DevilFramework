using org.vr.rts.modify;

namespace org.vr.rts.runner
{

    public class RTSSquareR : IRTSRunner
    {

        private IRTSLinker mVar;
        private IRTSRunner mVarR;
        private IRTSLinker mIndex;
        private IRTSRunner mIndexR;
        private util.RTSList<IRTSLinker> mArray;
        private IRTSRunner[] mArrayR;
        private bool mVarLoaded;
        private bool mIndexLoaded;
        private int mListCur;
        private object mValue;
        private IRTSRunner mOutput;

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
            mValue = null;
            mOutput = null;
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
            else if(mVarR == null)
            {
                mValue = new object[0];
            }
            else
            {
                object obj = mVarR.getOutput();
                object index = mIndexR == null ? null : mIndexR.getOutput();
                if(obj == null)
                {
                    mValue = null;
                }
                else if( index is string)
                {
                    var eng = stack.getThread().getEngine();
                    IRTSType tp = eng.getRTSType(obj.GetType());
                    mValue = tp.getProperty(eng, obj, RTSString.stringOf(index));
                }
                else if (obj is System.Collections.IList )
                {
                    int n = RTSInteger.valueOf(index);
                    mValue = ((System.Collections.IList)obj)[n];
                }
                else
                {
                    int n = RTSInteger.valueOf(index);
                    mValue = n == 0 ? obj : null;
                }
                mOutput = mValue as IRTSRunner;
                if (stack.getThread().loadRunner(mOutput))
                    return 0;
            }
            if(mOutput != null)
            {
                mValue = mOutput.getOutput();
                mOutput = mValue as IRTSRunner;
                if (stack.getThread().loadRunner(mOutput))
                {
                    return 0;
                }
            }
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            object obj = mVarR == null ? null : mVarR.getOutput();
            object index = mIndexR == null ? null : mIndexR.getOutput();
            if (obj == null)
            {
                return false;
            }
            else if (index is int && obj is System.Collections.IList)
            {
                int n = RTSInteger.valueOf(index);
                ((System.Collections.IList)obj)[n] = value;
                mValue = value;
                return true;
            }
            else if (index != null)
            {
                var eng = stack.getThread().getEngine();
                var type = eng.getRTSType(obj.GetType());
                mValue = type.setProperty(eng, obj, RTSString.stringOf(index), value);
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
