using Devil.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{


    #region subpage : GUI style

    public class GUIStyleViewport : CheckWindow.SubPage
    {
        public GUIStyleViewport(CheckWindow window) : base("GUI Styles", window) { }

        string search = "";
        Vector2 scrollPosition;
        List<GUIStyle> mStyles = new List<GUIStyle>();
        int offIndex;
        Vector2 offPos;

        public override void OnEnable()
        {
            mStyles.Clear();
            //foreach (GUIStyle style in GUI.skin)
            //{
            //    mStyles.Add(style);
            //}
        }

        public override void OnGUI()
        {
            if (mStyles.Count == 0)
            {
                foreach (GUIStyle style in GUI.skin)
                {
                    mStyles.Add(style);
                }
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            //GUILayout.Label("单击示例将复制其名到剪贴板", "label");
            //GUILayout.FlexibleSpace();
            //GUILayout.Label("查找:");
            search = QuickGUI.SearchTextBar(search);
            GUILayout.EndHorizontal();

            QuickGUI.StepVerticalScroll(ref offIndex, ref offPos, 30, mStyles.Count,
                (id, counter) =>
                {
                    GUIStyle style = mStyles[id];
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
                        return true;
                    }
                    return false;
                }, "box");

            //scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            //foreach (GUIStyle style in GUI.skin)
            //{
            //    if (style.name.ToLower().Contains(search.ToLower()))
            //    {
            //        GUILayout.BeginHorizontal("GridList");
            //        GUILayout.Space(7);
            //        if (GUILayout.Button(StringUtil.WrapString(style.name, 5, "."), style, GUILayout.Width(100)))
            //        {
            //            EditorGUIUtility.systemCopyBuffer = "\"" + style.name + "\"";
            //        }
            //        GUILayout.Space(10);
            //        GUILayout.Toggle(true, "t", style, GUILayout.Width(50));
            //        GUILayout.Space(7);
            //        GUILayout.Toggle(false, "f", style, GUILayout.Width(50));
            //        GUILayout.Space(10);
            //        GUILayout.FlexibleSpace();
            //        EditorGUILayout.SelectableLabel("\"" + style.name + "\"");
            //        GUILayout.EndHorizontal();
            //        GUILayout.Space(11);
            //    }
            //}
            //GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }

    #endregion


}
