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
        Assert.True(Comparer.Equals(null, null));
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionsInSortOrder), MemberType = typeof(ComparerTestData))]
    public void EqualsNullTest(string version)
    {
        var v = SemVersion.Parse(version);
        Assert.False(Comparer.Equals(v, null), $"Equals({v}, null)");
        Assert.False(Comparer.Equals(null, v), $"Equals(null, {v})");
    }

    [Fact]
    public void EqualsObjectsTest()
    {
        var ex = Assert.Throws<ArgumentException>(() => Comparer.Equals(new object(), new object()));

        Assert.Equal("Type of argument is not SemVersion.", ex.Message);
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

    [Fact]
    public void GetHashCodeNullTest()
    {
        // In .NET 4.8.1, `null` is an allowed value. In later frameworks it is disallowed.
        var actual = Comparer.GetHashCode(null!);

        Assert.Equal(0, actual);
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

        Assert.Equal("Type of argument is not SemVersion.", ex.Message);
    }

    [Fact]
    public void CompareNullTest()
    {
        foreach (var version in ComparerTestData.VersionsInSortOrder)
        {
            var v = SemVersion.Parse(version);
            Assert.True(Comparer.Compare(v, null) == 1, $"Compare({v}, null) == 1");
            Assert.True(Comparer.Compare(null, v) == -1, $"Compare(null, {v}) == -1");
        }

        Assert.True(Comparer.Compare(null, null) == 0, "Compare(null, null) == 0");
    }
    #endregion

    #region Passing to Operations
    [Fact]
    public void SortList()
    {
        var versions = new List<SemVersion?>() { null, new SemVersion(2, 0), null, new SemVersion(1, 0) };

        // Shows that it can be used on nullable elements
        versions.Sort(SemVersion.SortOrderComparer);

        Assert.Equal([null, null, new SemVersion(1, 0), new SemVersion(2, 0)], versions);
    }

    [Fact]
    public void OrderBy()
    {
        SemVersion?[] versions = [null, new SemVersion(2, 0), null, new SemVersion(1, 0)];

        // Shows that it can be used on nullable elements
        var ordered = versions.OrderBy(x => x, SemVersion.SortOrderComparer);

        Assert.Equal([null, null, new SemVersion(1, 0), new SemVersion(2, 0)], ordered.ToArray());
    }

    [Fact]
    public void Contains()
    {
        SemVersion?[] versions = [null, new SemVersion(2, 0), null, new SemVersion(1, 0)];

        // Shows that it can be used on nullable elements
        var result = versions.Contains(new SemVersion(2, 0), SemVersion.SortOrderComparer);

        Assert.True(result);
    }

    /// <summary>
    /// This test demonstrates that an <see cref="ISemVersionComparer"/> can be passed where a
    /// <see cref="IComparer{T}"/> of <see cref="SemVersion"/> is expected. This is important for
    /// v3.0.1 to not be a breaking change.
    /// </summary>
    [Fact]
    public void StoringInNonNullableComparer()
    {
        IComparer<SemVersion> comparer = Comparer;

        Assert.Same(Comparer, comparer);
    }
    #endregion
}
