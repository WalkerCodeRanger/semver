using Semver.Ranges;
using Semver.Test.Ranges.NpmSatisfyTestData;
using Xunit;

namespace Semver.Test.Ranges
{
    public class SemVersionNpmSatisfyTests
    {
        [Theory]
        [ClassData(typeof(ParseData))]
        public void ParseTests(string strRange, string expectedRange, bool includeAllPrerelease = false)
        {
            NpmRange.TryParse(strRange, includeAllPrerelease, out NpmRange range);
            string result = range?.ToString();

            Assert.Equal(expectedRange, result);
        }
    }
}
