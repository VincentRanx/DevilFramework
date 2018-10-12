namespace org.vr.rts
{

    public interface IRTSFunction
    {

        /**
         * �������� -1 ��ʾ������
         * 
         * @return
         */
        int argSize();

        string getArgDef(int index);

        IRTSLinker getBody();

        /**
         * ��������
         * 
         * @return
         */
        IRTSType returnType();

        /**
         * ��������ִ�ж���
         * 
         * @param thread
         * @param scope
         * @param args
         * @return
         */
        IRTSRunner createRunner(object[] args);

    }
    
}