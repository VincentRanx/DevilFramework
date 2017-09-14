namespace org.vr.rts
{

    public interface IRTSEngine
    {

        object getVar(string varName);

        bool containsVar(string varName);

        void removeVar(string varName);

        void addVar(string varName, object var);

        void clearVars();

        /**
         * 判断是否是关键词
         * 
         * @param src
         * @return
         */
        bool isKeyWord(string src);

        /**
         * 获取日志打组建
         * 
         * @return
         */
        IRTSLog getLogger();

        /**
         * 创建运算符对象
         * 
         * @param src
         * @param off
         * @param end
         * @return
         */
        IRTSLinker newLinker(string src);

        /**
         * 添加方法定义
         * 
         * @param funcName
         * @param func
         */
        void addFunction(string funcName, IRTSFunction func);

        /**
         * 删除方法定义
         * 
         * @param funcName
         * @param argCount
         */
        void removeFunction(string funcName, int argCount);

        /**
         * 获取方法定义
         * 
         * @param funcName
         * @param argCount
         * @return
         */
        IRTSFunction getFunction(string funcName, int argCount);

        IRTSType getRTSType(System.Type c);
    }
}