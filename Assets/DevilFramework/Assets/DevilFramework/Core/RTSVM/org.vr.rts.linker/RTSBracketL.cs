using org.vr.rts.runner;
using org.vr.rts.util;

namespace org.vr.rts.linker
{

    public class RTSBracketL : RTSLinker
    {

        protected IRTSDefine.Linker mRId;
        protected IRTSType mCastType;
        protected IRTSLinker mChild;
        private bool mClosed;

        public RTSBracketL(IRTSDefine.Linker id, IRTSDefine.Linker rightId)
            : base(id)
        {

            mRId = rightId;
        }

        override public bool isPriority(IRTSLinker linker)
        {
            if (!mClosed)
                return false;
            else
                return base.isPriority(linker);
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (mClosed)
                return linker;
            if (linker.getId() == mRId)
            {
                mClosed = true;
                return this;
            }
            else
            {
                IRTSLinker ret = mChild;
                if (ret != null)
                    ret.setSuper(null);
                mChild = linker;
                linker.setSuper(this);
                return ret;
            }
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            if (linker.getId() == IRTSDefine.Linker.BRACKET)
            {
                mCastType = ((RTSBracketL)linker).getCastType();
                return mCastType != null;
            }
            else
            {
                return false;
            }
        }

        override public IRTSRunner createRunner()
        {
            if (mChild != null)
            {
                IRTSRunner r = mChild.createRunner();
                if (mCastType == null || r == null)
                {
                    return r;
                }
                else
                {
                    return new RTSCastR(mCastType, r);
                }
            }
            else
            {
                return null;
            }
        }

        override public IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList)
        {
            if (!mClosed)
                return IRTSDefine.Error.Compiling_DenyLinker;
            if (mChild != null)
                compileList.add(mChild);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSBracketL br = new RTSBracketL(mId, mRId);
            br.mSrc = src;
            return br;
        }

        override public string ToString()
        {
            if (mCastType == null)
                return RTSUtil.linkString(' ', mSrc, mChild,
                        RTSUtil.isNullOrEmpty(mSrc) ? null : (mSrc[0] + 1).ToString());
            else
                return RTSUtil.linkString(' ', '(', mCastType.typeName(), ')', mSrc, mChild,
                        RTSUtil.isNullOrEmpty(mSrc) ? "" : (mSrc[0] + 1).ToString());
        }

        public IRTSType getCastType()
        {
            if (mCastType == null && mChild != null && mChild.getId() == IRTSDefine.Linker.TYPE && mClosed)
            {
                return ((RTSTypeL)mChild).getRTSType();
            }
            else
            {
                return null;
            }
        }

        public bool isVarList()
        {
            if (mChild is RTSCommaL)
                return ((RTSCommaL)mChild).isVarList();
            else
                return mChild == null
                        || (mChild.getId() == IRTSDefine.Linker.VARIABLE && RTSUtil.isGoodName(mChild.getSrc()));
        }

        public int lengthAsList()
        {
            if (mChild == null)
                return 0;
            else if (mChild is RTSCommaL)
                return ((RTSCommaL)mChild).listSize();
            else
                return 1;
        }

        public RTSList<IRTSLinker> getChildAsList()
        {
            if (mChild == null)
            {
                return null;
            }
            else if (mChild is RTSCommaL)
            {
                return ((RTSCommaL)mChild).getChildren();
            }
            else
            {
                org.vr.rts.util.RTSList<IRTSLinker> lst = new org.vr.rts.util.RTSList<IRTSLinker>(1);
                lst.add(mChild);
                return lst;
            }
        }

        public IRTSLinker getChild()
        {
            return mChild;
        }
    }
}