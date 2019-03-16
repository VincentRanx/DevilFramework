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
}