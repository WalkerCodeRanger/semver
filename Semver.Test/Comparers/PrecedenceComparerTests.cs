using System;
using System.Collections.Generic;
using Semver.Comparers;
using Xunit;

namespace Semver.Test.Comparers;

public class PrecedenceComparerTests
{
    private static readonly ISemVersionComparer Comparer = PrecedenceComparer.Instance;

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

        var expected = EqualPrecedence(v1, v2);
        var actual = Comparer.Equals(v1, v2);
        if (expected)
            Assert.True(actual, $"Equals({v1}, {v2})");
        else
            Assert.False(actual, $"Equals({v1}, {v2})");
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

        var expected = EqualPrecedence(v1, v2);
        var actual = Comparer.GetHashCode(v1) == Comparer.GetHashCode(v2);
        if (expected)
            Assert.True(actual, $"GetHashCode({v1}) == GetHashCode({v2})");
        else
            Assert.False(actual, $"GetHashCode({v1}) == GetHashCode({v2})");
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

        var equal = EqualPrecedence(v1, v2);
        if (equal)
            Assert.True(Comparer.Compare(v1, v2) == 0, $"Compare({v1}, {v2}) == 0");
        else
            Assert.True(Comparer.Compare(v1, v2) == -1, $"Compare({v1}, {v2}) == -1");
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionPairs), MemberType = typeof(ComparerTestData))]
    public void CompareLesserTest(string left, string right)
    {
        var v1 = SemVersion.Parse(left);
        var v2 = SemVersion.Parse(right);

        var equal = EqualPrecedence(v1, v2);
        if (equal)
            Assert.True(Comparer.Compare(v2, v1) == 0, $"Compare({v2}, {v1}) == 0");
        else
            Assert.True(Comparer.Compare(v2, v1) == 1, $"Compare({v2}, {v1}) == 1");
    }

    [Fact]
    public void CompareObjectsTest()
    {
        var ex = Assert.Throws<ArgumentException>(()
            => Comparer.Compare(new object(), new object()));

        Assert.Equal("Type of argument is not compatible with the generic comparer.", ex.Message);
    }

    [Theory]
    [MemberData(nameof(ComparerTestData.VersionsInSortOrder), MemberType = typeof(ComparerTestData))]
    public void CompareNullTest(string version)
    {
        Assert.True(Comparer.Compare(version, null) == 1, $"Compare({version}, null) == 1");
        Assert.True(Comparer.Compare(null, version) == -1, $"Compare(null, {version}) == -1");
    }

    [Fact]
    public void CompareNullToNullTest()
    {
        Assert.True(Comparer.Compare(null!, null!) == 0, "Compare(null, null) == 0");
    }
    #endregion

    private static bool EqualPrecedence(SemVersion v1, SemVersion v2)
        => v1.Major == v2.Major
           && v1.Minor == v2.Minor
           && v1.Patch == v2.Patch
           && EqualPrecedence(v1.PrereleaseIdentifiers, v2.PrereleaseIdentifiers);

    private static bool EqualPrecedence(
        IReadOnlyList<PrereleaseIdentifier> xIdentifiers,
        IReadOnlyList<PrereleaseIdentifier> yIdentifiers)
    {
        if (xIdentifiers.Count != yIdentifiers.Count) return false;

        for (int i = 0; i < xIdentifiers.Count; i++)
            if (xIdentifiers[i] != yIdentifiers[i])
                return false;

        return true;
    }
}
