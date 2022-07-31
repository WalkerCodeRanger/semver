using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Semver.Utility;

namespace Semver.Ranges.Parsers
{
    internal static class StandardRangeParser
    {
        public static Exception Parse(
            string range,
            SemVersionRangeOptions options,
            Exception ex,
            int maxLength,
            out SemVersionRange semverRange)
        {
#if DEBUG
            if (!options.IsValid())
                throw new ArgumentException("DEBUG: " + SemVersionRange.InvalidOptionsMessage, nameof(options));
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException("DEBUG: " + SemVersionRange.InvalidMaxLengthMessage, nameof(maxLength));
#endif

            // Assign null once so it doesn't have to be done any time parse fails
            semverRange = null;

            // Note: this method relies on the fact that the null coalescing operator `??`
            // is short circuiting to avoid constructing exceptions and exception messages
            // when a non-null exception is passed in.

            if (range is null) return ex ?? new ArgumentNullException(nameof(range));
            if (range.Length > maxLength) return ex ?? RangeError.NewTooLongException(range, maxLength);

            var unbrokenRanges = new List<UnbrokenSemVersionRange>(CountSplitOnOrOperator(range));
            foreach (var segment in SplitOnOrOperator(range))
            {
                var exception = ParseUnbrokenRange(segment, options, ex, maxLength, out var unbrokenRange);
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
#if DEBUG
            if (range is null) throw new ArgumentNullException(nameof(range), "DEBUG: Value cannot be null.");
#endif

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
#if DEBUG
            if (range is null) throw new ArgumentNullException(nameof(range), "DEBUG: Value cannot be null.");
#endif

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
            int maxLength,
            out UnbrokenSemVersionRange unbrokenRange)
        {
            segment = segment.TrimEndSpaces();
            var start = LeftBoundedRange.Unbounded;
            var end = RightBoundedRange.Unbounded;
            foreach (var comparison in SplitComparisons(segment))
            {
                var exception = ParseComparison(comparison, options, ex, maxLength, ref start, ref end);
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

        /// <remarks>The segment must be trimmed before calling this method.</remarks>
        private static Exception ParseComparison(
            StringSegment segment,
            SemVersionRangeOptions options,
            Exception ex,
            int maxLength,
            ref LeftBoundedRange leftBound,
            ref RightBoundedRange rightBound)
        {
#if DEBUG
            if (segment.IsEmpty) throw new ArgumentException("Cannot be empty", nameof(segment));
#endif
            // TODO identify the operator and report invalid operators
            // TODO Also report invalid chars?

            var firstChar = segment[0];
            var isOrEqual = segment.Length >= 2 && segment[1] == '=';

            switch (firstChar)
            {
                case '=':
                {
                    var version = segment.Subsegment(1);
                    var exception = SemVersionParser.Parse(version, options.ToStyles(), ex, maxLength, out var semver);
                    if (exception != null) return exception;
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    rightBound = rightBound.Min(new RightBoundedRange(semver, true));
                    return null;
                }
                case '>':
                {
                    var version = segment.Subsegment(isOrEqual ? 2 : 1);
                    var exception = SemVersionParser.Parse(version, options.ToStyles(), ex, maxLength, out var semver);
                    if (exception != null) return exception;
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, isOrEqual));
                    return null;
                }
                case '<':
                {
                    var version = segment.Subsegment(isOrEqual ? 2 : 1);
                    var exception = SemVersionParser.Parse(version, options.ToStyles(), ex, maxLength, out var semver);
                    if (exception != null) return exception;
                    rightBound = rightBound.Min(new RightBoundedRange(semver, isOrEqual));
                    return null;
                }
                case '~':
                case '^':
                    throw new NotImplementedException();
                default: // implied = (supports wildcard *)
                    throw new NotImplementedException();
            }
        }
    }
}
