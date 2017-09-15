using org.vr.rts.component;

namespace org.vr.rts.advance
{

    public class RTSInlineCompileR : IRTSRunner
    {
        string source;
        IRTSRunner child;
        RTSCompiler compiler;
        bool doCompile;

        public RTSInlineCompileR(string src)
        {
            source = src;
            compiler = new RTSCompiler();
        }

        public IRTSDefine.Stack applyStack()
        {
            return 0;
        }

        public bool isConst()
        {
            return false;
        }

        /**
         * �������߳�(RTSThread)����ʱ����,ִ��һЩ��ʼ������
         */
        public void loadedOnThread()
        {
            child = null;
            doCompile = !string.IsNullOrEmpty(source);
        }

        /**
         * ���� return ��Ϣ
         * 
         * @param returnTppe
         *            IRTSDefine.Scope.ACTION_RETURN or
         *            IRTSDefine.Scope.ACTION_CONTINUE or
         *            IRTSDefine.Scope.ACTION_BREAK
         * @param value
         * @return true �����ü������Ѿ�������ɣ����ԴӼ���������Ƴ�
         */
        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        /**
         * ��ȡ����ֵ
         * 
         * @return
         */
        public object getOutput()
        {
            return child == null ? null : child.getOutput();
        }

        /**
         * �������߳�(RTSThread)�м���,����ֵΪ��ʾreturn�����ͣ���return��break��continue
         * 
         * @param thread
         * @return returnType
         */
        public IRTSDefine.Stack run(IRTSStack stack)
        {
            if (doCompile)
            {
                doCompile = false;
                compiler.loadSource(source);
                while (compiler.isCompiling())
                {
                    IRTSDefine.Error err = compiler.onCompile(stack.getThread().getEngine());
                    if (err != 0)
                    {
                        IRTSLog log = stack.getThread().getEngine().getLogger();
                        if (log != null)
                        {
                            log.logError("�޷��������� \"" + source + "\"");
                        }
                        return 0;
                    }
                }
                IRTSLinker l = compiler.getRootLinker();
                child = l.createRunner();
                if (stack.getThread().loadRunner(child))
                    return 0;
            }
            return 0;
        }

        public bool evaluate(IRTSStack stack, object value)
        {
            return false;
        }
    }
}