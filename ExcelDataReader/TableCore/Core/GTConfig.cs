using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TableCore.Plugin;

namespace TableCore
{
    public enum EGTLang
    {
        csharp,
        java,
        php,
    }

    public class GTClassCfg
    {
        public EGTLang Lang { get; private set; }
        public string NamespaceValue { get; set; }
        public List<string> UsingNamespace { get; private set; }
        Dictionary<string, GTType> mGTTypes = new Dictionary<string, GTType>();
        Dictionary<string, string> mPatterns = new Dictionary<string, string>();
        Dictionary<string, IGenFormater> mFormaters = new Dictionary<string, IGenFormater>();
        public string OutputFolder { get; set; }

        public Dictionary<string, GTType> GTTypes { get { return mGTTypes; } }

        Dictionary<string, string> GetPatterns(XmlElement root)
        {
            if (root == null)
                return mPatterns;
            foreach (var node in root.ChildNodes)
            {
                XmlElement ele = node as XmlElement;
                if (ele != null)
                {
                    mPatterns[ele.Name] = ele.InnerText;
                }
            }
            return mPatterns;
        }

        Dictionary<string, IGenFormater> GetFormatters(XmlElement root)
        {
            if (root == null)
                return mFormaters;
            IGenFormater formater;
            foreach (var node in root.ChildNodes)
            {
                XmlElement f = node as XmlElement;
                if (f == null)
                    continue;
                var c = f.GetAttribute("class");
                formater = Utils.NewInstance(c) as IGenFormater;
                if (formater != null)
                {
                    formater.Init(f);
                    mFormaters[f.Name] = formater;
                    GTType tp;
                    if (mGTTypes.TryGetValue(f.Name, out tp))
                        tp.Formater = formater;
                }
            }
            return mFormaters;
        }   

        public GTClassCfg(XmlElement element)
        {
            Lang = (EGTLang)Enum.Parse(typeof(EGTLang), element.GetAttribute("lang"), true);
            OutputFolder = string.Format("../CODE_{0}", Lang);
            UsingNamespace = new List<string>();
            Merge(element);
        }

        public void Merge(XmlElement element)
        {
            XmlElement space = GTConfig.GetChildElement(element, "namespace");
            if (space != null)
            {
                var sname = space.GetAttribute("name");
                if(sname != NamespaceValue)
                {
                    NamespaceValue = sname;
                    UsingNamespace.Clear();
                }
                XmlNodeList imports = space.ChildNodes;
                if(imports != null)
                {
                    for (int i = 0; i < imports.Count; i++)
                    {
                        XmlElement import = imports[i] as XmlElement;
                        if (import != null && import.Name == "using")
                            UsingNamespace.Add(import.InnerText);
                    }
                }
            }

            XmlElement xfors = GTConfig.GetChildElement(element, "formaters");
            Dictionary<string, IGenFormater> formaters = GetFormatters(xfors);
            XmlElement patt = GTConfig.GetChildElement(element, "patterns");
            Dictionary<string, string> patterns = GetPatterns(patt);

            XmlElement types = GTConfig.GetChildElement(element, "types");
            if (types != null)
            {
                foreach (var node in types.ChildNodes)
                {
                    XmlElement tp = node as XmlElement;
                    if (tp != null)
                    {
                        GTType type = new GTType(Lang);
                        type.Init(tp, patterns, formaters);
                        mGTTypes[type.Name] = type;
                    }
                }
            }
        }

        public GTType GetGTType(string name)
        {
            GTType tp;
            if (mGTTypes.TryGetValue(name, out tp))
                return tp;
            else
                return null;
        }

    }

    public class GTOutputCfg
    {
        public string Category { get; private set; }
        public string DataExtension { get; set; }
        public string DataFolder { get; set; }
        public EDataMode DataMode { get; set; }
        public List<IDataModify> DataMofify { get; private set; }
        HashSet<string> mDataModifyNames = new HashSet<string>();
        public string IgnorePattern { get; set; }

        public GTOutputCfg(XmlElement element)
        {
            Category = element.GetAttribute("category");
            DataExtension = "txt";
            DataFolder = string.Format("../DATA_{0}", Category);
            DataMofify = new List<IDataModify>();
            Merge(element);
        }

        public void Merge(XmlElement element)
        {
            XmlElement ele = GTConfig.GetChildElement(element, "data-ext");
            if (ele != null)
                DataExtension = ele.InnerText;
            ele = GTConfig.GetChildElement(element, "data-mode");
            if (ele != null)
                DataMode = (EDataMode)Enum.Parse(typeof(EDataMode), ele.InnerText);
            ele = GTConfig.GetChildElement(element, "data-ignore");
            if (ele != null)
                IgnorePattern = ele.InnerText;
            var list = element.GetElementsByTagName("data-modify");
            if (list != null)
            {
                foreach (var mod in list)
                {
                    ele = mod as XmlElement;
                    if (ele == null)
                        continue;
                    var cname = ele.GetAttribute("class");
                    if (!mDataModifyNames.Add(cname))
                        continue;
                    IDataModify formater = Utils.NewInstance(cname) as IDataModify;
                    if (formater != null)
                    {
                        formater.Init(ele);
                        DataMofify.Add(formater);
                    }
                }
            }
        }

