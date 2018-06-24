using org.vr.rts.runner;

namespace org.vr.rts.linker
{

    public class RTSIfL : RTSLinker
    {

        private IRTSLinker mConditionL;
        private IRTSLinker mBodyL;
        private IRTSLinker mElseL;
        private bool mOnBody;
        private bool mOver;

        public RTSIfL()
            : base(IRTSDefine.Linker.IF)
        {

        }

        override public bool isPriority(IRTSLinker linker)
        {
            if (mOver)
                return linker.getId() != IRTSDefine.Linker.ELSE;
            else
                return false;
        }

        override public bool isStructure()
        {
            return true;
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            IRTSDefine.Linker id = linker.getId();
            if (mOnBody)
            {
                if (id == IRTSDefine.Linker.ELSE)
                {
                    if (mElseL != null)
                        return linker;
                    mOver = true;
                    mElseL = linker;
                    linker.setSuper(this);
                    return null;
                }
                else if (id == IRTSDefine.Linker.SEMICOLON)
                {
                    if (mBodyL == null || mOver)
                        return linker;
                    mOver = true;
                    return this;
                }
                else if (linker.isStructure())
                {
                    if (mBodyL != null)
                        return linker;
                    mOver = true;
                    mBodyL = linker;
                    linker.setSuper(this);
                    return null;
                }
                else
                {
                    if (mOver)
                    {
                        return linker;
                    }
                    IRTSLinker ret = mBodyL;
                    if (ret != null)
                        ret.setSuper(null);
                    mBodyL = linker;
                    linker.setSuper(this);
                    return ret;
                }
            }
            else
            {
                if (id == IRTSDefine.Linker.THEN)
                {
                    mOnBody = true;
                    return this;
                }
                IRTSLinker ret = mConditionL;
                if (ret != null)
                    ret.setSuper(null);
                mConditionL = linker;
                linker.setSuper(this);
                return ret;
            }

            //if (mConditionL == null)
            //{
            //    if (id != IRTSDefine.Linker.BRACKET)
            //        return linker;
            //    mConditionL = linker;
            //    linker.setSuper(this);
            //    return null;
            //}
            //else if (id == IRTSDefine.Linker.ELSE)
            //{
            //    if (mElseL != null)
            //        return linker;
            //    mOver = true;
            //    mElseL = linker;
            //    linker.setSuper(this);
            //    return null;
            //}
            //else if (id == IRTSDefine.Linker.SEMICOLON)
            //{
            //    if (mBodyL == null || mOver)
            //        return linker;
            //    mOver = true;
            //    return this;
            //}
            //else if (linker.isStructure())
            //{
            //    if (mBodyL != null)
            //        return linker;
            //    mOver = true;
            //    mBodyL = linker;
            //    linker.setSuper(this);
            //    return null;
            //}
            //else
            //{
            //    if (mOver)
            //        return linker;
            //    IRTSLinker ret = mBodyL;
            //    if (ret != null)
            //        ret.setSuper(null);
            //    mBodyL = linker;
            //    linker.setSuper(this);
            //    return ret;
            //}
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            return false;
        }

        override public IRTSRunner createRunner()
        {
            RTSIfElseR r = new RTSIfElseR(mConditionL, mBodyL, mElseL);
            return r;
        }

        override public IRTSDefine.Error onCompile(util.RTSList<IRTSLinker> compileList)
        {
            if (mConditionL == null || mBodyL == null )// || !mOver)
                return IRTSDefine.Error.Compiling_DenyLinker;
            compileList.add(mConditionL);
            compileList.add(mBodyL);
            if (mElseL != null)
                compileList.add(mElseL);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSIfL lin = new RTSIfL();
            lin.mSrc = src;
            return lin;
        }

        override public string ToString()
        {
            return util.RTSUtil.linkString('\0', mSrc, mConditionL, mBodyL, '\n', mElseL);
        }
    }
}
