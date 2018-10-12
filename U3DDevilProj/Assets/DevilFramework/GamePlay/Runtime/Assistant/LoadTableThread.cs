using Devil.ContentProvider;
using Devil.Utility;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public class LoadTableThread
    {
        interface ITableLoader : IIdentified
        {
            bool IsReady { get; }
            void LoadAsTable();
        }

        public class TableLoader<T> : ITableLoader where T : TableBase, new()
        {
            public int Identify { get; private set; }
            string mData;
            string mFile;
            bool mMerge;
            public TableLoader(string file, bool merge)
            {
                mFile = file;
                Identify = TableSet<T>.HashTable(file);
                AssetPath = string.Format("Assets/AB/Tables/{0}.txt.bytes", file);
                mMerge = merge;
            }

            public bool IsReady { get; private set; }

            public void LoadAsTable()
            {
                if (!string.IsNullOrEmpty(mData))
                {
                    if (mMerge)
                        TableSet<T>.Merge(mFile, mData);
                    else
                        TableSet<T>.Load(mFile, mData);
#if UNITY_EDITOR
                    RTLog.LogFormat(LogCat.Table, "{0} Table<{1}> @ {2}", mMerge ? "Merge" : "Load", typeof(T).Name, AssetPath);
#endif
                }
            }

            public void StartLoad()
            {
                AssetsUtil.GetAssetAsync<TextAsset>(AssetPath, HandleAsset, OnLoadAssetError);
            }
            
            public string AssetPath { get; private set; }

            private void HandleAsset(TextAsset asset)
            {
                mData = Encoding.UTF8.GetString(asset.bytes);
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

        Thread mThread;
        object mLock = new object();
        Queue<ITableLoader> mLoaders = new Queue<ITableLoader>();
        HashSet<int> mTaskQueue = new HashSet<int>();
        public bool IsLoading { get; private set; }

        public bool IsFileLoaded<T>(string file) where T : TableBase, new()
        {
            int id = TableSet<T>.HashTable(file);
            if (mTaskQueue.Contains(id) || TableSet<T>.Instance.Contains(id))
                return true;
            return false;
        }

        public void LoadTable<T>(string file, bool merge = true) where T : TableBase, new()
        {
            lock (mLock)
            {
                IsLoading = true;
                TableLoader<T> loader = new TableLoader<T>(file, merge);
                if (mTaskQueue.Contains(loader.Identify))
                    return;
                mLoaders.Enqueue(loader);
                loader.StartLoad();
                if (mThread == null)
                {
                    mThread = new Thread(OnTableThread);
                    mThread.Start();
                }
            }
        }

        void OnTableThread()
        {
            try
            {
                while (true)
                {
                    ITableLoader loader = null;
                    lock (mLock)
                    {
                        IsLoading = mLoaders.Count > 0;
                        if (!IsLoading)
                        {
                            mThread = null;
                            break;
                        }
                        loader = mLoaders.Dequeue();
                        mTaskQueue.Remove(loader.Identify);
                    }
                    while (!loader.IsReady)
                    {
                        Thread.Sleep(200);
                    }
                    loader.LoadAsTable();
                }
            }
            catch (System.Exception e)
            {
                RTLog.LogError(LogCat.Table, e.ToString());
            }
            finally
            {
                lock (mLock)
                {
                    mThread = null;
                    IsLoading = false;
                }
            }
        }

        public void Abort()
        {
            lock (mLock)
            {
                mLoaders.Clear();
                mTaskQueue.Clear();
            }
            if (mThread != null)
            {
                mThread.Abort();
                mThread = null;
            }
            IsLoading = false;
        }
    }
}