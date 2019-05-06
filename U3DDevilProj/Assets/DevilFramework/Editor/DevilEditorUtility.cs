using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class DevilEditorUtility
    {
        public struct Style
        {
            public GUIStyle style;
            public GUIStyle hint;
        }

        static readonly string intPattern = @"^(\-|\+)?\d+$";
        static readonly string floatPattern = @"^(\-|\+)?\d+(\.\d+)?$";

        static Dictionary<string, Texture2D> m2DCache = new Dictionary<string, Texture2D>();

        [InitializeOnLoadMethod]
        static void OnLoaded()
        {
            doloadgui = true;
        }

        readonly static GUIContent empty = new GUIContent("");
        public static GUIContent EmptyContent { get { return empty; } }
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
        #region assets
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
        #endregion

        #region gui

        static bool doloadgui;
        static Dictionary<string, Style> styles = new Dictionary<string, Style>();
        static void DoLoadGUI()
        {
            if (doloadgui)
            {
                foreach (GUIStyle skin in GUI.skin)
                {
                    Style style;
                    style.style = skin;
                    var hint = new GUIStyle(skin);
                    hint.normal.textColor *= 0.7f;
                    hint.fontStyle = FontStyle.Italic;
                    style.hint = hint;
                    styles[skin.name.ToLower()] = style;
                }
                doloadgui = false;
            }
        }

        public static GUIStyle NormalStyle(string style)
        {
            DoLoadGUI();
            return styles[style].style;
        }

        public static GUIStyle HintStyle(string style)
        {
            DoLoadGUI();
            return styles[style].hint;
        }

        public static Style StyleDefine(string style)
        {
            DoLoadGUI();
            return styles[style];
        }

        public static SerializedProperty FindRelativeProperty(SerializedProperty pro, string path)
        {
            if (path.IndexOf('/') == 0)
                return pro.FindPropertyRelative(path);
            var s = path.Split('/');
            var cfg = pro;
            for (int i = 0; i < s.Length; i++)
            {
                cfg = cfg.FindPropertyRelative(s[i]);
            }
            return cfg;
        }

        public static void Hint(Rect rect, string hint, string style = "label")
        {
            DoLoadGUI();
            var sty = styles[style];
            EditorGUI.SelectableLabel(rect, hint, sty.hint);
        }

        public static int MaskGUI(Rect rect, int mask, string[] names)
        {
            int len = Mathf.Min(names.Length, 32);
            if (len == 0)
                return mask;
            if (len == 1)
            {
                var tog = GUI.Toggle(rect, (mask & 1) != 0, names[0], "button");
                if (tog)
                    mask |= 1;
                else
                    mask &= ~1;
                return mask;
            }
            float w = rect.width / (float)len;
            for (int i = 0; i < len; i++)
            {
                int n = 1 << i;
                var style = i == 0 ? "buttonleft" : (i == len - 1 ? "buttonright" : "buttonmid");
                var tog = GUI.Toggle(new Rect(rect.x + i * w, rect.y, w, rect.height),
                    (mask & n) != 0, names[i], style);
                if (tog)
                    mask |= n;
                else
                    mask &= ~n;
            }
            return mask;
        }

        public static void TextArea(Rect rect, SerializedProperty prop, string style = "textarea")
        {
            prop.stringValue = TextArea(rect, prop.stringValue, prop.displayName, style);
        }

        public static string TextArea(Rect rect, string txt, string hint, string style = "textarea")
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = string.IsNullOrEmpty(txt);
            var s = EditorGUI.TextArea(rect, dohint ? hint : txt, dohint ? sty.hint : sty.style);
            if (s == hint)
                return "";
            else
                return s;
        }

        public static void TextArea(SerializedProperty prop, string style = "textarea", params GUILayoutOption[] options)
        {
            prop.stringValue = TextArea(prop.stringValue, prop.displayName, style, options);
        }

        public static string TextArea(string txt, string hint, string style = "textarea", params GUILayoutOption[] options)
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = string.IsNullOrEmpty(txt);
            var s = EditorGUILayout.TextArea(dohint ? hint : txt, dohint ? sty.hint : sty.style, options);
            if (s == hint)
                return "";
            else
                return s;
        }

        public static void TextField(SerializedProperty prop, string style = "textfield", params GUILayoutOption[] options)
        {
            prop.stringValue = TextField(prop.stringValue, prop.displayName, style, options);
        }

        public static string TextField(string txt, string hint, string style = "textfield", params GUILayoutOption[] options)
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = string.IsNullOrEmpty(txt);
            var s = EditorGUILayout.TextField(dohint ? hint : txt, dohint ? sty.hint : sty.style, options);
            if (s != hint)
                return s;
            else
                return "";
        }

        public static string TextField(string prefix, string txt, string hint, string style, params GUILayoutOption[] options)
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = string.IsNullOrEmpty(txt);
            var s = EditorGUILayout.TextField(prefix, dohint ? hint : txt, dohint ? sty.hint : sty.style, options);
            if (s != hint)
                return s;
            else
                return "";
        }

        public static void TextField(Rect rect, SerializedProperty prop, string style = "textfield")
        {
            prop.stringValue = TextField(rect, prop.stringValue, prop.displayName, style);
        }

        public static string TextField(Rect rect, string txt, string hint, string style = "textfield")
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = string.IsNullOrEmpty(txt);
            var s = EditorGUI.TextField(rect, dohint ? hint : txt, dohint ? sty.hint : sty.style);
            if (s != hint)
                return s;
            else
                return "";
        }
        
        public static void IntField(Rect rect, SerializedProperty prop, string style = "textfield")
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = prop.intValue == 0;

            var old = prop.intValue.ToString();
            string s = EditorGUI.TextField(rect, dohint ? prop.displayName : old, dohint ? sty.hint : sty.style);
            if (string.IsNullOrEmpty(s))
                prop.intValue = 0;
            else if (old != s && Regex.IsMatch(s, intPattern))
                prop.intValue = int.Parse(s);
        }

        public static int IntField(Rect rect, int value, string hint, string style = "textfield")
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = value == 0;

            var old = value.ToString();
            string s = EditorGUI.TextField(rect, dohint ? hint : old, dohint ? sty.hint : sty.style);
            if (string.IsNullOrEmpty(s) || s == hint)
                return 0;
            else if (old != s && Regex.IsMatch(s, intPattern))
                return int.Parse(s);
            else
                return value;
        }

        public static int IntField(string prefix, string hint, int value, string style = "textfield", params GUILayoutOption[] options)
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = value == 0;
            var old = value.ToString();
            var hintstr = string.IsNullOrEmpty(hint) ? prefix : hint;
            var s = EditorGUILayout.TextField(prefix, dohint ? hintstr : old, dohint ? sty.hint : sty.style, options);
            if (string.IsNullOrEmpty(s) || s == hintstr)
                return 0;
            else if (old != s && Regex.IsMatch(s, intPattern))
                return int.Parse(s);
            else
                return value;
        }

        public static int IntField(string hint, int value, string style = "textfield", params GUILayoutOption[] options)
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = value == 0;
            var old = value.ToString();
            var s = EditorGUILayout.TextField(dohint ? hint : old, dohint ? sty.hint : sty.style, options);
            if (string.IsNullOrEmpty(s) || s == hint)
                return 0;
            else if (old != s && Regex.IsMatch(s, intPattern))
                return int.Parse(s);
            else
                return value;
        }

        public static void IntField(SerializedProperty prop, string hint, string style = "textfield")
        {
            DoLoadGUI();
            var sty = styles[style];
            var lv = EditorGUI.indentLevel;
            bool dohint = prop.intValue == 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(prop.displayName);
            EditorGUI.indentLevel = 0;
            var old = prop.intValue.ToString();
            var hintsr = string.IsNullOrEmpty(hint) ? prop.displayName : hint;
            var s = EditorGUILayout.TextField(dohint ? hintsr : old, dohint ? sty.hint : sty.style);
            if (string.IsNullOrEmpty(s) || s == hintsr)
                prop.intValue = 0;
            else if (old != s && Regex.IsMatch(s, intPattern))
                prop.intValue = int.Parse(s);
            EditorGUI.indentLevel = lv;
            EditorGUILayout.EndHorizontal();
        }

        public static void FloatField(Rect rect, SerializedProperty prop, string style = "textfield")
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = prop.floatValue == 0;

            var old = prop.floatValue.ToString();
            string s = EditorGUI.TextField(rect, dohint ? prop.displayName : old, dohint ? sty.hint : sty.style);
            if (string.IsNullOrEmpty(s))
                prop.floatValue = 0;
            else if (old != s && Regex.IsMatch(s, floatPattern))
                prop.floatValue = float.Parse(s);
        }

        public static float FloatField(Rect rect, string hint, float value, string style = "textfield")
        {
            DoLoadGUI();
            var sty = styles[style];
            bool dohint = value == 0;

            var old = value.ToString();
            string s = EditorGUI.TextField(rect, dohint ? hint : old, dohint ? sty.hint : sty.style);
            if (string.IsNullOrEmpty(s))
                return 0;
            else if (old != s && Regex.IsMatch(s, floatPattern))
                return float.Parse(s);
            else
                return value;
        }
        
        // size:箭头粗细 arrowScale: 箭头大小/线条粗细
        public static void DrawArrow(Vector3 p0, Vector3 p1, Color color, float size, float arrowScale = 4)
        {
            var dir = p1 - p0;
            var length = dir.magnitude;
            if (length < 0.1f)
                return;
            dir /= length;
            var dir2 = Vector3.Cross(Vector3.forward, dir);
            Handles.color = color;
            if (length > size * arrowScale)
            {
                var a = p0 + dir2 * size * 0.5f;
                var b = p0 - dir2 * size * 0.5f;
                var c = b + dir * (length - size * arrowScale);
                var d = c - dir2 * size * (arrowScale * 0.5f - 0.5f);
                var e = p1;
                var f = d + dir2 * size * arrowScale;
                var g = c + dir2 * size;
                Handles.DrawAAConvexPolygon(a, b, c, g);
                Handles.DrawAAConvexPolygon(d, e, f);
            }
            else
            {
                var a = p1 - dir * length - dir2 * length * 0.5f;
                var b = p1;
                var c = a + dir2 * length;
                Handles.DrawAAConvexPolygon(a, b, c);
            }
        }

        public static void Draw2SideArrow(Vector3 p0, Vector3 p1, Color color, float size, float arrowScale = 4)
        {
            var dir = p1 - p0;
            var length = dir.magnitude;
            if (length < 0.1f)
                return;
            dir /= length;
            var dir2 = Vector3.Cross(Vector3.forward, dir);
            Handles.color = color;
            if (length > size * arrowScale * 2f) // <--------->
            {
                var a = p0 + dir2 * size * 0.5f + dir * size * arrowScale;
                var b = a - dir2 * size;
                var c = b + dir * (length - size * arrowScale * 2f);
                var d = c + dir2 * size;
                Handles.DrawAAConvexPolygon(a, b, c, d);
                var e = a + dir2 * size * (arrowScale * 0.5f - 0.5f);
                var f = p0;
                var g = e - dir2 * size * arrowScale;
                Handles.DrawAAConvexPolygon(e, f, g);
                e = c - dir2 * size * (arrowScale * 0.5f - 0.5f);
                f = p1;
                g = e + dir2 * size * arrowScale;
                Handles.DrawAAConvexPolygon(e, f, g);
            }
            else // <>
            {
                var a = (p0 + p1) * 0.5f - dir2 * length * 0.25f;
                var b = a + dir2 * length * 0.5f;
                Handles.DrawAAConvexPolygon(p0, a, p1, b);
            }
        }

        #endregion
    }
}