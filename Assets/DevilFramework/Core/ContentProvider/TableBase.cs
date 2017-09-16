using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DevilTeam.ContentProvider
{
    public class TableBase
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
        private Dictionary<int, T> m_Datas;
        private bool m_IsLoading;

        private TableSet()
        {
            m_Datas = new Dictionary<int, T>();
        }

        public bool Exsits(int id)
        {
            return m_Datas.ContainsKey(id);
        }

        public T this[int id]
        {
            get
            {
                T data;
                if (m_Datas.TryGetValue(id, out data))
                    return data;
                else
                    return null;
            }
        }

        public int Count { get { return m_Datas.Count; } }
        public IEnumerable<T> Values { get { return m_Datas.Values; } }
        public IEnumerable<int> Keys { get { return m_Datas.Keys; } }

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

        public static void Release()
        {
            if (s_Instance != null)
            {
                s_Instance.m_Datas.Clear();
                s_Instance = null;
            }
        }

        // 加载数据
        public static TableSet<T> Load(Stream inputReader)
        {
            TableSet<T> inst = Instance;
            inst.m_IsLoading = true;
            if (inputReader != null)
            {
                StreamReader reader = new StreamReader(inputReader);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;
                    JObject obj = JsonConvert.DeserializeObject<JObject>(line);
                    T table = new T();
                    table.Init(obj);
                    inst.m_Datas[table.Id] = table;
                }
                reader.Close();
                reader.Dispose();
            }
            inst.m_IsLoading = false;
            return inst;
        }

        public static TableSet<T> Load(string text)
        {
            TableSet<T> inst = Instance;
            inst.m_IsLoading = true;
            if (!string.IsNullOrEmpty(text))
            {
                string[] txt = text.Split('\n');
                for(int i = 0; i < txt.Length; i++)
                {
                    string s = txt[i].Trim();
                    if (string.IsNullOrEmpty(s))
                        continue;
                    JObject obj = JsonConvert.DeserializeObject<JObject>(s);
                    T table = new T();
                    table.Init(obj);
                    inst.m_Datas[table.Id] = table;
                }
            }
            inst.m_IsLoading = false;
            return inst;
        }
    }
}