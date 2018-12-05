using Devil.GamePlay.Assistant;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DevilEditor
{
    public static class UtilMenu
    {
        [MenuItem("Assets/Utils/Create Preload Assets")]
        static void CreatePreloadAssets()
        {
            List<GameObject> lst = new List<GameObject>();
            foreach(var t in Selection.gameObjects)
            {
                if(t != null && !t.scene.IsValid())
                {
                    lst.Add(t);
                }
            }
            if(lst.Count > 0)
            {
                string file = EditorUtility.SaveFilePanelInProject("Save Preload Asset", "PreloadAsset", "asset", "It's going to save preload asset", 
                    LocalFileUtil.ActiveProjectFolder);
                if(!string.IsNullOrEmpty(file))
                {
                    SavePreloadAsset(lst, file);
                }
            }
        }

        [MenuItem("Assets/Utils/Merge Preload Assets To")]
        static void MergeAssetsTo()
        {
            List<PreloadAssets> assets = new List<PreloadAssets>();
            foreach(var t in Selection.objects)
            {
                if (t is PreloadAssets)
                    assets.Add(t as PreloadAssets);
            }
            if(assets.Count > 0)
            {
                string file = EditorUtility.SaveFilePanelInProject("Save Preload Asset", "PreloadAsset", "asset", "It's going to save preload asset", 
                    LocalFileUtil.ActiveProjectFolder);
                if (!string.IsNullOrEmpty(file))
                {
                    MergeAssetsTo(assets, file);
                }
            }
        }

        public static void MergeAssetsTo(List<PreloadAssets> assets, string tofile)
        {
            if (assets == null || assets.Count == 0 || string.IsNullOrEmpty(tofile))
                return;
            var old = AssetDatabase.LoadAssetAtPath<PreloadAssets>(tofile);
            var preasset = old == null ? ScriptableObject.CreateInstance<PreloadAssets>() : old;
            foreach(var t in assets)
            {
                preasset.m_Assets.AddRange(t.m_Assets);
            }
            if (old != null)
            {
                EditorUtility.SetDirty(old);
            }
            else
            {
                if (File.Exists(tofile))
                    AssetDatabase.DeleteAsset(tofile);
                AssetDatabase.CreateAsset(preasset, tofile);
            }
            AssetDatabase.Refresh();
        }

        public static void SavePreloadAsset(List<GameObject> asset, string file)
        {
            if (asset == null || asset.Count == 0 || string.IsNullOrEmpty(file))
                return;
            var old = AssetDatabase.LoadAssetAtPath<PreloadAssets>(file);
            var preasset = old == null ? ScriptableObject.CreateInstance<PreloadAssets>() : old;
            preasset.m_Assets.AddRange(asset);
            if (old != null)
            {
                EditorUtility.SetDirty(old);
            }
            else
            {
                if (File.Exists(file))
                    AssetDatabase.DeleteAsset(file);
                AssetDatabase.CreateAsset(preasset, file);
            }
            AssetDatabase.Refresh();
        }
    }
}