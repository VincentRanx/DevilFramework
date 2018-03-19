using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Devil.GamePlay.Assistant
{
    public class Logcat : MonoBehaviour
    {
        public class Log
        {
            public string condition;
            public string stackTrace;
            public LogType type;
        }

        [Range(50, 10000)]
        public int m_MaxLogs = 500;
        public Text m_LogText;
        public ScrollRect m_Scroll;

        LinkedList<Log> mLogs;
        int mMaxLogs;
        StringBuilder mBuilder;
        int mLogCount;
        float mAlignTimer;

        private void Awake()
        {
            Application.logMessageReceivedThreaded += ReceiveMsg;
            mBuilder = new StringBuilder();
            mMaxLogs = Mathf.Max(50, m_MaxLogs);
            mLogs = new LinkedList<Log>();
            MainThread.Install();
        }

        private void OnDestroy()
        {
            Application.logMessageReceivedThreaded -= ReceiveMsg;
        }

        void ReceiveMsg(string condition, string stackTrace, LogType type)
        {
            Log log = new Log();
            log.condition = condition;
            log.stackTrace = stackTrace;
            log.type = type;
            MainThread.RunOnMainThread(AppendLog, log);
        }

        void BuildLog(Log log)
        {
            Log l = log;
            mBuilder.Append("<size=14>");
            if (l.type == LogType.Error || l.type == LogType.Exception)
            {
                mBuilder.Append("<color=red>");
            }
            else if (l.type == LogType.Warning)
            {
                mBuilder.Append("<color=yellow>");
            }
            else
            {
                mBuilder.Append("<color=whire>");
            }
            mBuilder.Append(l.condition).Append("\n");
            mBuilder.Append("<size=10>").Append(l.stackTrace).Append("</size>");
            mBuilder.Append("</color></size>");
            mBuilder.Append("\n");
        }

        void AppendLog(Log log)
        {
            if (mLogCount == mMaxLogs)
            {
                mBuilder.Remove(0, mBuilder.Length);
                mLogs.RemoveFirst();
                LinkedListNode<Log> node = mLogs.First;
                while(node != null)
                {
                    BuildLog(node.Value);
                    node = node.Next;
                }
            }
            else
            {
                mLogCount++;
            }
            mLogs.AddLast(log);
            BuildLog(log);
            if (m_LogText)
                m_LogText.text = mBuilder.ToString();
            mAlignTimer = 0.5f;
        }

        private void Update()
        {
            if (mAlignTimer > 0)
            {
                float v = m_Scroll.verticalNormalizedPosition;
                mAlignTimer -= Time.deltaTime;
                if (mAlignTimer < 0)
                    m_Scroll.verticalNormalizedPosition = 0;
                else
                    m_Scroll.verticalNormalizedPosition = Mathf.Lerp(v, 0, 0.1f);
            }
        }
    }
}