using System.Collections.Generic;
using System.Linq;

namespace Semver
{
    internal static class StringExtensions
    {
        public static IEnumerable<string> SplitExceptEmpty(this string value, char c)
        {
            var parts = value.Split(c);
            if (parts.Length == 1 && parts[0].Length == 0)
                return Enumerable.Empty<string>();
            return parts;
        }
    }
}
