using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class Installizer
    {
        public static string InstallRoot { get; private set; }

        public static GUIStyle titleStyle = new GUIStyle();
        public static GUIContent titleContent = new GUIContent();

        public static GUIStyle contentStyle = new GUIStyle();
        public static GUIContent contentContent = new GUIContent();

        public static float TitleHeight { get; private set; }
        public static float ContentHeight { get; private set; }

        public static Vector2 SizeOfTitle(string text)
        {
            titleContent.text = text ?? "";
            return titleStyle.CalcSize(titleContent) + new Vector2(30, 10);
        }

        public static Vector2 SizeOfContent(string text)
        {
            contentContent.text = text ?? "";
            return contentStyle.CalcSize(contentContent) + new Vector2(10, 5);
        }

        [MenuItem("Devil Framework/Reload")]
        static void ReloadDevilFramewrok()
        {
            OnUnityLoaded();
        }

        [InitializeOnLoadMethod]
        static void OnUnityLoaded()
        {
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.richText = true;
            titleStyle.fontSize = 13;
            titleStyle.wordWrap = false;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.green;
            titleStyle.onHover.textColor = Color.yellow;

            contentStyle.alignment = TextAnchor.MiddleCenter;
            contentStyle.richText = true;
            contentStyle.fontSize = 10;
            contentStyle.wordWrap = false;
            contentStyle.fontStyle = FontStyle.Normal;
            contentStyle.normal.textColor = Color.white;
            contentStyle.onHover.textColor = Color.white;


            string txt = "ABCDEFG";
            Vector2 size = SizeOfTitle(txt);
            TitleHeight = size.y;
            size = SizeOfContent(txt);
            ContentHeight = size.y;

            EditorPrefs.SetBool("Devil.Editor.first", false);
            string[] assets = AssetDatabase.FindAssets("t:script Installizer");
            string ends = "/DevilFramework/Core/Editor/Installizer.cs";
            foreach (string s in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.EndsWith(ends))
                {
                    path = path.Substring(0, path.Length - ends.Length);
                    EditorPrefs.SetString("Devil.Root", path);
                    InstallRoot = path;
                    Debug.Log("Devil Framework install at path: " + path);
                }
            }
        }
    }
}