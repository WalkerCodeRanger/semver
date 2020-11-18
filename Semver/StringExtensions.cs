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
    }
}
