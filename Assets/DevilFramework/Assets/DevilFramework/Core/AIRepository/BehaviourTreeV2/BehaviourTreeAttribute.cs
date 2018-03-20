namespace Devil.AI
{
    public enum EDataType
    {
        None,
        Integer,
        Float,
        Blackboard,
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class BehaviourTreeAttribute : System.Attribute
    {
        public string DisplayName { get; set; }
        public EDataType InputData { get; set; }
        public string FrameStyle { get; set; }
        public string IconPath { get; set; }
    }
}