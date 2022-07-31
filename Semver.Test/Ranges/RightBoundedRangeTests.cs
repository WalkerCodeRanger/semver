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
    }
}
