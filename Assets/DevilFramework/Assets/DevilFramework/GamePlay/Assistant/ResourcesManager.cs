//#define USE_ATLAS

using Devil;
using Devil.AsyncTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Devil.GamePlay
{

    public class ResourcesManager : Singleton<ResourcesManager>
    {
        public class ABLoader : IAsyncTask
        {
            string mFile;
            AssetBundleCreateRequest mLoader;

            public ABLoader(string abName)
            {
                mFile = abName;
            }

            public void OnStart()
            {
                mLoader = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, mFile));
            }

            public float Progress { get; private set; }

            public bool IsDone { get; private set; }

            public void OnInterrupt()
            {
                IsDone = true;
            }

            public void OnTick(float deltaTime)
            {
                if (mLoader != null)
                {
                    Progress = mLoader.progress;
                    IsDone = mLoader.isDone;
                }
                if (IsDone && mLoader != null)
                {
                    GetOrNewInstance().AddBunde(mLoader.assetBundle);
#if UNITY_EDITOR
                    Debug.LogFormat("AssetBundle Loaded: {0}", mFile);
#endif
                }
            }
        }

        Dictionary<string, AssetBundle> mBundles = new Dictionary<string, AssetBundle>();
        Dictionary<string, string> mABMap = new Dictionary<string, string>();

        public void AddBunde(AssetBundle bundle)
        {
            if (bundle == null)
                return;
            mBundles[bundle.name] = bundle;
            string[] assetNames = bundle.GetAllAssetNames();
            int len = assetNames == null ? 0 : assetNames.Length;
            for (int i = 0; i < len; i++)
            {
                mABMap[assetNames[i]] = bundle.name;
            }
        }

        public virtual T GetAsset<T>(string asset, string bundle = null) where T : Object
        {
#if UNITY_EDITOR && !USE_ATLAS
            if (asset != null && asset.StartsWith("Assets/"))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(asset);
#elif UNITY_EDITOR
        if (!Application.isPlaying && asset != null && asset.StartsWith("Assets/"))
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(asset);
#endif
            AssetBundle ab;
            asset = asset.ToLower();
            string abName;
            if (bundle != null || !mABMap.TryGetValue(asset, out abName))
                abName = bundle;
            if (!string.IsNullOrEmpty(abName) && mBundles.TryGetValue(abName, out ab))
            {
                return ab.LoadAsset<T>(asset);
            }
            return Resources.Load<T>(asset);
        }

        public virtual void GetAssetAsync<T>(string asset, string bundle, System.Action<T> handler, System.Action errorHandler = null) where T : Object
        {
#if UNITY_EDITOR && !USE_ATLAS
            if (asset != null && asset.StartsWith("Assets/"))
            {
                T result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(asset);
                if (handler != null && result != null)
                    handler(result);
                else if (result == null && errorHandler != null)
                    errorHandler();
                return;
            }
#elif UNITY_EDITOR
        if (!Application.isPlaying && asset != null && asset.StartsWith("Assets/"))
        {
            T result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(asset);
            if (handler != null && result != null)
                handler(result);
            else if (result == null && errorHandler != null)
                errorHandler();
            return;
        }
#endif
            T assetinst = GetAsset<T>(asset, bundle);
            if (assetinst != null && handler != null)
                handler(assetinst);
            else if (assetinst == null && errorHandler != null)
                errorHandler();
        }

        public void ParseABFiles()
        {
            TextAsset asset = GetAsset<TextAsset>("Assets/AB/ablist.json", "preset");
            if (asset != null)
            {
                mABMap.Clear();
                JObject obj = JsonConvert.DeserializeObject<JObject>(asset.text);
                foreach (JProperty pro in obj.Properties())
                {
                    mABMap[pro.Name] = pro.Value.ToString();
                }
            }
        }

        public bool HasAB(string abName)
        {
            return mBundles.ContainsKey(abName);
        }
    }
}