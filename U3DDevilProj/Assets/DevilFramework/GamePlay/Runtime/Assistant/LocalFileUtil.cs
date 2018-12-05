using Devil.Utility;
using System.IO;
using UnityEngine;

namespace Devil.GamePlay.Assistant
{
    public static class LocalFileUtil
    {
        public const int COMPRESS_LV = 5;

        // 资源解压路径
#if UNITY_EDITOR
        static string sExtractResFolder = Application.dataPath + "/../RESOURCE";
#else
    static string sExtractResFolder = Application.persistentDataPath + "/RESOURCE";
#endif

        static string sPrefabPath = "Assets/ArtRes/Prefabs";
        public static string ExtractResPath { get { return sExtractResFolder; } }

        public static string ABFolderName
        {
            get
            {
#if !UNITY_EDITOR && UNITY_ANDROID
            return GetABFolderName(RuntimePlatform.Android);
#elif !UNITY_EDITOR && UNITY_IOS
            return GetABFolderName(RuntimePlatform.IPhonePlayer);
#else
                return GetABFolderName(Application.platform);
#endif
            }
        }

        public static string AbPath
        {
            get
            {
#if !UNITY_EDITOR && UNITY_ANDROID
            return GetABPath(RuntimePlatform.Android);
#elif !UNITY_EDITOR && UNITY_IOS
            return GetABPath(RuntimePlatform.IPhonePlayer);
#else
                return GetABPath(Application.platform);
#endif
            }
        }

        public static string AbManifestName
        {
            get
            {
#if UNITY_EDITOR
                return "Windows";
#elif !UNITY_EDITOR && UNITY_ANDROID
            return "Android";
#elif !UNITY_EDITOR && UNITY_IOS
            return "IOS";
#else
            return "StreamingAssets";
#endif
            }
        }

        public static string PrefabPath { get { return sPrefabPath; } }

        public static string GetABPath(RuntimePlatform platform)
        {
            return StringUtil.Concat(Application.streamingAssetsPath, "/", GetABFolderName(platform));
        }

        public static string GetABFolderName(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.IPhonePlayer:
                    return "IOS";
                default:
                    return "AssetBundle";
            }
        }

#if UNITY_EDITOR
        public static string ActiveProjectFolder
        {
            get
            {
                string folder;
                var t = UnityEditor.Selection.activeObject;
                if (t == null)
                {
                    folder = "Asset";
                }
                else
                {
                    folder = UnityEditor.AssetDatabase.GetAssetPath(t);
                    if (File.Exists(folder))
                    {
                        var index = folder.LastIndexOf('/');
                        folder = folder.Substring(0, index);
                    }
                }
                return folder;
            }
        }
#endif
    }
}