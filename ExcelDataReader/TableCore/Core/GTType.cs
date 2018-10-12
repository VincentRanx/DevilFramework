using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;
using TableCore.Plugin;

namespace TableCore
{
    public enum ECaseType
    {
        normal,
        uper,
        lower,
    }

    public class GTType : IGenFormater
    {
        public ECaseType CaseType { get; set; }
        public EGTLang Lang { get; private set; }
        public string Name { get; private set; }
        public string DefaultValue { get; private set; }
        /// <summary>
        /// 生成对象类型名
        /// </summary>
        public string GTName { get; set; }
        public IGenFormater Formater { get; set; }
        public string OverrideCode { get; set; }
        string mPattern;
        public string Pattern { get { return mPattern; } }

        public GTType(EGTLang lang)
        {
            Lang = lang;
        }
        
        public void Init(XmlElement element)
        {
            Name = element.Name;
            DefaultValue = element.GetAttribute("default");
            GTName = element.GetAttribute("name");
            mPattern = element.InnerText;
            OverrideCode = Utils.ReadRelativeFile(string.Format("Config/{0}-input-{1}.txt", Name, Lang.ToString()));
        }

        public void Init(XmlElement element, Dictionary<string,string> patterns, Dictionary<string, IGenFormater> formaters)
        {
            Name = element.Name;
            DefaultValue = element.GetAttribute("default");
            GTName = element.GetAttribute("name");
            OverrideCode = Utils.ReadRelativeFile(string.Format("Config/{0}-input-{1}.txt", Name, Lang.ToString()));
            var str = element.GetAttribute("case");
            if (!string.IsNullOrEmpty(str))
                CaseType = (ECaseType)Enum.Parse(typeof(ECaseType), str);
            var patt = element.GetAttribute("pattern");
            if (string.IsNullOrEmpty(patt) || !patterns.TryGetValue(patt, out mPattern))
            {
                mPattern = element.InnerText;
            }
            IGenFormater formater;
            if (formaters.TryGetValue(Name, out formater))
                Formater = formater;
        }

        public string FormatInput(string input)
        {
            string v = string.IsNullOrEmpty(input) ? DefaultValue : input;
            if (v == null)
                return null;
            if (CaseType == ECaseType.uper)
                v = v.ToUpper();
            else if (CaseType == ECaseType.lower)
                v = v.ToLower();
            return v;
        }

        public bool IsValid(string input)
        {
            if (!string.IsNullOrEmpty(mPattern) && !Regex.IsMatch(input, mPattern))
                return false;
            if (Formater == null)
                return true;
            else
                return Formater.IsValid(input);
        }

        public JToken Format(string input, GTOutputCfg category)
        {
            if (Formater == null)
                return input;
            else
                return Formater.Format(input, category);
        }
        
        public IExportData ExportData(string data, string comment)
        {
            return Formater == null ? null : Formater.ExportData(data, comment);
        }
    }
}
