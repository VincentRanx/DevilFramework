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

        public static string SelectedFolder
        {
            get
            {
                var select = Selection.activeObject;
                if (select == null)
                    return FileUtil.GetProjectRelativePath(Application.dataPath);
                string path = AssetDatabase.GetAssetPath(select);
                if (string.IsNullOrEmpty(path))
                    return FileUtil.GetProjectRelativePath(Application.dataPath);
                if (Directory.Exists(path))
                    return FileUtil.GetProjectRelativePath(path);
                FileInfo f = new FileInfo(path);
                return FileUtil.GetProjectRelativePath(f.DirectoryName);
            }
        }

        public static void ReleaseCache()
        {
            m2DCache.Clear();
        }

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
            if (string.IsNullOrEmpty(path))
                return null;
            Texture2D tex;
            if(m2DCache.TryGetValue(path, out tex))
            {
                return tex;
            }
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            m2DCache[path] = tex;
            return tex;
        }

        public static string FindFile(string fileName)
        {
            DirectoryInfo root = new DirectoryInfo(Application.dataPath);
            Stack<DirectoryInfo> subdics = new Stack<DirectoryInfo>();
            subdics.Push(root);
            while(subdics.Count > 0)
            {
                DirectoryInfo dic = subdics.Pop();
                FileInfo[] files = dic.GetFiles();
                int len = files == null ? 0 : files.Length;
                for(int i = 0; i < len; i++)
                {
                    FileInfo f = files[i];
                    if(f.Name == fileName)
                    {
                        return f.FullName;
                    }
                }
                DirectoryInfo[] dics = dic.GetDirectories();
                len = dics == null ? 0 : dics.Length;
                for(int i = 0; i < dics.Length; i++)
                {
                    subdics.Push(dics[i]);
                }
            }
            return null;
        }
        
    }
}