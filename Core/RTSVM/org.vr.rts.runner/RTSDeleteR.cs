using org.vr.rts.linker;
using org.vr.rts.modify;

namespace org.vr.rts.runner
{

    public class RTSDeleteR : IRTSRunner
    {

        private org.vr.rts.util.RTSList<org.vr.rts.linker.RTSVariableL> mVars;

        public RTSDeleteR(org.vr.rts.util.RTSList<org.vr.rts.linker.RTSVariableL> vars)
        {
            mVars = vars;
        }

        public IRTSDefine.Stack applyStack()
        {
            return 0;
        }

        public bool isConst()
        {
            return false;
        }

        public void loadedOnThread()
        {

        }

        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        public object getOutput()
        {
            return RTSVoid.VOID;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (mVars != null)
            {
                IRTSEngine eng = stack.getThread().getEngine();
                for (int i = 0; i < mVars.length(); i++)
                {
                    RTSVariableL vl = mVars.get(i);
                    if (vl == null)
                        continue;
                    bool var = vl.getId() == IRTSDefine.Linker.VARIABLE;
                    if (!var)
                    {
                        eng.removeFunction(vl.getSrc(), vl.getArgc());
                    }
                    else if ((vl.getProperty() & IRTSDefine.Property.GLOBAL) == 0)
                    {
                        stack.removeVar(vl.getSrc());
                    }
                    else
                    {
                        eng.removeVar(vl.getSrc());
                    }
                }
            }
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}
