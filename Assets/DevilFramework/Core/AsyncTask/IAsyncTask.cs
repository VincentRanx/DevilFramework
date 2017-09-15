namespace DevilTeam.AsyncTask
{
    public interface IAsyncTask
    {
        // 加载进度
        float Progress { get; }

        // 是否完成任务
        bool IsDone { get; }
    }
}