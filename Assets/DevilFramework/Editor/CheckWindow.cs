using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using DevilTeam.Utility;

namespace DevilTeam.Editor
{

    public class CheckWindow : EditorWindow
    {

        public abstract class SubPage
        {
            string mName;
            CheckWindow mWindow;
            public SubPage(string name, CheckWindow window)
            {
                mName = name;
                mWindow = window;
            }
            public string Name { get { return mName; } }
            public CheckWindow SuperWindow { get { return mWindow; } }

            public virtual void OnGUI() { }
            public virtual void OnStatusGUI() { }

            public virtual void OnFocus() { }
            public virtual void OnLostFocus() { }

            public virtual void OnEnable() { }
            public virtual void OnDisable() { }

        }

        [MenuItem("Devil Framework/Check Toolkit")]
        static void OpenThisWindow()
        {
            CheckWindow cw = EditorWindow.GetWindow<CheckWindow>();
            cw.minSize = new Vector2(960, 480);
            cw.Show();
        }

        SubPage[] mPages;
        string[] mTitles;

        Dictionary<string, object> mStatusData = new Dictionary<string, object>();

        public object GetStatusData(string key)
        {
            if (mStatusData.ContainsKey(key))
                return mStatusData[key];
            else
                return null;
        }

        public void SetStatusData(string key, object data)
        {
            mStatusData[key] = data;
        }

        void OnEnable()
        {

            if (mPages == null)
            {
                mPages = new SubPage[] {
                new GameObjectFilter(this),
                new GUIStyleViewport(this),
            };
                mTitles = new string[mPages.Length];
            }
            for (int i = 0; i < mPages.Length; i++)
            {
                mTitles[i] = mPages[i].Name;
                mPages[i].OnEnable();
            }
        }

        void OnFocus()
        {
            if (mPages != null)
            {
                for (int i = 0; i < mPages.Length; i++)
                {
                    mPages[i].OnFocus();
                }
            }
        }

        void OnLostFocus()
        {
            if (mPages != null)
            {
                for (int i = 0; i < mPages.Length; i++)
                {
                    mPages[i].OnLostFocus();
                }
            }
        }

        void OnDisable()
        {
            if (mPages != null)
            {
                for (int i = 0; i < mPages.Length; i++)
                {
                    mPages[i].OnDisable();
                }
            }
        }

        int tab;

        void OnGUI()
        {
            //right
            GUILayout.BeginVertical();
            GUILayout.Space(4);
            int colLimit = (int)Mathf.Min(mTitles.Length, position.width / 200f);
            tab = QuickGUI.HTabBar(tab, 20, colLimit, mTitles);
            GUILayout.Space(3);
            GUILayout.BeginHorizontal("As TextArea");
            if (tab >= 0 && tab < mPages.Length)
            {
                mPages[tab].OnGUI();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("STATUS", GUILayout.Width(50));
            mPages[tab].OnStatusGUI();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            
        }

        public static void DrawRectangle(float width, float height, float space, Color c)
        {
            Handles.color = c;
            Vector3 v1 = new Vector3(space, space);
            Vector3 v2 = new Vector3(width - space, space);
            Handles.DrawLine(v1, v2);
            v1.y = height - space;
            v2.y = v1.y;
            Handles.DrawLine(v1, v2);
            v1 = new Vector3(space, space);
            v2 = new Vector3(space, height - space);
            Handles.DrawLine(v1, v2);
            v1.x = width - space;
            v2.x = v1.x;
            Handles.DrawLine(v1, v2);
        }

    }
}