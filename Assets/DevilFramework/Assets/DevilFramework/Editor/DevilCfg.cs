using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace DevilEditor
{
    public class DevilCfg
    {

        public static string[] AIImportFolder { get; private set; }// AI 模块根目录
        public static string AIExportFolder { get; private set; }
        public static string AIInnerFolder { get; private set; }// AI 内部模块目录
        public static string AIEditorFolder { get; private set; }
        static Dictionary<string, string> mTypePattern;
        static Dictionary<string, string> mTypeDefaultValue;
        const string UNKNOWN_TYPE_VALUE = "_";
        const string UNKNOWN_TYPE_PATTERN = "[\\w\\W]*";
        const string DEFAULT_CFG =
@"<configuration>
  <folder>
    <ai-default>Assets/DevilFramework/Core/AIRepository/BehaviourTreeV2</ai-default>
    <ai-import>Assets/Scripts/AIModules</ai-import>
    <ai-export>Assets/Scripts/AIModules</ai-export>
    <ai-editor>Assets/DevilFramework/Editor/AIDesigner</ai-editor>
  </folder>
  
  <type-pattern>
    <float default=" + "\"0\"" + @">^(\-|\+)?\d+(\.\d+)?$</float>
    <int default=" + "\"0\"" + @">^(\-|\+)?\d+$</int>
    <bool default=" + "\"false\"" + @">^(true|false)$</bool>
    <name default=" + "\"_\"" + @">^[a-zA-Z_]+$</name>
    <vector3 default=" + "\"(0,0,0)\"" + @">^\((\-|\+)?\d+(\.\d+)?,(\-|\+)?\d+(\.\d+)?,(\-|\+)?\d+(\.\d+)?\)$</vector3>
  </type-pattern>
  
</configuration>";
        
        public static string CFG_RAW { get; private set; }
        static XmlElement mCfgRoot;

        public static void LoadConfiguration()
        {
            string[] assets = AssetDatabase.FindAssets("t:textasset DevilFramework-Cfg");
            string cfg = DEFAULT_CFG;
            if (assets == null || assets.Length == 0)
            {
                EditorUtility.DisplayDialog("ERROR", "\"DevilFramework-Cfg.xml\" 文件丢失，将使用默认配置：\n" + cfg, "OK");
            }
            else
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                int num = 0;
                for(int i = 0; i < assets.Length; i++)
                {
                    string p = AssetDatabase.GUIDToAssetPath(assets[i]);
                    if(Path.GetFileName(p) == "DevilFramework-Cfg.xml")
                    {
                        num++;
                        path = p;
                    }
                }
                if(num == 0)
                {
                    EditorUtility.DisplayDialog("ERROR", "找不到配置文件 DevilFramework-Cfg.xml。", "OK");
                }
                else
                {
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    cfg = asset.text;
                    if (num > 1)
                    {
                        EditorUtility.DisplayDialog("TIP", string.Format("检测到多个 DevilFramework-Cfg.xml, 将使用 {0} 作为配置文件。", path), "OK");
                    }
                }
            }
            try
            {
                ParseCfg(cfg);
            }
            catch (System.Exception e)
            {
                cfg = DEFAULT_CFG;
                ParseCfg(cfg);
                Debug.LogException(e);
            }
            finally
            {
                CFG_RAW = cfg;
            }
        }

        static void ParseCfg(string cfg)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(cfg);
            XmlElement root = doc.FirstChild as XmlElement;
            mCfgRoot = root;
            XmlElement folder = root.GetElementsByTagName("folder").Item(0) as XmlElement;
            string str = folder.GetElementsByTagName("ai-import").Item(0).InnerText.Trim();
            string[] strs = str == null ? new string[0] : str.Split('\n');
            List<string> import = new List<string>();
            AIEditorFolder = folder.GetElementsByTagName("ai-editor").Item(0).InnerText.Trim();
            AIInnerFolder = folder.GetElementsByTagName("ai-default").Item(0).InnerText.Trim();
            AIExportFolder = folder.GetElementsByTagName("ai-export").Item(0).InnerText.Trim();
            for(int i = 0; i < strs.Length; i++)
            {
                string s = strs[i].Trim();
                if (!string.IsNullOrEmpty(s))
                    import.Add(s);
            }
            if(!import.Contains(AIInnerFolder)) 
                import.Add(AIInnerFolder);
            AIImportFolder = import.ToArray();
            mTypePattern = new Dictionary<string, string>();
            mTypeDefaultValue = new Dictionary<string, string>();
            XmlNodeList lst = root.GetElementsByTagName("type-pattern").Item(0).ChildNodes;
            for(int i = 0; i < lst.Count; i++)
            {
                XmlElement pattern = lst.Item(i) as XmlElement;
                string pat = pattern.InnerText.Trim();
                string val = pattern.GetAttribute("default");
                mTypePattern[pattern.Name] = pat;
                mTypeDefaultValue[pattern.Name] = val;
                string alias = pattern.GetAttribute("alias");
                if (!string.IsNullOrEmpty(alias))
                {
                    string[] aliasarray = alias.Split(',');
                    for(int j = 0; j < aliasarray.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(aliasarray[j]))
                        {
                            mTypePattern[aliasarray[j]] = pat;
                            mTypeDefaultValue[aliasarray[j]] = val;
                        }
                    }
                }
            }
        }

        public static string CfgValue(string path,string defaultValue = null, XmlElement refNode = null)
        {
            XmlElement ele = CfgAtPath(path, refNode);
            if (ele == null)
                return defaultValue;
            return ele.InnerText;
        }

        public static XmlElement CfgAtPath(string elementPath, XmlElement refNode = null)
        {
            XmlElement root = refNode ?? mCfgRoot;
            if (root == null)
                return null;
            int n = elementPath.IndexOf('/');
            string[] paths;
            if (n > 0)
            {
                paths = elementPath.Split('/');
            }
            else
            {
                paths = new string[] { elementPath };
            }
            XmlElement node = null;
            for(int i = 0; i < paths.Length; i++)
            {
                XmlNodeList lst = root.GetElementsByTagName(paths[i]);
                if (lst.Count > 0)
                    root = lst.Item(0) as XmlElement;
                else
                    root = null;
                if (root == null)
                    break;
                if (i == paths.Length - 1)
                    node = root;
            }
            return node;
        }

        public static string GetTypePattern(string type)
        {
            string pattern;
            if (mTypePattern.TryGetValue(type, out pattern))
                return pattern;
            else
                return UNKNOWN_TYPE_PATTERN;
        }

        public static string ReguexTypeValue(string type, string value, string defaultValue)
        {
            string pattern = GetTypePattern(type);
            if (Regex.IsMatch(value ?? "", pattern))
                return value;
            else 
                return defaultValue;
        }

        public static string DefaultTypeValue(string type)
        {
            string value;
            if (mTypeDefaultValue.TryGetValue(type, out value))
                return value;
            else
                return UNKNOWN_TYPE_VALUE;
        }
    }
}