using System;
using System.Collections.Generic;
using Semver.Utility;

namespace Semver.Ranges.Parsing
{
    internal static class RangeParser
    {
        public static Exception Parse(
            string range,
            SemVersionRangeOptions options,
            Exception ex,
            int maxLength,
            out SemVersionRange semverRange)
        {
            // TODO check max length
            var unbrokenRanges = new List<UnbrokenSemVersionRange>(CountSplitOnOrOperator(range));
            foreach (var segment in SplitOnOrOperator(range))
            {
                var exception = ParseUnbrokenRange(segment, options, ex, out var unbrokenRange);
                if (exception is null)
                    unbrokenRanges.Add(unbrokenRange);
                else
                {
                    semverRange = null;
                    return exception;
                }
            }

            // TODO sort ranges
            // TODO combine ranges
            semverRange = new SemVersionRange(unbrokenRanges.AsReadOnly());
            return null;
        }

        public static int CountSplitOnOrOperator(string range)
        {
            int count = 1; // Always one more item than there are separators
            bool possiblyInSeparator = false;
            for (int i = 0; i < range.Length; i++)
            {
                var isSeparatorChar = range[i] == '|';
                if (possiblyInSeparator && isSeparatorChar)
                {
                    count++;
                    possiblyInSeparator = false;
                }
                else
                    possiblyInSeparator = isSeparatorChar;
            }

            return count;
        }

        public static IEnumerable<StringSegment> SplitOnOrOperator(string range)
        {
            var possiblyInSeparator = false;
            int start = 0;
            for (int i = 0; i < range.Length; i++)
            {
                var isSeparatorChar = range[i] == '|';
                if (possiblyInSeparator && isSeparatorChar)
                {
                    possiblyInSeparator = false;
                    yield return range.Slice(start, i - 1 - start);
                    start = i + 1;
                }
                else
                    possiblyInSeparator = isSeparatorChar;
            }

            // The final segment from the last separator to the end of the string
            yield return range.Slice(start, range.Length - start);
        }

        private static Exception ParseUnbrokenRange(
            StringSegment segment,
            SemVersionRangeOptions options,
            Exception ex,
            out UnbrokenSemVersionRange unbrokenRange)
        {
            throw new NotImplementedException();
        }
    }
}
