namespace org.vr.rts
{

    public interface IRTSRunner
    {

        IRTSDefine.Stack applyStack();

        bool isConst();

        /**
         * 被运行线程(RTSThread)加载时调用,执行一些初始化操作
         */
        void loadedOnThread();

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
        bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value);

        /**
         * 获取返回值
         * 
         * @return
         */
        object getOutput();

        /**
         * 在运行线程(RTSThread)中计算,返回值为表示return的类型，有return，break，continue
         * 
         * @param thread
         * @return returnType
         */
        IRTSDefine.Stack run(IRTSStack stack);

        bool evaluate(IRTSStack stack, object value);
    }
}