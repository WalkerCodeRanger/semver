using System.Collections.Generic;
using Semver.Ranges;
using Semver.Test.Helpers;
using Semver.Utility;
using Xunit;
using static Semver.Test.Builders.VersionBuilder;

namespace Semver.Test.Ranges
{
    public class RightBoundedRangeTests
    {
        private static readonly SemVersion FakeVersion = new SemVersion(1, 2, 3);

        internal static readonly IReadOnlyList<RightBoundedRange> RangesInOrder = new List<RightBoundedRange>()
        {
            new RightBoundedRange(SemVersion.Min, false),
            new RightBoundedRange(SemVersion.Min, true),
            new RightBoundedRange(Version("0.0.0-5"), true),
            new RightBoundedRange(SemVersion.MinRelease, false),
            new RightBoundedRange(SemVersion.MinRelease, true),
            new RightBoundedRange(Version("1.2.3"), false),
            new RightBoundedRange(Version("1.2.3"), true),
            new RightBoundedRange(SemVersion.Max, false),
            RightBoundedRange.Unbounded,
        }.AsReadOnly();

        internal static readonly IReadOnlyList<(RightBoundedRange, RightBoundedRange)> RangePairs
            = RangesInOrder.AllPairs().ToReadOnlyList();

        [Fact]
        public void CompareToRightBoundedRange()
        {
            foreach (var (left, right) in RangePairs)
            {
                Assert.True(left.CompareTo(right) < 0, $"{left} < {right}");
                Assert.True(right.CompareTo(left) > 0, $"{right} > {left}");
            }
        }

        [Fact]
        public void MaxAtSameVersion()
        {
            var inclusive = new RightBoundedRange(FakeVersion, true);
            var exclusive = new RightBoundedRange(FakeVersion, false);

            var max = inclusive.Max(exclusive);

            Assert.Equal(inclusive, max);
        }

        [Fact]
        public void GetHashCodeAndEquality()
        {
            var range1 = new RightBoundedRange(FakeVersion, true);
            var range2 = new RightBoundedRange(FakeVersion, true);

            Assert.True(range1.Equals(range2));
            Assert.True(range1.Equals((object)range2));
            Assert.Equal(range1.GetHashCode(), range2.GetHashCode());
            Assert.True(range1 == range2);
            Assert.False(range1 != range2);
        }
    }
}
