using System;

namespace Semver.Utility
{
    internal static class Display
    {
        public const int Limit = 100;

        public static string ToStringLimitLength(this ReadOnlySpan<char> value)
        {
            if (value.Length > Limit) return value[..(Limit - 3)].ToString() + "...";

            return value.ToString();
        }
    }
}
