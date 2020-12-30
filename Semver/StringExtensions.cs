using System.Collections.Generic;
using System.Linq;

namespace Semver
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Split a string on a character, but if the string is empty, return no
        /// parts.
        /// </summary>
        public static IEnumerable<string> SplitExceptEmpty(this string value, char c)
        {
            var parts = value.Split(c);
            if (parts.Length == 1 && parts[0].Length == 0)
                return Enumerable.Empty<string>();
            return parts;
        }

        /// <summary>
        /// Is this string composed entirely of digits 0 to 9?
        /// </summary>
        public static bool IsDigits(this string value)
        {
            foreach (var c in value)
                if (!c.IsDigit())
                    return false;

            return true;
        }

        /// <summary>
        /// Is this string composed entirely of alphanumeric characters and hyphens?
        /// </summary>
        public static bool IsAlphanumericOrHyphens(this string value)
        {
            foreach (var c in value)
                if (!c.IsAlphaOrHyphen() && !c.IsDigit())
                    return false;

            return true;
        }
    }
}
