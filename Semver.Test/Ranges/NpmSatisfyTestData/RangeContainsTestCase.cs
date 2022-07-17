using Semver.Ranges;

namespace Semver.Test.Ranges.NpmSatisfyTestData
{
    /// <summary>
    /// An individual test case for a range test
    /// </summary>
    internal class RangeContainsTestCase
    {
        public RangeContainsTestCase(string range, string version, bool includeAllPrerelease = false)
        {
            Range = NpmRange.Parse(range, includeAllPrerelease);
            Version = SemVersion.Parse(version, SemVersionStyles.Strict);
        }

        public NpmRange Range { get; }
        public SemVersion Version { get; }
    }
}
