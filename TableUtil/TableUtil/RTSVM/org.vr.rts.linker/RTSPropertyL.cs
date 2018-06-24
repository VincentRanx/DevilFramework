namespace org.vr.rts.linker
{

    public class RTSPropertyL : RTSLinker
    {

        private IRTSDefine.Property mProperty;
        private IRTSDefine.Property mAllProperty;

        public RTSPropertyL(IRTSDefine.Property property)
            : base(IRTSDefine.Linker.PROPERTY)
        {

            mProperty = property;
            mAllProperty = property;
        }

        public IRTSDefine.Property getProperty()
        {
            return mAllProperty;
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (linker.getId() == IRTSDefine.Linker.PROPERTY)
            {
                mAllProperty |= ((RTSPropertyL)linker).getProperty();
                return this;
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
                mAllProperty |= ((RTSPropertyL)linker).getProperty();
                return true;
            }
            else
            {
                return false;
            }
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
            RTSPropertyL p = new RTSPropertyL(mProperty);
            p.mSrc = src;
            return p;
        }

        override public string ToString()
        {
            return mSrc;
        }

    }
}
