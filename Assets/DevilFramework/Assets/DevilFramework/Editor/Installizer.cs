using Devil.AI;
using Devil.Utility;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class Installizer
    {
        static string AI_IMPORT_FOLDER = "Assets/Scripts/AIModules";

        public static string InstallRoot { get; private set; }

        public static GUIStyle titleStyle = new GUIStyle();
        public static GUIContent titleContent = new GUIContent();

        public static GUIStyle contentStyle = new GUIStyle();
        public static GUIContent contentContent = new GUIContent();

        public static float TitleHeight { get; private set; }
        public static float ContentHeight { get; private set; }

        public static event System.Action OnReloaded = () => { };

        public static List<BehaviourMeta> BTTasks { get; private set; }
        public static List<BehaviourMeta> BTConditions { get; private set; }
        public static List<BehaviourMeta> BTServices { get; private set; }
        public static List<BehaviourMeta> BTControllers { get; private set; }

        static string libTemplate = 
@"%namespace%
public class BehaviourLib : BehaviourLibrary
{
    public BehaviourLib() : base()
    {

    }

    protected override void OnInit()
    {
%pattern%
    }
}";

        public static BehaviourMeta FindBTMeta(EBTNodeType type, string bname)
        {
            List<BehaviourMeta> lst = null;
            if (type == EBTNodeType.task)
                lst = BTTasks;
            else if (type == EBTNodeType.controller)
                lst = BTControllers;
            else if (type == EBTNodeType.condition)
                lst = BTConditions;
            else if (type == EBTNodeType.service)
                lst = BTServices;
            if (lst == null)
                return null;
            for(int i = 0; i < lst.Count; i++)
            {
                if (lst[i].Name == bname)
                    return lst[i];
            }
            return null;
        }

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
            string ends = "/DevilFramework/Editor/Installizer.cs";
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

                GenerateAILibrary();
                OnReloaded();
        }

        static void ParseAILibs(string scriptPath, List<string> genPatterns, HashSet<string> namespaces)
        {
            MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (mono == null)
                return;
            System.Type t = mono.GetClass();
            if (t == null)
                return;
            BehaviourTreeAttribute attr = Ref.GetCustomAttribute<BehaviourTreeAttribute>(t);
            BehaviourMeta meta = new BehaviourMeta(t);
            switch (meta.NodeType)
            {
                case EBTNodeType.task:
                    if (!string.IsNullOrEmpty(meta.Namespace))
                        namespaces.Add(meta.Namespace);
                    BTTasks.Add(meta);
                    genPatterns.Add(string.Format("mTasks[\"{0}\"] = () => new {1}();", meta.Name, meta.Name));
                    break;
                case EBTNodeType.service:
                    if (!string.IsNullOrEmpty(meta.Namespace))
                        namespaces.Add(meta.Namespace);
                    BTServices.Add(meta);
                    genPatterns.Add(string.Format("mServices[\"{0}\"] = () => new {1}();", meta.Name, meta.Name));
                    break;
                case EBTNodeType.condition:
                    if (!string.IsNullOrEmpty(meta.Namespace))
                        namespaces.Add(meta.Namespace);
                    BTConditions.Add(meta);
                    genPatterns.Add(string.Format("mConditions[\"{0}\"] = () => new {1}();", meta.Name, meta.Name));
                    break;
                case EBTNodeType.controller:
                    if (!string.IsNullOrEmpty(meta.Namespace))
                        namespaces.Add(meta.Namespace);
                    BTControllers.Add(meta);
                    genPatterns.Add(string.Format("mControllers[\"{0}\"] = (id) => new {1}(id);", meta.Name, meta.Name));
                    break;
                default:
                    break;
            }
        }

        static string GenCSharpe(List<string> genPatterns, HashSet<string> namespaces)
        {
            StringBuilder builder = new StringBuilder();
            int n0 = libTemplate.IndexOf("%namespace%");
            int len0 = 11;
            int n1 = libTemplate.IndexOf("%pattern%");
            int len2 = 9;
            builder.Append(libTemplate.Substring(0, n0));
            foreach (string str in namespaces)
            {
                builder.Append("using ").Append(str).Append(";\n");
            }
            builder.Append(libTemplate.Substring(n0 + len0, n1 - (n0 + len0)));
            foreach (string str in genPatterns)
            {
                builder.Append("        ").Append(str).Append('\n');
            }
            builder.Append(libTemplate.Substring(n1 + len2));
            return builder.ToString();
        }

        static void GenerateAILibrary()
        {
            BTTasks = new List<BehaviourMeta>();
            BTServices = new List<BehaviourMeta>();
            BTConditions = new List<BehaviourMeta>();
            BTControllers = new List<BehaviourMeta>();

            string[] scripts = AssetDatabase.FindAssets("t:script", new string[] { AI_IMPORT_FOLDER, Path.Combine(InstallRoot, "DevilFramework/Core/AIRepository/BehaviourTreeV2") });
            List<string> genPatterns = new List<string>();
            HashSet<string> namespaces = new HashSet<string>();
            namespaces.Add("Devil.AI");
            foreach (string guid in scripts)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ParseAILibs(path, genPatterns, namespaces);
            }

            if (!(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
            {
                string script = GenCSharpe(genPatterns, namespaces);
                File.WriteAllText(Path.Combine(AI_IMPORT_FOLDER, "BehaviourLib.cs"), script, Encoding.UTF8);

                AssetDatabase.Refresh();
            }
        }
    }
}