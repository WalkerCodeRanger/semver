using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Semver.Comparers;
using Semver.Utility;

namespace Semver.Ranges
{
    public sealed class UnbrokenSemVersionRange : IEquatable<UnbrokenSemVersionRange>
    {
        /// <summary>
        /// A standard representation for the empty range that contains no versions.
        /// </summary>
        /// <remarks><para>There are an infinite number of ways to represent the empty range. Any range
        /// where the start is greater than the end or where start equals end but one is not
        /// inclusive would be empty.
        /// See https://en.wikipedia.org/wiki/Interval_(mathematics)#Classification_of_intervals</para>
        ///
        /// <para>Since all <see cref="UnbrokenSemVersionRange"/> objects have a <see cref="Start"/> and
        /// <see cref="End"/>, the only unique empty version is the one whose start is the max
        /// version and end is the min version.</para>
        /// </remarks>
        public static UnbrokenSemVersionRange Empty { get; }
            = new UnbrokenSemVersionRange(new LeftBoundedRange(SemVersion.Max, false),
                new RightBoundedRange(SemVersion.Min, false), false, false);
        public static UnbrokenSemVersionRange AllRelease { get; } = AtMost(SemVersion.Max);
        public static UnbrokenSemVersionRange All { get; } = AtMost(SemVersion.Max, true);

        #region Static Factory Methods
        public static UnbrokenSemVersionRange Equals(SemVersion version)
            => Create(Validate(version, nameof(version)), true, version, true, false);

        public static UnbrokenSemVersionRange GreaterThan(SemVersion version, bool includeAllPrerelease = false)
            => Create(Validate(version, nameof(version)), false, SemVersion.Max, true, includeAllPrerelease);

        public static UnbrokenSemVersionRange AtLeast(SemVersion version, bool includeAllPrerelease = false)
            => Create(Validate(version, nameof(version)), true, SemVersion.Max, true, includeAllPrerelease);

        public static UnbrokenSemVersionRange LessThan(SemVersion version, bool includeAllPrerelease = false)
            => Create(null, false, Validate(version, nameof(version)), false, includeAllPrerelease);

        public static UnbrokenSemVersionRange AtMost(SemVersion version, bool includeAllPrerelease = false)
            => Create(null, false, Validate(version, nameof(version)), true, includeAllPrerelease);

