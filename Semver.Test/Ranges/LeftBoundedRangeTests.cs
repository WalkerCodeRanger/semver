﻿using System.Collections.Generic;
using Semver.Ranges;
using Semver.Test.Helpers;
using Semver.Utility;
using Xunit;
using static Semver.Test.Builders.VersionBuilder;

namespace Semver.Test.Ranges;

public class LeftBoundedRangeTests
{
    public static readonly SemVersion FakeVersion = Version("1.2.3");

    internal static readonly IReadOnlyList<LeftBoundedRange> RangesInOrder = new List<LeftBoundedRange>()
    {
        LeftBoundedRange.Unbounded,
        new LeftBoundedRange(SemVersion.Min, true),
        new LeftBoundedRange(SemVersion.Min, false),
        new LeftBoundedRange(Version("0.0.0-5"), true),
        new LeftBoundedRange(SemVersion.MinRelease, true),
        new LeftBoundedRange(SemVersion.MinRelease, false),
        new LeftBoundedRange(Version("1.2.3"), true),
        new LeftBoundedRange(Version("1.2.3"), false),
    }.AsReadOnly();

    internal static readonly IReadOnlyList<(LeftBoundedRange, LeftBoundedRange)> RangePairs
        = RangesInOrder.AllPairs().ToReadOnlyList();

    internal static readonly LeftBoundedRange LeftExclusive = new LeftBoundedRange(FakeVersion, false); // 1.2.3 + epsilon
    internal static readonly LeftBoundedRange LeftInclusive = new LeftBoundedRange(FakeVersion, true); // 1.2.3
    internal static readonly RightBoundedRange RightExclusive = new RightBoundedRange(FakeVersion, false); // 1.2.3 - epsilon
    internal static readonly RightBoundedRange RightInclusive = new RightBoundedRange(FakeVersion, true); // 1.2.3

    internal static readonly IReadOnlyList<(LeftBoundedRange, RightBoundedRange, Ordering)> CompareToRightBoundedRangeTestCases = new List<(LeftBoundedRange, RightBoundedRange, Ordering)>()
    {
        (LeftBoundedRange.Unbounded, RightBoundedRange.Unbounded, Ordering.Less),
        (LeftExclusive, RightExclusive, Ordering.Greater),
        (LeftExclusive, RightInclusive, Ordering.Greater),
        (LeftInclusive, RightExclusive, Ordering.Greater),
        (LeftInclusive, RightInclusive, Ordering.Equal),
    };

    [Fact]
    public void CompareToRightBoundedRange()
    {
        foreach (var (left, right, expected) in CompareToRightBoundedRangeTestCases)
            Assert.True(left.CompareTo(right) == (int)expected,
                $"{left} {expected.ToOperator()} {right}");
    }

    [Fact]
    public void CompareToLeftBoundedRange()
    {
        foreach (var (left, right) in RangePairs)
        {
            Assert.True(left.CompareTo(right) < 0, $"{left} < {right}");
            Assert.True(right.CompareTo(left) > 0, $"{right} > {left}");
        }
    }

    [Fact]
    public void MinAtSameVersion()
    {
        var inclusive = new LeftBoundedRange(FakeVersion, true);
        var exclusive = new LeftBoundedRange(FakeVersion, false);

        var max = inclusive.Min(exclusive);

        Assert.Equal(inclusive, max);
    }

    [Fact]
    public void GetHashCodeAndEquality()
    {
        var range1 = new LeftBoundedRange(FakeVersion, true);
        var range2 = new LeftBoundedRange(FakeVersion, true);

        Assert.True(range1.Equals(range2));
        Assert.True(range1.Equals((object)range2));
        Assert.Equal(range1.GetHashCode(), range2.GetHashCode());
        Assert.True(range1 == range2);
        Assert.False(range1 != range2);
    }
}
