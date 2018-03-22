namespace Devil.AI
{

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class BehaviourTreeAttribute : System.Attribute
    {
        public int SortOrder { get; set; } // 排序
        public string DisplayName { get; set; } // 标题
        public string SubTitle { get; set; } // 注释标题
        public string FrameStyle { get; set; } // 底框
        public string IconPath { get; set; } // 底框图片
        public string InputDatas { get; set; } // property1:type,property2:type, note:type=[row,text]
    }
}