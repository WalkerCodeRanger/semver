using Semver.Ranges;
using Semver.Ranges.Comparers.Npm;
using Semver.Test.NpmSatisfyTestData;
using Xunit;
using Xunit.Abstractions;

namespace Semver.Test
{
    public class SemVersionNpmSatisfyTests
    {
        private readonly ITestOutputHelper testOutput;

        public SemVersionNpmSatisfyTests(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
        }

        [Theory]
        [ClassData(typeof(ParseData))]
        public void ParseTests(string strRange, string expectedRange, NpmParseOptions options = default)
        {
            NpmRange.TryParse(strRange, options, out NpmRange range);
            string result = range?.ToString();

            Assert.Equal(expectedRange, result);
        }

        /// <summary>
        /// All ranges should be valid and include the version
        /// </summary>
        [Theory]
        [ClassData(typeof(IncludeData))]
        public void AllIncludes(string range, string version, NpmParseOptions options = default)
        {
            Assert.True(NpmRange.TryParse(range, options, out var parsedRange), "Failed to parse range");
            Assert.True(parsedRange.Contains(SemVersion.Parse(version, SemVersionStyles.Strict)), $"{parsedRange} does not include {version}");
            testOutput.WriteLine($"{parsedRange} includes {version}");
        }

        /// <summary>
        /// All ranges should be valid and exclude the version
        /// </summary>
        [Theory]
        [ClassData(typeof(ExcludeData))]
        public void AllExcludes(string range, string version, NpmParseOptions options = default)
        {
            Assert.True(NpmRange.TryParse(range, options, out var parsedRange), "Failed to parse range");
            Assert.False(parsedRange.Contains(SemVersion.Parse(version, SemVersionStyles.Strict)), $"{parsedRange} includes {version}");
            testOutput.WriteLine($"{parsedRange} excludes {version}");
        }
    }
}
