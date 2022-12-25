using System;
using System.Collections.Generic;
using Semver.Utility;
using static Semver.Ranges.Parsers.GeneralRangeParser;

namespace Semver.Ranges.Parsers
{
    internal static class NpmRangeParser
    {
        private const SemVersionRangeOptions StandardRangeOptions
            = SemVersionRangeOptions.AllowLowerV
              | SemVersionRangeOptions.AllowMetadata
              | SemVersionRangeOptions.OptionalMinorPatch;

        public static Exception Parse(
            string range,
            bool includeAllPrerelease,
            Exception ex,
            int maxLength,
            out SemVersionRange semverRange)
        {
            var options = StandardRangeOptions;
            if (includeAllPrerelease)
                options |= SemVersionRangeOptions.IncludeAllPrerelease;
            return Parse(range, options, ex, maxLength, out semverRange);
        }

        private static Exception Parse(
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
                throw new ArgumentOutOfRangeException(nameof(maxLength),
                    "DEBUG: " + SemVersionRange.InvalidMaxLengthMessage);
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
                    return exception;
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

            var includeAllPrerelease = rangeOptions.HasOption(SemVersionRangeOptions.IncludeAllPrerelease);

            // Handle empty string ranges
            if (segment.IsEmpty)
            {
                unbrokenRange = includeAllPrerelease ? UnbrokenSemVersionRange.All : UnbrokenSemVersionRange.AllRelease;
                return null;
            }

            var start = LeftBoundedRange.Unbounded;
            var end = RightBoundedRange.Unbounded;
            while (!segment.IsEmpty)
            {
                exception = ParseComparison(ref segment, rangeOptions, ref includeAllPrerelease, ex, maxLength,
                    ref start, ref end);
                if (exception != null) return exception;
            }

            unbrokenRange = UnbrokenSemVersionRange.Create(start, end, includeAllPrerelease);
            return null;
        }

        /// <summary>
        /// Parse a comparison from the beginning of the segment.
        /// </summary>
        /// <remarks>
        /// <para>Must have leading whitespace removed. Will consume trailing whitespace.</para>
        ///
        /// <para>When applying caret, tilde, or wildcards to versions already at
        /// <see cref="int.MaxValue"/> there are ranges that would be equivalent to being able to
        /// increment beyond max value. However, for simplicity, this is treated as an error instead.
        /// This also makes sense given that these ranges would logically include versions valid
        /// according to the spec that can't be represented by this library due to the limitations
        /// of <see cref="int"/>. Finally, if these equivalent ranges were supported they would also
        /// need special case handling in the <see cref="UnbrokenSemVersionRange.ToString"/> method.
        /// </para></remarks>
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
            if (segment.IsEmpty) throw new ArgumentException("DEBUG: Cannot be empty", nameof(segment));
#endif
            var exception = ParseOperator(ref segment, ex, out var @operator);
            if (exception != null) return exception;

            exception = ParseWhitespace(ref segment, ex);
            if (exception != null) return exception;

            exception = ParseVersion(ref segment, rangeOptions, ParsingOptions, ex, maxLength,
                            out var semver, out var wildcardVersion);
            if (exception != null) return exception;
            // TODO check for wildcards combined with metadata which is not allow by npm
            // Remove the metadata the npm ranges allow
            semver = semver.WithoutMetadata();

            exception = ParseWhitespace(ref segment, ex);
            if (exception != null) return exception;

