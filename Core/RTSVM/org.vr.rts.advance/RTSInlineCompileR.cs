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
         * 被运行线程(RTSThread)加载时调用,执行一些初始化操作
         */
        public void loadedOnThread()
        {
            child = null;
            doCompile = !string.IsNullOrEmpty(source);
        }

        /**
         * 接受 return 消息
         * 
         * @param returnTppe
         *            IRTSDefine.Scope.ACTION_RETURN or
         *            IRTSDefine.Scope.ACTION_CONTINUE or
         *            IRTSDefine.Scope.ACTION_BREAK
         * @param value
         * @return true 表明该计算器已经计算完成，可以从计算队列中移除
         */
        public bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value)
        {
            return true;
        }

        /**
         * 获取返回值
         * 
         * @return
         */
        public object getOutput()
        {
            return child == null ? null : child.getOutput();
        }

        /**
         * 在运行线程(RTSThread)中计算,返回值为表示return的类型，有return，break，continue
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
                            log.logError("无法解析内容 \"" + source + "\"");
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