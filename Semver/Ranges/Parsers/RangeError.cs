using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Semver.Utility;

namespace Semver.Ranges.Parsers
{
    internal static class RangeError
    {
        private const string TooLongMessage = "Exceeded maximum length of {1} for '{0}'.";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FormatException NewTooLongException(string range, int maxLength)
            => NewFormatException(TooLongMessage, LimitLength(range), maxLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FormatException NewFormatException(string messageTemplate, params object[] args) =>
            new FormatException(string.Format(CultureInfo.InvariantCulture, messageTemplate, args));

        private const int RangeDisplayLimit = 100;

        private static string LimitLength(StringSegment range)
        {
            if (range.Length > RangeDisplayLimit)
                range = range.Subsegment(0, RangeDisplayLimit - 3) + "...";

            return range.ToString();
        }
    }
}
