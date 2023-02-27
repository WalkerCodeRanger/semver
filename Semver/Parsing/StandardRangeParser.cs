using System;
using System.Collections.Generic;
using System.Linq;
using Semver.Ranges;
using Semver.Utility;

namespace Semver.Parsing
{
    internal static class StandardRangeParser
    {
        public static Exception? Parse(
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
                if (!(exception is null)) return exception;
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

            // Parse off leading whitespace
            var exception = GeneralRangeParser.ParseOptionalSpaces(ref segment, ex);
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
            ref bool includeAllPrerelease,
            Exception? ex,
            int maxLength,
            ref LeftBoundedRange leftBound,
            ref RightBoundedRange rightBound)
        {
            DebugChecks.IsNotEmpty(segment, nameof(segment));

            var exception = ParseOperator(ref segment, ex, out var @operator);
            if (exception != null) return exception;

            exception = GeneralRangeParser.ParseOptionalSpaces(ref segment, ex);
            if (exception != null) return exception;

            var versionSegment = segment;
            exception = GeneralRangeParser.ParseVersion(ref segment, rangeOptions, ParsingOptions, ex, maxLength,
                            out var semver, out var wildcardVersion);
            if (exception != null) return exception;
            DebugChecks.IsNotNull(semver, nameof(semver));

            if (@operator != StandardOperator.None && wildcardVersion != WildcardVersion.None)
                return ex ?? RangeError.WildcardNotSupportedWithOperator(segment.Source);

            exception = GeneralRangeParser.ParseOptionalSpaces(ref segment, ex);
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
                        if (semver.Major == int.MaxValue) return ex ?? RangeError.MaxVersion(versionSegment.AsSpan());
                        major = semver.Major + 1;
                    }
                    else if (semver.Minor != 0)
                    {
                        if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(versionSegment.AsSpan());
                        minor = semver.Minor + 1;
                    }
                    else
                    {
                        if (semver.Patch == int.MaxValue) return ex ?? RangeError.MaxVersion(versionSegment.AsSpan());
                        patch = semver.Patch + 1;
                    }

