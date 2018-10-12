using Devil.Utility;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public struct AssetSetting
    {
        // 初始化资源表大小
        public int initCapacity;
        // ab文件夹
        public string abFolder;
        // manifest 文件
        public string manifestName;

        public static AssetSetting DefaultSetting
        {
            get
            {
                AssetSetting set;
                set.initCapacity = 512;
                set.abFolder = Application.streamingAssetsPath;
                set.manifestName = "StreamingAssets";
                return set;
            }
        }   
    }

    public class ABAssetsUtil : AssetsUtil
    {
        // 资源加载接口
        private interface IAssetHandler
        {
            int AssetId { get; }
            void RegistHandler<T>(AssetHandler<T> handler) where T : Object;
            void UnregistHandler<T>(AssetHandler<T> handler) where T : Object;
            void RegistErrorHandler(ErrorHandler errorhandler);
            void UnregistErrorHandler(ErrorHandler errorhandler);
            void DoComplete(System.Action<IAssetHandler> complete);
            void StartLoad();
            bool IsDone { get; }
            float Progress { get; }
        }

        private class AbMeta : IIdentified
        {
            public int Identify { get; private set; }
            public string name { get; private set; }
            public string path { get; private set; }
            public ABAssetsUtil util { get; private set; }
            public AssetBundle assetBundle { get; private set; }

            public AbMeta(ABAssetsUtil util, string name, string path)
            {
                this.util = util;
                Identify = HashAssetID(name);
                this.name = name;
                this.path = path;
            }

            public void SetAssetBundle(AssetBundle asset)
            {
                assetBundle = asset;
                if(asset != null)
                {
                    var assets = asset.GetAllAssetNames();
                    int len = assets == null ? 0 : assets.Length;
                    for(int i = 0; i < len; i++)
                    {
                        var meta = util.GetOrNewMeta(assets[i]);
                        meta.useAb = true;
                        meta.abData = this;
                    }
                }
            }
        }

        // 资源信息
        private class AssetMeta : IIdentified
        {
            public int Identify { get; private set; }
            public string assetPath { get; private set; }
            public bool useAb;
            public AbMeta abData;
            public bool isBad; // 不可用
            public Object assetInstence; // 资源实例

            public AssetMeta(string assetpath)
            {
                this.assetPath = assetpath;
                this.useAb = StringUtil.StartWithIgnoreCase(assetpath, "assets/");
                this.Identify = HashAssetID(assetpath);
            }
        }

        //private AssetSetting mABSetting;
        // ab manifest
        private AssetBundleManifest mManifest;
        // 资源表
        private Dictionary<int, AssetMeta> mAssets;
        // ab 列表
        AbMeta[] mAbs;
        LinkedList<IAssetHandler> mHandlers;

        public ABAssetsUtil(AssetSetting set) : base()
        {
            mHandlers = new LinkedList<IAssetHandler>();
            //mABSetting = set;
            mAssets = new Dictionary<int, AssetMeta>(set.initCapacity);
            var ab = AssetBundle.LoadFromFile(Path.Combine(set.abFolder, set.manifestName));
            if (ab != null)
                mManifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (mManifest == null)
            {
                RTLog.LogError(LogCat.Asset, "Lost AssetBundle Manifest File.");
                mAbs = new AbMeta[0];
            }
            else
            {
                var abs = mManifest.GetAllAssetBundles();
                mAbs = new AbMeta[abs == null ? 0 : abs.Length];
                for(int i= 0; i < mAbs.Length; i++)
                {
                    mAbs[i] = new AbMeta(this, abs[i], Path.Combine(set.abFolder, abs[i]));
                }
                GlobalUtil.Sort(mAbs, (x, y) => x.Identify <= y.Identify ? -1 : 1);
            }
        }

        // 以进行的加载任务（完成所有任务时重置）
        int mLoaders;
        // 以完成的加载任务
        int mCompleteLoaders;
        protected override bool IsDone { get { return mHandlers.Count == 0; } }

        protected override float Progress
        {
            get
            {
                if (IsDone)
                    return 1;
                float progress = mCompleteLoaders;
                var node = mHandlers.First;
                if (node != null)
                    progress += node.Value.Progress;
                return progress / mLoaders;
            }
        }

        // 资源加载完成
        private void FinishAndLoadNextAsset(IAssetHandler handler = null)
        {
            LinkedListNode<IAssetHandler> node;
            if (handler != null)
            {
                mCompleteLoaders++;
                node = mHandlers.First;
                while (node != null)
                {
                    var tmp = node.Value;
                    if (tmp.AssetId == handler.AssetId)
                    {
                        mHandlers.Remove(node);
                        break;
                    }
                    node = node.Next;
                }
            }
            node = mHandlers.First;
            if (node != null)
            {
                node.Value.StartLoad();
            }
            else
            {
                mCompleteLoaders = 0;
                mLoaders = 0;
            }
        }

        private IAssetHandler GetHandler(int id)
        {
            var node = mHandlers.First;
            while(node != null)
            {
                var tmp = node.Value;
                if (tmp.AssetId == id)
                    return tmp;
                node = node.Next;
            }
            return null;
        }

        private AssetMeta GetOrNewMeta(string asset)
        {
            var id = HashAssetID(asset);
            AssetMeta meta;
            if (!mAssets.TryGetValue(id, out meta))
            {
                meta = new AssetMeta(asset);
                mAssets[id] = meta;
            }
            return meta;
        }

        protected override T LoadAsset<T>(string assetPath)
        {
            if (typeof(T) == typeof(AssetBundle))
            {
                var ab = mAbs.Binsearch(HashAssetID(assetPath));
#if UNITY_EDITOR
                if (ab == null || ab.assetBundle == null)
                    RTLog.LogErrorFormat(LogCat.Asset, "Faild to load AssetBundle[{0}], sync load ab only support for cached assets.", assetPath);
#endif
                return ab == null ? null : ab.assetBundle as T;
            }
            else
            {
                var meta = GetOrNewMeta(assetPath);
                var holder = GetHandler(meta.Identify);
                if (holder != null)
                {
#if UNITY_EDITOR
                    RTLog.LogErrorFormat(LogCat.Asset, "Faild to load asset \"{0}\", because of another async loading of this asset.", assetPath);
#endif
                    return null;
                }
                if (meta.assetInstence == null)
                {
                    if (!meta.useAb)
                        meta.assetInstence = Resources.Load<T>(assetPath);
                    else if (meta.abData != null && meta.abData.assetBundle != null)
                        meta.assetInstence = meta.abData.assetBundle.LoadAsset<T>(assetPath);
#if UNITY_EDITOR
                    RTLog.LogFormat(LogCat.Asset, "\"{0}\" was loaded(AssetBundle: {1}).",
                        meta.assetPath, meta.abData == null ? "Unkown" : meta.abData.name);
#endif
                }
                return meta.assetInstence as T;
            }
        }

        protected override void LoadAssetAsync<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler = null)
        {
            bool load;
            if (typeof(T) == typeof(AssetBundle))
            {
                var id = HashAssetID(assetPath);
                var ab = mAbs.Binsearch(id);
                load = LoadAB(assetPath, ab, handler, errorhandler);
            }
            else
            {
                var meta = GetOrNewMeta(assetPath);
                load = LoadAsset<T>(meta, handler, errorhandler);
            }
            if (load)
                FinishAndLoadNextAsset();
        }

        // 加载 AB
        private bool LoadAB<T>(string name, AbMeta ab, AssetHandler<T> handler, ErrorHandler errorhandler) where T: Object
        {
            if(ab == null)
            {
                var error = string.Format("Can't find AssetBundle \"{0}\"", name);
#if UNITY_EDITOR
                RTLog.LogError(LogCat.Asset, error);
#endif
                if (errorhandler != null)
                    errorhandler(error);
                return false;
            }
            if (ab.assetBundle != null)
            {
                if (handler != null)
                    handler(ab.assetBundle as T);
                return false;
            }
            bool load = false;
            // 加载依赖包
            var dependencies = mManifest.GetDirectDependencies(ab.name);
            if (dependencies != null)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    var id = HashAssetID(dependencies[i]);
                    var dab = mAbs.Binsearch(id);
                    load |= LoadAB<AssetBundle>(dependencies[i], dab, null, null);
                }
            }
            // 加载 ab
            var assethandler = GetHandler(ab.Identify);
            if (assethandler == null)
            {
                assethandler = new ABObtain(ab);
                mHandlers.AddLast(assethandler);
                mLoaders++;
                load = true;
                assethandler.DoComplete(FinishAndLoadNextAsset);
            }
            if (handler != null)
                assethandler.RegistHandler(handler);
            if (errorhandler != null)
                assethandler.RegistErrorHandler(errorhandler);
            return load;
        }

        // 加载资源
        private bool LoadAsset<T>(AssetMeta meta, AssetHandler<T> handler, ErrorHandler errorhandler) where T : Object
        {
            if (meta.assetInstence != null)
            {
                if (handler != null)
                    handler(meta.assetInstence as T);
                return false;
            }
            bool load = false;
            if (meta.abData != null && meta.abData.assetBundle == null)
                load |= LoadAB<AssetBundle>(meta.abData.name, meta.abData, null, null);
            var assethandler = GetHandler(meta.Identify);
            if (assethandler == null)
            {
                assethandler = new AssetObtain<T>(meta);
                mHandlers.AddLast(assethandler);
                mLoaders++;
                load = true;
                assethandler.DoComplete(FinishAndLoadNextAsset);
            }
            if (handler != null)
                assethandler.RegistHandler(handler);
            if (errorhandler != null)
                assethandler.RegistErrorHandler(errorhandler);
            return load;
        }

        protected override void AbortAsyncTask<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler)
        {
            if(handler == null && errorhandler == null)
            {
                return;
            }
            else
            {
                var id = HashAssetID(assetPath);
                var holder = GetHandler(id);
                if(holder != null)
                {
                    if (handler != null)
                        holder.UnregistHandler(handler);
                    if (errorhandler != null)
                        holder.UnregistErrorHandler(errorhandler);
                }
            }
        }

        #region handler implement
        private class ABObtain : IAssetHandler
        {
            AbMeta mMeta;
            bool isLoading;
            ErrorHandler OnErrorOccured;
            AssetHandler<AssetBundle> OnHandleAsset;
            System.Action<IAssetHandler> OnComplete;
            AsyncOperation mReq;
            public bool IsDone { get { return !isLoading; } }
            public float Progress
            {
                get
                {
                    if (isLoading)
                        return mReq == null ? 0 : mReq.progress;
                    else
                        return 1;
                }
            }

            public ABObtain(AbMeta meta)
            {
                mMeta = meta;
            }

            public int AssetId { get { return mMeta.Identify; } }

            public void RegistErrorHandler(ErrorHandler errorhandler)
            {
                OnErrorOccured += errorhandler;
            }

            public void RegistHandler<T>(AssetHandler<T> handler) where T : Object
            {
                var hand = handler as AssetHandler<AssetBundle>;
                OnHandleAsset += hand;
            }

            public void DoComplete(System.Action<IAssetHandler> complete)
            {
                OnComplete += complete;
            }

            void FireComplishEvent()
            {
                if(OnComplete != null)
                {
                    OnComplete(this);
                    OnComplete = null;
                }
            }

            public void StartLoad()
            {
                if (isLoading)
                    return;
                var req = AssetBundle.LoadFromFileAsync(mMeta.path);
                mReq = req;
                if(req == null)
                {
                    var error = string.Format("Can't find AssetBundle[{0}]", mMeta.name);
#if UNITY_EDITOR
                    RTLog.LogError(LogCat.Asset, error);
#endif
                    if (OnErrorOccured != null)
                        OnErrorOccured(error);
                    FireComplishEvent();
                }
                else
                {
                    isLoading = true;
                    req.completed += Complete;
                }
            }

            void Complete(AsyncOperation oper)
            {
                AssetBundleCreateRequest req = oper as AssetBundleCreateRequest;
                if(req != null && req.assetBundle != null)
                {
                    mMeta.SetAssetBundle(req.assetBundle);
#if UNITY_EDITOR
                    RTLog.LogFormat(LogCat.Asset, "AssetBundle \"{0}\" was loaded.", mMeta.name);
#endif
                    if (OnHandleAsset != null)
                        OnHandleAsset(mMeta.assetBundle);
                }
                else
                {
                    var error = string.Format("Can't find AssetBundle[{0}]", mMeta.name);
#if UNITY_EDITOR
                    RTLog.LogError(LogCat.Asset, error);
#endif
                    if (OnErrorOccured != null)
                        OnErrorOccured(error);
                }
                isLoading = false;
                FireComplishEvent();
            }

            public void UnregistErrorHandler(ErrorHandler errorhandler)
            {
                OnErrorOccured -= errorhandler;
            }

            public void UnregistHandler<T>(AssetHandler<T> handler) where T : Object
            {
                var hand = handler as AssetHandler<AssetBundle>;
                OnHandleAsset -= hand;
            }
        }

        private class AssetObtain<T> : IAssetHandler where T: Object
        {
            AssetMeta mMeta;
            ErrorHandler OnErrorOccured;
            AssetHandler<T> OnHandleAsset;
            bool isLoading;
            System.Action<IAssetHandler> OnComplete;
            AsyncOperation mReq;
            public bool IsDone { get { return !isLoading; } }
            public float Progress
            {
                get
                {
                    if (isLoading)
                        return mReq == null ? 0 : mReq.progress;
                    else
                        return 1;
                }
            }
            public AssetObtain(AssetMeta meta)
            {
                mMeta = meta;
            }
            public int AssetId { get { return mMeta.Identify; } }

            public void RegistErrorHandler(ErrorHandler errorhandler)
            {
                OnErrorOccured += errorhandler;
            }

            public void RegistHandler<T1>(AssetHandler<T1> handler) where T1 : Object
            {
                var hand = handler as AssetHandler<T>;
                OnHandleAsset += hand;
            }

            public void DoComplete(System.Action<IAssetHandler> complete)
            {
                OnComplete += complete;
            }

            void FireComplishEvent()
            {
                if (OnComplete != null)
                {
                    OnComplete(this);
                    OnComplete = null;
                }
            }

            public void StartLoad()
            {
                if (isLoading)
                    return;
                if (!mMeta.useAb)
                {
                    var req = Resources.LoadAsync<T>(mMeta.assetPath);
                    mReq = req;
                    if(req == null)
                    {
                        var error = string.Format("\"{0}\" doesn't exist.", mMeta.assetPath);
#if UNITY_EDITOR
                        RTLog.LogError(LogCat.Asset, error);
#endif
                        if (OnErrorOccured != null)
                            OnErrorOccured(error);
                        FireComplishEvent();
                    }
                    else
                    {
                        isLoading = true;
                        req.completed += CompleteInnerAsset;
                    }
                }
                else if (mMeta.abData != null && mMeta.abData.assetBundle != null)
                {
                    var req = mMeta.abData.assetBundle.LoadAssetAsync<T>(mMeta.assetPath);
                    if(req == null)
                    {
                        var error = string.Format("\"{0}\" doesn't exist (AssetBundle: {1}).",
                        mMeta.assetPath, mMeta.abData == null ? "Unkown" : mMeta.abData.name);
#if UNITY_EDITOR
                        RTLog.LogError(LogCat.Asset, error);
#endif
                        if (OnErrorOccured != null)
                            OnErrorOccured(error);
                        FireComplishEvent();
                    }
                    else
                    {
                        isLoading = true;
                        req.completed += ComplteABAsset;
                    }
                }
                else
                {
                    var error = string.Format("\"{0}\" doesn't exist (requre AssetBundle: {1}).",
                        mMeta.assetPath, mMeta.abData == null ? "Unkown" : mMeta.abData.name);
#if UNITY_EDITOR
                    RTLog.LogError(LogCat.Asset, error);
#endif
                    if (OnErrorOccured != null)
                        OnErrorOccured(error);
                    FireComplishEvent();
                }
            }

            void CompleteInnerAsset(AsyncOperation oper)
            {
                isLoading = false;
                var req = oper as ResourceRequest;
                if (req != null && req.asset != null)
                {
                    mMeta.assetInstence = req.asset;
#if UNITY_EDITOR
                    RTLog.LogFormat(LogCat.Asset, "\"{0}\" was loaded.", mMeta.assetPath);
#endif
                    if (OnHandleAsset != null)
                        OnHandleAsset(mMeta.assetInstence as T);
                }
                else
                {
                    var error = string.Format("\"{0}\" load faild.", mMeta.assetPath);
#if UNITY_EDITOR
                    RTLog.LogError(LogCat.Asset, error);
#endif
                    if (OnErrorOccured != null)
                        OnErrorOccured(error);
                }
                FireComplishEvent();
            }

            void ComplteABAsset(AsyncOperation oper)
            {
                var req = oper as AssetBundleRequest;
                if(req != null && req.asset != null)
                {
                    mMeta.assetInstence = req.asset;
#if UNITY_EDITOR
                    RTLog.LogFormat(LogCat.Asset, "\"{0}\" was loaded(AssetBundle: {1}).",
                        mMeta.assetPath, mMeta.abData == null ? "Unkown" : mMeta.abData.name);
#endif
                    if (OnHandleAsset != null)
                        OnHandleAsset(mMeta.assetInstence as T);
                }
                else
                {
                    var error = string.Format("\"{0}\" load faild(AssetBundle: {1}).",
                        mMeta.assetPath, mMeta.abData == null ? "Unkown" : mMeta.abData.name);
#if UNITY_EDITOR
                    RTLog.LogError(LogCat.Asset, error);
#endif
                    if (OnErrorOccured != null)
                        OnErrorOccured(error);
                }
                FireComplishEvent();
            }

            public void UnregistErrorHandler(ErrorHandler errorhandler)
            {
                OnErrorOccured -= errorhandler;
            }

            public void UnregistHandler<T1>(AssetHandler<T1> handler) where T1 : Object
            {
                var hand = handler as AssetHandler<T>;
                OnHandleAsset -= hand;
            }
        }

        #endregion
    }

}