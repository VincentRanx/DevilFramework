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
         * �ж��Ƿ��ǹؼ���
         * 
         * @param src
         * @return
         */
        bool isKeyWord(string src);

        /**
         * ��ȡ��־���齨
         * 
         * @return
         */
        IRTSLog getLogger();

        /**
         * �������������
         * 
         * @param src
         * @param off
         * @param end
         * @return
         */
        IRTSLinker newLinker(string src);

        /**
         * ��ӷ�������
         * 
         * @param funcName
         * @param func
         */
        void addFunction(string funcName, IRTSFunction func);

        /**
         * ɾ����������
         * 
         * @param funcName
         * @param argCount
         */
        void removeFunction(string funcName, int argCount);

        /**
         * ��ȡ��������
         * 
         * @param funcName
         * @param argCount
         * @return
         */
        IRTSFunction getFunction(string funcName, int argCount);

        IRTSType getRTSType(System.Type c);
    }
}