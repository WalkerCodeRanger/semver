using System;
using System.Collections.Generic;
using System.Linq;

namespace Semver.Test.Builders
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Split a string on a character, but if the string is empty, return no
        /// parts.
        /// </summary>
        public static IEnumerable<string> SplitExceptEmpty(this string value, char c)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) return Enumerable.Empty<string>();
            return value.Split(c);
        }
    }
}
