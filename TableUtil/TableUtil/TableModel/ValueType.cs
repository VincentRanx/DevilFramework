using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TableUtil.TableModel
{
    public class ValueType
    {
        public string Name { get; private set; }
        public string Pattern { get; private set; }

        public ValueType(string name, string pattern)
        {
            Name = name;
            Pattern = pattern;
        }

        public string ToRegexString(string value, string defaultValue)
        {
            if (Regex.IsMatch(value, Pattern))
                return value;
            else
                return defaultValue;
        }

        public virtual void DefineCsField(StringBuilder builder, int tab, string name)
        {
            builder.Tabs(tab);
            builder.Append("private ").Append(Name).Append(" _").Append(name).Append(";\n");
        }

        public virtual void DefineCsPropoerty(StringBuilder builder, int tab, string name)
        {
            builder.Tabs(tab);
            builder.Append("public ").Append(Name).Append(" ").Append(name).Append(" { get{ return ").Append("_").Append(name).Append("; }}\n");
        }
    }

    public class ArrayType : ValueType
    {
        public ValueType Type { get;private set; }

        public ArrayType(ValueType vtype):base(vtype.Name, vtype.Pattern)
        {

        }

        public override void DefineCsField(StringBuilder builder, int tab, string name)
        {
            builder.Tabs(tab);
            builder.Append("private ").Append(Name).Append("[] _").Append(name).Append(";\n");
        }

        public override void DefineCsPropoerty(StringBuilder builder, int tab, string name)
        {
            builder.Tabs(tab);
            builder.Append("public int ").Append(name).Append("Length { get{return _").Append(name).Append(".Length; }}\n");
            builder.Tabs(tab);
            builder.Append("public ").Append(Name).Append(" Get").Append(name).Append("(int index){ return _").Append(name).Append("[index]; }\n");
        }
    }
}
