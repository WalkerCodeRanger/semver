using System;
using System.Collections.Generic;
using System.Linq;

namespace Semver.Test.Builders
{
    public static class StringExtensions
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

        private const int DisplayLimit = 100;

        public static string LimitLength(this string value)
        {
            if (value?.Length > DisplayLimit)
                value = value.Substring(0, DisplayLimit - 3) + "...";

            return value;
        }
    }
}
