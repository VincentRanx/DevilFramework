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
            AssetDatabase.Refresh();
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