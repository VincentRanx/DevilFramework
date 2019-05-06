using UnityEngine;

namespace Devil.AI
{
    public delegate object Deserializer(string data);
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BTCompositeAttribute : System.Attribute
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public string IconPath { get; set; }
        public string Category { get; set; }
        public string color { get; set; }
        public KeyCode HotKey { get; set; }
    }
    
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct | System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class BTSharedTypeAttribute : System.Attribute
    {
    }

    public enum EVarType
    {
        Any,
        Variable,
        List,
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class BTVariableReferenceAttribute : PropertyAttribute
    {
        public EVarType VarType { get; set; }
        public System.Type VarClass { get; set; }

        public BTVariableReferenceAttribute()
        {
        }

        public BTVariableReferenceAttribute(System.Type type)
        {
            this.VarClass = type;
            this.VarType = EVarType.Any;
        }

        public BTVariableReferenceAttribute(System.Type type, EVarType vartype)
        {
            this.VarClass = type;
            this.VarType = vartype;
        }
    }
    
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class BTVariableAttribute: PropertyAttribute
    {
        public EVarType VarType { get; set; }
        public System.Type VarClass { get; set; }
        public string VarName { get; set; }
        public string VarValue { get; set; }

        /// <summary>
        /// define string varName as variable name
        /// define anytpye varValue as variable value
        /// </summary>
        public BTVariableAttribute()
        {
            VarName = "varName";
            VarValue = "varValue";
        }

        public BTVariableAttribute(System.Type type)
        {
            VarName = "varName";
            VarValue = "varValue";
            this.VarClass = type;
            this.VarType = EVarType.Any;
        }

        public BTVariableAttribute(System.Type type, EVarType varType)
        {
            VarName = "varName";
            VarValue = "varValue";
            this.VarClass = type;
            this.VarType = varType;
        }
    }
    
}