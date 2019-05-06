using Devil.Utility;
using LitJson;
using System.Collections.Generic;

namespace Devil.ContentProvider
{
    public delegate void TableLoadedCallback<T>(TableSet<T> table, bool isMerge) where T : TableBase, new();

    public interface ITable : System.Collections.IEnumerable
    {
        int Count { get; }
        object GetValue(int id);
    }

    // 数据集合
    public class TableSet<T> : ITable where T : TableBase, new()
    {

        private T[] m_Datas;
        public bool IsLoading { get; private set; }
        public string TableName { get; private set; }
        HashSet<int> mMerged = new HashSet<int>();

        public event TableLoadedCallback<T> OnLoadComplete;
        TableLoadedCallback<T> mLoadCallback;
        public void OnComplete(TableLoadedCallback<T> callback)
        {
            mLoadCallback += callback;
        }

        public TableSet() { }
        
        public bool ContainsTable(int tableid)
        {
            return mMerged.Contains(tableid);
        }

        public void WaitForLoading()
        {
            IsLoading = true;
        }

        public bool ContainsTable(string tablename)
        {
            return mMerged.Contains(HashTable(tablename));
        }

        public bool Exist(int id)
        {
            return m_Datas != null &&  GlobalUtil.BinsearchIndex(m_Datas, id) != -1;
        }

        public T GetData(int id)
        {
            return m_Datas == null ? null : GlobalUtil.Binsearch(m_Datas, id);
        }

        public object GetValue(int id)
        {
            return GetData(id);
        }

        public T this[int id] { get { return GetData(id); } }

        public System.Collections.IEnumerator GetEnumerator()
        {
            if (m_Datas == null)
                m_Datas = new T[0];
            return m_Datas.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return m_Datas == null ? 0 : m_Datas.Length;
            }
        }

        public T[] AsArray()
        {
            if (m_Datas == null)
                m_Datas = new T[0];
            return m_Datas;
        }

        public IEnumerable<T> Values
        {
            get
            {
                if (m_Datas == null)
                    m_Datas = new T[0];
                return m_Datas;
            }
        }
        
        public void Clear()
        {
            if (m_Datas != null && m_Datas.Length > 0)
                m_Datas = null;
        }

        void FireCompleteEvent(bool merge)
        {
            if(mLoadCallback != null)
            {
                mLoadCallback(this, merge);
                mLoadCallback = null;
            }
            if (OnLoadComplete != null)
                OnLoadComplete(this, merge);
        }

        private static TableSet<T> s_Instance;
        public static TableSet<T> Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new TableSet<T>();
                }
                return s_Instance;
            }
        }

        public static T Value(int id)
        {
            return s_Instance == null ? null : s_Instance[id];
        }

        public static void Release()
        {
            if (s_Instance != null)
            {
                s_Instance.Clear();
                s_Instance = null;
            }
        }

        // notify load complete.
        public static void LoadComplete(TableSet<T> table, bool merge)
        {
            if(table != null && table.IsLoading)
            {
                table.IsLoading = false;
                MainThread.RunOnMainThread(table.FireCompleteEvent, merge);
            }
        }

        public static void MergeTo(TableSet<T> tables, TableSet<T> mergeTo)
        {
            if (tables == null || tables.Count == 0 || mergeTo == null)
                return;
            mergeTo.IsLoading = true;
            foreach (var tab in tables.mMerged)
            {
                mergeTo.mMerged.Add(tab);
            }
            List<T> list = new List<T>(mergeTo.Count + tables.Count);
            int a = 0;
            int b = 0;
            while (a < mergeTo.Count || b < tables.Count)
            {
                var ta = a < mergeTo.Count ? mergeTo.m_Datas[a] : null;
                var tb = b < tables.Count ? tables.m_Datas[b] : null;
                if (ta == null && tb == null)
                    continue;
                if (ta != null && tb != null)
                {
                    if (tb.Identify < ta.Identify)
                    {
                        list.Add(tb);
                        b++;
                    }
                    else if (ta.Identify < tb.Identify)
                    {
                        list.Add(ta);
                        a++;
                    }
                    else
                    {
                        list.Add(tb);
                        b++;
                        a++;
                    }
                }
                else if (ta == null)
                {
                    list.Add(tb);
                    b++;
                }
                else
                {
                    list.Add(ta);
                    a++;
                }
            }
            mergeTo.m_Datas = list.ToArray();
            mergeTo.IsLoading = false;
            MainThread.RunOnMainThread(mergeTo.FireCompleteEvent, true);
        }

        public static void LoadTo(string tablename, string text, TableSet<T> tab)
        {
            if (!string.IsNullOrEmpty(text) && tab != null)
            {
                tab.IsLoading = true;
                tab.TableName = tablename;
                tab.mMerged.Clear();
                tab.mMerged.Add(HashTable(tablename));
                JsonData arr = JsonMapper.ToObject(text);
                //JArray arr = JsonConvert.DeserializeObject<JArray>(text);
                if (tab.m_Datas == null || tab.m_Datas.Length != arr.Count)
                    tab.m_Datas = new T[arr.Count];
                for (int i = 0; i < arr.Count; i++)
                {
                    T table = new T();
                    table.Init(arr[i]);
                    tab.m_Datas[i] = table;
                }
                tab.IsLoading = false;
                MainThread.RunOnMainThread(tab.FireCompleteEvent, false);
            }
        }
        
        public static TableSet<T> LoadAsNew(string tablename, string text)
        {
            TableSet<T> inst = new TableSet<T>();
            LoadTo(tablename, text, inst);
            return inst;
        }

        public static TableSet<T> Load(string tablename, string text)
        {
            TableSet<T> inst = Instance;
            LoadTo(tablename, text, inst);
            return inst;
        }
        
        public static TableSet<T> Merge(string tablename, string text)
        {
            TableSet<T> inst = LoadAsNew(tablename, text);
            if (s_Instance == null)
                s_Instance = inst;
            else
                MergeTo(inst, s_Instance);
            return s_Instance;
        }

        public static int HashTable(string tablename)
        {
            string hash = StringUtil.Concat(typeof(T).FullName, tablename);
            return StringUtil.IgnoreCaseToHash(hash);
        }

    }
}