        public IDataModify PrepareTableModify(GTStatus status, string category, string tablename)
        {
            foreach(var o in DataMofify)
            {
                if (o.PrepareTable(status, category, tablename))
                    return o;
            }
            return null;
        }
    }

    public class GTConfig
    {
        static GTConfig mDefaultCfg;
        public static GTConfig DefaultCfg
        {
            get
            {
                if (mDefaultCfg == null)
                    mDefaultCfg = NewDefaultCfg();
                return mDefaultCfg;
            }
        }

        public static GTConfig NewDefaultCfg()
        {
            return new GTConfig(Utils.GetRelativePath("Config/setup-default.xml"));
        }

        public static XmlElement GetChildElement(XmlElement root, string elementName)
        {
            if (root == null)
                return null;
            XmlNodeList lst = root.GetElementsByTagName(elementName);
            for(int i = 0; i < lst.Count; i++)
            {
                XmlElement ele = lst[i] as XmlElement;
                if (ele != null)
                    return ele;
            }
            return null;
        }

        Dictionary<EGTLang, GTClassCfg> mGTClasses = new Dictionary<EGTLang, GTClassCfg>();
        public Dictionary<EGTLang, GTClassCfg> GTClasses { get { return mGTClasses; } }
        Dictionary<string, GTOutputCfg> mGTOutputs = new Dictionary<string, GTOutputCfg>();
        public Dictionary<string, GTOutputCfg> GTOutputs { get { return mGTOutputs; } }
        GTClassCfg mClassCfg;
        public EGTLang ActiveLang
        {
            get
            {
                return mClassCfg.Lang;
            }
            set
            {
                mClassCfg = GetClass(value);
            }
        }
        public GTClassCfg ActiveClass { get { return mClassCfg; } }
        GTOutputCfg mOutputCfg;
        public string ActiveCategory
        {
            get { return mOutputCfg.Category; }
            set
            {
                var cat = GetOutput(value);
                mOutputCfg = cat ?? throw new Exception(string.Format("Category({0}) is not defined.", value));
            }
        }
        public GTOutputCfg ActiveData { get { return mOutputCfg; } }
        public string FileName { get; private set; }
        HashSet<string> mFiles = new HashSet<string>();
        
        public GTConfig(string cfgFile)
        {
            LoadCfg(cfgFile, false);
        }

        public void MergeCfg(string cfgFile)
        {
            LoadCfg(cfgFile, true);
        }

        void LoadCfg(string cfgFile, bool merge)
        {
            if (!merge)
            {
                mGTClasses.Clear();
                mGTOutputs.Clear();
                mFiles.Clear();
            }
            mFiles.Add(cfgFile);
            var f = new FileInfo(cfgFile);
            FileName = f.Name;
            XmlDocument doc = new XmlDocument();
            doc.Load(cfgFile);
            
            XmlElement root = doc.GetElementsByTagName("setup")[0] as XmlElement;
            var lst = root.GetElementsByTagName("class-mode");
            if (lst != null)
            {
                foreach (var node in lst)
                {
                    XmlElement ele = node as XmlElement;
                    if (ele != null)
                    {
                        var lang = (EGTLang)Enum.Parse(typeof(EGTLang), ele.GetAttribute("lang"), true);
                        GTClassCfg cfg;
                        if (merge && mGTClasses.TryGetValue(lang, out cfg))
                        {
                            cfg.Merge(ele);
                        }
                        else
                        {
                            cfg = new GTClassCfg(ele);
                            mGTClasses[cfg.Lang] = cfg;
                        }
                        if (mClassCfg == null)
                        {
                            mClassCfg = cfg;
                        }
                    }
                }
            }
            lst = root.GetElementsByTagName("output");
            if (lst != null)
            {
                foreach (var node in lst)
                {
                    XmlElement ele = node as XmlElement;
                    if (ele != null)
                    {
                        GTOutputCfg cfg;
                        var dtype = ele.GetAttribute("category");
                        if (merge && mGTOutputs.TryGetValue(dtype, out cfg))
                        {
                            cfg.Merge(ele);
                        }
                        else
                        {
                            cfg = new GTOutputCfg(ele);
                            mGTOutputs[cfg.Category] = cfg;
                        }
                        if (mOutputCfg == null)
                        {
                            mOutputCfg = cfg;
                        }
                    }
                }
            }
        }
        
        public GTClassCfg GetClass(EGTLang lang)
        {
            GTClassCfg cfg;
            if (mGTClasses.TryGetValue(lang, out cfg))
                return cfg;
            else
                return null;
        }

        public GTOutputCfg GetOutput(string category)
        {
            GTOutputCfg cfg;
            if (mGTOutputs.TryGetValue(category, out cfg))
                return cfg;
            else
                return null;
        }

        public void GetOutputCategories(ICollection<string> cols)
        {
            foreach (var cat in mGTOutputs.Keys)
                cols.Add(cat);
        }
    }
}
