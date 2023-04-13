using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Extensions.Primitives;
using Semver.Ranges;
using Semver.Utility;

namespace Semver.Parsing
{
    internal static class NpmRangeParser
    {
        private const SemVersionRangeOptions StandardRangeOptions
            = SemVersionRangeOptions.AllowLowerV
              | SemVersionRangeOptions.AllowMetadata
              | SemVersionRangeOptions.OptionalMinorPatch;

        public static Exception? Parse(
            string? range,
            bool includeAllPrerelease,
            Exception? ex,
            int maxLength,
            out SemVersionRange? semverRange)
        {
            var options = StandardRangeOptions;
            if (includeAllPrerelease)
                options |= SemVersionRangeOptions.IncludeAllPrerelease;
            return Parse(range, options, ex, maxLength, out semverRange);
        }

        private static Exception? Parse(
            string? range,
            SemVersionRangeOptions rangeOptions,
            Exception? ex,
            int maxLength,
            out SemVersionRange? semverRange)
        {
            DebugChecks.IsValid(rangeOptions, nameof(rangeOptions));
            DebugChecks.IsValidMaxLength(maxLength, nameof(maxLength));

            // Assign null once so it doesn't have to be done any time parse fails
            semverRange = null;

            // Note: this method relies on the fact that the null coalescing operator `??`
            // is short circuiting to avoid constructing exceptions and exception messages
            // when a non-null exception is passed in.

            if (range is null) return ex ?? new ArgumentNullException(nameof(range));
            if (range.Length > maxLength) return ex ?? RangeError.TooLong(range, maxLength);

            var unbrokenRanges = new List<UnbrokenSemVersionRange>(GeneralRangeParser.CountSplitOnOrOperator(range));
            foreach (var segment in GeneralRangeParser.SplitOnOrOperator(range))
            {
                var exception = ParseUnbrokenRange(segment, rangeOptions, ex, maxLength, out var unbrokenRange);
                if (exception is not null) return exception;
                DebugChecks.IsNotNull(unbrokenRange, nameof(unbrokenRange));

                unbrokenRanges.Add(unbrokenRange);
            }

            semverRange = SemVersionRange.Create(unbrokenRanges);
            return null;
        }

        private static Exception? ParseUnbrokenRange(
            StringSegment segment,
            SemVersionRangeOptions rangeOptions,
            Exception? ex,
            int maxLength,
            out UnbrokenSemVersionRange? unbrokenRange)
        {
            // Assign null once so it doesn't have to be done any time parse fails
            unbrokenRange = null;

            var includeAllPrerelease = rangeOptions.HasOption(SemVersionRangeOptions.IncludeAllPrerelease);

            var start = LeftBoundedRange.Unbounded;
            var end = RightBoundedRange.Unbounded;

            // Try to split before removing leading whitespace because of invalid ranges like ' - 2.0.0'
            if (TrySplitOnHyphenRangeSeparator(segment, out var segment1, out var segment2))
            {
                var exception = ParseHyphenRange(segment1, segment2, rangeOptions, includeAllPrerelease, ex,
                    maxLength, ref start, ref end);
                if (exception != null) return exception;
            }
            else
            {
                // Parse off leading whitespace
                GeneralRangeParser.ParseOptionalWhitespace(ref segment);

                // Handle empty string ranges
                if (segment.IsEmpty())
                {
                    unbrokenRange = includeAllPrerelease
                        ? UnbrokenSemVersionRange.All
                        : UnbrokenSemVersionRange.AllRelease;
                    return null;
                }

                while (!segment.IsEmpty())
                {
                    var exception = ParseComparison(ref segment, rangeOptions, includeAllPrerelease, ex, maxLength,
                        ref start, ref end);
                    if (exception != null) return exception;
                }
            }

            unbrokenRange = UnbrokenSemVersionRange.Create(start, end, includeAllPrerelease);
            return null;
        }

        private static bool TrySplitOnHyphenRangeSeparator(
            StringSegment segment,
            out StringSegment segment1,
            out StringSegment segment2)
        {
            var searchLength = segment.Length - 1;
            int start = 1;
            int i;
            while (start < segment.Length && (i = segment.IndexOf('-', start, searchLength - start)) >= 0)
            {
                var indexBefore = i - 1;
                var indexAfter = i + 1;

                if (char.IsWhiteSpace(segment[indexBefore]) && char.IsWhiteSpace(segment[indexAfter]))
                {
                    // Split in two before and after the whitespace around the hyphen
                    segment1 = segment.Subsegment(0, indexBefore);
                    segment2 = segment.Subsegment(indexAfter + 1);
                    return true;
                }

                start = indexAfter;
            }
            // No hyphen, just don't split but have to set the out params to something
            segment1 = segment2 = segment;
            return false;
        }

