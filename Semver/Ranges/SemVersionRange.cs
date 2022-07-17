using System;

namespace Semver.Ranges
{
    internal class SemVersionRange
    {
        internal static readonly SemVersion MinVersion = new SemVersion(0, 0, 0, new[] { new PrereleaseIdentifier(0) });
        internal static readonly SemVersion MaxVersion = new SemVersion(int.MaxValue, int.MaxValue, int.MaxValue);

        public static readonly SemVersionRange Empty
            = new SemVersionRange(LeftBoundedRange.Unbounded,
                new RightBoundedRange(MinVersion, false), false);
        public static readonly SemVersionRange AllRelease = AtMost(MaxVersion);
        public static readonly SemVersionRange All = AtMost(MaxVersion, true);

        public static SemVersionRange GreaterThan(SemVersion version, bool includeAllPrerelease = false)
            => Create(version ?? throw new ArgumentNullException(nameof(version)), false,
                MaxVersion, true, includeAllPrerelease);

        public static SemVersionRange AtLeast(SemVersion version, bool includeAllPrerelease = false)
            => Create(version ?? throw new ArgumentNullException(nameof(version)), true,
                MaxVersion, true, includeAllPrerelease);

        public static SemVersionRange LessThan(SemVersion version, bool includeAllPrerelease = false)
            => Create(null, false,
                version ?? throw new ArgumentNullException(nameof(version)), false, includeAllPrerelease);

        public static SemVersionRange AtMost(SemVersion version, bool includeAllPrerelease = false)
            => Create(null, false,
                version ?? throw new ArgumentNullException(nameof(version)), true, includeAllPrerelease);

        public static SemVersionRange Inclusive(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(start ?? throw new ArgumentNullException(nameof(start)), true,
                end ?? throw new ArgumentNullException(nameof(end)), true, includeAllPrerelease);

        public static SemVersionRange InclusiveOfStart(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(start ?? throw new ArgumentNullException(nameof(start)), true,
                end ?? throw new ArgumentNullException(nameof(end)), false, includeAllPrerelease);

        public static SemVersionRange InclusiveOfEnd(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(start ?? throw new ArgumentNullException(nameof(start)), false,
                end ?? throw new ArgumentNullException(nameof(end)), true, includeAllPrerelease);

        public static SemVersionRange Exclusive(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(start ?? throw new ArgumentNullException(nameof(start)), false,
                end ?? throw new ArgumentNullException(nameof(end)), false, includeAllPrerelease);

        private static SemVersionRange Create(
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
            return new SemVersionRange(start, end, includeAllPrerelease);
        }

        private SemVersionRange(LeftBoundedRange start, RightBoundedRange end, bool includeAllPrerelease)
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

        // TODO this isn't quite intersect
        public SemVersionRange Intersect(SemVersionRange range)
        {
            var includeAllPrerelease = IncludeAllPrerelease && range.IncludeAllPrerelease;
            var newStart = start.Max(range.start);
            var newEnd = end.Min(range.end);
            // Always return the same empty range
            if (IsEmpty(newStart, newEnd, includeAllPrerelease)) return Empty;
            return new SemVersionRange(newStart, newEnd, includeAllPrerelease);
        }

        private static bool IsEmpty(LeftBoundedRange start, RightBoundedRange end, bool includeAllPrerelease)
        {
            var comparison = SemVersion.ComparePrecedence(start.Version, end.Version);
            if (comparison > 0) return true;
            if (comparison == 0) return !(start.Inclusive && end.Inclusive);

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
