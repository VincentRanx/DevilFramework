using UnityEngine;
using Devil.Utility;

namespace Devil.GamePlay.Assistant
{
    public delegate void AssetHandler<T>(T asset) where T : Object;
    public delegate void ErrorHandler(string error);

    public abstract class AssetsUtil
    {
        #region instance
        static AssetsUtil sInstance;
        protected static AssetsUtil UtilInstance
        {
            get
            {
                if(sInstance == null)
                {
#if UNITY_EDITOR
                    sInstance = new InnerAssetsUtil();
#else
                    sInstance = new ABAssetsUtil(AssetSetting.DefaultSetting);
#endif
                }
                return sInstance;
            }
        }

        public static int HashAssetID(string asset)
        {
            return StringUtil.IgnoreCaseToHash(asset);
        }

        public static void SetInstance(AssetsUtil instance)
        {
            if (instance != null)
                sInstance = instance;
        }
        #endregion

        #region asset operate

        // 该方法仅建议在编辑器模式以及存在资源缓存的情况下使用，因为设计为所有的资源加载都为异步方式。
        public static T GetAsset<T>(string assetPath) where T : Object
        {
            return UtilInstance.LoadAsset<T>(assetPath);
        }

        public static void GetAssetAsync<T>(string assetPath, AssetHandler<T> handler = null, ErrorHandler errorhandler = null) where T : Object
        {
            UtilInstance.LoadAssetAsync(assetPath, handler, errorhandler);
        }

        public static void AbortLoadAssetAsync<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler = null) where T : Object
        {
            UtilInstance.AbortAsyncTask(assetPath, handler, errorhandler);
        }
        
        public static bool IsStilLoading { get { return !UtilInstance.IsDone; } }
        public static bool IsComplete { get { return UtilInstance.IsDone; } }
        public static float LoadProgress { get { return UtilInstance.Progress; } }

        #endregion
        
        #region implement
        protected AssetsUtil() { }
        protected abstract T LoadAsset<T>(string assetPath) where T : Object;
        protected abstract void LoadAssetAsync<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler = null) where T : Object;
        protected abstract void AbortAsyncTask<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler) where T : Object;
        protected abstract bool IsDone { get; }
        protected abstract float Progress { get; }
        #endregion
    }
}