            switch (@operator)
            {
                case StandardOperator.GreaterThan:
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, false));
                    return null;
                case StandardOperator.GreaterThanOrEqual:
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    return null;
                case StandardOperator.LessThan:
                    rightBound = rightBound.Min(new RightBoundedRange(semver.WildcardMin(wildcardVersion), false));
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
                                    "0", PrereleaseIdentifiers.Zero,
                                    "", ReadOnlyList<MetadataIdentifier>.Empty), false));
                    return null;
                case StandardOperator.Tilde:
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                    rightBound = rightBound.Min(new RightBoundedRange(
                        semver.With(minor: semver.Minor + 1, patch: 0, prerelease: PrereleaseIdentifiers.Zero),
                        false));
                    return null;
                case StandardOperator.Equals:
                case StandardOperator.None: // implied = (supports wildcard *)
                    wildcardVersion.RemoveOption(WildcardVersion.PrereleaseWildcard);
                    if (wildcardVersion != WildcardVersion.None && semver.IsPrerelease)
                        return ex ?? RangeError.PrereleaseNotSupportedWithWildcardVersion(segment.Source);
                    switch (wildcardVersion)
                    {
                        case WildcardVersion.None:
                            leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                            rightBound = rightBound.Min(new RightBoundedRange(semver, true));
                            return null;
                        case WildcardVersion.MajorMinorPatchWildcard:
                            // No further bound is places on the left and right bounds
                            return null;
                        case WildcardVersion.MinorPatchWildcard:
                            leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                            if (semver.Major == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                            rightBound = rightBound.Min(new RightBoundedRange(
                                new SemVersion(semver.Major + 1, 0, 0,
                                    "0", PrereleaseIdentifiers.Zero,
                                    "", ReadOnlyList<MetadataIdentifier>.Empty), false));
                            return null;
                        case WildcardVersion.PatchWildcard:
                            leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                            if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                            rightBound = rightBound.Min(new RightBoundedRange(
                                new SemVersion(semver.Major, semver.Minor + 1, 0,
                                    "0", PrereleaseIdentifiers.Zero,
                                    "", ReadOnlyList<MetadataIdentifier>.Empty), false));
                            return null;
                        default:
                            // This code should be unreachable
                            throw new ArgumentException($"DEBUG: Invalid {nameof(WildcardVersion)} value {wildcardVersion}");
                    }
                default:
                    // This code should be unreachable
                    throw new ArgumentException($"DEBUG: Invalid {nameof(StandardOperator)} value {@operator}");
            }
        }

        /// <summary>
        /// The minimum version that would be included in a wildcard range.
        /// </summary>
        private static SemVersion WildcardMin(this SemVersion semver, WildcardVersion wildcardVersion)
        {
            switch (wildcardVersion)
            {
                case WildcardVersion.MajorMinorPatchWildcard:
                case WildcardVersion.MinorPatchWildcard:
                case WildcardVersion.PatchWildcard:
                    // Wildcard places already filled with zeros
                    return semver.WithPrerelease(PrereleaseIdentifier.Zero);
                case WildcardVersion.None:
                    return semver;
                default:
                    // This code should be unreachable
                    throw new ArgumentException($"DEBUG: Invalid {nameof(WildcardVersion)} value {wildcardVersion}");

            }
        }

        private static SemVersion WildcardMax(this SemVersion semver, WildcardVersion wildcardVersion)
        {
            switch (wildcardVersion)
            {
                case WildcardVersion.MajorMinorPatchWildcard:
                case WildcardVersion.MinorPatchWildcard:
                case WildcardVersion.MinorWildcard:

                case WildcardVersion.None:
                    return semver;
                default:
                    // This code should be unreachable
                    throw new ArgumentException($"DEBUG: Invalid {nameof(WildcardVersion)} value {wildcardVersion}");

            }
        }

        private static SemVersion RoundDownPrerelease(this SemVersion semver)
            => semver.IsPrerelease ? semver : semver.WithPrerelease(PrereleaseIdentifier.Zero);

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
                || (opSegment.Length == 2
                    && opSegment[1] != '='
                    && !(opSegment[0] == '~' && opSegment[1] == '>')))
                return ex ?? RangeError.InvalidOperator(opSegment);

            var firstChar = opSegment[0];
            var isOrEqual = opSegment.Length == 2 && opSegment[1] == '=';
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
                    // '~>' operator is allowed by check for invalid above and matched by this
                    @operator = StandardOperator.Tilde;
                    return null;
                case '^' when !isOrEqual:
                    @operator = StandardOperator.Caret;
                    return null;
                default:
                    return ex ?? RangeError.InvalidOperator(opSegment);
            }
        }

        private static readonly SemVersionParsingOptions ParsingOptions
            = new SemVersionParsingOptions(true, false, true, c => c == 'x' || c == 'X' || c == '*');
    }
}
