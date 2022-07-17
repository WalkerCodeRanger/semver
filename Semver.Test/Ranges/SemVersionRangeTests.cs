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
    }
}
