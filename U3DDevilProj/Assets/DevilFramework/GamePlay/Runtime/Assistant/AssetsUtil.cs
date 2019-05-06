#if UNITY_EDITOR
#define INNER_ASSET
#else
#endif

using UnityEngine;
using Devil.Utility;
using Devil.AsyncTask;
using UnityEngine.U2D;

namespace Devil.GamePlay.Assistant
{
    public delegate void AssetHandler<T>(T asset) where T : Object;
    public delegate void ErrorHandler(string error);

    /*
     * 因为设计所有的资源加载都为异步方式,
     * 建议仅在编辑器模式以及存在资源缓存的情况下使用同步的加载方法。
     */
    public abstract class AssetsUtil : IAsyncTask
    {
        #region instance
        private static AssetsUtil sInstance;
        public static AssetsUtil UtilInstance
        {
            get
            {
                if(sInstance == null)
                {
#if INNER_ASSET
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

        public const string DEFAULT_ATLAS = "Assets/ArtRes/Sprites/Common.spriteatlas";
        public const string ATLAS_EXTENSION = ".spriteatlas";

        public static Sprite GetSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
                return null;
            var index = spriteName.LastIndexOf('.');
            string atlas;
            string spr;
            if (index == -1)
            {
                atlas = DEFAULT_ATLAS;
                spr = spriteName;
            }
            else
            {
                atlas = spriteName.Substring(0, index);
                spr = spriteName.Substring(index + 1);
            }
            var asset = GetAsset<SpriteAtlas>(atlas);
            return asset == null ? null : asset.GetSprite(spr);
        }

        public static void GetSpriteAsync(string spriteName, AssetHandler<Sprite> handler, ErrorHandler errorhandler = null)
        {
            if (handler == null || string.IsNullOrEmpty(spriteName))
                return;
            var index = spriteName.LastIndexOf('.');
            string atlas;
            string spr;
            if (index == -1)
            {
                atlas = DEFAULT_ATLAS;
                spr = spriteName;
            }
            else
            {
                atlas = spriteName.Substring(0, index);
                spr = spriteName.Substring(index + 1);
                if (!StringUtil.EndWithIgnoreCase(atlas, ATLAS_EXTENSION))
                    atlas = StringUtil.Concat(atlas, ATLAS_EXTENSION);
            }
            GetAssetAsync<SpriteAtlas>(atlas, (x) => {
                var sprite = x.GetSprite(spr);
                if (sprite != null)
                    handler(sprite);
                else if (errorhandler != null)
                    errorhandler(StringUtil.Concat(spriteName, " doesn't exist."));
            }, errorhandler);
        }

        public static void GetSpriteAsync(string atlas, string spriteName, AssetHandler<Sprite> handler, ErrorHandler errorhandler = null)
        {
            if (handler == null)
                return;
            GetAssetAsync<SpriteAtlas>(atlas, (x) => {
                var sprite = x.GetSprite(spriteName);
                if (sprite != null)
                    handler(sprite);
                else if (errorhandler != null)
                    errorhandler(StringUtil.Concat(spriteName, " doesn't exist."));
            }, errorhandler);
        }

        public static T GetAsset<T>(string assetPath) where T : Object
        {
            return UtilInstance.LoadAsset<T>(assetPath);
        }

        public static void GetAssetAsync<T>(string assetPath, AssetHandler<T> handler = null, ErrorHandler errorhandler = null) where T : Object
        {
            UtilInstance.LoadAssetAsync(assetPath, handler, errorhandler);
        }

        public static void GetAllAssetsAsync<T>(string abname, AssetHandler<T> handler = null, ErrorHandler errorhandler = null) where T: Object
        {
            UtilInstance.LoadAllAssets<T>(abname, handler, errorhandler);
        }

        public static void AbortLoadAssetAsync<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler = null) where T : Object
        {
            UtilInstance.AbortAsyncTask(assetPath, handler, errorhandler);
        }

        public static void ReleaseAsset(string assetPath)
        {
            if (sInstance != null)
            {
                sInstance.UnloadAsset(assetPath);
            }
        }

        public static void Destroy()
        {
            if(sInstance != null)
            {
                sInstance.OnDestroy();
                sInstance = null;
            }
        }

        public static bool IsStilLoading { get { return !UtilInstance.IsDone; } }
        public static bool IsComplete { get { return UtilInstance.IsDone; } }
        public static float LoadProgress { get { return UtilInstance.Progress; } }
        #endregion

        #region implement
        protected AssetsUtil() { }
        public void Start() { }
        public void Abort() { RTLog.LogError(LogCat.Asset, "Invoke static method \"AssetsUtil.AbortLoadAssetAsync<T>()\" instead of Abort()."); }
        public void OnTick(float deltaTime) { }
        protected abstract T LoadAsset<T>(string assetPath) where T : Object;
        protected abstract void UnloadAsset(string assetPath);
        protected abstract void OnDestroy();
        protected abstract void LoadAssetAsync<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler = null) where T : Object;
        protected abstract void AbortAsyncTask<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler) where T : Object;
        protected abstract void LoadAllAssets<T>(string abname, AssetHandler<T> handler, ErrorHandler errorHandler) where T : Object;
        public abstract bool IsDone { get; }
        public abstract float Progress { get; }
        #endregion
    }
}