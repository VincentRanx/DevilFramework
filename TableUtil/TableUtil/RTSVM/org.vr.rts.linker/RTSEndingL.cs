namespace org.vr.rts.linker
{

    public class RTSEndingL : RTSLinker
    {

        public RTSEndingL(IRTSDefine.Linker id)
            : base(id)
        {

        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            return linker;
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            return false;
        }

        override public IRTSRunner createRunner()
        {
            return null;
        }

        override public IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList)
        {
            return IRTSDefine.Error.Compiling_DenyLinker;
        }

        override public IRTSLinker createInstance(string src)
        {
            return this;
        }

        override public string ToString()
        {
            return mSrc == null ? base.ToString() : mSrc;
        }
    }
}