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
                    return GreaterThan(semver, wildcardVersion, ex, ref leftBound);
                case StandardOperator.GreaterThanOrEqual:
                    if (wildcardVersion == WildcardVersion.MajorMinorPatchWildcard)
                        // No further bound is places on the left and right bounds
                        return null;
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    return null;
                case StandardOperator.LessThan:
                    return LessThan(semver, wildcardVersion, ex, ref rightBound);
                case StandardOperator.LessThanOrEqual:
                    rightBound = rightBound.Min(new RightBoundedRange(semver, true));
                    return null;
                case StandardOperator.Caret:
                    if (wildcardVersion == WildcardVersion.MajorMinorPatchWildcard)
                        // No further bound is places on the left and right bounds
                        return null;
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    int major = 0, minor = 0, patch = 0;
                    if (semver.Major != 0 || wildcardVersion == WildcardVersion.MinorPatchWildcard)
                    {
                        if (semver.Major == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                        major = semver.Major + 1;
                    }
                    else if (semver.Minor != 0 || wildcardVersion == WildcardVersion.PatchWildcard)
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
                    if (wildcardVersion == WildcardVersion.MajorMinorPatchWildcard)
                        // No further bound is places on the left and right bounds
                        return null;
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    if (wildcardVersion == WildcardVersion.MinorPatchWildcard)
                    {
                        if (semver.Major == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                        rightBound = rightBound.Min(new RightBoundedRange(
                            new SemVersion(semver.Major + 1, 0, 0, "0", PrereleaseIdentifiers.Zero, "",
                                ReadOnlyList<MetadataIdentifier>.Empty), false));
                    }
                    else
                    {
                        if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                        rightBound = rightBound.Min(new RightBoundedRange(
                            semver.With(minor: semver.Minor + 1, patch: 0, prerelease: PrereleaseIdentifiers.Zero),
                            false));
                    }
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
        /// The greater than operator taking into account the wildcard.
        /// </summary>
        private static Exception GreaterThan(
            SemVersion semver,
            WildcardVersion wildcardVersion,
            Exception ex,
            ref LeftBoundedRange leftBound)
        {
            bool inclusive;
            switch (wildcardVersion)
            {
                case WildcardVersion.MajorMinorPatchWildcard:
                    // No version matches
                    leftBound = leftBound.Max(new LeftBoundedRange(SemVersion.Max, false));
                    return null;
                case WildcardVersion.MinorPatchWildcard:
                    if (semver.Major == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                    semver = new SemVersion(semver.Major + 1, 0, 0);
                    inclusive = true;
                    break;
                case WildcardVersion.PatchWildcard:
                    if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(semver);
                    semver = new SemVersion(semver.Major, semver.Minor + 1, 0);
                    inclusive = true;
                    break;
                case WildcardVersion.None:
                    inclusive = false;
                    break;
                default:
                    // This code should be unreachable
                    throw new ArgumentException($"DEBUG: Invalid {nameof(WildcardVersion)} value {wildcardVersion}");
            }
            leftBound = leftBound.Max(new LeftBoundedRange(semver, inclusive));
            return null;
        }

        /// <summary>
        /// The less than operator taking into account the wildcard.
        /// </summary>
        private static Exception LessThan(
            SemVersion semver,
            WildcardVersion wildcardVersion,
            Exception ex,
            ref RightBoundedRange rightBound)
        {
            switch (wildcardVersion)
            {
                case WildcardVersion.MajorMinorPatchWildcard:
                case WildcardVersion.MinorPatchWildcard:
                case WildcardVersion.PatchWildcard:
                    // TODO what if this is already a prerelease version?
                    // Wildcard places already filled with zeros
                    semver = semver.WithPrerelease(PrereleaseIdentifier.Zero);
                    break;
                case WildcardVersion.None:
                    // No changes to version
                    break;
                default:
                    // This code should be unreachable
                    throw new ArgumentException($"DEBUG: Invalid {nameof(WildcardVersion)} value {wildcardVersion}");
            }

            rightBound = rightBound.Min(new RightBoundedRange(semver, false));
            return null;
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
