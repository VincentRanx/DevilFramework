using Devil.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.ContentProvider
{
    public class TableBase : IIdentified
    {
        private int _id;
        public int Identify { get { return _id; } }

#if UNITY_EDITOR
        string mFormatString;
#endif

        public TableBase()
        {
#if UNITY_EDITOR
            mFormatString = string.Format("<{0}>", GetType().Name);
#endif
        }

        public virtual void Init(JObject jobj)
        {
#if UNITY_EDITOR
            if (jobj == null)
            {
                RTLog.LogError(LogCat.Table, string.Format("{0}.Init(JObject) must has an instance of JObject parameter", GetType()));
                return;
            }
            mFormatString = string.Format("<{0}>{1}", GetType().Name, JsonConvert.SerializeObject(jobj));
#endif
            JToken tok;
            if (jobj.TryGetValue("id", out tok))
            {
                _id = tok.ToObject<int>();
            }
#if UNITY_EDITOR
            else
            {
                RTLog.LogError(LogCat.Table, string.Format("\"id\" is required for table base.\njson: {0}", jobj));
            }
#endif
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            return mFormatString;
        }
#endif
    }
    
    // 数据集合
    public class TableSet<T> where T : TableBase, new()
    {
        private T[] m_Datas;

        public string TableName { get; private set; }
        HashSet<int> mMerged = new HashSet<int>();
        public bool Contains(int tableid)
        {
            return mMerged.Contains(tableid);
        }
        public bool Contains(string tablename)
        {
            return mMerged.Contains(HashTable(tablename));
        }

        private TableSet()
        {
        }
        
        public bool Exsits(int id)
        {
            return m_Datas != null && m_Datas.BinsearchIndex(id) != -1;
        }

        public T GetData(int id)
        {
            return m_Datas == null ? null : m_Datas.Binsearch(id);
        }

        public T this[int id] { get { return GetData(id); } }

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

        public void Merge(TableSet<T> tables)
        {
            if (tables == null || tables.Count == 0)
                return;
            foreach (var tab in tables.mMerged)
            {
                mMerged.Add(tab);
            }
            List<T> list = new List<T>(Count + tables.Count);
            int a = 0;
            int b = 0;
            while (a < Count || b < tables.Count)
            {
                var ta = a < Count ? m_Datas[a] : null;
                var tb = b < tables.Count ? tables.m_Datas[b] : null;
                if (ta == null && tb == null)
                    continue;
                if (ta != null && tb != null)
                {
                    if(tb.Identify < ta.Identify)
                    {
                        list.Add(tb);
                        b++;
                    }
                    else if(ta.Identify < tb.Identify)
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
                else if(ta == null)
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
            m_Datas = list.ToArray();
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
                s_Instance.m_Datas = null;
                s_Instance = null;
            }
        }

        static void Load(string tablename, string text, TableSet<T> tab)
        {
            if (!string.IsNullOrEmpty(text))
            {
                tab.TableName = tablename;
                tab.mMerged.Clear();
                tab.mMerged.Add(HashTable(tablename));
                JArray arr = JsonConvert.DeserializeObject<JArray>(text);
                tab.m_Datas = new T[arr.Count];
                for (int i = 0; i < arr.Count; i++)
                {
                    JObject obj = arr[i] as JObject;
                    T table = new T();
                    table.Init(obj);
                    tab.m_Datas[i] = table;
                }
            }
        }
        
        public static TableSet<T> LoadAsNew(string tablename, string text)
        {
            TableSet<T> inst = new TableSet<T>();
            Load(tablename, text, inst);
            return inst;
        }

        public static TableSet<T> Load(string tablename, string text)
        {
            TableSet<T> inst = Instance;
            Load(tablename, text, inst);
            return inst;
        }
        
        public static TableSet<T> Merge(string tablename, string text)
        {
            TableSet<T> inst = LoadAsNew(tablename, text);
            if (s_Instance == null)
                s_Instance = inst;
            else
                s_Instance.Merge(inst);
            return s_Instance;
        }

        public static int HashTable(string tablename)
        {
            string hash = StringUtil.Concat(typeof(T).FullName, tablename);
            return StringUtil.IgnoreCaseToHash(hash);
        }

    }
}