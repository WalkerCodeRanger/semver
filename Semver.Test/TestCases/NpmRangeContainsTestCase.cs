using Semver.Ranges;

namespace Semver.Test.TestCases
{
    /// <summary>
    /// An individual test case for a range test
    /// </summary>
    public class NpmRangeContainsTestCase
    {
        public static NpmRangeContainsTestCase NpmIncludes(string range, string version, bool includeAllPrerelease = false)
            => new NpmRangeContainsTestCase(range, includeAllPrerelease, version, true);

        public static NpmRangeContainsTestCase NpmExcludes(string range, string version, bool includeAllPrerelease = false)
            => new NpmRangeContainsTestCase(range, includeAllPrerelease, version, false);

        private NpmRangeContainsTestCase(string range, bool includeAllPrerelease, string version, bool versionIncluded)
        {
            Range = range;
            IncludeAllPrerelease = includeAllPrerelease;
            Version = SemVersion.Parse(version, SemVersionStyles.Strict);
            VersionIncluded = versionIncluded;
        }

        public string Range { get; }
        public bool IncludeAllPrerelease { get; }
        public SemVersion Version { get; }
        public bool VersionIncluded { get; }

        public override string ToString()
        {
            var includes = VersionIncluded ? "includes" : "excludes";
            return $"{ToRangeString()} {includes} {Version}";
        }

        public void Assert()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var rangeSet = SemVersionRangeSet.ParseNpm(Range, IncludeAllPrerelease);
#pragma warning restore CS0618 // Type or member is obsolete
            if (VersionIncluded)
                Xunit.Assert.True(rangeSet.Contains(Version), $"{ToRangeString()} should include {Version}");
            else
                Xunit.Assert.False(rangeSet.Contains(Version), $"{ToRangeString()} should exclude {Version}");
        }

        private string ToRangeString()
            => IncludeAllPrerelease ? Range + " w/ Prerelease" : Range;
    }
}
