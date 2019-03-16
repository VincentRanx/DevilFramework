using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Devil.Utility;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Devil
{
    public class BakedLightmapData : ScriptableObject
    {
        [System.Serializable]
        public struct LightmapInfo
        {
            public Texture2D lightColor;
            public Texture2D lightDir;
            public Texture2D shadowMask;
        }

        public LightmapInfo[] m_Lightmaps;

        public void LoadToScene()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                var lightmap = new LightmapData[m_Lightmaps.Length];
                for (int i = 0; i < lightmap.Length; i++)
                {
                    var data = new LightmapData();
                    data.lightmapColor = m_Lightmaps[i].lightColor;
                    data.lightmapDir = m_Lightmaps[i].lightDir;
                    data.shadowMask = m_Lightmaps[i].shadowMask;
                    lightmap[i] = data;
                }
                LightmapSettings.lightmaps = lightmap;
#if UNITY_EDITOR
            }
#endif
        }

#if UNITY_EDITOR

        [MenuItem("Assets/Utils/Save Scene's Lightmap")]
        public static void BakeLightmap()
        {
            string folder;
            var t = Selection.activeObject;
            if(t == null)
            {
                folder = "Asset/";
            }
            else
            {
                folder = AssetDatabase.GetAssetPath(t);
                if(File.Exists(folder))
                {
                    var index = folder.LastIndexOf('/');
                    folder = folder.Substring(0, index + 1);
                }
            }
            CreateAssetForActiveScene(folder);
        }

        [MenuItem("Assets/Utils/Load as Lightmap Data")]
        public static void LoadData()
        {
            var sel = Selection.activeObject as BakedLightmapData;
            if (sel != null)
                sel.LoadToScene();
        }

        public static BakedLightmapData CreateAssetForActiveScene(string folder)
        {
            var scene = SceneManager.GetActiveScene().name;
            string file = EditorUtility.SaveFilePanelInProject("Save Baked Lightmap", scene, "asset", "Save Asset", folder);
            if (string.IsNullOrEmpty(file))
                return null;
            folder = Path.GetDirectoryName(file);
            var fname = Path.GetFileName(file);
            if (StringUtil.EndWithIgnoreCase(fname, ".asset"))
                fname = fname.Substring(0, fname.Length - 6);
            BakedLightmapData data = ScriptableObject.CreateInstance<BakedLightmapData>();
            var lightmaps = LightmapSettings.lightmaps;
            int len = lightmaps == null ? 0 : lightmaps.Length;
            data.m_Lightmaps = new LightmapInfo[len];
            var ptitle = "Copy Lightmap Datas";
            var plen = (float)len * 3;
            for (int i = 0; i < len; i++)
            {
                LightmapInfo info;
                var color = lightmaps[i].lightmapColor == null ? null : AssetDatabase.GetAssetPath(lightmaps[i].lightmapColor);
                var dir = lightmaps[i].lightmapDir == null ? null : AssetDatabase.GetAssetPath(lightmaps[i].lightmapDir);
                var mask = lightmaps[i].shadowMask == null ? null : AssetDatabase.GetAssetPath(lightmaps[i].shadowMask);
                var color2 = color == null ? null : string.Format("{0}/{1}_{2}_light.png", folder, fname, i);
                var dir2 = dir == null ? null : string.Format("{0}/{1}_{2}_dir.png", folder, fname, i);
                var mask2 = mask == null ? null : string.Format("{0}/{1}_{2}_shadowmask.png", folder, fname, i);

                EditorUtility.DisplayProgressBar(ptitle, string.Format("Copy light colors[{0}]...", i), (float)(i * 3) / plen);
                if (color2 != null && File.Exists(color2))
                    AssetDatabase.DeleteAsset(color2);
                if (color2 != null && AssetDatabase.CopyAsset(color, color2))
                    info.lightColor = AssetDatabase.LoadAssetAtPath<Texture2D>(color2);
                else
                    info.lightColor = null;
                EditorUtility.DisplayProgressBar(ptitle, string.Format("Copy light dirs[{0}]...", i), (float)(i * 3 + 1) / plen);
                if (dir2 != null && File.Exists(dir2))
                    AssetDatabase.DeleteAsset(dir2);
                if (dir2 != null && AssetDatabase.CopyAsset(dir, dir2))
                    info.lightDir = AssetDatabase.LoadAssetAtPath<Texture2D>(dir2);
                else
                    info.lightDir = null;
                EditorUtility.DisplayProgressBar(ptitle, string.Format("Copy shadow masks[{0}]...", i), (float)(i * 3 + 2) / plen);
                if (mask2 != null && File.Exists(mask2))
                    AssetDatabase.DeleteAsset(mask2);
                if (mask2 != null && AssetDatabase.CopyAsset(mask, mask2))
                    info.shadowMask = AssetDatabase.LoadAssetAtPath<Texture2D>(mask2);
                else
                    info.shadowMask = null;
                data.m_Lightmaps[i] = info;
            }
            if (File.Exists(file))
                AssetDatabase.DeleteAsset(file);
            AssetDatabase.CreateAsset(data, file);
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            return data;
        }
#endif
    }
}