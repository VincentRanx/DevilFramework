namespace org.vr.rts
{

    public enum LogLevel
    {
        LEVEL_DEBUG = 1,
        LEVEL_WARNING = 2,
        LEVEL_ERROR = 3,
    }

    /**
     * 日志组件
     * @author Administrator
     *
     */
    public interface IRTSLog
    {

        IRTSLog logInfo(object msg);

        IRTSLog logWarning(object msg);

        IRTSLog logError(object msg);

    }
}