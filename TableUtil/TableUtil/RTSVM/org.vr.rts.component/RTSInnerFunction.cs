using org.vr.rts.runner;
using org.vr.rts.util;

namespace org.vr.rts.component
{

    public class RTSInnerFunction : IRTSFunction
    {
        private bool mLimitArg;
        private IRTSType mReturnType;
        private RTSList<IRTSLinker> mArg;
        private IRTSLinker mBody;

        public RTSInnerFunction(IRTSType returnType, RTSList<IRTSLinker> arg, IRTSLinker body)
        {
            mLimitArg = true;
            mArg = arg;
            mBody = body;
            mReturnType = returnType;
        }
        
        public RTSInnerFunction(IRTSLinker body)
        {
            mLimitArg = false;
            mReturnType = null;
            mArg = null;
            mBody = body;
        }

        public int argSize()
        {
            return mLimitArg ? (mArg == null ? 0 : mArg.length()) : -1;
        }

        public IRTSType returnType()
        {
            return mReturnType;
        }

        public IRTSRunner createRunner(object[] args)
        {
            RTSInnerFuncR r = new RTSInnerFuncR(args, this);
            return r;
        }

        public string getArgDef(int index)
        {
            if (mArg != null)
            {
                IRTSLinker l = mArg.get(index);
                return l == null ? null : l.getSrc();
            }
            else
            {
                return null;
            }
        }

        public IRTSLinker getBody()
        {
            return mBody;
        }
    }
}