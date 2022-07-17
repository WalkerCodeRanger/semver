using Semver.Ranges;
using Xunit;

namespace Semver.Test.Ranges
{
    public class SemVersionRangeTests
    {
        [Fact]
        public void MinVersionIsZeroWithZeroPrerelease()
        {
            Assert.Equal(SemVersion.Parse("0.0.0-0", SemVersionStyles.Strict), SemVersionRange.MinVersion);
        }

        [Fact]
        public void MaxVersionIsIntMax()
        {
            Assert.Equal(new SemVersion(int.MaxValue, int.MaxValue, int.MaxValue), SemVersionRange.MaxVersion);
        }

        [Fact]
        public void EmptyIsMaximallyEmpty()
        {
            var empty = SemVersionRange.Empty;
            Assert.Equal(SemVersionRange.MaxVersion, empty.Start);
            Assert.False(empty.StartInclusive, "Start Inclusive");
            Assert.Equal(SemVersionRange.MinVersion, empty.End);
            Assert.False(empty.EndInclusive, "End Inclusive");
            Assert.False(empty.IncludeAllPrerelease, "Include All Prerelease");
        }

        [Theory]
        [InlineData("0.0.0")]
        [InlineData("0.0.0-0")]
        [InlineData("1.2.3")]
        [InlineData("4.5.6-rc")]
        [InlineData("7.8.9-beta.0")]
        [InlineData("2147483647.2147483647.2147483647")]
        public void EmptyDoesNotContain(string version)
        {
            Assert.False(SemVersionRange.Empty.Contains(SemVersion.Parse(version, SemVersionStyles.Strict)));
        }
    }
}
