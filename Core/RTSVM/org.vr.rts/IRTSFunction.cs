namespace org.vr.rts
{

    public interface IRTSFunction
    {

        /**
         * 参数个数 -1 表示不限制
         * 
         * @return
         */
        int argSize();

        string getArgDef(int index);

        IRTSLinker getBody();

        /**
         * 返回类型
         * 
         * @return
         */
        IRTSType returnType();

        /**
         * 创建方法执行对象
         * 
         * @param thread
         * @param scope
         * @param args
         * @return
         */
        IRTSRunner createRunner(object[] args);

    }
    
}