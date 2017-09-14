namespace org.vr.rts
{

    /**
     * ÔËËã·û¹¹ÔìÆ÷
     * 
     * @author Administrator
     * 
     */
    public interface IRTSLinker
    {

        IRTSDefine.Linker getId();

        void setSuper(IRTSLinker linker);

        IRTSLinker getSuper();

        string getSrc();

        IRTSLinker appendRightChild(IRTSLinker linker);

        bool appendLeftChild(IRTSLinker linker);

        bool isPriority(IRTSLinker linker);

        IRTSRunner createRunner();

        // return error code
        IRTSDefine.Error onCompile(org.vr.rts.util.RTSList<IRTSLinker> compileList);

        IRTSLinker createInstance(string src);

        bool isStructure();
    }
}