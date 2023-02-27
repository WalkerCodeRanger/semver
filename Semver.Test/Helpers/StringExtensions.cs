using Semver.Utility;

namespace Semver.Test.Helpers
{
    public static class StringExtensions
    {
        public static string LimitLength(this string value)
        {
            if (value?.Length > Display.Limit)
                value = value.Substring(0, Display.Limit - 3) + "...";

            return value;
        }
    }
}