                    rightBound = rightBound.Min(new RightBoundedRange(new SemVersion(
                                    major, minor, patch,
                                    "0", PrereleaseIdentifiers.Zero,
                                    "", ReadOnlyList<MetadataIdentifier>.Empty), false));
                    return null;
                case StandardOperator.Tilde:
                    leftBound = leftBound.Max(new LeftBoundedRange(semver, true));
                    if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(versionSegment.AsSpan());
                    rightBound = rightBound.Min(new RightBoundedRange(
                        semver.With(minor: semver.Minor + 1, patch: 0, prerelease: PrereleaseIdentifiers.Zero),
                        false));
                    return null;
                case StandardOperator.None: // implied = (supports wildcard *)
                    var prereleaseWildcard = wildcardVersion.HasOption(WildcardVersion.PrereleaseWildcard);
                    includeAllPrerelease |= prereleaseWildcard;
                    wildcardVersion.RemoveOption(WildcardVersion.PrereleaseWildcard);
                    if (wildcardVersion != WildcardVersion.None && semver.IsPrerelease)
                        return ex ?? RangeError.PrereleaseNotSupportedWithWildcardVersion(segment.Source);
                    switch (wildcardVersion)
                    {
                        case WildcardVersion.None:
                            leftBound = leftBound.Max(WildcardLowerBound(semver, prereleaseWildcard));
                            return PrereleaseWildcardUpperBound(ex, ref rightBound, versionSegment, semver, prereleaseWildcard);
                        case WildcardVersion.MajorMinorPatchWildcard:
                            // No further bound is places on the left and right bounds
                            return null;
                        case WildcardVersion.MinorPatchWildcard:
                            leftBound = leftBound.Max(WildcardLowerBound(semver, prereleaseWildcard));
                            if (semver.Major == int.MaxValue) return ex ?? RangeError.MaxVersion(versionSegment.AsSpan());
                            rightBound = rightBound.Min(new RightBoundedRange(
                                new SemVersion(semver.Major + 1, 0, 0,
                                    "0", PrereleaseIdentifiers.Zero,
                                    "", ReadOnlyList<MetadataIdentifier>.Empty), false));
                            return null;
                        case WildcardVersion.PatchWildcard:
                            leftBound = leftBound.Max(WildcardLowerBound(semver, prereleaseWildcard));
                            if (semver.Minor == int.MaxValue) return ex ?? RangeError.MaxVersion(versionSegment.AsSpan());
                            rightBound = rightBound.Min(new RightBoundedRange(
                                new SemVersion(semver.Major, semver.Minor + 1, 0,
                                    "0", PrereleaseIdentifiers.Zero,
                                    "", ReadOnlyList<MetadataIdentifier>.Empty), false));
                            return null;
                        default:
                            // dotcover disable next line
                            throw Unreachable.InvalidEnum(wildcardVersion);
                    }
                default:
                    // dotcover disable next line
                    throw Unreachable.InvalidEnum(@operator);
            }
        }

        private static LeftBoundedRange WildcardLowerBound(SemVersion semver, bool prereleaseWildcard)
        {
            if (prereleaseWildcard)
                semver = semver.IsPrerelease
                    ? semver.WithPrerelease(semver.PrereleaseIdentifiers.Concat(PrereleaseIdentifiers.Zero))
                    : semver.WithPrerelease(PrereleaseIdentifier.Zero);
            return new LeftBoundedRange(semver, true);
        }

        private static Exception? PrereleaseWildcardUpperBound(
            Exception? ex,
            ref RightBoundedRange rightBound,
            StringSegment versionSegment,
            SemVersion semver,
            bool prereleaseWildcard)
        {
            var inclusive = false;
            if (prereleaseWildcard)
            {
                if (semver.IsPrerelease)
                    semver = new SemVersion(semver.Major, semver.Minor, semver.Patch,
                        PrereleaseWildcardUpperBoundPrereleaseIdentifiers(semver.PrereleaseIdentifiers));
                else
                {
                    if (semver.Patch == int.MaxValue) return ex ?? RangeError.MaxVersion(versionSegment.AsSpan());
                    semver = new SemVersion(semver.Major, semver.Minor, semver.Patch + 1,
                        "0", PrereleaseIdentifiers.Zero, "", ReadOnlyList<MetadataIdentifier>.Empty);
                }
            }
            else
                inclusive = true;

            rightBound = rightBound.Min(new RightBoundedRange(semver, inclusive));
            return null;
        }

        private static IEnumerable<PrereleaseIdentifier> PrereleaseWildcardUpperBoundPrereleaseIdentifiers(
            IReadOnlyList<PrereleaseIdentifier> identifiers)
        {
            for (int i = 0; i < identifiers.Count - 1; i++)
                yield return identifiers[i];

            var lastIdentifier = identifiers[^1];

            yield return lastIdentifier.NextIdentifier();
        }

        private static Exception? ParseOperator(
            ref StringSegment segment, Exception? ex, out StandardOperator @operator)
        {
            var end = 0;
            while (end < segment.Length && GeneralRangeParser.IsPossibleOperatorChar(segment[end], SemVersionRangeOptions.Strict))
                end++;
            var opSpan = segment.AsSpan()[..end];
            segment = segment.Subsegment(end);

            if (opSpan.Length == 0)
            {
                @operator = StandardOperator.None;
                return null;
            }

            // Assign invalid once so it doesn't have to be done any time parse fails
            @operator = 0;
            if (opSpan.Length > 2
                || (opSpan.Length == 2 && opSpan[1] != '='))
                return ex ?? RangeError.InvalidOperator(opSpan);

            var firstChar = opSpan[0];
            var isOrEqual = opSpan.Length == 2; // Already checked for second char != '='
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
                    return ex ?? RangeError.InvalidOperator(opSpan);
            }
        }

        private static readonly SemVersionParsingOptions ParsingOptions
            = new SemVersionParsingOptions(true, true, false, c => c == '*');
    }
}
