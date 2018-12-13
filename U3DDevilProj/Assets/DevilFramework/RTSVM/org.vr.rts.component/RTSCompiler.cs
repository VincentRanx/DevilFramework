using org.vr.rts.linker;
using org.vr.rts.util;

namespace org.vr.rts.component
{

    public class RTSCompiler
    {

        private RTSTextReader mReader;
        private bool mCompiling;
        private IRTSLinker mExeLinker;
        private IRTSLinker mTempLinker;

        private RTSList<IRTSLinker> mCompileList;
        private bool mBuild;
        private bool mCompile;

        public RTSCompiler()
        {
            mReader = new RTSTextReader();
            mReader.SetSeperator(" \r\t");
            mReader.SetOperators("+-*/%&|~!^=<>?:;,@\n");
            mCompileList = new RTSList<IRTSLinker>(10);
        }

        public bool isCompiling()
        {
            return mCompiling;
        }

        public void reset()
        {
            mCompiling = false;
            mReader.Reset();
            mExeLinker = null;
            mTempLinker = null;
        }

        public RTSTextReader getReader()
        {
            return mReader;
        }

        public bool loadSource(string text)
        {
            if (RTSUtil.isNullOrEmpty(text))
                return false;
            mReader.Reload(text);
            mCompiling = mReader.hasNext();
            if (mCompiling)
            {
                mBuild = true;
                mCompile = true;
                mTempLinker = new RTSExecL();
                mCompileList.clear();
            }
            return mCompiling;
        }

        public string getCompilingSource()
        {
            return mReader.getSource();
        }

        public IRTSLinker getRootLinker()
        {
            return mExeLinker;
        }

        public IRTSDefine.Error onCompile(IRTSEngine lib)
        {
            if (mCompiling)
            {
                IRTSDefine.Error ret = 0;
                if (mBuild)
                {
                    ret = onBuild(lib);
                }
                else if (mCompile)
                {
                    ret = onCompileBuild(lib);
                }
                else
                {
                    mCompiling = false;
                    return 0;
                }
                if (ret != 0)
                {
                    mCompiling = false;
                }
                return ret;
            }
            return 0;
        }

        private IRTSDefine.Error onBuild(IRTSEngine lib)
        {
            mReader.fixCharsOffset();
            if (mReader.hasNext())
            {
                string src = mReader.nextWord(lib);
                IRTSLinker newLinker = lib.newLinker(src);
                IRTSLinker right = newLinker;
                IRTSLinker left = getPriorityRoot(mTempLinker, right);
                if (left.isPriority(right))
                {
                    if (!right.appendLeftChild(left))
                    {
                        mTempLinker = left;
                        return IRTSDefine.Error.Compiling_InvalidLinkerL;
                    }
                    mTempLinker = right;
                }
                else
                {
                    IRTSLinker replace = left.appendRightChild(right);
                    if (replace == null)
                    {
                        mTempLinker = right;
                    }
                    else if (replace == left)
                    {
                        mTempLinker = left;
                    }
                    else if (replace == right)
                    {
                        mTempLinker = right;
                        return IRTSDefine.Error.Compiling_InvalidLinkerR;
                    }
                    else
                    {
                        mTempLinker = right;
                        if (!right.appendLeftChild(replace))
                        {
                            mTempLinker = replace;
                            return IRTSDefine.Error.Compiling_InvalidLinkerL_rep;
                        }
                    }
                }
                return 0;
            }
            else
            {
                mBuild = false;
                mExeLinker = getDeepRoot(mTempLinker);
                if (mExeLinker != null)
                    mCompileList.add(mExeLinker);
                return 0;
            }
        }

        private IRTSDefine.Error onCompileBuild(IRTSEngine lib)
        {
            if (mCompileList.length() > 0)
            {
                IRTSLinker l = mCompileList.removeLast();
                mTempLinker = l;
                if (l != null)
                {
                    return l.onCompile(mCompileList);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                mCompile = false;
                return 0;
            }
        }

        private IRTSLinker getDeepRoot(IRTSLinker linker)
        {
            IRTSLinker l = linker;
            while (l != null)
            {
                IRTSLinker l1 = l.getSuper();
                if (l1 == null)
                    break;
                else
                    l = l1;
            }
            return l;
        }

        private IRTSLinker getPriorityRoot(IRTSLinker left, IRTSLinker right)
        {
            IRTSLinker l = left;
            while (l != null && l.isPriority(right))
            {
                IRTSLinker l1 = l.getSuper();
                if (l1 == null)
                    break;
                else
                    l = l1;
            }
            return l;
        }

        public IRTSLinker getCompilingLinker()
        {
            return mTempLinker;
        }
    }
}