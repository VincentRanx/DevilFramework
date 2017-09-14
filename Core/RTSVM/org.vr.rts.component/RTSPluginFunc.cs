using org.vr.rts.runner;

namespace org.vr.rts.component
{

    public class RTSPluginFunc : IRTSFunction, IRTSPlugin
    {
        private IRTSType mReturnType;
        private RTSPluginDelegate mTarget;
        private int mArgC;

        public RTSPluginFunc(IRTSType retType, RTSPluginDelegate dele, int argC)
        {
            mReturnType = retType;
            mTarget = dele;
            mArgC = argC;
        }

        public object pluginFunction(object[] args)
        {
            return mTarget(args);
        }

        public int argSize()
        {
            return mArgC;
        }

        public string getArgDef(int index)
        {
            return "$"+index;
        }

        public IRTSLinker getBody()
        {
            return null;
        }

        public IRTSType returnType()
        {
            return mReturnType;
        }

        public IRTSRunner createRunner(object[] args)
        {
            return new RTSPluginFuncR(mReturnType, args, this);
        }
    }
}
