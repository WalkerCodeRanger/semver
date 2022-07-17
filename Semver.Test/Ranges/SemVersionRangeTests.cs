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

        public static readonly TheoryData<string> FullRangeOfVersions = new TheoryData<string>
        {
            "0.0.0",
            "0.0.0-0",
            "1.2.3",
            "4.5.6-rc",
            "7.8.9-beta.0",
            "2147483647.2147483647.2147483647-alpha",
            "2147483647.2147483647.2147483647",
        };

        [Theory]
        [MemberData(nameof(FullRangeOfVersions))]
        public void EmptyDoesNotContain(string version)
        {
            Assert.False(SemVersionRange.Empty.Contains(SemVersion.Parse(version, SemVersionStyles.Strict)));
        }

        [Fact]
        public void AllReleaseProperties()
        {
            var allRelease = SemVersionRange.AllRelease;
            Assert.Null(allRelease.Start);
            Assert.False(allRelease.StartInclusive, "Start Inclusive");
            Assert.Equal(SemVersionRange.MaxVersion, allRelease.End);
            Assert.True(allRelease.EndInclusive, "End Inclusive");
            Assert.False(allRelease.IncludeAllPrerelease, "Include All Prerelease");
        }

        [Theory]
        [MemberData(nameof(FullRangeOfVersions))]
        public void AllReleaseContains(string version)
        {
            var semVersion = SemVersion.Parse(version, SemVersionStyles.Strict);
            Assert.Equal(!semVersion.IsPrerelease, SemVersionRange.AllRelease.Contains(semVersion));
        }

        [Fact]
        public void AllProperties()
        {
            var all = SemVersionRange.All;
            Assert.Null(all.Start);
            Assert.False(all.StartInclusive, "Start Inclusive");
            Assert.Equal(SemVersionRange.MaxVersion, all.End);
            Assert.True(all.EndInclusive, "End Inclusive");
            Assert.True(all.IncludeAllPrerelease, "Include All Prerelease");
        }

        [Theory]
        [MemberData(nameof(FullRangeOfVersions))]
        public void AllContains(string version)
        {
            var semVersion = SemVersion.Parse(version, SemVersionStyles.Strict);
            Assert.True(SemVersionRange.All.Contains(semVersion));
        }
    }
}
