using UnityEngine;
using UnityEditor;

namespace DevilTeam.Editor
{
    public class QuickGUI
    {
        static Rect cachedViewportRect;
        static Vector2 mousePos;
        static Vector2 mouseDeltaPos;

        public delegate bool DrawCallback(int id, int counter);

        public static int TitleBar(string title, int size, FontStyle style, Color color, params string[] moreBtns)
        {
            int defSize = GUI.skin.label.fontSize;
            FontStyle defStyle = GUI.skin.label.fontStyle;
            Color defColor = GUI.skin.label.normal.textColor;

            GUI.skin.label.fontSize = size;
            GUI.skin.label.fontStyle = style;
            GUI.skin.label.normal.textColor = color;

            int ret = -1;

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("t_" + title);
            bool b = GUILayout.Toggle(true, title, "label", GUILayout.Height(size + 10));
            if (!b)
                ret = 0;
            string dest;
            float width;
            for (int i = 0; i < moreBtns.Length; i++)
            {
                string sty;
                if (moreBtns.Length > 1)
                {
                    sty = i == 0 ? "ButtonLeft" : i == moreBtns.Length - 1 ? "ButtonRight" : "ButtonMid";
                }
                else
                {
                    sty = "button";
                }
                if (!WidthOption(moreBtns[i], out dest, out width))
                {
                    width = 10 + Mathf.Min(30, moreBtns[i].Length * 10);
                }
                bool ok = dest.StartsWith("true:");
                if (ok)
                    dest = dest.Substring(5);
                else if (dest.StartsWith("false:"))
                    dest = dest.Substring(6);
                bool ok2 = GUILayout.Toggle(ok, dest, sty, GUILayout.MaxWidth(width));
                if (ok2 ^ ok && ret == -1)
                    ret = i + 1;
                //if (GUILayout.Button(dest, sty, GUILayout.MaxWidth(width)) && ret == -1)
                //{
                //    ret = i + 1;
                //}
            }


            EditorGUILayout.EndHorizontal();

            GUI.skin.label.fontSize = defSize;
            GUI.skin.label.fontStyle = defStyle;
            GUI.skin.label.normal.textColor = defColor;
            return ret;
        }

        //标题栏,返回按钮索引,0表示标题本身
        public static int TitleBar(string title, int size, params string[] moreBtns)
        {
            int defSize = GUI.skin.label.fontSize;
            FontStyle defStyle = GUI.skin.label.fontStyle;

            GUI.skin.label.fontSize = size;
            GUI.skin.label.fontStyle = FontStyle.Bold;

            int ret = -1;

            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("t_" + title);
            bool b = GUILayout.Toggle(true, title, "label", GUILayout.Height(size + 10));
            if (!b)
                ret = 0;
            string dest;
            float width;
            for (int i = 0; i < moreBtns.Length; i++)
            {
                string sty;
                if (moreBtns.Length > 1)
                {
                    sty = i == 0 ? "ButtonLeft" : i == moreBtns.Length - 1 ? "ButtonRight" : "ButtonMid";
                }
                else
                {
                    sty = "button";
                }
                if (!WidthOption(moreBtns[i], out dest, out width))
                {
                    width = 10 + Mathf.Min(30, moreBtns[i].Length * 10);
                }
                bool ok = dest.StartsWith("true:");
                if (ok)
                    dest = dest.Substring(5);
                else if (dest.StartsWith("false:"))
                    dest = dest.Substring(6);
                bool ok2 = GUILayout.Toggle(ok, dest, sty, GUILayout.MaxWidth(width));
                if (ok2 ^ ok && ret == -1)
                    ret = i + 1;
                //if (GUILayout.Button(dest, sty, GUILayout.MaxWidth(width)) && ret == -1)
                //{
                //    ret = i + 1;
                //}
            }

            EditorGUILayout.EndHorizontal();

            GUI.skin.label.fontSize = defSize;
            GUI.skin.label.fontStyle = defStyle;

            return ret;
        }

