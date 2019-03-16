using Devil.AsyncTask;
using Devil.ContentProvider;
using Devil.Utility;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public class LoadTableThread : IAsyncTask
    {
        public interface ITableLoader : IIdentified
        {
            bool IsReady { get; }
            void StartLoad();
            void LoadAsTable();
        }

        public class TableLoader<T> : ITableLoader where T : TableBase, new()
        {
            public int Identify { get; private set; }
            TableSet<T> mTable;
            string mData;
            string mFile;
            bool mMerge;

            public TableLoader(int id, string file, bool merge, TableSet<T> table, string folder, string extension)
            {
                mFile = file;
                mMerge = merge;
                mTable = table == null ? TableSet<T>.Instance : table;
                Identify = id;// TableSet<T>.HashTable(file);
                AssetPath = StringUtil.Concat(folder, file, extension);
                mTable.WaitForLoading();
            }

            public bool IsReady { get; private set; }

            public void LoadAsTable()
            {
                if (!string.IsNullOrEmpty(mData))
                {
                    if (mMerge)
                    {
                        TableSet<T>.MergeTo(TableSet<T>.LoadAsNew(mFile, mData), mTable);
                    }
                    else
                    {
                        TableSet<T>.LoadTo(mFile, mData, mTable);
                    }
#if UNITY_EDITOR
                    RTLog.LogFormat(LogCat.Table, "{0} Table<{1}> @ {2}", mMerge ? "Merge" : "Load", typeof(T).Name, AssetPath);
#endif
                }
                else
                {
                    TableSet<T>.LoadComplete(mTable, mMerge);
                }
            }

            public void StartLoad()
            {
                AssetsUtil.GetAssetAsync<TextAsset>(AssetPath, HandleAsset, OnLoadAssetError);
            }
            
            public string AssetPath { get; private set; }

            private void HandleAsset(TextAsset asset)
            {
                mData = asset.text;// Encoding.UTF8.GetString(asset.bytes);
                IsReady = true;
            }

            private void OnLoadAssetError(string error)
            {
                IsReady = true;
#if UNITY_EDITOR
                RTLog.LogErrorFormat(LogCat.Table, "Load Asset:{0} error:{1}", AssetPath, error);
#endif
            }
        }

        string mTableFolder;
        string mTableExtension;
        ThreadPoolState mThreadState;
        object mLock = new object();
        Queue<ITableLoader> mLoaders = new Queue<ITableLoader>();
        HashSet<int> mTaskQueue = new HashSet<int>();
        public bool IsLoading { get; private set; }
        public event System.Action OnComplete;

        public LoadTableThread(string tableFolder = "Assets/Tables/", string tableExtension = ".txt", string name= "default table loader")
        {
            if (tableFolder.EndsWith("/"))
                mTableFolder = tableFolder;
            else
                mTableFolder = StringUtil.Concat(tableFolder, "/");
            if (tableExtension.StartsWith(".") || string.IsNullOrEmpty(tableExtension))
                mTableExtension = tableExtension;
            else
                mTableExtension = StringUtil.Concat(".", tableExtension);
            mThreadState = new ThreadPoolState();
            mThreadState.name = name;
        }

        public bool IsTableLoaded<T>(string file) where T : TableBase, new()
        {
            int id = TableSet<T>.HashTable(file);
            if (mTaskQueue.Contains(id) || TableSet<T>.Instance.ContainsTable(id))
                return true;
            return false;
        }

        public bool LoadTable<T>(string file, bool merge, TableSet<T> table = null) where T : TableBase, new()
        {
            lock (mLock)
            {
                int id = TableSet<T>.HashTable(file);
                if (!mTaskQueue.Add(id))
                    return false;
                TableLoader<T> loader = new TableLoader<T>(id, file, merge, table, mTableFolder, mTableExtension);
                IsLoading = true;
                mLoaders.Enqueue(loader);
                return true;
            }
        }

        void NotifyComplete()
        {
            if (OnComplete != null)
                OnComplete();
        }

        void OnTableThread(object state)
        {
#if UNITY_EDITOR
            RTLog.LogFormat(LogCat.Table, "Begin Thread[{0}]", mThreadState.name);
#endif
            try
            {
                while (mThreadState.isAlive)
                {
                    ITableLoader loader = null;
                    lock (mLock)
                    {
                        IsLoading = mLoaders.Count > 0;
                        if (!IsLoading)
                        {
                            //mThread = null;
                            mThreadState.isAlive = false;
                            break;
                        }
                        loader = mLoaders.Dequeue();
                        mTaskQueue.Remove(loader.Identify);
                    }
                    if(mThreadState.isAlive)
                        MainThread.RunOnMainThread(loader.StartLoad);
                    while (mThreadState.isAlive && !loader.IsReady)
                    {
#if UNITY_EDITOR
                        if (!MainThread.IsInitilized)
                            mThreadState.isAlive = false;
                        MainThread.RunOnMainThread((x) => { if (!Application.isPlaying) x.isAlive = false; }, mThreadState);
#endif
                        Thread.Sleep(200);
                    }
                    if(mThreadState.isAlive)
                        loader.LoadAsTable();
                }
            }
            catch (System.Exception e)
            {
                RTLog.LogError(LogCat.Table, e.ToString());
            }
            finally
            {
                Abort();
                MainThread.RunOnMainThread(NotifyComplete);
#if UNITY_EDITOR
                RTLog.LogFormat(LogCat.Table, "End Thread[{0}]", mThreadState.name);
#endif
            }
        }

        public void Abort()
        {
            lock (mLock)
            {
                mLoaders.Clear();
                mTaskQueue.Clear();
                mThreadState.isAlive = false;
                IsLoading = false;
            }
        }

        #region async task implemention
        public bool IsDone { get { return !IsLoading; } }
        public float Progress { get; private set; }
        public void Start()
        {
            if (!MainThread.IsInitilized)
            {
                RTLog.LogErrorFormat(LogCat.Table, "Can't start Table Loader \"{0}\" becaouse of no MainThread installized.", mThreadState.name);
                return;
            }
            if (!mThreadState.isAlive && mTaskQueue.Count > 0)
            {
                if (ThreadPool.QueueUserWorkItem(OnTableThread, mThreadState))
                    mThreadState.isAlive = true;
            }
        }
        public void OnTick(float deltaTime)
        {
            Progress += deltaTime;
            if (IsLoading && Progress > 0.9f)
                Progress = 0.9f;
        }
        #endregion
    }
}