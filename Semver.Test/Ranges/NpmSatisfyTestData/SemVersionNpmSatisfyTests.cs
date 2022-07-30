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
#pragma warning disable CS0618 // Type or member is obsolete
            NpmRangeSet.TryParse(strRange, includeAllPrerelease, out NpmRangeSet range);
#pragma warning restore CS0618 // Type or member is obsolete
            string result = range?.ToString();

            Assert.Equal(expectedRange, result);
        }
    }
}
