using org.vr.rts.component;
using org.vr.rts.util;

namespace org.vr.rts.runner
{

    public class RTSFuncDefineR : IRTSRunner
    {

        private string mFuncName;
        private RTSInnerFunction mFunction;

        public RTSFuncDefineR(IRTSType returnType, string funcName, RTSList<IRTSLinker> arg, IRTSLinker body)
        {
            mFunction = new RTSInnerFunction(returnType, arg, body);
            mFuncName = funcName;
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
            return modify.RTSVoid.VOID;
        }

        public IRTSDefine.Stack run(IRTSStack stack)
        {
            IRTSEngine engine = stack.getThread().getEngine();
            engine.addFunction(mFuncName, mFunction);
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }

    }
}