        private static Exception? ParseHyphenRange(
            StringSegment beforeHyphenSegment,
            StringSegment afterHyphenSegment,
            SemVersionRangeOptions rangeOptions,
            bool includeAllPrerelease,
            Exception? ex,
            int maxLength,
            ref LeftBoundedRange leftBound,
            ref RightBoundedRange rightBound)
        {
            var exception = ParseHyphenSegment(beforeHyphenSegment, rangeOptions, ex, maxLength,
                out var semver1, out var wildcardVersion1);
            if (exception != null) return exception;
            DebugChecks.IsNotNull(semver1, nameof(semver1));

            exception = ParseHyphenSegment(afterHyphenSegment, rangeOptions, ex, maxLength,
                out var semver2, out var wildcardVersion2);
            if (exception != null) return exception;
            DebugChecks.IsNotNull(semver2, nameof(semver2));

            WildcardLowerBound(includeAllPrerelease, ref leftBound, semver1, wildcardVersion1);
            return WildcardUpperBound(ex, ref rightBound, afterHyphenSegment, semver2, wildcardVersion2);
        }

        private static Exception? ParseHyphenSegment(
            StringSegment segment,
            SemVersionRangeOptions rangeOptions,
            Exception? ex,
            int maxLength,
            out SemVersion? semver,
            out WildcardVersion wildcardVersion)
        {
            // Assign null once so it doesn't have to be done any time parse fails
            semver = null;
            wildcardVersion = WildcardVersion.None;

            // Parse off leading whitespace from before hyphen segment
            GeneralRangeParser.ParseOptionalWhitespace(ref segment);

            // Check for missing version number
            if (segment.Length == 0)
                return ex ?? RangeError.MissingVersionInHyphenRange(segment.Buffer!);

            // Check for invalid chars, like an operator, before the version
            if (!GeneralRangeParser.IsPossibleVersionChar(segment[0], rangeOptions))
                return ex ?? RangeError.UnexpectedInHyphenRange(segment[0].ToString());

            // Now parse the actual version number
            var exception = ParseNpmVersion(ref segment, rangeOptions, ex, maxLength,
                out semver, out wildcardVersion);
            if (exception != null) return exception;

            // Parse off trailing whitespace from before hyphen segment
            GeneralRangeParser.ParseOptionalWhitespace(ref segment);

            if (segment.Length != 0) return ex ?? RangeError.UnexpectedInHyphenRange(segment.ToString());

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
        private static Exception? ParseComparison(
            ref StringSegment segment,
            SemVersionRangeOptions rangeOptions,
            bool includeAllPrerelease,
            Exception? ex,
            int maxLength,
            ref LeftBoundedRange leftBound,
            ref RightBoundedRange rightBound)
        {
            DebugChecks.IsNotEmpty(segment, nameof(segment));

            var exception = ParseOperator(ref segment, ex, out var @operator);
            if (exception != null) return exception;

            GeneralRangeParser.ParseOptionalWhitespace(ref segment);

            var versionSegment = segment;
            exception = ParseNpmVersion(ref segment, rangeOptions, ex, maxLength,
                            out var semver, out var wildcardVersion);
            if (exception != null) return exception;
            DebugChecks.IsNotNull(semver, nameof(semver));

            GeneralRangeParser.ParseOptionalWhitespace(ref segment);

            switch (@operator)
            {
                case StandardOperator.GreaterThan:
                    return GreaterThan(includeAllPrerelease, ex, ref leftBound, versionSegment, semver, wildcardVersion);
                case StandardOperator.GreaterThanOrEqual:
                    WildcardLowerBound(includeAllPrerelease, ref leftBound, semver, wildcardVersion);
                    return null;
                case StandardOperator.LessThan:
                    return LessThan(ref rightBound, semver, wildcardVersion);
                case StandardOperator.LessThanOrEqual:
                    return WildcardUpperBound(ex, ref rightBound, versionSegment, semver, wildcardVersion);
                case StandardOperator.Caret:
                    if (wildcardVersion == WildcardVersion.MajorMinorPatchWildcard)
                        // No further bound is places on the left and right bounds
                        return null;
                    WildcardLowerBound(includeAllPrerelease, ref leftBound, semver, wildcardVersion);
                    BigInteger major = 0, minor = 0, patch = 0;
                    if (semver.Major != 0 || wildcardVersion == WildcardVersion.MinorPatchWildcard)
                        major = semver.Major + 1;
                    else if (semver.Minor != 0 || wildcardVersion == WildcardVersion.PatchWildcard)
                        minor = semver.Minor + 1;
                    else
                        patch = semver.Patch + 1;

                    rightBound = rightBound.Min(new RightBoundedRange(new SemVersion(
                                    major, minor, patch,
                                    "0", PrereleaseIdentifiers.Zero,
                                    "", ReadOnlyList<MetadataIdentifier>.Empty), false));
                    return null;
                case StandardOperator.Tilde:
                    if (wildcardVersion == WildcardVersion.MajorMinorPatchWildcard)
                        // No further bound is places on the left and right bounds
                        return null;
                    WildcardLowerBound(includeAllPrerelease, ref leftBound, semver, wildcardVersion);
                    if (wildcardVersion == WildcardVersion.MinorPatchWildcard)
                    {
                        rightBound = rightBound.Min(new RightBoundedRange(
                            new SemVersion(semver.Major + 1, 0, 0, "0", PrereleaseIdentifiers.Zero, "",
                                ReadOnlyList<MetadataIdentifier>.Empty), false));
                    }
                    else
                    {
                        rightBound = rightBound.Min(new RightBoundedRange(
                            semver.With(minor: semver.Minor + 1, patch: 0, prerelease: PrereleaseIdentifiers.Zero),
                            false));
                    }
                    return null;
                case StandardOperator.Equals:
                case StandardOperator.None: // implied =
                    WildcardLowerBound(includeAllPrerelease, ref leftBound, semver, wildcardVersion);
                    return WildcardUpperBound(ex, ref rightBound, versionSegment, semver, wildcardVersion);
                default:
                    // dotcover disable next line
                    throw Unreachable.InvalidEnum(@operator);
            }
        }

        public static Exception? ParseNpmVersion(
            ref StringSegment segment,
            SemVersionRangeOptions rangeOptions,
            Exception? ex,
            int maxLength,
            out SemVersion? semver,
            out WildcardVersion wildcardVersion)
        {
            var exception = GeneralRangeParser.ParseVersion(ref segment, rangeOptions, ParsingOptions, ex, maxLength,
                out semver, out wildcardVersion);
            if (exception != null) return exception;
            DebugChecks.IsNotNull(semver, nameof(semver));
            if (wildcardVersion != WildcardVersion.None && semver.IsPrerelease)
                return ex ?? RangeError.PrereleaseNotSupportedWithWildcardVersion(segment.Buffer!);
            // Remove the metadata the npm ranges allow (note we always allow metadata even though
            // npm rejects it for partial versions)
            semver = semver.WithoutMetadata();
            return null;
        }

        /// <summary>
        /// The greater than operator taking into account the wildcard.
        /// </summary>
        private static Exception? GreaterThan(
            bool includeAllPrerelease,
            Exception? ex,
            ref LeftBoundedRange leftBound,
            StringSegment versionSegment,
            SemVersion semver,
            WildcardVersion wildcardVersion)
        {
            DebugChecks.IsNotWildcardVersionWithPrerelease(wildcardVersion, semver);

            bool inclusive;
            switch (wildcardVersion)
            {
                case WildcardVersion.MajorMinorPatchWildcard:
                    // No version matches
                    leftBound = leftBound.Max(new LeftBoundedRange(SemVersion.Max, false));
                    return null;
                case WildcardVersion.MinorPatchWildcard:
                {
                    var prereleaseString = includeAllPrerelease ? "0" : "";
                    var prerelease = includeAllPrerelease ? PrereleaseIdentifiers.Zero : ReadOnlyList<PrereleaseIdentifier>.Empty;
                    semver = new SemVersion(semver.Major + 1, 0, 0, prereleaseString, prerelease,
                        "", ReadOnlyList<MetadataIdentifier>.Empty);
                    inclusive = true;
                    break;
                }
                case WildcardVersion.PatchWildcard:
                {
                    var prereleaseString = includeAllPrerelease ? "0" : "";
                    var prerelease = includeAllPrerelease ? PrereleaseIdentifiers.Zero : ReadOnlyList<PrereleaseIdentifier>.Empty;
                    semver = new SemVersion(semver.Major, semver.Minor + 1, 0, prereleaseString, prerelease,
                        "", ReadOnlyList<MetadataIdentifier>.Empty);
                    inclusive = true;
                    break;
                }
                case WildcardVersion.None:
                    inclusive = false;
                    break;
                default:
                    // dotcover disable next line
                    throw Unreachable.InvalidEnum(wildcardVersion);
            }
            leftBound = leftBound.Max(new LeftBoundedRange(semver, inclusive));
            return null;
        }

        /// <summary>
        /// The less than operator taking into account the wildcard.
        /// </summary>
        private static Exception? LessThan(
            ref RightBoundedRange rightBound,
            SemVersion semver,
            WildcardVersion wildcardVersion)
        {
            DebugChecks.IsNotWildcardVersionWithPrerelease(wildcardVersion, semver);

            switch (wildcardVersion)
            {
                case WildcardVersion.MajorMinorPatchWildcard:
                case WildcardVersion.MinorPatchWildcard:
                case WildcardVersion.PatchWildcard:
                    // Wildcard places already filled with zeros
                    semver = semver.WithPrerelease(PrereleaseIdentifier.Zero);
                    break;
                case WildcardVersion.None:
                    // No changes to version
                    break;
                default:
                    // dotcover disable next line
                    throw Unreachable.InvalidEnum(wildcardVersion);
            }

            rightBound = rightBound.Min(new RightBoundedRange(semver, false));
            return null;
        }

        private static void WildcardLowerBound(
            bool includeAllPrerelease,
            ref LeftBoundedRange leftBound,
            SemVersion semver,
            WildcardVersion wildcardVersion)
        {
            DebugChecks.IsNotWildcardVersionWithPrerelease(wildcardVersion, semver);

            switch (wildcardVersion)
            {
                case WildcardVersion.MajorMinorPatchWildcard:
                    // No further bound placed
                    return;
                case WildcardVersion.MinorPatchWildcard:
                case WildcardVersion.PatchWildcard:
                    if (includeAllPrerelease)
                        semver = semver.WithPrerelease(PrereleaseIdentifier.Zero);
                    break;
                case WildcardVersion.None:
                    // No changes to version
                    break;
                default:
                    // dotcover disable next line
                    throw Unreachable.InvalidEnum(wildcardVersion);
            }

            leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
        }

        private static Exception? WildcardUpperBound(
            Exception? ex,
            ref RightBoundedRange rightBound,
            StringSegment versionSegment,
            SemVersion semver,
            WildcardVersion wildcardVersion)
        {
            DebugChecks.IsNotWildcardVersionWithPrerelease(wildcardVersion, semver);

            switch (wildcardVersion)
            {
                case WildcardVersion.None:
                    rightBound = rightBound.Min(new RightBoundedRange(semver, true));
                    return null;
                case WildcardVersion.MajorMinorPatchWildcard:
                    // No further bounds placed
                    return null;
                case WildcardVersion.MinorPatchWildcard:
                    rightBound = rightBound.Min(new RightBoundedRange(
                        new SemVersion(semver.Major + 1, 0, 0, "0", PrereleaseIdentifiers.Zero, "",
                            ReadOnlyList<MetadataIdentifier>.Empty), false));
                    return null;
                case WildcardVersion.PatchWildcard:
                    rightBound = rightBound.Min(new RightBoundedRange(
                        new SemVersion(semver.Major, semver.Minor + 1, 0, "0", PrereleaseIdentifiers.Zero, "",
                            ReadOnlyList<MetadataIdentifier>.Empty), false));
                    return null;
                default:
                    // dotcover disable next line
                    throw Unreachable.InvalidEnum(wildcardVersion);
            }
        }

        private static Exception? ParseOperator(
            ref StringSegment segment, Exception? ex, out StandardOperator @operator)
        {
            var end = 0;
            while (end < segment.Length && GeneralRangeParser.IsPossibleOperatorChar(segment[end], SemVersionRangeOptions.AllowMetadata)) end++;
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
