using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TableUtil.TableModel
{
    public class ClassModel
    {
        public class Property
        {
            public string Name { get; set; }
            public ValueType Type { get; set; }
        }

        public string Namespace { get; set; }
        public string Name { get; set; }
        public List<string> UsingNamespaces { get; private set; }
        public List<Property> Properties { get; private set; }

        public ClassModel()
        {
            Properties = new List<Property>();
            UsingNamespaces = new List<string>();
        }

        public string ExportToCs()
        {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < UsingNamespaces.Count; i++)
            {
                builder.Append("using \"").Append(UsingNamespaces[i]).Append("\";\n");
            }
            builder.Append('\n');
            bool nameSpace = !string.IsNullOrEmpty(Namespace);
            int tab = 0;
            if (nameSpace)
            {
                tab++;
                builder.Append("namespace ").Append(Namespace).Append("\n{\n");
            }
            
            builder.Tabs(tab).Append("public class ").Append(Name).Append(" : TableBase\n");
            builder.Tabs(tab).Append("{\n");
            tab++;
            
            tab--;
            builder.Tabs(tab).Append("}\n");
            if (nameSpace)
            {
                tab--;
                builder.Append("}\n");
            }
            return builder.ToString();
        }

    }
}
