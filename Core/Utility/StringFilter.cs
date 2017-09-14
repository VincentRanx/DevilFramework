
namespace DevilTeam.Utility
{

    public class StringFilter
    {
        public enum FilterType
        {
            none,
            contains,
            contains_ignoreCase,
            starts_with,
            starts_with_ignoreCase,
            ends_with,
            ends_with_ignoreCase,
            equals,
            equals_ignoreCase,
        }

        public static bool TestStr(string a, string b, FilterType type)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;
            switch (type)
            {
                case FilterType.contains:
                    return a.Contains(b);
                case FilterType.contains_ignoreCase:
                    return a.ToLower().Contains(b.ToLower());
                case FilterType.starts_with:
                    return a.StartsWith(b);
                case FilterType.starts_with_ignoreCase:
                    return a.ToLower().StartsWith(b.ToLower());
                case FilterType.ends_with:
                    return a.EndsWith(b);
                case FilterType.ends_with_ignoreCase:
                    return a.ToLower().EndsWith(b.ToLower());
                case FilterType.equals:
                    return a.Equals(b);
                case FilterType.equals_ignoreCase:
                    return a.ToLower().Equals(b.ToLower());
                default:
                    return true;
            }
        }
    }
}