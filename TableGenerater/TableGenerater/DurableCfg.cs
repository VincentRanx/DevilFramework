using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace TableGenerater
{
    public class DurableCfg
    {
        public List<string> excelFiles = new List<string>();
        public List<string> csharpOutput = new List<string>();
        public List<string> jsonOutput = new List<string>();

        static DurableCfg _cfg;

        public static DurableCfg Cfg
        {
            get
            {
                if (_cfg == null)
                    _cfg = new DurableCfg();
                return _cfg;
            }
        }

        public static void Init()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(path, "TableGenerater/cfg.json");
            if (File.Exists(path))
            {
                string cfg = File.ReadAllText(path);
                _cfg = JsonConvert.DeserializeObject<DurableCfg>(cfg);
            }
        }

        public static void Save()
        {
            if(_cfg != null)
            {
                string s = JsonConvert.SerializeObject(_cfg);
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = Path.Combine(path, "TableGenerater");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, "cfg.json");
                File.WriteAllText(path, s);
            }
        }
    }
}
