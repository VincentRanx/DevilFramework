using org.vr.rts.util;

namespace org.vr.rts.linker
{

    public class RTSCommaL : RTSLinker
    {

        private RTSList<IRTSLinker> mLins;
        private IRTSLinker mCur;

        public RTSCommaL()
            : base(IRTSDefine.Linker.COMMA)
        {

            mLins = new RTSList<IRTSLinker>(3);
        }

        override public bool isPriority(IRTSLinker linker)
        {
            if (linker.getId() == mId)
                return false;
            else
                return base.isPriority(linker);
        }

        override public IRTSLinker appendRightChild(IRTSLinker linker)
        {
            if (linker.getId() == mId)
            {
                mLins.add(mCur);
                mCur = null;
                return this;
            }
            else
            {
                IRTSLinker lin = mCur;
                if (lin != null)
                    lin.setSuper(null);
                mCur = linker;
                linker.setSuper(this);
                return lin;
            }
        }

        override public bool appendLeftChild(IRTSLinker linker)
        {
            if (mLins.length() == 0)
            {
                mLins.add(linker);
                linker.setSuper(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        override public IRTSRunner createRunner()
        {
            IRTSLinker l = mLins.getLast();
            return l == null ? null : l.createRunner();
        }

        public RTSList<IRTSLinker> getChildren()
        {
            return mLins;
        }

        override public IRTSDefine.Error onCompile(RTSList<IRTSLinker> compileList)
        {
            if (mCur != null)
            {
                mLins.add(mCur);
                mCur = null;
            }
            compileList.addList(mLins);
            return 0;
        }

        override public IRTSLinker createInstance(string src)
        {
            RTSCommaL l = new RTSCommaL();
            l.mSrc = src;
            return l;
        }

        override public string ToString()
        {
            if (mLins.length() == 0)
                return mSrc;
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            for (int i = 0; i < mLins.length(); i++)
            {
                IRTSLinker l = mLins.get(i);
                if (l != null)
                    buf.Append(l.ToString());
                buf.Append(mSrc);
            }
            return buf.ToString();
        }

        public bool isVarList()
        {
            for (int i = 0; i < mLins.length(); i++)
            {
                IRTSLinker l = mLins.get(i);
                if (l == null || l.getId() != IRTSDefine.Linker.VARIABLE)
                    return false;
                if (!RTSUtil.isGoodName(l.getSrc()))
                    return false;
            }
            return true;
        }

        public int listSize()
        {
            return mLins.length();
        }
    }
}