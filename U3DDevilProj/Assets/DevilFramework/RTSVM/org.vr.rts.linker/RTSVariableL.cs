using org.vr.rts.runner;
using org.vr.rts.util;

namespace org.vr.rts.linker
{

    public class RTSVariableL : RTSLinker
    {

        private IRTSLinker mArg;
        private object mConst;
        private IRTSDefine.Property mProperty;
        private IRTSType mCastType;
        private bool mBeConst;

        public RTSVariableL()
            : base(IRTSDefine.Linker.VARIABLE)
        {

        }

        public RTSVariableL(object asConstValue)
            : base(IRTSDefine.Linker.VARIABLE)
        {

            mConst = asConstValue;
            mBeConst = true;
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (mArg == null && !mBeConst)
            {
                if (linker.getId() != IRTSDefine.Linker.BRACKET)
                    return linker;
                mArg = linker;
                linker.setSuper(this);
                mId = IRTSDefine.Linker.FUNCTION;
                return null;
            }
            else
            {
                return linker;
            }
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            if (linker.getId() == IRTSDefine.Linker.BRACKET)
            {
                IRTSType ctp = ((RTSBracketL)linker).getCastType();
                mCastType = ctp;
                return ctp != null;
            }
            else if (linker.getId() == IRTSDefine.Linker.PROPERTY)
            {
                mProperty |= ((RTSPropertyL)linker).getProperty();
                return true;
            }
            else if (linker.getId() == IRTSDefine.Linker.TYPE)
            {
                mProperty |= IRTSDefine.Property.DECALRE;
                mCastType = ((RTSTypeL)linker).getRTSType();
                linker.setSuper(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        override public IRTSRunner createRunner()
        {
            IRTSRunner r = null;
            if (mId == IRTSDefine.Linker.VARIABLE)
            {
                if (mConst == null && !mBeConst)
                    r = new RTSVariableR(mProperty, mCastType, mSrc);
                else
                    r = new RTSVariableR(mCastType, mConst);
                return r;
            }
            else if (mId == IRTSDefine.Linker.FUNCTION)
            {
                RTSList<IRTSLinker> lins = ((RTSBracketL)mArg).getChildAsList();
                RTSList<IRTSRunner> lrs = null;
                if (lins != null)
                {
                    lrs = new RTSList<IRTSRunner>(lins.length());
                    for (int i = 0; i < lins.length(); i++)
                    {
                        IRTSLinker l = lins.get(i);
                        if (l == null)
                            lrs.add(null);
                        else
                            lrs.add(l.createRunner());
                    }
                }
                r = new RTSFuncR(mCastType, mSrc, lrs);
                return r;
            }
            else
            {
                return null;
            }
        }

        override public IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList)
        {
            if (mBeConst)
            {
                return 0;
            }
            else if (mId == IRTSDefine.Linker.VARIABLE)
            {
                mConst = RTSConverter.getCompileValue(mSrc);
                if (mConst == null && !RTSUtil.isGoodName(mSrc))
                    return IRTSDefine.Error.Compiling_DenyLinker;
                return 0;
            }
            else if (mId == IRTSDefine.Linker.FUNCTION)
            {
                if (!RTSUtil.isGoodName(mSrc))
                    return IRTSDefine.Error.Compiling_DenyLinker;
                compileList.add(mArg);
                return 0;
            }
            else
            {
                return IRTSDefine.Error.Compiling_DenyLinker;
            }
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSVariableL var;
            if (mBeConst)
                var = new RTSVariableL(mConst);
            else
                var = new RTSVariableL();
            var.mSrc = src;
            return var;
        }

        override public string ToString()
        {
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            if (mCastType != null)
            {
                buf.Append('(').Append(mCastType.typeName()).Append(')');
            }
            if (mProperty != 0)
            {
                buf.Append(RTSUtil.propertyName(mProperty));
            }
            buf.Append(mSrc);
            if (mArg != null)
                buf.Append(mArg.ToString());
            return buf.ToString();
        }

        public int getArgc()
        {
            if (mArg == null)
                return 0;
            else
                return ((RTSBracketL)mArg).lengthAsList();
        }

        public IRTSDefine.Property getProperty()
        {
            return mProperty;
        }
    }
}