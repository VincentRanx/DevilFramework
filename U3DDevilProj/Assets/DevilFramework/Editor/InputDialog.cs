using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class InputDialog : EditorWindow 
	{
        public const string FLOAT_PATTERN = @"^(\-|\+)?\d+(\.\d+)?$";
        public const string INT_PATTERN = @"^(\-|\+)?\d+$";

        public static void Ask(string title, string msg, string defaultvalue, string inputPattern, System.Action<bool, string> handleResult)
        {
            var win = GetWindow<InputDialog>(true, title, true);
            win.maxSize = new Vector2(250, 200);
            win.minSize = new Vector2(250, 200);
            win.msg = msg;
            win.content = defaultvalue;
            win.pattern = inputPattern;
            win.handler = handleResult;
            win.ShowUtility();
        }

        public static void AskFloat(string title, string msg, System.Action<bool,float> handler)
        {
            Ask(title, msg, "0", FLOAT_PATTERN, (x, y) => {
                if (handler != null)
                {
                    if (x)
                        handler(true, float.Parse(y));
                    else
                        handler(false, 0);
                }
            });
        }

        string pattern = "";
        string msg = "";
        string content = "";
        System.Action<bool, string> handler;

        private void OnGUI()
        {
            EditorGUILayout.LabelField(msg);
            var result = EditorGUILayout.TextArea(content);
            if (string.IsNullOrEmpty(pattern) || Regex.IsMatch(result, pattern))
                content = result;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(100);
            var ok = GUILayout.Button("OK");
            var cancel = GUILayout.Button("Cancel");
            EditorGUILayout.EndVertical();
            if(ok)
            {
                if (handler != null)
                    handler(true, content);
                handler = null;
                Close();
            }
            else if (cancel)
            {
                if (handler != null)
                    handler(false, null);
                handler = null;
                Close();
            }
        }
    }
}