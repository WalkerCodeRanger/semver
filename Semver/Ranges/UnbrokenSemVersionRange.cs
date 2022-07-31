using System;
using System.Linq;
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
        public static readonly UnbrokenSemVersionRange Empty
            = new UnbrokenSemVersionRange(new LeftBoundedRange(SemVersion.Max, false),
                new RightBoundedRange(SemVersion.Min, false), false);
        public static readonly UnbrokenSemVersionRange AllRelease = AtMost(SemVersion.Max);
        public static readonly UnbrokenSemVersionRange All = AtMost(SemVersion.Max, true);

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

        private static UnbrokenSemVersionRange Create(
            SemVersion startVersion,
            bool startInclusive,
            SemVersion endVersion,
            bool endInclusive,
            bool includeAllPrerelease)
        {
            var start = new LeftBoundedRange(startVersion, startInclusive);
            var end = new RightBoundedRange(endVersion, endInclusive);
            // Always return the same empty range
            if (IsEmpty(start, end, includeAllPrerelease)) return Empty;
            return new UnbrokenSemVersionRange(start, end, includeAllPrerelease);
        }

        internal static UnbrokenSemVersionRange Create(
            LeftBoundedRange start,
            RightBoundedRange end,
            bool includeAllPrerelease)
        {
            // Always return the same empty range
            if (IsEmpty(start, end, includeAllPrerelease)) return Empty;
            return new UnbrokenSemVersionRange(start, end, includeAllPrerelease);
        }

        private UnbrokenSemVersionRange(LeftBoundedRange leftBound, RightBoundedRange rightBound, bool includeAllPrerelease)
        {
            LeftBound = leftBound;
            RightBound = rightBound;
            IncludeAllPrerelease = includeAllPrerelease;
        }

        internal readonly LeftBoundedRange LeftBound;
        internal readonly RightBoundedRange RightBound;


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

        // TODO implement ToString()

        internal bool Overlaps(UnbrokenSemVersionRange other)
        {
            // see https://stackoverflow.com/a/3269471/268898
            return LeftBound.CompareTo(other.RightBound) <= 0
                   && other.LeftBound.CompareTo(RightBound) <= 0;
        }

        /// <summary>
        /// Whether this range contains the other. For this to be the case, it must contain all the
        /// versions accounting for which prerelease versions are in each range.
        /// </summary>
        internal bool Contains(UnbrokenSemVersionRange other)
        {
            // It contains prerelease we don't
            // TODO what if those prerelease were covered by our ends?
            if (other.IncludeAllPrerelease && !IncludeAllPrerelease) return false;

            // If our bounds don't contain the other bounds, there is no containment
            if (LeftBound.CompareTo(other.LeftBound) > 0
                || other.RightBound.CompareTo(RightBound) > 0) return false;

            // Our bounds contain the other bounds, but that doesn't mean it contains if there
            // are prerelease versions that are being missed.

            // If we contain all prerelease versions, it is safe
            if (IncludeAllPrerelease) return true;

            // Make sure we include prerelease at the start
            if (other.Start.IsPrerelease)
            {
                if (!Start.IsPrerelease
                    || !Start.MajorMinorPatchEquals(other.Start)) return false;
            }

            // Make sure we include prerelease at the end
            if (other.End.IsPrerelease)
            {
                if (!Start.IsPrerelease
                    || !End.MajorMinorPatchEquals(other.End)) return false;
            }

            return true;
        }

        /// <summary>
        /// Try to union this range with the other. This is a complex operation because it must
        /// account for
        /// </summary>
        internal bool TryUnion(UnbrokenSemVersionRange other, out UnbrokenSemVersionRange union)
        {
            if (this.Contains(other))
            {
                union = this;
                return true;
            }

            if (other.Contains(this))
            {
                union = other;
                return true;
            }

            throw new NotImplementedException();
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
