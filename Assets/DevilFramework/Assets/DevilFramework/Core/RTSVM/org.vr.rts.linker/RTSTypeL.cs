using org.vr.rts.runner;
using org.vr.rts.util;

namespace org.vr.rts.linker
{

    public class RTSTypeL : RTSLinker
    {

        private IRTSType mType;
        private string mVarName;
        private IRTSLinker mArg;
        private IRTSLinker mBody;
        private IRTSDefine.Property mProperty;

        public RTSTypeL(IRTSType type)
            : base(IRTSDefine.Linker.TYPE)
        {

            mType = type;
        }

        public IRTSType getRTSType()
        {
            return mType;
        }

        override public bool isPriority(IRTSLinker linker)
        {
            if (mId == IRTSDefine.Linker.FUNCTION_DEFINE && mBody == null)
                return false;
            else
                return base.isPriority(linker);
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (mVarName == null)
            {
                if (linker.getId() != IRTSDefine.Linker.VARIABLE || !RTSUtil.isGoodName(linker.getSrc()))
                    return linker;
                mVarName = linker.getSrc();
                return this;
            }
            else if (mArg == null)
            {
                if (linker.getId() != IRTSDefine.Linker.BRACKET)
                    return linker;
                mId = IRTSDefine.Linker.FUNCTION_DEFINE;
                mArg = linker;
                linker.setSuper(this);
                return null;
            }
            else if (mBody == null)
            {
                if (linker.getId() != IRTSDefine.Linker.BRACKET_FLOWER)
                    return linker;
                mId = IRTSDefine.Linker.FUNCTION_DEFINE;
                mBody = linker;
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
            if (linker.getId() == IRTSDefine.Linker.PROPERTY)
            {
                mProperty |= ((RTSPropertyL)linker).getProperty();
                return true;
            }
            else
            {
                return false;
            }
        }

        override public IRTSRunner createRunner()
        {
            if (mId == IRTSDefine.Linker.TYPE)
            {
                if (mVarName == null)
                    return new RTSVariableR(null, mType);
                else
                    return new RTSVariableR(IRTSDefine.Property.DECALRE | mProperty, mType, mVarName);
            }
            else
            {
                return new RTSFuncDefineR(mType, mVarName, ((RTSBracketL)mArg).getChildAsList(), mBody);
            }
        }

        override public IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList)
        {
            if (mId == IRTSDefine.Linker.TYPE)
            {
                if (mVarName == null)
                    return 0;
                else
                    return RTSUtil.isGoodName(mVarName) ? 0 : IRTSDefine.Error.Compiling_DenyLinker;
            }
            else
            {
                if (mArg == null || mBody == null)
                    return IRTSDefine.Error.Compiling_DenyLinker;
                if (!((RTSBracketL)mArg).isVarList())
                    return IRTSDefine.Error.Compiling_DenyLinker;
                compileList.add(mArg);
                compileList.add(mBody);
                return 0;
            }
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSTypeL lin = new RTSTypeL(mType);
            lin.mSrc = src;
            return lin;
        }

        override public string ToString()
        {
            return RTSUtil.linkString(' ', mSrc, mVarName, mArg, mBody);
        }
    }
}