        public static int HTabBar(int chosen, int size, int colLimit, params string[] tabs)
        {
            if (tabs == null || tabs.Length == 0)
                return -1;

            int tab = chosen;

            int defSize = GUI.skin.label.fontSize;
            FontStyle defStyle = GUI.skin.label.fontStyle;
            Color defColor = GUI.skin.label.normal.textColor;

            GUI.skin.label.fontSize = size;
            GUI.skin.label.fontStyle = FontStyle.Bold;

            int col = colLimit > 0 ? colLimit : tabs.Length;
            int row = tabs.Length / col;
            if (tabs.Length % col > 0)
                row++;
            EditorGUILayout.BeginHorizontal();
            if (row > 1)
            {
                EditorGUILayout.BeginVertical();
                int selectRow = chosen / col;
                for (int r = 0; r < row; r++)
                {
                    if (r == selectRow)
                        continue;
                    EditorGUILayout.BeginHorizontal();
                    for (int c = 0; c < col; c++)
                    {
                        int i = r * col + c;
                        if (i >= tabs.Length)
                            break;
                        GUI.skin.label.normal.textColor = i == chosen ? Color.blue : Color.black;
                        bool b = GUILayout.Toggle(true, tabs[i], "label", GUILayout.Height(size + 10));
                        if (!b)
                            tab = i;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                for (int c = 0; c < col; c++)
                {
                    int i = selectRow * col + c;
                    if (i >= tabs.Length)
                        break;
                    GUI.skin.label.normal.textColor = i == chosen ? Color.blue : Color.black;
                    bool b = GUILayout.Toggle(true, tabs[i], "label", GUILayout.Height(size + 10));
                    if (!b)
                        tab = i;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            else
            {
                for (int i = 0; i < tabs.Length; i++)
                {
                    GUI.skin.label.normal.textColor = i == chosen ? Color.blue : Color.black;
                    bool b = GUILayout.Toggle(true, tabs[i], "label", GUILayout.Height(size + 10));
                    if (!b)
                        tab = i;
                }
            }

            EditorGUILayout.EndHorizontal();

            GUI.skin.label.fontSize = defSize;
            GUI.skin.label.fontStyle = defStyle;
            GUI.skin.label.normal.textColor = defColor;
            return tab;
        }

        public static void HSeperator()
        {
            Color c = Handles.color;
            Rect r = EditorGUILayout.BeginHorizontal();
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(r.xMin, r.yMax - 1), new Vector3(r.xMax, r.yMax - 1));
            Handles.color = Color.white;
            Handles.DrawLine(new Vector3(r.xMin, r.yMax), new Vector3(r.xMax, r.yMax));
            EditorGUILayout.EndHorizontal();
            Handles.color = c;
        }


        public static void HLine(Color color)
        {
            Color c = Handles.color;
            Rect r = EditorGUILayout.BeginHorizontal();
            Handles.color = color;
            Handles.DrawLine(new Vector3(r.xMin, r.yMax), new Vector3(r.xMax, r.yMax));
            EditorGUILayout.EndHorizontal();
            Handles.color = c;
        }

        public static string SearchTextBar(string txt, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            string s = GUILayout.TextField(txt, (GUIStyle)"SearchTextField");
            if (GUILayout.Button("", "SearchCancelButton"))
            {
                s = "";
            }
            EditorGUILayout.EndHorizontal();
            return s;
        }

        public static int MultiOptionBar(int selected, string[] titles, params GUILayoutOption[] options)
        {
            return MultiOptionBar(selected, titles, 0, titles.Length, options);
        }

        public static int MultiOptionBar(int selected, string[] titles, int start, int end, params GUILayoutOption[] options)
        {
            int ret = selected;
            EditorGUILayout.BeginHorizontal(options);
            string dest;
            float width;
            for (int i = start; i < end; i++)
            {
                int p = 0x1 << i;
                bool v = (selected & p) != 0;
                bool nv;
                string sty = titles.Length == 1 ? "button" : (i == 0 ? "ButtonLeft" : (i == end - 1 ? "ButtonRight" : "ButtonMid"));
                if (WidthOption(titles[i], out dest, out width))
                    nv = GUILayout.Toggle(v, dest, sty, GUILayout.Width(width));
                else
                    nv = GUILayout.Toggle(v, dest, sty);
                if (nv ^ v)
                {
                    if (nv)
                        ret |= p;
                    else
                        ret &= ~p;
                }
            }
            EditorGUILayout.EndHorizontal();
            return ret;
        }

        public static int GroupButton(string[] texts, params GUILayoutOption[] options)
        {
            int ret = -1;
            EditorGUILayout.BeginHorizontal(options);
            string dest;
            float width;
            for (int i = 0; i < texts.Length; i++)
            {
                string sty = texts.Length == 1 ? "button" : (i == 0 ? "ButtonLeft" : (i == texts.Length - 1 ? "ButtonRight" : "ButtonMid"));
                bool click;
                if (WidthOption(texts[i], out dest, out width))
                    click = GUILayout.Button(dest, sty, GUILayout.Width(width));
                else
                    click = GUILayout.Button(dest, sty);
                if (click)
                {
                    ret = i;
                }
            }
            EditorGUILayout.EndHorizontal();
            return ret;
        }

        static bool WidthOption(string src, out string dest, out float width)
        {
            int n1 = src.IndexOf('<');
            int n2 = n1 >= 0 ? src.IndexOf('>', n1) : 0;
            dest = src;
            width = 0f;
            if (n1 == 0 && n2 > n1 + 1 && float.TryParse(src.Substring(n1 + 1, n2 - n1 - 1), out width))
            {
                dest = src.Substring(n2 + 1);
                return true;
            }
            else
                return false;
        }

        public static void StepVerticalScroll(ref int offset, ref Vector2 position, int drawLen, int maxLen, DrawCallback drawer,
            GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical();
            position = GUILayout.BeginScrollView(position, options);
            int len = 0;
            int logEnd = offset;
            Rect r0 = default(Rect);
            for (int i = offset; len < drawLen && i < maxLen; i++)
            {
                if (drawer(i, len))
                {
                    if (len == 0)
                        r0 = GUILayoutUtility.GetLastRect();
                    len++;
                }
                logEnd = i + 1;
            }
            Rect r1 = len > 0 ? GUILayoutUtility.GetLastRect() : r0;
            GUILayout.EndScrollView();
            Rect r2 = GUILayoutUtility.GetLastRect();
            if (r0.height > 1 && len > 0)
            {
                if ((position.y < r0.height || len < drawLen) && offset > 0)
                {
                    offset--;
                    position.y += r0.height;
                }
                else if (logEnd < maxLen && r1.yMin < r2.height + position.y && r1.yMax > position.y)
                {
                    offset++;
                    position.y -= r1.height;
                }
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Convenience function that displays a list of sprites and returns the selected value.
        /// </summary>

        static public string DrawList(string field, string[] list, string selection, params GUILayoutOption[] options)
        {
            if (list != null && list.Length > 0)
            {
                int index = 0;
                if (string.IsNullOrEmpty(selection)) selection = list[0];

                // We need to find the sprite in order to have it selected
                if (!string.IsNullOrEmpty(selection))
                {
                    for (int i = 0; i < list.Length; ++i)
                    {
                        if (selection.Equals(list[i], System.StringComparison.OrdinalIgnoreCase))
                        {
                            index = i;
                            break;
                        }
                    }
                }

                // Draw the sprite selection popup
                index = string.IsNullOrEmpty(field) ?
                    EditorGUILayout.Popup(index, list, options) :
                    EditorGUILayout.Popup(field, index, list, options);

                return list[index];
            }
            return null;
        }

        /// <summary>
        /// Draw a distinctly different looking header label
        /// </summary>

        static public bool DrawHeader(string text, string key, bool forceOn)
        {
            bool state = EditorPrefs.GetBool(key, true);

            GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(3f);

            GUI.changed = false;
#if UNITY_3_5
		if (state) text = "\u25B2 " + text;
		else text = "\u25BC " + text;
		if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
#else
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25B2 " + text;
            else text = "\u25BC " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
#endif
            if (GUI.changed) EditorPrefs.SetBool(key, state);

            GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }

        /// <summary>
        /// Begin drawing the content area.
        /// </summary>
        static public void BeginContents(float minHeight)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(minHeight));
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }

        /// <summary>
        /// End drawing the content area.
        /// </summary>
        static public void EndContents()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }
        
        static public void ReportView(ref Rect clipRect, Vector2 position, System.Action drawCallback, float height = 300, float grid = 50, string title = "Viewport" )
        {
            Rect rect = EditorGUILayout.BeginHorizontal("CurveEditorBackground", GUILayout.Height(height));
            if(rect.width > 1)
            {
                cachedViewportRect = rect;
            }

            clipRect = cachedViewportRect;
            clipRect.xMin += 1;
            clipRect.xMax -= 1;
            clipRect.yMin += 1;
            clipRect.yMax -= 1;

            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Label(title);
            EditorGUI.EndDisabledGroup();

            GUI.BeginClip(clipRect);
            if (grid > 0)
            {
                Handles.color = Color.gray * 0.5f;
                float h = Mathf.Repeat(-position.x, grid);
                while (h < rect.xMax)
                {
                    if (h > 0)
                        Handles.DrawLine(new Vector3(h, 0), new Vector3(h, clipRect.height));
                    h += grid;
                }
                h = Mathf.Repeat(-position.y, grid);
                while (h < rect.yMax)
                {
                    if (h > 0)
                        Handles.DrawLine(new Vector3(0, h), new Vector3(clipRect.width, h));
                    h += grid;
                }
            }

            if (drawCallback != null)
                drawCallback();
            GUI.EndClip();
            EditorGUILayout.EndHorizontal();
        }
    }
}