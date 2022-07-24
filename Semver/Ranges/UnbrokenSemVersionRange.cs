using System;

namespace Semver.Ranges
{
    internal class UnbrokenSemVersionRange
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

        public static UnbrokenSemVersionRange GreaterThan(SemVersion version, bool includeAllPrerelease = false)
            => Create(version ?? throw new ArgumentNullException(nameof(version)), false,
                SemVersion.Max, true, includeAllPrerelease);

        public static UnbrokenSemVersionRange AtLeast(SemVersion version, bool includeAllPrerelease = false)
            => Create(version ?? throw new ArgumentNullException(nameof(version)), true,
                SemVersion.Max, true, includeAllPrerelease);

        public static UnbrokenSemVersionRange LessThan(SemVersion version, bool includeAllPrerelease = false)
            => Create(null, false,
                version ?? throw new ArgumentNullException(nameof(version)), false, includeAllPrerelease);

        public static UnbrokenSemVersionRange AtMost(SemVersion version, bool includeAllPrerelease = false)
            => Create(null, false,
                version ?? throw new ArgumentNullException(nameof(version)), true, includeAllPrerelease);

        public static UnbrokenSemVersionRange Inclusive(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(start ?? throw new ArgumentNullException(nameof(start)), true,
                end ?? throw new ArgumentNullException(nameof(end)), true, includeAllPrerelease);

        public static UnbrokenSemVersionRange InclusiveOfStart(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(start ?? throw new ArgumentNullException(nameof(start)), true,
                end ?? throw new ArgumentNullException(nameof(end)), false, includeAllPrerelease);

        public static UnbrokenSemVersionRange InclusiveOfEnd(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(start ?? throw new ArgumentNullException(nameof(start)), false,
                end ?? throw new ArgumentNullException(nameof(end)), true, includeAllPrerelease);

        public static UnbrokenSemVersionRange Exclusive(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(start ?? throw new ArgumentNullException(nameof(start)), false,
                end ?? throw new ArgumentNullException(nameof(end)), false, includeAllPrerelease);

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

        private UnbrokenSemVersionRange(LeftBoundedRange start, RightBoundedRange end, bool includeAllPrerelease)
        {
            this.start = start;
            this.end = end;
            IncludeAllPrerelease = includeAllPrerelease;
        }

        private readonly LeftBoundedRange start;
        private readonly RightBoundedRange end;

        public SemVersion Start => start.Version;
        public bool StartInclusive => start.Inclusive;
        public SemVersion End => end.Version;
        public bool EndInclusive => end.Inclusive;
        public bool IncludeAllPrerelease { get; }

        public bool Contains(SemVersion version)
        {
            if (version is null) throw new ArgumentNullException(nameof(version));

            if (!start.Contains(version) || !end.Contains(version)) return false;

            if (IncludeAllPrerelease || !version.IsPrerelease) return true;

            // Prerelease versions must match either the start or end
            return Start?.IsPrerelease == true && version.MajorMinorPatchEquals(Start)
                   || End.IsPrerelease && version.MajorMinorPatchEquals(End);
        }

        // TODO Test. Is this correct? Should it be included?
        public UnbrokenSemVersionRange Intersect(UnbrokenSemVersionRange range)
        {
            var includeAllPrerelease = IncludeAllPrerelease && range.IncludeAllPrerelease;
            var newStart = start.Max(range.start);
            var newEnd = end.Min(range.end);
            // Always return the same empty range
            if (IsEmpty(newStart, newEnd, includeAllPrerelease)) return Empty;
            return new UnbrokenSemVersionRange(newStart, newEnd, includeAllPrerelease);
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
    }
}
