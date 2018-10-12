using Devil.Utility;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public class InnerAssetsUtil : AssetsUtil
    {
        protected override bool IsDone { get { return true; } }

        protected override float Progress { get { return 1; } }

        protected override T LoadAsset<T>(string assetPath)
        {
            if (typeof(T) == typeof(AssetBundle))
                return null;
#if UNITY_EDITOR
            if (StringUtil.StartWithIgnoreCase(assetPath, "assets/"))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            else
#endif
                return Resources.Load<T>(assetPath);
        }

        protected override void LoadAssetAsync<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler = null)
        {
            var asset = LoadAsset<T>(assetPath);
            if(asset == null)
            {
                var error = string.Format("Can't load Asset: \"{0}\"", assetPath);
#if UNITY_EDITOR
                if(typeof(T) != typeof(AssetBundle))
                    RTLog.LogError(LogCat.Asset, error);
#endif
                if (errorhandler != null)
                    errorhandler(error);
            }
            else if(handler != null)
            {
                handler(asset);
            }
        }

        protected override void AbortAsyncTask<T>(string assetPath, AssetHandler<T> handler, ErrorHandler errorhandler) { }
    }
}