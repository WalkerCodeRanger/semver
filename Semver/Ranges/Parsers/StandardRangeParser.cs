using System;
using System.Collections.Generic;
using Semver.Utility;
using static Semver.Ranges.Parsers.GeneralRangeParser;

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
                throw new ArgumentOutOfRangeException(nameof(maxLength), "DEBUG: " + SemVersionRange.InvalidMaxLengthMessage);
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
            semverRange = SemVersionRange.Create(unbrokenRanges);
            return null;
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
            var includeAllPrerelease = options.HasOption(SemVersionRangeOptions.IncludeAllPrerelease);
            foreach (var comparison in SplitComparisons(segment))
            {
                var exception = ParseComparison(comparison, options, ref includeAllPrerelease, ex, maxLength, ref start, ref end);
                if (exception != null)
                {
                    unbrokenRange = null;
                    return exception;
                }
            }

            // TODO this makes empty mean *, is that what we want?
            unbrokenRange = UnbrokenSemVersionRange.Create(start, end, includeAllPrerelease);
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
                while (end < segment.Length && IsPossibleOperatorChar(segment[end])) end++;

                // Skip spaces after operators
                while (end < segment.Length && segment[end] == ' ') end += 1;

                // Now find the next space or operator
                while (end < segment.Length && !IsPossibleOperatorOrSpaceChar(segment[end])) end++;

                yield return segment.Subsegment(start, end - start);
                start = end;
            }

            if (start < end)
                // TODO not sure if this case can be hit
                yield return segment.Subsegment(start, end - start);
        }

        private static Exception ParseComparison(
            StringSegment segment,
            SemVersionRangeOptions options,
            ref bool includeAllPrerelease,
            Exception ex,
            int maxLength,
            ref LeftBoundedRange leftBound,
            ref RightBoundedRange rightBound)
        {
#if DEBUG
            if (segment.IsEmpty) throw new ArgumentException("Cannot be empty", nameof(segment));
#endif
            var exception = ParseWhitespace(ref segment, ex);
            if (exception != null) return exception;

            exception = ParseOperator(ref segment, ex, out var @operator);
            if (exception != null) return exception;

            exception = ParseWhitespace(ref segment, ex);
            if (exception != null) return exception;

            exception = SemVersionParser.Parse(segment, options.ToStyles(), ex, maxLength, out var semver);
            if (exception != null) return exception;

            switch (@operator)
            {
                case StandardOperator.Equals:
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    rightBound = rightBound.Min(new RightBoundedRange(semver, true));
                    return null;
                case StandardOperator.GreaterThan:
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, false));
                    return null;
                case StandardOperator.GreaterThanOrEqual:
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    return null;
                case StandardOperator.LessThan:
                    rightBound = rightBound.Min(new RightBoundedRange(semver, false));
                    return null;
                case StandardOperator.LessThanOrEqual:
                    rightBound = rightBound.Min(new RightBoundedRange(semver, true));
                    return null;
                case StandardOperator.Caret:
                case StandardOperator.Tilde:
                case StandardOperator.None: // implied = (supports wildcard *)
                    throw new NotImplementedException();
                default:
                    // This code should be unreachable
                    throw new ArgumentException($"DEBUG: Invalid {nameof(StandardOperator)} value {@operator}");
            }
        }

        private static Exception ParseOperator(
            ref StringSegment segment, Exception ex, out StandardOperator @operator)
        {
            var end = 0;
            while (end < segment.Length && IsPossibleOperatorChar(segment[end])) end++;
            var opSegment = segment.Subsegment(0, end);
            segment = segment.Subsegment(end);

            if (opSegment.Length == 0)
            {
                @operator = StandardOperator.None;
                return null;
            }

            // Assign invalid once so it doesn't have to be done any time parse fails
            @operator = 0;
            if (opSegment.Length > 2
                || (opSegment.Length == 2 && opSegment[1] != '='))
                return ex ?? RangeError.InvalidOperator(opSegment.ToString());

            var firstChar = opSegment[0];
            var isOrEqual = opSegment.Length == 2; // Already checked for second char != '='
            switch (firstChar)
            {
                case '=' when !isOrEqual:
                    @operator = StandardOperator.Equals;
                    return null;
                case '<' when isOrEqual:
                    @operator = StandardOperator.LessThanOrEqual;
                    return null;
                case '<':
                    @operator = StandardOperator.LessThan;
                    return null;
                case '>' when isOrEqual:
                    @operator = StandardOperator.GreaterThanOrEqual;
                    return null;
                case '>':
                    @operator = StandardOperator.GreaterThan;
                    return null;
                case '~' when !isOrEqual:
                    @operator = StandardOperator.Tilde;
                    return null;
                case '^' when !isOrEqual:
                    @operator = StandardOperator.Caret;
                    return null;
                default:
                    return ex ?? RangeError.InvalidOperator(opSegment.ToString());
            }
        }
    }
}
