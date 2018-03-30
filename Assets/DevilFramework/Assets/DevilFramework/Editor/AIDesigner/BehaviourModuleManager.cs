using Devil;
using Devil.AI;
using Devil.Utility;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class BehaviourModuleManager : Singleton<BehaviourModuleManager>
    {
        string libTemplate =
@"using UnityEngine;
%namespace%

[DefaultExecutionOrder(-100)]
public class BehaviourLib : BehaviourLibrary
{
    public BehaviourLib() : base()
    {

    }

    protected override void OnInit()
    {
%pattern%
    }
}";
        int mVersion;
        List<BehaviourMeta> mModules = new List<BehaviourMeta>(); // 行为树模块列表
        public List<BehaviourMeta> Modules { get { return mModules; } }
        List<BehaviourMeta> mComposites = new List<BehaviourMeta>();
        public List<BehaviourMeta> Composites { get { return mComposites; } }
        List<BehaviourMeta> mDecorators = new List<BehaviourMeta>();
        public List<BehaviourMeta> Decorators { get { return mDecorators; } }
        Dictionary<string, Color> mCategoryStyle = new Dictionary<string, Color>();

        List<System.Type> mSharedType = new List<System.Type>(50); // 可用于行为树的数据类型
        public List<System.Type> SharedType { get { return mSharedType; } }
        public string[] SharedTypeNames { get; private set; }

        protected override void OnInit()
        {
        }

        [MenuItem("Devil Framework/AI/Update Behaviour Library")]
        static void GenerateAICSharpe()
        {
            HashSet<string> namespaces = new HashSet<string>();
            List<string> genPatterns = new List<string>();
            namespaces.Add("Devil.AI");
            BehaviourModuleManager inst = GetOrNewInstance();
            foreach (BehaviourMeta meta in inst.mModules)
            {
                if (!string.IsNullOrEmpty(meta.Namespace))
                    namespaces.Add(meta.Namespace);
                switch (meta.NodeType)
                {
                    case EBTNodeType.task:
                        genPatterns.Add(string.Format("mTasks[\"{0}\"] = (id) => new {1}(id);", meta.Name, meta.Name));
                        break;
                    case EBTNodeType.condition:
                        genPatterns.Add(string.Format("mConditions[\"{0}\"] = (id) => new {1}(id);", meta.Name, meta.Name));
                        break;
                    case EBTNodeType.service:
                        genPatterns.Add(string.Format("mServices[\"{0}\"] = (id) => new {1}(id);", meta.Name, meta.Name));
                        break;
                    case EBTNodeType.controller:
                        genPatterns.Add(string.Format("mControllers[\"{0}\"] = (id) => new {1}(id);", meta.Name, meta.Name));
                        break;
                    default:
                        break;
                }
            }
            StringBuilder builder = new StringBuilder();
            int n0 = inst.libTemplate.IndexOf("%namespace%");
            int len0 = 11;
            int n1 = inst.libTemplate.IndexOf("%pattern%");
            int len2 = 9;
            builder.Append(inst.libTemplate.Substring(0, n0));
            foreach (string str in namespaces)
            {
                builder.Append("using ").Append(str).Append(";\n");
            }
            builder.Append(inst.libTemplate.Substring(n0 + len0, n1 - (n0 + len0)));
            foreach (string str in genPatterns)
            {
                builder.Append("        ").Append(str).Append('\n');
            }
            builder.Append(inst.libTemplate.Substring(n1 + len2));

            File.WriteAllText(Path.Combine(DevilCfg.AIExportFolder, "BehaviourLib.cs"), builder.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        public Color GetCategoryColor(string category)
        {
            Color style;
            if (mCategoryStyle.TryGetValue(category, out style))
                return style;
            else
                return new Color(0.3f, 0.3f, 0.3f);
        }

        public void Load()
        {
            mSharedType.Clear();
            mSharedType.Add(typeof(int));
            mSharedType.Add(typeof(float));
            mSharedType.Add(typeof(string));
            mSharedType.Add(typeof(bool));
            mSharedType.Add(typeof(Vector3));
            mSharedType.Add(typeof(GameObject));
            mSharedType.Add(typeof(Transform));
            LoadModules();

            SharedTypeNames = new string[mSharedType.Count];
            for(int i = 0; i < mSharedType.Count; i++)
            {
                SharedTypeNames[i] = mSharedType[i].FullName;
            }
        }

        void LoadModules()
        {
            XmlElement cfg = DevilCfg.CfgAtPath("behaviour-category");
            XmlElement child = cfg == null ? null : cfg.FirstChild as XmlElement;
            Color c;
            while (child != null)
            {
                if (ColorUtility.TryParseHtmlString(DevilCfg.CfgValue("color", "#404040", child), out c))
                    mCategoryStyle[child.Name] = c;
                child = child.NextSibling as XmlElement;
            }
            mModules.Clear();
            mComposites.Clear();
            mDecorators.Clear();
            string[] scripts = AssetDatabase.FindAssets("t:script", DevilCfg.AIImportFolder);
            foreach (string guid in scripts)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ParseAILibs(path);
            }
            GlobalUtil.Sort(mModules, (x, y) => x.Category.CompareTo(y.Category) * 10000 + (x.SortOrder - y.SortOrder));
            string dcate = null;
            string ccate = null;
            for (int i = 0; i < mModules.Count; i++)
            {
                BehaviourMeta meta = mModules[i];
                if (meta.NodeType == EBTNodeType.condition || meta.NodeType == EBTNodeType.service)
                {
                    mDecorators.Add(meta);
                    if(dcate!= meta.Category)
                    {
                        dcate = meta.Category;
                    }
                }
                else
                {
                    mComposites.Add(meta);
                    if(ccate != meta.Category)
                    {
                        ccate = meta.Category;
                    }
                }
            }
        }
        
        void ParseAILibs(string scriptPath)
        {
            MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (mono == null)
                return;
            System.Type t = mono.GetClass();
            if (t == null)
                return;
            // 解析数据类型
            if (System.Attribute.IsDefined(t, typeof(BTSharedTypeAttribute), false))
            {
                mSharedType.Add(t);
            }

            // 解析行为组件
            if (t.GetConstructor(new System.Type[] { typeof(int) }) == null)
                return;
            BehaviourMeta meta = new BehaviourMeta(t);
            if(meta.NodeType != EBTNodeType.invalid)
                mModules.Add(meta);
        }

        public BehaviourMeta FindBTMeta(EBTNodeType type, string bname)
        {
            List<BehaviourMeta> lst = mModules;
            for (int i = 0; i < lst.Count; i++)
            {
                BehaviourMeta meta = lst[i];
                if (meta.NodeType == type && meta.Name == bname)
                    return meta;
            }
            return null;
        }

    }
}