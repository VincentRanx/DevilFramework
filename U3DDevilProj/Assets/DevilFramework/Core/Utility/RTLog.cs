using UnityEngine;

namespace Devil.Utility
{
    public enum LogCat
    {
        Editor = 1,
        Game = 2,
        Asset = 4,
        Table = 8,

        Network = 0x10,
        AI = 0x20,
        Persistent = 0x40,
        Native = 0x80,

        Date = 0x100,
        Shop = 0x200,
        Npc = 0x400,
    }

    // 打印日志
    [ExecuteInEditMode]
    public class RTLog : MonoBehaviour
    {
        static RTLog sInstance;

        [MaskField]
        [SerializeField]
        LogCat m_Categories = (LogCat)(-1);

#if UNITY_EDITOR
        LogWriter mLogWriter;
#endif

        private void Awake()
        {
            if (sInstance == null)
            {
                sInstance = this;
                gameObject.name = "#[Logger]";
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
#endif
                    DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
                    mLogWriter = new LogWriter(Application.dataPath + "/../logs");
                    mLogWriter.StartWrite();
                }
#endif
            }
        }

        private void OnDestroy()
        {
            if (sInstance == this)
                sInstance = null;
#if UNITY_EDITOR
            if (mLogWriter != null)
            {
                mLogWriter.StopWrite();
                mLogWriter = null;
            }
#endif
        }

        static string Format(LogCat cat, string content)
        {
            return StringUtil.Concat("[", cat.ToString(), "] ", content);
        }

        static string Format(LogCat cat, string format, params object[] args)
        {
            return StringUtil.Concat("[", cat.ToString(), "] ", string.Format(format, args));
        }

#if UNITY_EDITOR
        static void WriteLog(string lv, LogCat cat, string content)
        {
            if (sInstance != null && sInstance.mLogWriter != null)
                sInstance.mLogWriter.Append(lv, cat, content);
        }
#endif

        public static bool UseCategory(LogCat cat)
        {
            return sInstance == null || (sInstance.m_Categories & cat) != 0;
        }

        public static void Log(LogCat cat, string content)
        {
#if UNITY_EDITOR
            WriteLog("I", cat, content);
#endif
            if (UseCategory(cat))
                Debug.Log(Format(cat, content));
        }

        public static void LogFormat(LogCat cat, string format, params object[] args)
        {
#if UNITY_EDITOR
            WriteLog("I", cat, string.Format(format, args));
#endif
            if (UseCategory(cat))
                Debug.Log(Format(cat, format, args));
        }

        public static void LogWarning(LogCat cat, string content)
        {
#if UNITY_EDITOR
            WriteLog("W", cat, content);
#endif
            if (UseCategory(cat))
                Debug.LogWarning(Format(cat, content));
        }

        public static void LogWarningFormat(LogCat cat, string format, params object[] args)
        {
#if UNITY_EDITOR
            WriteLog("W", cat, string.Format(format, args));
#endif
            if (UseCategory(cat))
                Debug.LogWarning(Format(cat, format, args));
        }

        public static void LogError(LogCat cat, string content)
        {
#if UNITY_EDITOR
            WriteLog("E", cat, content);
#endif
            if (UseCategory(cat))
                Debug.LogError(Format(cat, content));
        }

        public static void LogErrorFormat(LogCat cat, string format, params object[] args)
        {
#if UNITY_EDITOR
            WriteLog("E", cat, string.Format(format, args));
#endif
            if (UseCategory(cat))
                Debug.LogError(Format(cat, format, args));
        }
    }
}