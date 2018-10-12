using System.Collections.Generic;
using System.IO;
using System.Text;
using TableCore.Exceptions;

namespace TableCore
{
    public class CsharpGenerater : ICodeGenerater
    {
        GTClassCfg mCfg;
        public CsharpGenerater(GTClassCfg cfg)
        {
            mCfg = cfg;
        }
        
        public void GenerateCode(GTStatus status, string file)
        {
            GTStatus gt = GTStatus.Instance;
            ClassModel mod = gt.ClassMod;
            if (mod == null)
                throw new NoClassDefineException();

            StringBuilder builder = new StringBuilder();
            AppendUsing(builder);
            int tab = 0;
            bool usenamespace = !string.IsNullOrEmpty(mCfg.NamespaceValue);
            if (usenamespace)
            {
                builder.Append("namespace ").Append(mCfg.NamespaceValue).Append("\n");
                builder.Append("{");
                tab++;
            }
            builder.AppendWithTab("public class ", tab).Append(mod.ClassName).Append(" : TableBase\n");
            builder.AppendWithTab("{\n", tab++);

            for(int i = 0; i < mod.PropertyCount; i++)
            {
                ClassModel.Property p = mod.GetProperty(i);
                if (p.IsID || p.Ignore)
                    continue;
                builder.AppendWithTab("public ", tab).Append(p.GenType.GTName).Append(" ").Append(p.Name).Append(" {get; private set;}\n");
            }

            AppendOverrideInit(builder, tab, mod);

            builder.AppendWithTab("}\n", --tab);

            if (usenamespace)
            {
                tab--;
                builder.Append("}");
            }
            FileInfo f = new FileInfo(file);
            if (!Directory.Exists(f.DirectoryName))
                Directory.CreateDirectory(f.DirectoryName);
            string text = builder.ToString();
            File.WriteAllText(file, text, Encoding.UTF8);
        }
        
        void AppendOverrideInit(StringBuilder builder, int tab, ClassModel mod)
        {
            if (mod.PropertyCount < 2)
                return;
            builder.AppendWithTab("public override void Init(JObject obj)\n", tab);
            builder.AppendWithTab("{\n", tab++);
            builder.AppendWithTab("base.Init(obj);\n", tab);
            for(int i = 0; i < mod.PropertyCount; i++)
            {
                ClassModel.Property p = mod.GetProperty(i);
                if (p.IsID || p.Ignore)
                    continue;
                if (string.IsNullOrEmpty(p.GenType.OverrideCode))
                {
                    builder.AppendWithTab(p.Name, tab).Append(" = obj.Value<").Append(p.GenType.GTName)
                        .Append(">(\"").Append(p.Name).Append("\");\n");
                }
                else
                {
                    string str = p.GenType.OverrideCode.Replace("{name}", p.Name);
                    str = str.Replace("{input}", "obj");
                    string[] lines = str.Split('\n');
                    for(int n = 0; n < lines.Length; n++)
                    {
                        builder.AppendWithTab(lines[n], tab).Append("\n");
                    }
                }
            }
            builder.AppendWithTab("}\n", --tab);
        }

        void AppendUsing(StringBuilder builder)
        {
            List<string> use = mCfg.UsingNamespace;
            for(int i = 0; i < use.Count; i++)
            {
                builder.Append("using ").Append(use[i]).Append(";\n");
            }
        }

    }
}
