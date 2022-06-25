using Semver.Ranges;

namespace Semver.Test.Ranges.NpmSatisfyTestData
{
    /// <summary>
    /// An individual test case for a range test
    /// </summary>
    public class RangeContainsTestCase
    {
        public RangeContainsTestCase(string range, string version, NpmParseOptions options = null)
        {
            Range = NpmRange.Parse(range, options ?? NpmParseOptions.Default);
            Version = SemVersion.Parse(version, SemVersionStyles.Strict);
        }

        public NpmRange Range { get; }
        public SemVersion Version { get; }
    }
}
