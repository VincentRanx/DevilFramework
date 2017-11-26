using UnityEditor;
using UnityEngine;

namespace DevilTeam.Editor
{
    public class Installizer
    {
        public static string InstallRoot { get; private set; }

        [InitializeOnLoadMethod]
        static void OnUnityLoaded()
        {
            //bool first = EditorPrefs.GetBool("Devil.Editor.first", true);
            EditorPrefs.SetBool("Devil.Editor.first", false);
            string[] assets = AssetDatabase.FindAssets("t:script Installizer");
            string ends = "/DevilFramework/Editor/Installizer.cs";
            foreach (string s in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.EndsWith(ends))
                {
                    path = path.Substring(0, path.Length - ends.Length);
                    EditorPrefs.SetString("Devil.Root", path);
                    InstallRoot = path;
                }
            }
        }
    }
}