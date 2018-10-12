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
        public bool HideProperty { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class BTVariableAttribute : System.Attribute
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string DefaultVallue { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class BTSharedTypeAttribute : System.Attribute
    {
    }
    
}