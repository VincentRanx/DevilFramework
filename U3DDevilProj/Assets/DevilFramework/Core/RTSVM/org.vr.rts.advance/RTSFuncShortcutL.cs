using org.vr.rts.linker;
using org.vr.rts.runner;
using org.vr.rts.util;

namespace org.vr.rts.advance
{

    public class RTSFuncShortcutL : RTSLinker
    {

        private IRTSLinker mArg;
        private IRTSDefine.Property mProperty;
        private IRTSType mCastType;

        public RTSFuncShortcutL()
            : base(IRTSDefine.Linker.FUNCTION)
        {

        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (mArg == null)
            {
                if (linker.getId() != IRTSDefine.Linker.BRACKET)
                    return linker;
                mArg = linker;
                linker.setSuper(this);
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
            RTSList<IRTSRunner> lrs = null;
            if (mArg != null)
            {
                RTSList<IRTSLinker> lins = ((RTSBracketL)mArg).getChildAsList();
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
            }
            r = new RTSFuncR(mCastType, mSrc, lrs);
            return r;
        }

        override public IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList)
        {
            if (!RTSUtil.isGoodName(mSrc))
                return IRTSDefine.Error.Compiling_DenyLinker;
            if (mArg != null)
                compileList.add(mArg);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSFuncShortcutL lin = new RTSFuncShortcutL();
            lin.mSrc = src;
            return lin;
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