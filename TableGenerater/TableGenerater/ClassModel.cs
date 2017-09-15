using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ExcelApp = Microsoft.Office.Interop.Excel.Application;
using ExcelBook = Microsoft.Office.Interop.Excel.Workbook;
using ExcelSheet = Microsoft.Office.Interop.Excel.Worksheet;
using ExcelRange = Microsoft.Office.Interop.Excel.Range;
using System.Windows.Forms;

namespace TableGenerater
{
    public enum Errors
    {
        none = 0,
        invalid_namespace = 0x1,
        invalid_name = 0x2,
        invalid_fieldName = 0x4,
        invalid_type = 0x8,
        no_id = 0x10,
        no_folder = 0x20,
        no_file = 0x40,
        repeat_property = 0x80,
    }

    public class ClassModel
    {
      
        public class Field
        {
            public string name;
            public string primType;
            public string type;
            public string comment;
            public bool isArray;
            public int arrayLen;
            public bool isBaseType;

            private int baseHash;

            public Field()
            {
                baseHash = "ClassModel.Field".GetHashCode();
            }

            public string GetJsonValue(string src)
            {
                src = src ?? "";
                if(type == "bool" && isArray)
                    return string.Format("\"{0}\"", src.ToLower());
                else if (isArray || type == "string")
                    return string.Format("\"{0}\"", src);
                else if (type == "bool")
                    return src.ToLower() == "true" ? "true" : "false";
                else if (type == "float")
                    return !Regex.IsMatch(src,"[0-9]+.?[0-9]*") ? "0" : src;
                else if (type == "int")
                    return !Regex.IsMatch(src, "[0-9]+") ? "0" : src;
                else
                    return src;
            }
            
            public override bool Equals(object obj)
            {
                Field f = obj as Field;
                return f != null && f.name == this.name;
            }

            public override int GetHashCode()
            {
                return name.GetHashCode() | baseHash;
            }
        }

        public string NameSpace { get; private set; }
        public string ClassName { get; private set; }
        public List<Field> Fields { get; private set; }
        public Errors Error { get; private set; }

        HashSet<string> baseType;

        public ClassModel(string nameSpace, string className, ExcelReader excel)
        {
            baseType = new HashSet<string>();
            baseType.Add("bool");
            baseType.Add("int");
            baseType.Add("string");
            baseType.Add("float");

            NameSpace = nameSpace;
            ClassName = className;
            Fields = new List<Field>();
            int i = 2;
            while (true)
            {
                string range = excel.GetCell(1, i);
                
                if (string.IsNullOrEmpty(range))
                    break;
                Field f = new Field();
                f.name = range;
                f.primType = excel.GetCell(2, i);
                f.comment = excel.GetCell(3, i);
                f.isArray = Regex.IsMatch(f.primType, "^[a-zA-Z]+\\[[0-9]*\\]$");
                if (f.isArray)
                {
                    int n0 = f.primType.IndexOf('[');
                    int n1 = f.primType.IndexOf(']');
                    f.type = f.primType.Substring(0, n0);
                    if (n1 <= n0 + 1 || !int.TryParse(f.primType.Substring(n0 + 1, n1 - n0 - 1), out f.arrayLen))
                    {
                        f.arrayLen = -1;
                    }
                }
                else
                {
                    f.type = f.primType;
                }
                f.isBaseType = baseType.Contains(f.type);
                if (Fields.Contains(f))
                {
                    MessageBox.Show(f.name + " 属性重复", "错误");
                    Error |= Errors.repeat_property;
                }
                else
                {
                    Fields.Add(f);
                }
                i++;
            }
        }

        public string FullName { get { return string.Format("{0}.{1}", NameSpace, ClassName); } }

        public bool IsVaild
        {
            get
            {
                if (!IsValidNamespace(NameSpace))
                {
                    Error |= Errors.invalid_namespace;
                }
                if (!IsValidName(ClassName))
                {
                    Error |= Errors.invalid_name;
                }
                bool hasId = false;
                for(int i = 0; i < Fields.Count; i++)
                {
                    Field f = Fields[i];
                    if(!IsValidFieldName(f.name))
                    {
                        Error |= Errors.invalid_fieldName;
                    }
                    if (f.name == "id" && f.type == "int")
                        hasId = true;
                }
                if (!hasId)
                    Error |= Errors.no_id;
                return Error == 0 ;
            }
        }

        public static bool IsValidName(string className)
        {
            return Regex.IsMatch(className, "^[A-Z][a-zA-Z]*$");
        }

        public static bool IsValidNamespace(string nameSpace)
        {
            return Regex.IsMatch(nameSpace, "^[a-zA-Z]+[a-zA-Z|.]*[a-zA-Z]+$");
        }

        public static bool IsValidFieldName(string fieldName)
        {
            return Regex.IsMatch(fieldName, "^[a-z][a-zA-Z]*$");
        }

