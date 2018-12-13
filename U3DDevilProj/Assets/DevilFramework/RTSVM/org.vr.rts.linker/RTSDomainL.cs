using org.vr.rts.runner;
using org.vr.rts.util;

namespace org.vr.rts.linker
{

    public class RTSDomainL : RTSLinker
    {

        IRTSLinker mLeft;
        string mDomain;
        IRTSLinker mArgs;

        public RTSDomainL()
            : base(IRTSDefine.Linker.DOMAIN)
        {

        }

        override public IRTSDefine.Error onCompile(util.RTSList<IRTSLinker> compileList)
        {
            if (mLeft == null || !RTSUtil.isGoodName(mDomain))
                return IRTSDefine.Error.Compiling_DenyLinker;
            if (mArgs != null && mArgs.getId() != IRTSDefine.Linker.BRACKET)
                return IRTSDefine.Error.Compiling_DenyLinker;
            compileList.add(mLeft);
            compileList.add(mArgs);
            return 0;
        }

        override public IRTSRunner createRunner()
        {
                RTSBracketL args = mArgs as RTSBracketL;
            if(args == null)
            {
                return new RTSDomainR(mLeft, mDomain);
            }
            else
            {
                return new RTSDomainR(mLeft, mDomain, args.getChildAsList());
            }
        }
        
        override public IRTSLinker createInstance(string src)
        {
            RTSDomainL dom = new RTSDomainL();
            dom.mSrc = src;
            return dom;
        }

        public override IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (string.IsNullOrEmpty(mDomain))
            {
                mDomain = linker.getSrc();
                return this;
            }
            else
            {
                var ret = mArgs;
                if (ret != null)
                    ret.setSuper(null);
                mArgs = linker;
                if (mArgs != null)
                    mArgs.setSuper(this);
                return ret;
            }
        }

        public override bool appendLeftChild(IRTSLinker linker)
        {
            if(mLeft == null)
            {
                mLeft = linker;
                mLeft.setSuper(this);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}