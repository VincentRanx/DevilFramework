using Devil.ContentProvider;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class TablesForEditor
    {
        static Dictionary<System.Type, object> sTables = new Dictionary<System.Type, object>();
        
        [MenuItem("Assets/Utils/Reload Editor Tables")]
        static void Reload()
        {
            sTables.Clear();
        }

        public static TableSet<T> GetTable<T>(string file) where T : TableBase, new()
        {
            var tp = typeof(T);
            TableSet<T> t = null;
            object tab;
            if (sTables.TryGetValue(tp, out tab))
            {
                t = tab as TableSet<T>;
            }
            if (t == null)
            {
                t = new TableSet<T>();
                sTables[tp] = t;
            }
            if (!t.ContainsTable(file))
            {
                var f = string.Format("Assets/Tables/{0}.txt", file);
                var txt = AssetDatabase.LoadAssetAtPath<TextAsset>(f);
                if(txt != null)
                {
                    var merge = TableSet<T>.LoadAsNew(file, txt.text);
                    TableSet<T>.MergeTo(merge, t);
                }
            }
            return t;
        }

        public static T Get<T>(string file, int id) where T : TableBase, new()
        {
            var tb = GetTable<T>(file);
            return tb == null ? null : tb[id];
        }

        public static string GetText(int id, string tab = "TextExport_cn")
        {
            var t = GetTable<TextRes>(tab);
            var cfg = t[id];
            return cfg == null ? "" : cfg.text;
        }
    }
}