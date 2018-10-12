using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TableCore
{
    public class ClassModel
    {
        public class Property
        {
            string mPName;
            public string Name
            {
                get { return mPName; }
                set
                {
                    IsID = Utils.EqualIgnoreCase(value, "id");
                    if (IsID)
                        mPName = "id";
                    else
                        mPName = value;
                }
            }
            public GTType GenType { get; set; }
            public string Comment { get; set; }
            public bool IsID { get; private set; }
            public int Index { get; set; }
            bool mIgnore;
            public bool Ignore
            {
                get { return (mIgnore && !IsID) || string.IsNullOrEmpty(Name); }
                set
                {
                    mIgnore = value;
                }
            }

            public Property()
            {

            }

            public Property(string name, GTType type, string comment)
            {
                Name = name;
                GenType = type;
                Comment = comment;
                Ignore = false;
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Name);
                while (builder.Length < 20)
                    builder.Append(' ');
                builder.Append(":").Append(GenType.Name);
                return builder.ToString();
            }
        }

        public string ClassName { get; private set; }
        Property[] m_Properties;
        public bool IsIdDefined { get; private set; }
        public int IdIndex { get; private set; }

        public ClassModel(string className, GTStatus status)
        {
            ClassName = className;
            GetProperties(status);
        }

        public ClassModel(string classname, Property[] propreties)
        {
            ClassName = classname;
            m_Properties = propreties;
            for (int i = 0; i < m_Properties.Length; i++)
            {
                m_Properties[i].Index = i;
                if (m_Properties[i].IsID)
                {
                    IsIdDefined = true;
                    IdIndex = i;
                }
            }
        }

        public void ChangeClassName(string newname, GTStatus status)
        {
            if (string.IsNullOrEmpty(newname) || !Regex.IsMatch(newname, GTStatus.NAME_PATTERN))
                return;
            if(newname != ClassName)
            {
                ClassName = newname;
                GetProperties(status);
            }
        }

        void GetProperties(GTStatus status)
        {
            List<Property> lst = new List<Property>();
            for (int col = 0; col < status.TableCols; col++)
            {
                Property p;
                if (status.GetPropertyDefine(col, ClassName, out p))
                {
                    lst.Add(p);
                    if (p.IsID && p.GenType.Name == "int")
                    {
                        IsIdDefined = true;
                    }
                }
                else
                {
                    break;
                }
            }
            m_Properties = lst.ToArray();
            for(int i = 0; i < m_Properties.Length; i++)
            {
                m_Properties[i].Index = i;
                if (m_Properties[i].IsID)
                    IdIndex = i;
            }
        }
        
        public int PropertyCount { get { return m_Properties == null ? 0 : m_Properties.Length; } }

        public Property GetProperty(int index)
        {
            return m_Properties[index];
        }

        public void IgnoreWithPattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                foreach (var p in m_Properties)
                {
                    p.Ignore = false;
                }
            }
            else
            {
                foreach (var p in m_Properties)
                {
                    p.Ignore = Regex.IsMatch(p.Name, pattern);
                }
            }
        }

        public int IndexOfProperty(string pname)
        {
            if (m_Properties == null)
                return -1;
            for(int i = 0; i < m_Properties.Length; i++)
            {
                if (m_Properties[i].Name == pname)
                    return i;
            }
            return -1;
        }

        public bool GetProperty(string pname, out Property property)
        {
            for (int i = PropertyCount - 1; i >= 0; i--)
            {
                Property p = m_Properties[i];
                if (p.Name == pname)
                {
                    property = p;
                    return true;
                }
            }
            property = default(Property);
            return false;
        }

        public Property[] Properties { get { return m_Properties; } }

        public string GetFormatString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("BEGIN:").Append(ClassName).Append("\n");
            int num = 0;
            for(int i = 0; i < PropertyCount; i++)
            {
                int len = m_Properties[i].Name.Length;
                if (len > num)
                    num = len;
            }
            for (int i = 0; i < PropertyCount; i++)
            {
                Property p = m_Properties[i];
                builder.Append("    ");
                builder.Append(p.Name);
                for(int e = num + 2 - p.Name.Length; e > 0; e--)
                    builder.Append(' ');
                builder.Append(":").Append(p.GenType.Name);
                builder.Append("\n");
            }
            builder.Append("End:").Append(ClassName);
            return builder.ToString();
        }
    }
}
