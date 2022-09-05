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
            SemVersionRangeOptions rangeOptions,
            Exception ex,
            int maxLength,
            out SemVersionRange semverRange)
        {
#if DEBUG
            if (!rangeOptions.IsValid())
                throw new ArgumentException("DEBUG: " + SemVersionRange.InvalidOptionsMessage, nameof(rangeOptions));
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
                var exception = ParseUnbrokenRange(segment, rangeOptions, ex, maxLength, out var unbrokenRange);
                if (exception is null)
                    unbrokenRanges.Add(unbrokenRange);
                else
                {
                    semverRange = null;
                    return exception;
                }
            }

            semverRange = SemVersionRange.Create(unbrokenRanges);
            return null;
        }

        private static Exception ParseUnbrokenRange(
            StringSegment segment,
            SemVersionRangeOptions rangeOptions,
            Exception ex,
            int maxLength,
            out UnbrokenSemVersionRange unbrokenRange)
        {
            // Assign null once so it doesn't have to be done any time parse fails
            unbrokenRange = null;

            // Parse off leading whitespace
            var exception = ParseWhitespace(ref segment, ex);
            if (exception != null) return exception;

            // Reject empty string ranges
            if (segment.IsEmpty) return ex ?? RangeError.MissingComparison(segment.Offset, segment.Source);

            var start = LeftBoundedRange.Unbounded;
            var end = RightBoundedRange.Unbounded;
            var includeAllPrerelease = rangeOptions.HasOption(SemVersionRangeOptions.IncludeAllPrerelease);
            while (!segment.IsEmpty)
            {
                exception = ParseComparison(ref segment, rangeOptions, ref includeAllPrerelease, ex, maxLength, ref start, ref end);
                if (exception != null) return exception;
            }

            unbrokenRange = UnbrokenSemVersionRange.Create(start, end, includeAllPrerelease);
            return null;
        }

        /// <summary>
        /// Parse a comparison from the beginning of the segment.
        /// </summary>
        /// <remarks>Must have leading whitespace removed. Will consume trailing whitespace.</remarks>
        private static Exception ParseComparison(
            ref StringSegment segment,
            SemVersionRangeOptions rangeOptions,
            ref bool includeAllPrerelease,
            Exception ex,
            int maxLength,
            ref LeftBoundedRange leftBound,
            ref RightBoundedRange rightBound)
        {
#if DEBUG
            if (segment.IsEmpty) throw new ArgumentException("Cannot be empty", nameof(segment));
#endif
            var exception = ParseOperator(ref segment, ex, out var @operator);
            if (exception != null) return exception;

            exception = ParseWhitespace(ref segment, ex);
            if (exception != null) return exception;

            exception = ParseVersion(ref segment, rangeOptions, ParsingOptions, ex, maxLength,
                            out var semver, out var wildcardVersion);
            if (exception != null) return exception;

            exception = ParseWhitespace(ref segment, ex);
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
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    int major = 0, minor = 0, patch = 0;
                    if (semver.Major != 0)
                    {
                        if (semver.Major == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                        major = semver.Major + 1;
                    }
                    else if (semver.Minor != 0)
                    {
                        if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                        minor = semver.Minor + 1;
                    }
                    else
                    {
                        if (semver.Patch == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                        patch = semver.Patch + 1;
                    }

                    rightBound = rightBound.Min(new RightBoundedRange(new SemVersion(
                                    major, minor, patch,
                                    "0", PrereleaseIdentifiers.Zero, "",
                                    ReadOnlyList<MetadataIdentifier>.Empty), false));
                    return null;
                case StandardOperator.Tilde:
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                    rightBound = rightBound.Min(new RightBoundedRange(
                        semver.With(minor: semver.Minor + 1, patch: 0, prerelease: PrereleaseIdentifiers.Zero),
                        false));
                    return null;
                case StandardOperator.None: // implied = (supports wildcard *)
                    // TODO support wildcards
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    rightBound = rightBound.Min(new RightBoundedRange(semver, true));
                    return null;
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
                return ex ?? RangeError.InvalidOperator(opSegment);

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
                    return ex ?? RangeError.InvalidOperator(opSegment);
            }
        }

        private static readonly SemVersionParsingOptions ParsingOptions =
            new SemVersionParsingOptions(true, true, c => c == '*');
    }
}
