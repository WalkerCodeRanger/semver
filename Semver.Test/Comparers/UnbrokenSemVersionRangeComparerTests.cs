using Semver.Comparers;
using Xunit;

namespace Semver.Test.Comparers;

public class UnbrokenSemVersionRangeComparerTests
{
    private static readonly UnbrokenSemVersionRangeComparer Comparer = UnbrokenSemVersionRangeComparer.Instance;

    [Fact]
    public void CompareInclusivenessOfStart()
    {
        var inclusive = UnbrokenSemVersionRange.AtLeast(new SemVersion(1, 2, 3));
        var exclusive = UnbrokenSemVersionRange.GreaterThan(new SemVersion(1, 2, 3));

        AssertInOrder(inclusive, exclusive);
    }

    [Fact]
    public void CompareInclusivenessOfEnd()
    {
        var inclusive = UnbrokenSemVersionRange.AtMost(new SemVersion(1, 2, 3));
        var exclusive = UnbrokenSemVersionRange.LessThan(new SemVersion(1, 2, 3));

        AssertInOrder(inclusive, exclusive);
    }

    [Fact]
    public void CompareByIncludePrerelease()
    {
        var start = new SemVersion(1, 2, 3);
        var end = new SemVersion(1, 3, 6);
        var release = UnbrokenSemVersionRange.Inclusive(start, end, false);
        var prerelease = UnbrokenSemVersionRange.Inclusive(start, end, true);

        AssertInOrder(prerelease, release);
    }

    private static void AssertInOrder(UnbrokenSemVersionRange left, UnbrokenSemVersionRange right)
    {
        Assert.Equal(-1, Comparer.Compare(left, right));
        Assert.Equal(1, Comparer.Compare(right, left));
    }
}
