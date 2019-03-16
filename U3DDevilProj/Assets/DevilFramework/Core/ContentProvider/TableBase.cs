using Devil.Utility;
using LitJson;

namespace Devil.ContentProvider
{
    public class TableBase : IIdentified
    {
        private int _id;
        public int Identify { get { return _id; } }

#if UNITY_EDITOR
        string mFormatString;
#endif

        public TableBase()
        {
#if UNITY_EDITOR
            mFormatString = string.Format("<{0}>", GetType().Name);
#endif
        }

        public virtual void Init(JsonData jobj)
        {
#if UNITY_EDITOR
            if (jobj == null)
            {
                RTLog.LogError(LogCat.Table, string.Format("{0}.Init(JObject) must has an instance of JObject parameter", GetType()));
                return;
            }
            mFormatString = string.Format("<{0}>{1}", GetType().Name, jobj.ToJson());
#endif
            _id = (int)jobj["id"];
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            return mFormatString;
        }
#endif
    }
    
}