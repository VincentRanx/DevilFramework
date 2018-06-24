using org.vr.rts.linker;
using org.vr.rts.util;
using org.vr.rts.runner;

namespace org.vr.rts.advance
{

    public class RTSCmdL : RTSLinker
    {
        RTSList<IRTSLinker> mArgs;

        public RTSCmdL() : base(IRTSDefine.Linker.COMMAND_H)
        {
            mArgs = new RTSList<IRTSLinker>();
        }

        public override bool isPriority(IRTSLinker linker)
        {
            return linker.getId() == IRTSDefine.Linker.SEMICOLON;
        }

        public override bool appendLeftChild(IRTSLinker linker)
        {
            return false;
        }

        public override IRTSLinker appendRightChild(IRTSLinker linker)
        {
            linker.setSuper(this);
            mArgs.add(linker);
            return null;
        }

        public override IRTSLinker createInstance(string src)
        {
            RTSCmdL lin = new RTSCmdL();
            lin.mSrc = src;
            return lin;
        }

        public override IRTSRunner createRunner()
        {
            IRTSRunner r = null;
            RTSList<IRTSRunner> lrs = null;
            if(mArgs.length() > 0)
            {
                lrs = new RTSList<IRTSRunner>(mArgs.length());
                for (int i = 0; i < mArgs.length(); i++)
                {
                    IRTSLinker l = mArgs.get(i);
                    if (l == null)
                        lrs.add(null);
                    else
                        lrs.add(l.createRunner());
                }
            }
            r = new RTSFuncR(null, "_" + mSrc, lrs);
            return r;
        }

        public override IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList)
        {
            if (!RTSUtil.isGoodName(mSrc))
                return IRTSDefine.Error.Compiling_DenyLinker;
            for (int i = 0; i < mArgs.length(); i++)
            {
                compileList.add(mArgs.get(i));
            }
            return 0;
        }
    }
}