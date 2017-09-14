namespace org.vr.rts
{

    public interface IRTSRunner
    {

        IRTSDefine.Stack applyStack();

        bool isConst();

        /**
         * �������߳�(RTSThread)����ʱ����,ִ��һЩ��ʼ������
         */
        void loadedOnThread();

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
        bool onReturnAndSkip(IRTSDefine.Stack returnTppe, object value);

        /**
         * ��ȡ����ֵ
         * 
         * @return
         */
        object getOutput();

        /**
         * �������߳�(RTSThread)�м���,����ֵΪ��ʾreturn�����ͣ���return��break��continue
         * 
         * @param thread
         * @return returnType
         */
        IRTSDefine.Stack run(IRTSStack stack);

        bool evaluate(IRTSStack stack, object value);
    }
}