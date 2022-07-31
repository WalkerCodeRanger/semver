using Semver.Ranges;
using Xunit;

namespace Semver.Test.Ranges
{
    public class LeftBoundedRangeTests
    {
        [Fact]
        public void CompareToSameVersion()
        {
            var version = new SemVersion(1, 2, 3);
            var leftExclusive = new LeftBoundedRange(version, false); // 1.2.3 + epsilon
            var leftInclusive = new LeftBoundedRange(version, true); // 1.2.3
            var rightExclusive = new RightBoundedRange(version, false); // 1.2.3 - epsilon
            var rightInclusive = new RightBoundedRange(version, true); // 1.2.3

            Assert.True(leftExclusive.CompareTo(rightExclusive) > 0, "leftExclusive > rightExclusive");
            Assert.True(leftExclusive.CompareTo(rightInclusive) > 0, "leftExclusive > rightInclusive");
            Assert.True(leftInclusive.CompareTo(rightExclusive) > 0, "leftInclusive > rightExclusive");
            Assert.True(leftInclusive.CompareTo(rightInclusive) == 0, "leftInclusive == rightInclusive");
        }
    }
}
