using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            segment = segment.TrimEndSpaces();
            var start = LeftBoundedRange.Unbounded;
            var end = RightBoundedRange.Unbounded;
            foreach (var comparison in SplitComparisons(segment))
            {
                var exception = ParseComparison(comparison, options, ex, ref start, ref end);
                if (exception != null)
                {
                    unbrokenRange = null;
                    return exception;
                }
            }

            // TODO this make empty mean *, is that what we want?

            unbrokenRange = UnbrokenSemVersionRange.Create(start, end,
                options.HasOption(SemVersionRangeOptions.IncludeAllPrerelease));
            return null;
        }

        private static IEnumerable<StringSegment> SplitComparisons(StringSegment segment)
        {
            var start = 0;
            var end = 0;
            while (end < segment.Length)
            {
                // Skip leading spaces
                while (end < segment.Length && segment[end] == ' ') start = end += 1;

                // Skip operators
                while (end < segment.Length && IsOperatorChar(segment[end])) end++;

                // Skip spaces after operators
                while (end < segment.Length && segment[end] == ' ') end += 1;

                // Now find the next space or operator
                while (end < segment.Length && !IsOperatorOrSpaceChar(segment[end])) end++;

                yield return segment.Subsegment(start, end - start);
                start = end;
            }

            if (start < end)
                // TODO not sure if this case can be hit
                yield return segment.Subsegment(start, end - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsOperatorChar(char c)
            => c == '=' || c == '<' || c == '>' || c == '~' || c == '^';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsOperatorOrSpaceChar(char c) => c == ' ' || IsOperatorChar(c);

        private static bool IsVersionChar(char c)
            => c.IsAlphaOrHyphen() || c == '.' || c == '*';

        /// <remarks>The segment must be trimmed before calling this method.</remarks>
        private static Exception ParseComparison(
            StringSegment segment,
            SemVersionRangeOptions options,
            Exception exception,
            ref LeftBoundedRange leftBound,
            ref RightBoundedRange rightBound)
        {
#if DEBUG
            if (segment.IsEmpty) throw new ArgumentException("Cannot be empty", nameof(segment));
#endif
            var firstChar = segment[0];
            var isOrEqual = segment.Length >= 2 && segment[1] == '=';

            //switch (firstChar)
            //{
            //    case '=':
            //        var versionSegment = segment.Subsegment(1);
            //        // TODO parse version
            //        break;
            //    case '>':
            //    case '<':
            //    case '~':
            //    case '^':
            //    default: // implied =
            //        var version
            //}

            throw new NotImplementedException();
        }
    }
}
