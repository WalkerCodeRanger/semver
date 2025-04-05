using System;
using System.Collections.Generic;
using System.Linq;
using Semver.Comparers;
using Xunit;

namespace Semver.Test.Comparers;

public class SortOrderComparerTests
{
    private static readonly ISemVersionComparer Comparer = SortOrderComparer.Instance;

    #region Equals
    [Theory]
    [MemberData(nameof(ComparerTestData.VersionsInSortOrder), MemberType = typeof(ComparerTestData))]
    public void EqualsIdenticalTest(string version)
    {
        var v = SemVersion.Parse(version);
        // Construct an identical version, but different instance
        var identical = new SemVersion(v.Major, v.Minor, v.Patch,
            v.PrereleaseIdentifiers, v.MetadataIdentifiers);
        Assert.True(Comparer.Equals(v, identical), v.ToString());
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionsInSortOrder), MemberType = typeof(ComparerTestData))]
    public void EqualsSameTest(string version)
    {
        var v = SemVersion.Parse(version);
        Assert.True(Comparer.Equals(v, v), v.ToString());
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionPairs), MemberType = typeof(ComparerTestData))]
    public void EqualsDifferentTest(string left, string right)
    {
        var v1 = SemVersion.Parse(left);
        var v2 = SemVersion.Parse(right);

        Assert.False(Comparer.Equals(v1, v2), $"Equals({v1}, {v2})");
    }

    [Fact]
    public void EqualsNullNullTest()
    {
        Assert.True(Comparer.Equals(null!, null!));
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionsInSortOrder), MemberType = typeof(ComparerTestData))]
    public void EqualsNullTest(string version)
    {
        var v = SemVersion.Parse(version);
        Assert.False(Comparer.Equals(v, null!), $"Equals({v}, null)");
        Assert.False(Comparer.Equals(null!, v), $"Equals(null, {v})");
    }
    #endregion

    #region GetHashCode
    [Theory]
    [MemberData(nameof(ComparerTestData.VersionsInSortOrder), MemberType = typeof(ComparerTestData))]
    public void GetHashCodeIdenticalTest(string version)
    {
        var v = SemVersion.Parse(version);
        // Construct an identical version, but different instance
        var identical = new SemVersion(v.Major, v.Minor, v.Patch,
            v.PrereleaseIdentifiers, v.MetadataIdentifiers);
        Assert.True(Comparer.GetHashCode(v) == Comparer.GetHashCode(identical), v.ToString());
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionPairs), MemberType = typeof(ComparerTestData))]
    public void GetHashCodeDifferentTest(string left, string right)
    {
        var v1 = SemVersion.Parse(left);
        var v2 = SemVersion.Parse(right);

        Assert.False(Comparer.GetHashCode(v1) == Comparer.GetHashCode(v2),
            $"GetHashCode({v1}) == GetHashCode({v2})");
    }
    #endregion

    #region CompareTo
    [Theory]
    [MemberData(nameof(ComparerTestData.VersionsInSortOrder), MemberType = typeof(ComparerTestData))]
    public void CompareIdenticalTest(string version)
    {
        var v = SemVersion.Parse(version);
        // Construct an identical version, but different instance
        var identical = new SemVersion(v.Major, v.Minor, v.Patch,
            v.PrereleaseIdentifiers, v.MetadataIdentifiers);
        Assert.True(Comparer.Compare(v, identical) == 0, v.ToString());
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionsInSortOrder), MemberType = typeof(ComparerTestData))]
    public void CompareSameTest(string version)
    {
        var v = SemVersion.Parse(version);
        Assert.True(Comparer.Compare(v, v) == 0, v.ToString());
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionPairs), MemberType = typeof(ComparerTestData))]
    public void CompareGreaterTest(string left, string right)
    {
        var v1 = SemVersion.Parse(left);
        var v2 = SemVersion.Parse(right);

        Assert.True(Comparer.Compare(v1, v2) == -1, $"Compare({v1}, {v2}) == -1");
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionPairs), MemberType = typeof(ComparerTestData))]
    public void CompareLesserTest(string left, string right)
    {
        var v1 = SemVersion.Parse(left);
        var v2 = SemVersion.Parse(right);

        Assert.True(Comparer.Compare(v2, v1) == 1, $"Compare({v2}, {v1}) == 1");
    }

    [Fact]
    public void CompareObjectsTest()
    {
        var ex = Assert.Throws<ArgumentException>(()
            => Comparer.Compare(new object(), new object()));

        Assert.Equal("Type of argument is not compatible with the generic comparer.", ex.Message);
    }

    [Fact]
    public void CompareNullTest()
    {
        foreach (var version in ComparerTestData.VersionsInSortOrder)
        {
            var v = SemVersion.Parse(version);
            Assert.True(Comparer.Compare(v, null!) == 1, $"Compare({v}, null) == 1");
            Assert.True(Comparer.Compare(null!, v) == -1, $"Compare(null, {v}) == -1");
        }

        Assert.True(Comparer.Compare(null!, null!) == 0, "Compare(null, null) == 0");
    }
    #endregion

    #region Passing to Operations
    [Fact]
    public void SortList()
    {
        var versions = new List<SemVersion?>() { null, new SemVersion(2, 0), null, new SemVersion(1, 0) };

        // TODO remove ! with fix to comparer nullability
        versions.Sort(SemVersion.SortOrderComparer!);

        Assert.Equal([null, null, new SemVersion(1, 0), new SemVersion(2, 0)], versions);
    }

    [Fact]
    public void OrderBy()
    {
        SemVersion?[] versions = [null, new SemVersion(2, 0), null, new SemVersion(1, 0)];

        // TODO remove ! with fix to comparer nullability
        var ordered = versions.OrderBy(x => x, SemVersion.SortOrderComparer!);

        Assert.Equal([null, null, new SemVersion(1, 0), new SemVersion(2, 0)], ordered.ToArray());
    }

    [Fact]
    public void Contains()
    {
        SemVersion?[] versions = [null, new SemVersion(2, 0), null, new SemVersion(1, 0)];

        // TODO remove ! with fix to comparer nullability
        var result = versions.Contains(new SemVersion(2, 0), SemVersion.SortOrderComparer!);

        Assert.True(result);
    }
    #endregion
}
