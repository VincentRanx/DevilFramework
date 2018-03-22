using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class DevilEditorUtility
    {
        static Dictionary<string, Texture2D> m2DCache = new Dictionary<string, Texture2D>();

        public static T CreateAsset<T>(string fullPath, bool selectFile = false) where T:ScriptableObject
        {
            if (selectFile)
            {
                fullPath = EditorUtility.SaveFilePanel("Create " + typeof(T).Name, Installizer.InstallRoot, typeof(T).Name, "asset");
            }
            if (string.IsNullOrEmpty(fullPath))
                return null;
            fullPath = FileUtil.GetProjectRelativePath(fullPath);
            if (string.IsNullOrEmpty(fullPath))
            {
                EditorUtility.DisplayDialog("ERROR", "无效的文件夹名，请选择一个项目内的文件夹！", "OK");
                return null;
            }
            T t = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(t, fullPath);
            return t;
        }

        public static T CreateAsset<T>(string path, string fileName, bool confirmDirectory = true) where T : ScriptableObject
        {
            if(!Directory.Exists(path))
            {
                if (!confirmDirectory || !EditorUtility.DisplayDialog("Error", string.Format("路径({0})不存在, 是否创建路径？", path), "创建", "取消"))
                    return null;
                Directory.CreateDirectory(path);
            }
            T t = ScriptableObject.CreateInstance<T>();
            string p = Path.Combine(path, fileName);
            AssetDatabase.CreateAsset(t, p);
            return t;
        }

        public static Texture2D GetTexture(string path)
        {
            Texture2D tex;
            if(m2DCache.TryGetValue(path, out tex))
            {
                return tex;
            }
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            m2DCache[path] = tex;
            return tex;
        }
    }
}