using Devil.Utility;

namespace Devil.GamePlay
{
    public static class SitcomUtil
    {
        public const string NUMBER_PATTERN = @"^(\-|\+)?\d+(\.\d+)?$";

        public static bool ParseAsBool(object v)
        {
            if (v is bool)
                return (bool)v;
            if (v is string)
                return StringUtil.EqualIgnoreCase("true", (string)v);
            return v != null;
        }

        public static float ParseAsNumber(object v)
        {
            if (v is float)
                return (float)v;
            float f;
            if (v is string && float.TryParse((string)v, out f))
                return f;
            return v == null ? 0 : v.GetHashCode();
        }

        public static bool AsBool(object v)
        {
            if (v == null)
                return false;
            if (v is bool)
                return (bool)v;
            var meta = SitcomHeap.Global.GetMeta(v.GetType());
            return meta == null ? true : meta.AsBool(v);
        }

        public static float AsNumber(object v)
        {
            if (v == null)
                return 0;
            if (v is float)
                return (float)v;
            var meta = SitcomHeap.Global.GetMeta(v.GetType());
            return meta == null ? 0 : meta.AsNumber(v);
        }
	}
}