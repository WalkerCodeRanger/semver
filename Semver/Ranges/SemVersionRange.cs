using System;

namespace Semver.Ranges
{
    internal class SemVersionRange
    {
        private static readonly SemVersion LowestVersion = new SemVersion(0, 0, 0, new[] { new PrereleaseIdentifier(0) });
        private static readonly SemVersion HighestVersion = new SemVersion(int.MaxValue, int.MaxValue, int.MaxValue);

        public static readonly SemVersionRange Empty
            = new SemVersionRange(new LeftBoundedRange(null, false),
                new RightBoundedRange(LowestVersion, false), false);
        public static readonly SemVersionRange AllRelease = AtMost(HighestVersion);
        public static readonly SemVersionRange All = AtMost(HighestVersion, true);

        public static SemVersionRange GreaterThan(SemVersion version, bool includeAllPrerelease = false)
            => Create(version ?? throw new ArgumentNullException(nameof(version)), false,
                HighestVersion, true, includeAllPrerelease);

        public static SemVersionRange AtLeast(SemVersion version, bool includeAllPrerelease = false)
            => Create(version ?? throw new ArgumentNullException(nameof(version)), true,
                HighestVersion, true, includeAllPrerelease);

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
            if (!start.Overlaps(end)) return Empty;
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
            if (!start.Overlaps(end)) return Empty;
            return new SemVersionRange(newStart, newEnd, includeAllPrerelease);
        }
    }
}
