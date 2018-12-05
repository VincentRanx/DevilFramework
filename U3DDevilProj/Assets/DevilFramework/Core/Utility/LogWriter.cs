using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Devil.Utility
{
    public class LogWriter
    {
        object mLock = new object();
        Queue<string> mLogs; // 日志
        string mFile;
        string mFolder;
        bool mRunThread;

        StreamWriter mWriter;
        bool mIsDirty;

        public LogWriter(string folder)
        {
            mFolder = folder;
            mFile = Path.Combine(folder, string.Format("{0}.log", System.DateTime.Now.ToString("yyMMddHHmm")));
            mLogs = new Queue<string>();
        }

        public void StartWrite()
        {
            lock (mLock)
            {
                if (!mRunThread && ThreadPool.QueueUserWorkItem(OnThreadWrite))
                {
                    mRunThread = true;
                    mIsDirty = false;
                    UnityEngine.Debug.LogFormat("Write log file to \"{0}\"", mFile);
                    if (!Directory.Exists(mFolder))
                        Directory.CreateDirectory(mFolder);
                    var tmp = mFile + ".tmp";
                    var textwriter = File.AppendText(tmp);
                    mWriter = textwriter;
                }
            }
        }

        public void StopWrite()
        {
            lock (mLock)
            {
                mRunThread = false;
                if (mWriter != null)
                {
                    while (mLogs.Count > 0)
                    {
                        mWriter.WriteLine(mLogs.Dequeue());
                    }
                    mWriter.Flush();
                    mWriter.Close();
                    mWriter.Dispose();
                    mWriter = null;
                }
                var tmp = mFile + ".tmp";
                if (File.Exists(mFile))
                    File.Delete(mFile);
                if (mIsDirty)
                    File.Move(tmp, mFile);
                else if (File.Exists(tmp))
                    File.Delete(tmp);
            }
        }

        public void Append(string lv, LogCat cat, string log)
        {
            lock (mLock)
            {
                var s = string.Format("[{0}] [{1}] [{2}] {3}", lv, System.DateTime.Now.ToString("HH:mm:ss.ffff"), cat, log);
                mLogs.Enqueue(s);
            }
        }

        void OnThreadWrite(object state)
        {
            while (mRunThread)
            {
                if (mLogs.Count == 0 || mWriter == null)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    lock (mLock)
                    {
                        mIsDirty = true;
                        mWriter.WriteLine(mLogs.Dequeue());
                    }
                    Thread.Sleep(10);
                }
            }
        }
    }
}