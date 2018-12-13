using org.vr.rts.linker;
using org.vr.rts.util;

namespace org.vr.rts.advance
{

    public class AsyncL : RTSLinker
    {

        private IRTSRuntime mRuntime;
        private IRTSLinker mChild;
        //private IRTSType mCastType;

        public AsyncL(IRTSRuntime runtime)
            : base(IRTSDefine.Linker.COMMAND_H)
        {
            mRuntime = runtime;
        }

        override public IRTSRunner createRunner()
        {
            return new AsyncR(mRuntime, mChild);
        }

        override public IRTSLinker createInstance(string src)
        {
            AsyncL asyncl = new AsyncL(mRuntime);
            asyncl.mSrc = src;
            return asyncl;
        }

        public override IRTSLinker appendRightChild(IRTSLinker linker)
        {
            IRTSLinker ret = mChild;
            if (ret != null)
                ret.setSuper(null);
            mChild = linker;
            mChild.setSuper(this);
            return ret;
        }

        public override bool appendLeftChild(IRTSLinker linker)
        {
            if (linker.getId() == IRTSDefine.Linker.BRACKET)
            {
                IRTSType ctp = ((RTSBracketL)linker).getCastType();
                //mCastType = ctp;
                return ctp != null;
            }
            return false;
        }

        public override IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList)
        {
            if (mChild == null)
                return IRTSDefine.Error.Compiling_NullLinker;
            compileList.add(mChild);
            return 0;
        }
    }
}