        public static UnbrokenSemVersionRange Inclusive(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(Validate(start, nameof(start)), true,
                Validate(end, nameof(end)), true, includeAllPrerelease);

        public static UnbrokenSemVersionRange InclusiveOfStart(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(Validate(start, nameof(start)), true,
                Validate(end, nameof(end)), false, includeAllPrerelease);

        public static UnbrokenSemVersionRange InclusiveOfEnd(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(Validate(start, nameof(start)), false,
                Validate(end, nameof(end)), true, includeAllPrerelease);

        public static UnbrokenSemVersionRange Exclusive(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(Validate(start, nameof(start)), false,
                Validate(end, nameof(end)), false, includeAllPrerelease);
        #endregion

        // TODO support parsing unbroken ranges?

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static UnbrokenSemVersionRange Create(
            SemVersion startVersion,
            bool startInclusive,
            SemVersion endVersion,
            bool endInclusive,
            bool includeAllPrerelease)
        {
            var start = new LeftBoundedRange(startVersion, startInclusive);
            var end = new RightBoundedRange(endVersion, endInclusive);
            return Create(start, end, includeAllPrerelease);
        }

        internal static UnbrokenSemVersionRange Create(
            LeftBoundedRange start,
            RightBoundedRange end,
            bool includeAllPrerelease)
        {
            // Always return the same empty range
            if (IsEmpty(start, end, includeAllPrerelease)) return Empty;

            var allPrereleaseCoveredByEnds = false;

            // Equals ranges include all prerelease if they are prerelease
            if (start.Version == end.Version)
                allPrereleaseCoveredByEnds = includeAllPrerelease = start.Version.IsPrerelease;
            // Some ranges have all the prerelease versions in them covered by the bounds
            else if (!(start.Version is null) && (start.IncludesPrerelease || end.IncludesPrerelease))
            {
                if (start.Version.MajorMinorPatchEquals(end.Version))
                    allPrereleaseCoveredByEnds = true;
                else if ((end.IncludesPrerelease || end.Version.PrereleaseIsZero)
                         && start.Version.Major == end.Version.Major && start.Version.Minor == end.Version.Minor
                         // Subtract instead of add to avoid overflow
                         && start.Version.Patch == end.Version.Patch - 1)
                    allPrereleaseCoveredByEnds = true;
            }

            return new UnbrokenSemVersionRange(start, end, includeAllPrerelease, allPrereleaseCoveredByEnds);
        }

        private UnbrokenSemVersionRange(
            LeftBoundedRange leftBound,
            RightBoundedRange rightBound,
            bool includeAllPrerelease,
            bool allPrereleaseCoveredByEnds)
        {
            LeftBound = leftBound;
            RightBound = rightBound;
            IncludeAllPrerelease = includeAllPrerelease | allPrereleaseCoveredByEnds;
            this.allPrereleaseCoveredByEnds = allPrereleaseCoveredByEnds;
        }

        internal readonly LeftBoundedRange LeftBound;
        internal readonly RightBoundedRange RightBound;
        /// <summary>
        /// If this <see cref="IncludeAllPrerelease"/> and those prerelease versions are entirely
        /// covered by the left and right bounds so that effectively, it doesn't need to include all
        /// prerelease.
        /// </summary>
        private readonly bool allPrereleaseCoveredByEnds;
        private string toStringCache;

        public SemVersion Start => LeftBound.Version;
        public bool StartInclusive => LeftBound.Inclusive;
        public SemVersion End => RightBound.Version;
        public bool EndInclusive => RightBound.Inclusive;
        public bool IncludeAllPrerelease { get; }

        public bool Contains(SemVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            if (!LeftBound.Contains(version) || !RightBound.Contains(version)) return false;

            if (IncludeAllPrerelease || !version.IsPrerelease) return true;

            // Prerelease versions must match either the start or end
            return Start?.IsPrerelease == true && version.MajorMinorPatchEquals(Start)
                   || End.IsPrerelease && version.MajorMinorPatchEquals(End);
        }

        public static implicit operator Predicate<SemVersion>(UnbrokenSemVersionRange range)
            => range.Contains;

        #region Equality
        public bool Equals(UnbrokenSemVersionRange other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return LeftBound.Equals(other.LeftBound)
                   && RightBound.Equals(other.RightBound)
                   && IncludeAllPrerelease == other.IncludeAllPrerelease;
        }

        public override bool Equals(object obj)
            => obj is UnbrokenSemVersionRange other && Equals(other);

        public override int GetHashCode()
            => CombinedHashCode.Create(LeftBound, RightBound, IncludeAllPrerelease);

        public static bool operator ==(UnbrokenSemVersionRange left, UnbrokenSemVersionRange right)
            => Equals(left, right);

        public static bool operator !=(UnbrokenSemVersionRange left, UnbrokenSemVersionRange right)
            => !Equals(left, right);
        #endregion

        public override string ToString()
            => toStringCache ?? (toStringCache = ToStringInternal());

        private string ToStringInternal()
        {
            if (this == Empty)
                // Must combine with including prerelease and still be empty
                return "<0.0.0-0";

            // Simple Equals ranges
            if (LeftBound.Inclusive && RightBound.Inclusive && SemVersion.Equals(Start, End))
                return Start.ToString();

            var includesPrereleaseNotCoveredByEnds = IncludeAllPrerelease && !allPrereleaseCoveredByEnds;

            // All versions ranges
            var leftUnbounded = LeftBound == LeftBoundedRange.Unbounded;
            var rightUnbounded = RightBound == RightBoundedRange.Unbounded;
            if (leftUnbounded && rightUnbounded)
                return includesPrereleaseNotCoveredByEnds ? "*-*" : "*";

            if (TryToSpecialString(includesPrereleaseNotCoveredByEnds, out var result))
                return result;

            string range;
            if (leftUnbounded)
                range = RightBound.ToString();
            else if (rightUnbounded)
                range = LeftBound.ToString();
            else
                range = $"{LeftBound} {RightBound}";

            return includesPrereleaseNotCoveredByEnds ? "*-* " + range : range;
        }

        private bool TryToSpecialString(bool includesPrereleaseNotCoveredByEnds, out string result)
        {
            // Most special ranges follow the pattern '>=X.Y.Z <P.Q.R-0'
            if (LeftBound.Inclusive && !RightBound.Inclusive && End.PrereleaseIsZero)
            {
                // Wildcard Ranges like 2.*, 2.*-*, 2.3.*, and 2.3.*-*
                if (Start.Patch == 0 && End.Patch == 0 && (!Start.IsPrerelease || Start.PrereleaseIsZero))
                {
                    // Subtract instead of add to avoid overflow
                    if (Start.Major == End.Major && Start.Minor == End.Minor - 1)
                        // Wildcard patch
                        result = $"{Start.Major}.{Start.Minor}.*";
                    // Subtract instead of add to avoid overflow
                    else if (Start.Major == End.Major - 1 && Start.Minor == 0 && End.Minor == 0)
                        // Wildcard minor
                        result = $"{Start.Major}.*";
                    else
                        goto tilde;

                    if (!includesPrereleaseNotCoveredByEnds)
                        return true;

                    result = Start.PrereleaseIsZero ? result + "-*" : "*-* " + result;
                    return true;
                }

                // Wildcard ranges like 2.1.4-* follow the pattern '>=X.Y.Z-0 <X.Y.(Z+1)-0'
                if (Start.PrereleaseIsZero
                    && Start.Major == End.Major && Start.Minor == End.Minor
                    // Subtract instead of add to avoid overflow
                    && Start.Patch == End.Patch - 1)
                {
                    result = $"{Start.Major}.{Start.Minor}.{Start.Patch}-*";
                    return true;
                }

            tilde:
                // Tilde ranges like ~1.2.3, and ~1.2.3-rc
                if (Start.Major == End.Major
                    // Subtract instead of add to avoid overflow
                    && Start.Minor == End.Minor - 1 && End.Patch == 0)
                {
                    result = (includesPrereleaseNotCoveredByEnds ? "*-* ~" : "~") + Start;
                    return true;
                }

                // Note: caret ranges like ^0.1.2 and ^0.2.3-rc are converted to tilde ranges

                if (Start.Major != 0)
                {
                    // Caret ranges like ^1.2.3 and ^1.2.3-rc
                    // Subtract instead of add to avoid overflow
                    if (Start.Major == End.Major - 1 && End.Minor == 0 && End.Patch == 0)
                    {
                        result = (includesPrereleaseNotCoveredByEnds ? "*-* ^" : "^") + Start;
                        return true;
                    }
                }
                else if (End.Major == 0
                         && Start.Minor == 0 && End.Minor == 0
                         && Start.Patch == End.Patch - 1)
                {
                    // Caret ranges like ^0.0.2 and ^0.0.2-rc
                    result = (includesPrereleaseNotCoveredByEnds ? "*-* ^" : "^") + Start;
                    return true;
                }
            }

            // Assign null once
            result = null;

            // Wildcards with prerelease follow the pattern >=X.Y.Z-φ.α.0 <X.Y.Z-φ.β
            if (LeftBound.Inclusive && !RightBound.Inclusive
                && LeftBound.Version?.MajorMinorPatchEquals(RightBound.Version) == true
                && LeftBound.Version.IsPrerelease && RightBound.Version.IsPrerelease)
            {
                var leftPrerelease = LeftBound.Version.PrereleaseIdentifiers;
                var rightPrerelease = RightBound.Version.PrereleaseIdentifiers;
                if (leftPrerelease.Count < 2
                    || leftPrerelease[leftPrerelease.Count-1] != PrereleaseIdentifier.Zero
                    || leftPrerelease.Count - 1 != rightPrerelease.Count)
                    return false;

                // But they must be equal in prerelease up to the correct point
                for (int i = 0; i < leftPrerelease.Count-2; i++)
                    if (leftPrerelease[i] != rightPrerelease[i])
                        return false;

                // And the prerelease identifiers must have the correct relationship
                if (leftPrerelease[leftPrerelease.Count - 2].NextIdentifier()
                    != rightPrerelease[rightPrerelease.Count - 1])
                    return false;

                var originalPrerelease = string.Join(".", leftPrerelease.Take(leftPrerelease.Count-1));
                result = $"{Start.Major}.{Start.Minor}.{Start.Patch}-{originalPrerelease}.*";
                return true;
            }

            return false;
        }

        internal bool Overlaps(UnbrokenSemVersionRange other)
        {
            // see https://stackoverflow.com/a/3269471/268898
            return LeftBound.CompareTo(other.RightBound) <= 0
                   && other.LeftBound.CompareTo(RightBound) <= 0;
        }

        internal bool OverlapsOrAbuts(UnbrokenSemVersionRange other)
        {
            if (Overlaps(other)) return true;

            // The empty range is never considered to abut anything even though its "ends" might
            // actually abut things.
            if (Empty.Equals(this) || Empty.Equals(other)) return false;

            // To check abutting, we just need to put them in the right order and check the gab between them
            var isLessThanOrEqual = UnbrokenSemVersionRangeComparer.Instance.Compare(this, other) <= 0;
            var leftRangeEnd = (isLessThanOrEqual ? this : other).RightBound;
            var rightRangeStart = (isLessThanOrEqual ? other : this).LeftBound;

            // Note: the case where a major, minor, or patch version is at max value and so is just
            // less than the next prerelease version is being ignored.

            // If one of the ends is inclusive then it is sufficient for them to be the same version.
            if ((leftRangeEnd.Inclusive || rightRangeStart.Inclusive)
                && leftRangeEnd.Version.Equals(rightRangeStart.Version))
                return true;

            // But they could also abut if the prerelease versions between them are being excluded.
            if (IncludeAllPrerelease || other.IncludeAllPrerelease
                || leftRangeEnd.IncludesPrerelease || rightRangeStart.IncludesPrerelease)
                return false;

            return rightRangeStart.Inclusive
                   && leftRangeEnd.Version.MajorMinorPatchEquals(rightRangeStart.Version);
        }

        /// <summary>
        /// Whether this range contains the other. For this to be the case, it must contain all the
        /// versions accounting for which prerelease versions are in each range.
        /// </summary>
        internal bool Contains(UnbrokenSemVersionRange other)
        {
            // The empty set is a subset of every other set, even itself
            if (other == Empty) return true;

            // It contains prerelease we don't
            if (other.IncludeAllPrerelease && !IncludeAllPrerelease) return false;

            // If our bounds don't contain the other bounds, there is no containment
            if (LeftBound.CompareTo(other.LeftBound) > 0
                || other.RightBound.CompareTo(RightBound) > 0) return false;

            // Our bounds contain the other bounds, but that doesn't mean it contains if there
            // are prerelease versions that are being missed.

            // If we contain all prerelease versions, it is safe
            if (IncludeAllPrerelease) return true;

            // Make sure we include prerelease at the start
            if (other.LeftBound.IncludesPrerelease)
            {
                if (!(Start?.IsPrerelease ?? false)
                    || !Start.MajorMinorPatchEquals(other.Start)) return false;
            }

            // Make sure we include prerelease at the end
            if (other.RightBound.IncludesPrerelease)
            {
                if (!(End?.IsPrerelease ?? false)
                    || !End.MajorMinorPatchEquals(other.End)) return false;
            }

            return true;
        }

        /// <summary>
        /// Try to union this range with the other. This is a complex operation because it must
        /// account for prerelease versions that may be accepted at the endpoints of the ranges.
        /// </summary>
        internal bool TryUnion(UnbrokenSemVersionRange other, out UnbrokenSemVersionRange union)
        {
            // First deal with simple containment. This handles cases where the containing range
            // includes all prerelease that aren't handled with the union below. It also handles
            // containment of empty ranges.
            if (Contains(other))
            {
                union = this;
                return true;
            }

            if (other.Contains(this))
            {
                union = other;
                return true;
            }

            // Assign null once so it doesn't need to be assigned in every return case
            union = null;

            // Can't union ranges with different prerelease coverage
            if (IncludeAllPrerelease != other.IncludeAllPrerelease) return false;

            // No overlap means no union
            if (!OverlapsOrAbuts(other)) return false;

            var leftBound = LeftBound.Min(other.LeftBound);
            var rightBound = RightBound.Max(other.RightBound);
            var includeAllPrerelease = IncludeAllPrerelease; // note that other.IncludeAllPrerelease is equal

            // Create the union early to use it for containment checks
            var possibleUnion = Create(leftBound, rightBound, includeAllPrerelease);

            // If all prerelease is included, then the prerelease versions from the dropped ends
            // will be covered.
            if (!includeAllPrerelease)
            {
                var otherLeftBound = LeftBound.Max(other.LeftBound);
                if (otherLeftBound.IncludesPrerelease
                    && !possibleUnion.Contains(otherLeftBound.Version))
                    return false;

                var otherRightBound = RightBound.Min(other.RightBound);
                if (otherRightBound.IncludesPrerelease
                    && !possibleUnion.Contains(otherRightBound.Version))
                    return false;
            }

            union = possibleUnion;
            return true;
        }

        private static bool IsEmpty(LeftBoundedRange start, RightBoundedRange end, bool includeAllPrerelease)
        {
            var comparison = SemVersion.ComparePrecedence(start.Version, end.Version);
            if (comparison > 0) return true;
            if (comparison == 0) return !(start.Inclusive && end.Inclusive);

            // else start < end

            if (start.Version is null)
            {
                if (end.Inclusive) return false;
                // A range like "<0.0.0" is empty if prerelease isn't allowed and
                // "<0.0.0-0" is empty even it if isn't
                return end.Version == SemVersion.Min
                       || (!includeAllPrerelease && end.Version == SemVersion.MinRelease);
            }

            // A range like ">1.0.0 <1.0.1" is still empty if prerelease isn't allowed.
            // If prerelease is allowed, there is always an infinite number of versions in the range
            // (e.g. ">1.0.0-0 <1.0.1-0" contains "1.0.0-0.between").
            if (start.Inclusive || end.Inclusive
                || includeAllPrerelease || start.Version.IsPrerelease || end.Version.IsPrerelease)
                return false;

            return start.Version.Major == end.Version.Major
                   && start.Version.Minor == end.Version.Minor
                   // Subtract instead of add to avoid overflow
                   && start.Version.Patch == end.Version.Patch - 1;
        }

        private static SemVersion Validate(SemVersion version, string paramName)
        {
            if (version is null) throw new ArgumentNullException(paramName);
            if (version.MetadataIdentifiers.Any()) throw new ArgumentException(InvalidMetadataMessage, paramName);
            return version;
        }

        private const string InvalidMetadataMessage = "Cannot have metadata.";
    }
}
