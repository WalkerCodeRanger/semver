using Semver.Ranges.Npm;
using Xunit;

namespace Semver.Test.Ranges.NpmSatisfyTestData
{
    public class SemVersionNpmSatisfyTests
    {
        [Theory]
        [ClassData(typeof(ParseData))]
        public void ParseTests(string strRange, string expectedRange, bool includeAllPrerelease = false)
        {
            NpmRangeSet.TryParse(strRange, includeAllPrerelease, out NpmRangeSet range);
            string result = range?.ToString();

            Assert.Equal(expectedRange, result);
        }
    }
}
