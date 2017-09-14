using org.vr.rts.util;

namespace org.vr.rts.linker
{

    public abstract class RTSLinker : IRTSLinker
    {

        protected string mSrc;
        protected IRTSLinker mSuper;
        protected IRTSDefine.Linker mId;

        public RTSLinker(IRTSDefine.Linker id)
        {
            mId = id;
        }

        public IRTSDefine.Linker getId()
        {
            return mId;
        }

        public void setSuper(IRTSLinker linker)
        {
            mSuper = linker;
        }

        public IRTSLinker getSuper()
        {
            return mSuper;
        }

        public string getSrc()
        {
            return mSrc;
        }

        virtual public bool isPriority(IRTSLinker linker)
        {
            return (mId & IRTSDefine.Linker.PRIORITY_MASK) >= (linker.getId() & IRTSDefine.Linker.PRIORITY_MASK);
        }

        virtual public bool isStructure()
        {
            return false;
        }

        abstract public IRTSLinker appendRightChild(IRTSLinker linker);

        abstract public bool appendLeftChild(IRTSLinker linker);

        abstract public IRTSRunner createRunner();

        abstract public IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList);

        abstract public IRTSLinker createInstance(string src);
    }
}