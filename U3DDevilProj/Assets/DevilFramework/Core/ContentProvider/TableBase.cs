using Devil.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public virtual void Init(JObject jobj)
        {
#if UNITY_EDITOR
            if (jobj == null)
            {
                RTLog.LogError(LogCat.Table, string.Format("{0}.Init(JObject) must has an instance of JObject parameter", GetType()));
                return;
            }
            mFormatString = string.Format("<{0}>{1}", GetType().Name, JsonConvert.SerializeObject(jobj));
#endif
            JToken tok;
            if (jobj.TryGetValue("id", out tok))
            {
                _id = tok.ToObject<int>();
            }
#if UNITY_EDITOR
            else
            {
                RTLog.LogError(LogCat.Table, string.Format("\"id\" is required for table base.\njson: {0}", jobj));
            }
#endif
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            return mFormatString;
        }
#endif
    }
    
}