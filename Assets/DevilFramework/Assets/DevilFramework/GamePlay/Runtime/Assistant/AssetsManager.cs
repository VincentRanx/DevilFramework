//#define PLAY_MODE
using Devil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.GamePlay
{
    public class AssetsManager : Singleton<AssetsManager>
    {
        Dictionary<int, AssetBundle> mAssets; // <bundleId, bundle>
        Dictionary<int, int> mBundleMap; // <assetId, bundleId>

#if UNITY_EDITOR && !PLAY_MODE
        public static T GetAsset<T>(string assetPath) where T : Object
        {
            if (assetPath.StartsWith("Assets/"))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            else
                return Resources.Load<T>(assetPath);
        }

        public static void GetAssetAsync<T>(string assetPath, System.Action<T> handler, System.Action<string> errorHandler = null) where T : Object
        {
            if (handler == null)
                return;
            T asset = GetAsset<T>(assetPath);
            if (asset != null)
                handler(asset);
            else if (errorHandler != null)
                errorHandler(string.Format("Cannot Load Asset:{0}", assetPath));
        }
#else
        public static T GetAsset<T>(string assetPath) where T: Object
        {
            AssetsManager inst = AssetsManager.GetOrNewInstance();
            AssetBundle bundle;
            return null;
        }

        public static void GetAssetAsync<T>(string assetPath, System.Action<T> handler, System.Action<string> errorHandler = null) where T : Object
        {
        }
#endif

    }
}
