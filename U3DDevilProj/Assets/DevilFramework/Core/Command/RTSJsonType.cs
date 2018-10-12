using Newtonsoft.Json.Linq;
using org.vr.rts;
using org.vr.rts.modify;
using System.Text.RegularExpressions;

namespace Devil.Command
{
    public class RTSJsonType : RTSGeneral
    {
        static RTSJsonType _jsonInst;
        public static RTSJsonType JSON
        {
            get
            {
                if (_jsonInst == null)
                    _jsonInst = new RTSJsonType();
                return _jsonInst;
            }
        }

        private RTSJsonType() : base()
        {

        }

        public override object getProperty(IRTSEngine engine, object target, string propertyname)
        {
            JToken tok = target as JToken;
            switch (tok.Type)
            {
                case JTokenType.Array: // target.item0[1,2,3...]
                    if(Regex.IsMatch(propertyname, "^item\\d+$"))
                    {
                        int n = int.Parse(propertyname.Substring(4));
                        JArray arr = (JArray)tok;
                        if (n < arr.Count)
                            return ((JArray)tok)[n];
                        else
                            return null;
                    }
                    else
                    {
                        return base.getProperty(engine, target, propertyname);
                    }
                case JTokenType.Object:
                    return ((JObject)tok).GetValue(propertyname);
                default:
                    return base.getProperty(engine, target, propertyname);
            }
        }

        public override object setProperty(IRTSEngine engine, object target, string propertyname, object value)
        {
            JToken tok = target as JToken;
            JToken ret;
            switch (tok.Type)
            {
                case JTokenType.Array:// target.item0[1,2,3...]
                    int n = int.Parse(propertyname.Substring(4));
                    ret = (JToken)castValue(value);
                    JArray arr = (JArray)tok;
                    while (arr.Count <= n)
                        arr.Add("");
                    ((JArray)tok)[n] = ret;
                    return ret;
                case JTokenType.Object:
                    ret = value == null ? null : ValueOf(value);
                    ((JObject)tok)[propertyname] = ret;
                    return ret;
                default:
                    return base.setProperty(engine, target, propertyname, value);
            }
        }

        public override object castValue(object target)
        {
            return ValueOf(target);
        }

        public override string typeName()
        {
            return "JSON";
        }

        public static JToken ValueOf(object target)
        {
            if (target is JToken)
                return (JToken)target;
            else if (target is int)
                return (int)target;
            else if (target is uint)
                return (uint)target;
            else if (target is long)
                return (long)target;
            else if (target is ulong)
                return (ulong)target;
            else if (target is float)
                return (float)target;
            else if (target is double)
                return (double)target;
            else if (target is string)
                return (string)target;
            else if (target is System.Array)
            {
                JArray arr = new JArray();
                foreach (var e in (System.Array)target)
                {
                    arr.Add(ValueOf(e));
                }
                return arr;
            }
            else if (target != null)
                return target.ToString();
            else
                return "";
        }
    }
}