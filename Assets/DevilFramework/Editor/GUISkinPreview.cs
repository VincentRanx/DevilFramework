using DevilTeam.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilTeam.Editor
{


    #region subpage : GUI style

    public class GUIStyleViewport : CheckWindow.SubPage
    {
        public GUIStyleViewport(CheckWindow window) : base("GUI Styles", window) { }

        string search = "";
        Vector2 scrollPosition;

        public override void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            //GUILayout.Label("单击示例将复制其名到剪贴板", "label");
            //GUILayout.FlexibleSpace();
            //GUILayout.Label("查找:");
            search = QuickGUI.SearchTextBar(search);
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (GUIStyle style in GUI.skin)
            {
                if (style.name.ToLower().Contains(search.ToLower()))
                {
                    GUILayout.BeginHorizontal("GridList");
                    GUILayout.Space(7);
                    if (GUILayout.Button(StringUtil.WrapString(style.name, 5, "."), style, GUILayout.Width(100)))
                    {
                        EditorGUIUtility.systemCopyBuffer = "\"" + style.name + "\"";
                    }
                    GUILayout.Space(10);
                    GUILayout.Toggle(true, "t", style, GUILayout.Width(50));
                    GUILayout.Space(7);
                    GUILayout.Toggle(false, "f", style, GUILayout.Width(50));
                    GUILayout.Space(10);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.SelectableLabel("\"" + style.name + "\"");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(11);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }

    #endregion


}