        void GenerateFields(StringBuilder fstr)
        {
            for (int i = 0; i < Fields.Count; i++)
            {
                Field f = Fields[i];
                if (f.name == "id")
                    continue;
                string pname = (char)(f.name[0] - 32) + f.name.Substring(1);
                fstr.Append("\t\t/* ").Append(f.comment).Append(" */\n");
                fstr.Append("\t\tprivate ");
                fstr.Append(f.isBaseType ? f.type : "int");
                if (f.isArray)
                    fstr.Append("[]");
                fstr.Append(" ").Append(f.name).Append(";\n");
                if (f.isArray)
                {
                    fstr.Append("\t\tpublic int ").Append(pname).Append("Length");
                    fstr.Append("{ get { return ").Append(f.name).Append(".Length; } }\n");
                    fstr.Append("\t\tpublic ").Append(f.type).Append(" ").Append(pname).Append("At(int index) { return ");
                    if (f.isBaseType)
                    {
                        fstr.Append(f.name).Append("[index]; }\n");
                    }
                    else
                    {
                        fstr.Append("TableSet<").Append(f.type).Append(">.Instance[ ");
                        fstr.Append(f.name).Append("[index] ");
                        fstr.Append("]; }\n");
                    }
                }
                else
                {
                    fstr.Append("\t\tpublic ").Append(f.type).Append(" ").Append(pname);
                    fstr.Append(" { get { return ");
                    if (f.isBaseType)
                    {
                        fstr.Append(f.name);
                    }
                    else
                    {
                        fstr.Append("TableSet<").Append(f.type).Append(">.Instance[").Append(f.name).Append("]");
                    }
                    fstr.Append("; } }\n");
                }
            }
        }

        void GenerateStringToType(StringBuilder fstr, string input, string type)
        {
            if (type == "string")
                fstr.Append(input);
            else
                fstr.Append(type).Append(".Parse(").Append(input).Append(")");
        }

        void GenerateInitCode(StringBuilder fstr)
        {
            fstr.Append("\t\t\tbase.Init(jobj);\n");
            fstr.Append("\t\t\tJToken tok;\n");
            for(int i = 0; i < Fields.Count; i++)
            {
                Field f = Fields[i];
                if (f.name == "id")
                    continue;
                string tp = f.isBaseType ? f.type : "int";
                fstr.Append("\t\t\tif(jobj.TryGetValue(\"").Append(f.name).Append("\", out tok))\n");
                fstr.Append("\t\t\t{\n");
                if (f.isArray)
                {
                    fstr.Append("\t\t\t\tstring[] list = tok.ToObject<string>().Split(',');\n");
                    if (f.arrayLen == -1 && tp == "string")
                    {
                        fstr.Append("\t\t\t\t").Append(f.name).Append(" = list;\n");
                    }
                    else
                    {
                        fstr.Append("\t\t\t\t").Append(f.name).Append(" = new ").Append(tp);
                        fstr.Append("[").Append(f.arrayLen == -1 ? "list.Length" : f.arrayLen.ToString());
                        fstr.Append("];\n");
                        fstr.Append("\t\t\t\tfor(int i = ");
                        fstr.Append("UnityEngine.Mathf.Min(list.Length, ").Append(f.name);
                        fstr.Append(".Length) - 1; i >= 0; i--)\n");
                        fstr.Append("\t\t\t\t{\n");
                        fstr.Append("\t\t\t\t\t").Append(f.name).Append("[i]").Append(" = ");
                        GenerateStringToType(fstr, "list[i]", tp);
                        fstr.Append(";\n");
                        fstr.Append("\t\t\t\t}\n");
                    }
                }
                else
                {
                    fstr.Append("\t\t\t\t").Append(f.name);
                    fstr.Append(" = tok.ToObject<").Append(tp).Append(">();\n");
                }
                fstr.Append("\t\t\t}\n");
            }
        }

        public Errors GenerateCSharp(string outputFolder)
        {
            if (!IsVaild)
            {
                return Error;
            }
            if (!Directory.Exists(outputFolder))
            {
                return Errors.no_folder;
            }
           
            StringBuilder fstr = new StringBuilder();
            fstr.Append("using DevilTeam.ContentProvider;\n");
            fstr.Append("using Newtonsoft.Json.Linq;\n\n");
            fstr.Append("namespace ").Append(NameSpace).Append("\n");
            fstr.Append("{\n");

            fstr.Append("\tpublic class ").Append(ClassName).Append(" : TableBase\n");
            fstr.Append("\t{\n");
            GenerateFields(fstr);
            fstr.Append("\n");
            fstr.Append("\t\tpublic override void Init(JObject jobj)\n");
            fstr.Append("\t\t{\n");
            GenerateInitCode(fstr);
            fstr.Append("\t\t}\n");

            fstr.Append("\t}\n");

            fstr.Append("}");

            File.WriteAllText(Path.Combine(outputFolder, ClassName + ".cs"),fstr.ToString());
            return 0;
        }

    }

}
