namespace Devil.AsyncTask
{
    public interface IAsyncTask : ITick
    {
        // 加载进度
        float Progress { get; }

        // 是否完成任务
        bool IsDone { get; }

        // 中断
        void Interrupt();
    }
}