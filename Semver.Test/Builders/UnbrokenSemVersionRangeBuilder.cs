namespace Semver.Test.Builders
{
    public static class UnbrokenSemVersionRangeBuilder
    {
        public static readonly UnbrokenSemVersionRange Empty = UnbrokenSemVersionRange.Empty;
        public static readonly UnbrokenSemVersionRange All = UnbrokenSemVersionRange.All;
        public static readonly UnbrokenSemVersionRange AllRelease = UnbrokenSemVersionRange.AllRelease;

        public static UnbrokenSemVersionRange EqualsVersion(string version)
            => UnbrokenSemVersionRange.Equals(SemVersion.Parse(version, SemVersionStyles.Strict));

        public static UnbrokenSemVersionRange Inclusive(string start, string end, bool includeAllPrerelease = false)
            => UnbrokenSemVersionRange.Inclusive(SemVersion.Parse(start, SemVersionStyles.Strict),
                SemVersion.Parse(end, SemVersionStyles.Strict), includeAllPrerelease);

        public static UnbrokenSemVersionRange InclusiveOfStart(string start, string end, bool includeAllPrerelease = false)
            => UnbrokenSemVersionRange.InclusiveOfStart(SemVersion.Parse(start, SemVersionStyles.Strict),
                SemVersion.Parse(end, SemVersionStyles.Strict), includeAllPrerelease);

        public static UnbrokenSemVersionRange InclusiveOfEnd(string start, string end, bool includeAllPrerelease = false)
            => UnbrokenSemVersionRange.InclusiveOfEnd(SemVersion.Parse(start, SemVersionStyles.Strict),
                SemVersion.Parse(end, SemVersionStyles.Strict), includeAllPrerelease);

        public static UnbrokenSemVersionRange Exclusive(string start, string end, bool includeAllPrerelease = false)
            => UnbrokenSemVersionRange.Exclusive(SemVersion.Parse(start, SemVersionStyles.Strict),
                SemVersion.Parse(end, SemVersionStyles.Strict), includeAllPrerelease);

        public static UnbrokenSemVersionRange GreaterThan(string version, bool includeAllPrerelease = false)
            => UnbrokenSemVersionRange.GreaterThan(SemVersion.Parse(version, SemVersionStyles.Strict), includeAllPrerelease);

        public static UnbrokenSemVersionRange AtLeast(string version, bool includeAllPrerelease = false)
            => UnbrokenSemVersionRange.AtLeast(SemVersion.Parse(version, SemVersionStyles.Strict), includeAllPrerelease);

        public static UnbrokenSemVersionRange LessThan(string version, bool includeAllPrerelease = false)
            => UnbrokenSemVersionRange.LessThan(SemVersion.Parse(version, SemVersionStyles.Strict), includeAllPrerelease);

        public static UnbrokenSemVersionRange AtMost(string version, bool includeAllPrerelease = false)
            => UnbrokenSemVersionRange.AtMost(SemVersion.Parse(version, SemVersionStyles.Strict), includeAllPrerelease);
    }
}
