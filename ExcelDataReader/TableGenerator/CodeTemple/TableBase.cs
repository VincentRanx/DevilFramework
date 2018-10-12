#define BIN_SEARCH

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TableCore.Core
{
    public class TableBase : IIdentified
    {
        private int _id;
        public int Id { get { return _id; } }

        public virtual void Init(JObject jobj)
        {
#if UNITY_EDITOR
            if (jobj == null)
            {
                Debug.LogError(string.Format("{0}.Init(JObject) must has an instance of JObject parameter", GetType()));
                return;
            }
#endif
            JToken tok;
            if (jobj.TryGetValue("id", out tok))
            {
                _id = tok.ToObject<int>();
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError(string.Format("\"id\" is required for table base.\njson: {0}", jobj));
            }
#endif
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
#endif
    }

    // 数据集合
    public class TableSet<T> where T : TableBase, new()
    {
#if BIN_SEARCH
        private T[] m_Datas;
        public readonly bool sorted = true;
#else
        public readonly bool sorted = false;
        private Dictionary<int, T> m_Datas;
#endif
        //private bool m_IsLoading;

        private TableSet()
        {
#if BIN_SEARCH
            //m_Datas
#else
            m_Datas = new Dictionary<int, T>();
#endif
        }

#if BIN_SEARCH
        T Binsearch(int id)
        {
            if (m_Datas == null)
                return null;
            int n = Utils.BinsearchIndex(m_Datas, id, 0, m_Datas.Length);
            return n == -1 ? null : m_Datas[n];
        }
#endif

        public bool Exsits(int id)
        {
#if BIN_SEARCH
            return Binsearch(id) != null;
#else
            return m_Datas.ContainsKey(id);
#endif
        }

        public T this[int id]
        {
            get
            {
#if BIN_SEARCH
                return Binsearch(id);
#else
                T data;
                if (m_Datas.TryGetValue(id, out data))
                    return data;
                else
                    return null;
#endif
            }
        }

        public int Count
        {
            get
            {
#if BIN_SEARCH
                return m_Datas == null ? 0 : m_Datas.Length;
#else
                return m_Datas.Count;
#endif
            }
        }

        public T[] AsArray()
        {
#if BIN_SEARCH
            if (m_Datas == null)
                m_Datas = new T[0];
            return m_Datas;
#else
            T[] array = new T[Count];
            int p = 0;
            foreach(var t in Values)
            {
                array[p++] = t;
            }
            return array;
#endif
        }

        public IEnumerable<T> Values
        {
            get
            {
#if BIN_SEARCH
                if (m_Datas == null)
                    m_Datas = new T[0];
                return m_Datas;
#else
                return m_Datas.Values;
#endif
            }
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
#if BIN_SEARCH
                s_Instance.m_Datas = null;
#else
                s_Instance.m_Datas.Clear();
#endif
                s_Instance = null;
            }
        }

        static void Load(string text, TableSet<T> tab)
        {
            if (!string.IsNullOrEmpty(text))
            {
                JArray arr = JsonConvert.DeserializeObject<JArray>(text);
#if BIN_SEARCH
                tab.m_Datas = new T[arr.Count];
#endif
                for (int i = 0; i < arr.Count; i++)
                {
                    JObject obj = arr[i] as JObject;
                    T table = new T();
                    table.Init(obj);
#if BIN_SEARCH
                    tab.m_Datas[i] = table;
#else
                    tab.m_Datas[table.Id] = table;
#endif
                }
            }
        }

        public static TableSet<T> LoadAsNew(string text)
        {
            TableSet<T> inst = new TableSet<T>();
            Load(text, inst);
            return inst;
        }

        public static TableSet<T> Load(string text)
        {
            TableSet<T> inst = Instance;
            Load(text, inst);
            return inst;
        }

    }
}