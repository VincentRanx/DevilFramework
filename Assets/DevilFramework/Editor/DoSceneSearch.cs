using DevilTeam.Utility;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilTeam.Editor
{

    #region sub page : search scene

    class TypeCompare : IComparer<System.Type>
    {
        public int Compare(System.Type a, System.Type b)
        {
            if (b == null)
                return -1;
            if (a == null)
                return 1;
            return a.Name.CompareTo(b.Name);
        }
    }

    public class SearchResult
    {
        public GameObject gameObj;
        public Dictionary<string, object> values;

        public Component[] cmps;

        public SearchResult(GameObject go)
        {
            gameObj = go;
            values = new Dictionary<string, object>();
        }

        public T GetValue<T>(string vName)
        {
            if (values != null && values.ContainsKey(vName))
            {
                object o = values[vName];
                return (o is T) ? (T)o : default(T);
            }
            else
                return default(T);
        }
        public void SetValue<T>(string vName, T value)
        {
            if (values != null)
                values[vName] = value;
        }

        public void CleanUp()
        {
            foreach (object o in values.Values)
            {
                if (o is UnityEditor.Editor)
                    Object.DestroyImmediate(o as UnityEditor.Editor);
            }
        }
    }

    public class GameObjectFilter : CheckWindow.SubPage
    {

        GameObject mSearchRoot;

        GameObject mDingObject;
        bool mDing;
        HashSet<int> popDingCmps = new HashSet<int>();
        Dictionary<int, UnityEditor.Editor> popDingEditors = new Dictionary<int, UnityEditor.Editor>();
        Vector2 dingScroll;

        int rootType = 1;
        public GameObject SearchRoot
        {
            get { return mSearchRoot; }
        }

        public string mObjName = ""; //gameObject name
        public StringFilter.FilterType mNameFilterType;//
        public bool mActive = true;//可见
        public bool mInactive = true;//不可见
        public bool mNigative;//反选

        public override void OnLostFocus()
        {
            EditorPrefs.SetString("f.mObjName", mObjName);
            EditorPrefs.SetInt("f.mNameFilterType", (int)mNameFilterType);
            EditorPrefs.SetInt("f.cmpIndex", cmpIndex);
            EditorPrefs.SetBool("f.mActive", mActive);
            EditorPrefs.SetBool("f.mInactive", mInactive);
            EditorPrefs.SetBool("f.mNigative", mNigative);
            EditorPrefs.SetInt("f.rootType", rootType);
            EditorPrefs.SetBool("f.ding", mDing);
        }

        public override void OnFocus()
        {
            mObjName = EditorPrefs.GetString("f.mObjName");
            if (mObjName == null)
                mObjName = "";
            mNameFilterType = (StringFilter.FilterType)EditorPrefs.GetInt("f.mNameFilterType");
            mActive = EditorPrefs.GetBool("f.mActive");
            mInactive = EditorPrefs.GetBool("f.mInactive");
            mNigative = EditorPrefs.GetBool("f.mNigative");
            rootType = EditorPrefs.GetInt("f.rootType");
            cmpIndex = EditorPrefs.GetInt("f.cmpIndex");
            mDing = EditorPrefs.GetBool("f.ding");
        }

        List<SearchResult> mResults = new List<SearchResult>();
        public List<SearchResult> Results { get { return mResults; } }

        public GameObjectFilter(CheckWindow window)
            : base("Search Scene", window)
        {
            RefreshAllComponentTypes();
        }

        int cmpIndex;
        public System.Type ComponentType { get { return (cmpIndex < mAllCmpTypes.Count + 1 && cmpIndex > 0) ? mAllCmpTypes[cmpIndex - 1] : null; } }
        List<System.Type> mAllCmpTypes = new List<System.Type>();

        public System.Type GetComponentTypeByName(string name)
        {
            for (int i = mAllCmpTypes.Count - 1; i >= 0; i--)
            {
                if (mAllCmpTypes[i].Name == name)
                    return mAllCmpTypes[i];
            }
            return null;
        }

        void RefreshAllComponentTypes()
        {
            mAllCmpTypes.Clear();
            Component[] cmps;
            if (!mSearchRoot)
                cmps = GameObject.FindObjectsOfType<Component>();
            else
                cmps = mSearchRoot.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < cmps.Length; i++)
            {
                if (!cmps[i])
                    continue;
                System.Type tp = cmps[i].GetType();
                if ((cmps[i] is Transform) || mAllCmpTypes.Contains(tp))
                    continue;
                mAllCmpTypes.Add(tp);
            }
            mAllCmpTypes.Sort(new TypeCompare());
            cmpIndex = Mathf.Min(0, mAllCmpTypes.Count);
        }

        bool FilterTarget(GameObject obj)
        {
            if (!obj)
                return false;
            System.Type tp = ComponentType;
            if (tp != null && !obj.GetComponent(tp))
            {
                return mNigative;
            }

            if (mNameFilterType != StringFilter.FilterType.none)
            {
                if (!StringFilter.TestStr(obj.name, mObjName, mNameFilterType))
                    return mNigative;
            }

            bool ret = false;
            if (mActive && obj.activeInHierarchy)
                ret = true;
            if (mInactive && !obj.activeInHierarchy)
                ret = true;

            return ret ^ mNigative;
        }

        Vector2 cmpPos;
        string showCmps = "";

        void DrawRootSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search Root", GUILayout.Width(100));
            rootType = EditorGUILayout.Popup(rootType, new string[] { "Selectoin", "Customer", "None" }, "DropDown");
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
            GameObject searchRoot;
            EditorGUI.BeginDisabledGroup(rootType != 1);
            EditorGUILayout.BeginHorizontal();
            if (rootType == 0)
                searchRoot = EditorGUILayout.ObjectField(Selection.activeGameObject, typeof(GameObject), true) as GameObject;
            else if (rootType == 1)
                searchRoot = EditorGUILayout.ObjectField(SearchRoot, typeof(GameObject), true) as GameObject;
            else
                searchRoot = EditorGUILayout.ObjectField(null, typeof(GameObject), true) as GameObject;
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            bool refreshType = mSearchRoot != searchRoot;
            mSearchRoot = searchRoot;
            if (refreshType)
                RefreshAllComponentTypes();
        }

        void DrawComponentField()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Component", GUILayout.Width(100));
            string[] pops = new string[mAllCmpTypes.Count + 1];
            pops[0] = "All";
            for (int i = 1; i < pops.Length; i++)
            {
                pops[i] = mAllCmpTypes[i - 1].Name;
            }
            cmpIndex = Mathf.Min(cmpIndex, pops.Length - 1);
            GUILayout.Label(pops[cmpIndex], "ShurikenModuleTitle");
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            showCmps = QuickGUI.SearchTextBar(showCmps);
            EditorGUILayout.EndHorizontal();
            cmpPos = GUILayout.BeginScrollView(cmpPos, "box");
            bool first = true;
            for (int i = 0; i < pops.Length; i++)
            {
                bool old = cmpIndex == i;
                if (!old && !pops[i].ToLower().Contains(showCmps.ToLower()))
                    continue;
                if (!first)
                {
                    GUILayout.Space(3);
                    QuickGUI.HLine(Color.gray);
                }
                bool last = GUILayout.Toggle(old, pops[i], "PlayerSettingsLevel");
                if (last)
                {
                    cmpIndex = i;
                }
                if (last ^ old)
                    showCmps = "";
                first = false;
            }
            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        void DrawMatchName()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Match Name", GUILayout.Width(100));
            mNameFilterType = (StringFilter.FilterType)EditorGUILayout.EnumPopup(mNameFilterType, "DropDown");
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(mNameFilterType == StringFilter.FilterType.none);
            mObjName = EditorGUILayout.TextField(mObjName);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        void DrawActiveOption()
        {
            //GUILayout.BeginHorizontal();
            int act = 0;
            if (mActive)
                act |= 0x1;
            if (mInactive)
                act |= 0x2;
            if (mNigative)
                act |= 0x4;
            act = QuickGUI.MultiOptionBar(act, new string[] { "Active", "Inactive", "Revert" });
            mActive = (act & 0x1) != 0;
            mInactive = (act & 0x2) != 0;
            mNigative = (act & 0x4) != 0;
        }

        public override void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawSearch();
            DrawResultsList();
            DrawDingObject();
            EditorGUILayout.EndHorizontal();
        }

        void DrawSearch()
        {
            EditorGUILayout.BeginVertical("flow overlay box", GUILayout.Width(250));

            //QuickGUI.HLine(Color.gray);
            DrawRootSelector();
            GUILayout.Space(10);

            DrawComponentField();
            GUILayout.Space(10);

            DrawMatchName();
            GUILayout.Space(10);

            DrawActiveOption();

            EditorGUILayout.BeginHorizontal();
            bool search = GUILayout.Button("Search", GUILayout.Height(40));
            EditorGUILayout.EndHorizontal();
            if (search)
            {
                CleanUp();

                Transform[] trans;
                if (mSearchRoot)
                    trans = mSearchRoot.GetComponentsInChildren<Transform>(true);
                else
                    trans = GameObject.FindObjectsOfType<Transform>();
                for (int i = 0; i < trans.Length; i++)
                {
                    if (FilterTarget(trans[i].gameObject))
                        mResults.Add(new SearchResult(trans[i].gameObject));
                }
                UpdateShowResult(sampleSearch, true);
                mDing = true;
                if (mSearchRoot)
                    mDingObject = mSearchRoot;
            }
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        void DrawResultItem(SearchResult sr, bool change)
        {
            Component[] cmps = sr.cmps;
            System.Type tp = ComponentType;
            if (cmps == null)
            {
                cmps = sr.gameObj.GetComponents<Component>();
            }
            if (cmps.Length == 1)
            {
                return;
            }
            EditorGUILayout.BeginVertical("flow overlay box");
            for (int i = 0; i < cmps.Length; i++)
            {
                if (cmps[i] is Transform)
                    continue;
                System.Type ctp = cmps[i].GetType();
                if (tp != null && ctp != tp && !ctp.IsSubclassOf(tp))
                    continue;
                string id = cmps[i].GetInstanceID().ToString();
                bool v = sr.GetValue<bool>(id);
                bool v2 = EditorGUILayout.InspectorTitlebar((v && !change) || (change && tp != null), cmps[i]);
                sr.SetValue<bool>(id, v2);
                if (v2)
                {
                    UnityEditor.Editor edi = sr.GetValue<UnityEditor.Editor>(cmps[i].GetType().Name);
                    if (!edi)
                    {
                        edi = UnityEditor.Editor.CreateEditor(cmps[i]);
                        sr.SetValue<UnityEditor.Editor>(cmps[i].GetType().Name, edi);
                    }
                    edi.OnInspectorGUI();
                }
            }
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
        }

        void DrawDingObject()
        {
            if (!mDingObject || !mDing)
                mDingObject = Selection.activeGameObject;
            if (!mDingObject)
            {
                EditorGUILayout.BeginVertical("flow overlay box", GUILayout.Width(350));
                GUILayout.Label("<size=25><b>NOTHINE SELECTED.</b></size>");
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.BeginVertical("flow overlay box", GUILayout.Width(350));
            string t = mDingObject.name;
            int s = QuickGUI.TitleBar(t, 15,
                new string[] { mDing ? "<25>true:\u2764" : "<25>\u2764", mDingObject.activeSelf ? "<25>true:\u2714" : "<25>\u2714" });

            if (s == 1)
            {
                mDing = !mDing;
                if (!mDing && Selection.activeGameObject)
                    mDingObject = Selection.activeGameObject;
            }
            else if (s == 2)
            {
                mDingObject.gameObject.SetActive(!mDingObject.activeSelf);
            }
            if (s >= 0)
                EditorGUIUtility.PingObject(mDingObject);
            dingScroll = EditorGUILayout.BeginScrollView(dingScroll);
            Component[] cps = mDingObject.GetComponents<Component>();

            for (int i = 0; i < cps.Length; i++)
            {
                if (!cps[i] || cps[i] is Transform)
                    continue;
                //System.Type ctp = cps[i].GetType();
                int id = cps[i].GetInstanceID();
                bool v = popDingCmps.Contains(id);
                bool v2 = EditorGUILayout.InspectorTitlebar(v, cps[i]);
                if (v2 ^ v)
                {
                    if (!v2)
                    {
                        popDingCmps.Remove(id);
                        popDingEditors.Remove(id);
                    }
                    else
                    {
                        popDingCmps.Add(id);
                    }
                }
                if (v2)
                {
                    UnityEditor.Editor edi;// sr.GetValue<Editor>(cmps[i].GetType().Name);
                    if (!popDingEditors.TryGetValue(id, out edi) || !edi)
                    {
                        edi = UnityEditor.Editor.CreateEditor(cps[i]);
                        popDingEditors[id] = edi;
                    }
                    if (edi)
                        edi.OnInspectorGUI();
                }
            }
            GUILayout.Space(10);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        Vector2 scrollPos;
        string sampleSearch = "";
        List<SearchResult> showResults = new List<SearchResult>();

        //PageOption fixPage;
        float pPercent;

        void FixPagePercent()
        {

        }

        bool UpdateShowResult(string filter, bool forceUpdate = false)
        {
            if (!forceUpdate && filter == sampleSearch)
                return false;
            sampleSearch = filter;
            showResults.Clear();
            for (int i = Results.Count - 1; i >= 0; i--)
            {
                SearchResult sr = Results[i];
                if (!sr.gameObj)
                {
                    Results.RemoveAt(i);
                    continue;
                }
                bool pass = QuickFilter(sampleSearch, sr);
                if (pass)
                    showResults.Insert(0, sr);
            }
            off = 0;
            return true;
        }

        int off;
        int foutIndex = -1;

        bool DrawResultItem(int index, int counter)
        {
            if (index >= showResults.Count)
                return false;
            SearchResult sr = showResults[index];
            if (!sr.gameObj)
            {
                return false;
            }
            if (counter > 0)
            {
                GUILayout.Space(5);
                QuickGUI.HLine(Color.gray);
            }
            GameObject go = sr.gameObj;

            bool oldFout = foutIndex == index;// EditorPrefs.GetBool("go" + index);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            string goName = go.name;
            GUILayout.BeginHorizontal();
            bool act = GUILayout.Toggle(go.activeSelf, "", GUILayout.Width(15));
            if (act ^ go.activeSelf)
                go.SetActive(act);
            bool fout = GUILayout.Toggle(oldFout, goName, "PlayerSettingsLevel");
            bool ding = GUILayout.Toggle(go == mDingObject, "\u2764", "SearchModeFilter", GUILayout.MaxWidth(20));
            if (ding)
            {
                mDingObject = go;
                mDing = true;
            }
            GUILayout.EndHorizontal();
            if (fout ^ oldFout)
            {
                //EditorGUIUtility.PingObject(go);
                if (fout)
                    PopupResult(index);
            }
            if (fout)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(3);
                DrawResultItem(sr, !oldFout);
                GUILayout.Space(3);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            return true;
        }

        void DrawResultsList()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("QuickSearch");
            string s = QuickGUI.SearchTextBar(sampleSearch);
            UpdateShowResult(s);

            GUILayout.Label(string.Format("{0}/{1}", off, showResults.Count), "ProgressBarBack");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            QuickGUI.StepVerticalScroll(ref off, ref scrollPos, 30, showResults.Count, DrawResultItem, null);

            GUILayout.BeginHorizontal();

            pPercent = showResults.Count > 30 ? (float)off / (float)(showResults.Count - 30) : 1f;
            float p2 = GUILayout.HorizontalSlider(pPercent, 0f, 1f);
            if(p2 != pPercent && showResults.Count > 30)
            {
                off = Mathf.FloorToInt((showResults.Count - 30) * p2);
            }
            pPercent = p2;

            GUILayout.Label(string.Format("{0}/{1}", Mathf.Min(off + 30, showResults.Count), showResults.Count), "ProgressBarBack");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        bool IsTypeNameContains(System.Type type, string str, bool considerBaseType)
        {
            if (type.Name.ToLower().Contains(str))
                return true;
            if (considerBaseType)
            {
                System.Type baseType = type.BaseType;
                while (baseType != null && baseType.IsSubclassOf(typeof(Component)))
                {
                    if (baseType.Name.ToLower().Contains(str))
                        return true;
                    baseType = baseType.BaseType;
                }
            }
            return false;
        }

        bool QuickFilter(string str, SearchResult sr)
        {
            sr.cmps = null;
            if (string.IsNullOrEmpty(str))
                return true;
            string s = str.Trim().ToLower();
            if (s.StartsWith("?c "))//过滤components
            {
                string cname = s.Substring(3);
                sr.cmps = sr.gameObj.GetComponents<Component>();
                bool ok = false;
                for (int i = 0; i < sr.cmps.Length; i++)
                {
                    bool match = IsTypeNameContains(sr.cmps[i].GetType(), cname, false);// cmps[i].GetType().Name.ToLower().Contains(cname);
                    ok |= match;
                    sr.SetValue<bool>(sr.cmps[i].GetInstanceID().ToString(), match);
                }
                return ok;
            }
            else if (s.StartsWith("?sc "))
            {
                string cname = s.Substring(4);
                sr.cmps = sr.gameObj.GetComponents<Component>();
                bool ok = false;
                for (int i = 0; i < sr.cmps.Length; i++)
                {
                    bool match = IsTypeNameContains(sr.cmps[i].GetType(), cname, true);
                    ok |= match;
                    sr.SetValue<bool>(sr.cmps[i].GetInstanceID().ToString(), match);
                }
                return ok;
            }
            else if (s.StartsWith("?"))
            {
                return true;
            }
            if (sr.gameObj.name.ToLower().Contains(s))
                return true;
            return false;
        }

        public override void OnDisable()
        {
            CleanUp();
        }

        void CleanUp()
        {
            PopupResult(-1);
            mResults.Clear();
        }


        public void PopupResult(int index)
        {
            //for (int i = 0; i < mResults.Count; i++)
            //{
            //    EditorPrefs.SetBool("go" + i.ToString(), index == i);
            //    if (index != i)
            //        mResults[i].CleanUp();
            //}
            if (foutIndex >= 0 && foutIndex < mResults.Count)
            {
                mResults[foutIndex].CleanUp();
            }
            foutIndex = index;
        }
    }

    #endregion

}
