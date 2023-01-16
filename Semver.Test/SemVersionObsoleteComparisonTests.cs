using System.Collections.Generic;
using Semver.Test.Helpers;
using Semver.Utility;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of any comparison or equality related functionality of <see cref="SemVersion"/>.
    /// This includes both standard comparison and precedence comparison. It also includes
    /// <see cref="SemVersion.GetHashCode()"/> because this is connected to equality.
    ///
    /// Because it is possible to construct invalid semver versions, the comparison
    /// tests must be based off constructing <see cref="SemVersion"/> rather than just
    /// using semver strings. The approach used is to work from a list of versions
    /// in their correct order and then compare versions within the list. To
    /// avoid issues with xUnit serialization of <see cref="SemVersion"/>, this
    /// is done within the test rather than using theory data.
    /// </summary>
    public class SemVersionObsoleteComparisonTests
    {
        public static readonly IReadOnlyList<SemVersion> VersionsInSortOrder = new List<SemVersion>()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            new SemVersion(-2),
            new SemVersion(-1, -1),
            new SemVersion(-1),
            new SemVersion(0, -1),
            new SemVersion(0, 0, -1),
            new SemVersion(0),
            new SemVersion(0, 0, 1, "13"),
            new SemVersion(0, 0, 1, "."),
            new SemVersion(0, 0, 1, ".."),
            new SemVersion(0, 0, 1, ".a"),
            new SemVersion(0, 0, 1, "b"),
            new SemVersion(0, 0, 1, "gamma.12.87"),
            new SemVersion(0, 0, 1, "gamma.12.87.1"),
            new SemVersion(0, 0, 1, "gamma.12.87.99"),
            new SemVersion(0, 0, 1, "gamma.12.87.-"),
            new SemVersion(0, 0, 1, "gamma.12.87.X"),
            new SemVersion(0, 0, 1, "gamma.12.88"),
            new SemVersion(0, 0, 1),
            new SemVersion(0, 0, 1, "", "12"),
            new SemVersion(0, 0, 1, "", "."),
            new SemVersion(0, 0, 1, "", ".."),
            new SemVersion(0, 0, 1, "", ".a"),
            new SemVersion(0, 0, 1, "", "bu"),
            new SemVersion(0, 0, 1, "", "build.12"),
            new SemVersion(0, 0, 1, "", "build.12.2"),
            new SemVersion(0, 0, 1, "", "build.13"),
            new SemVersion(0, 0, 1, "", "build.-"),
            new SemVersion(0, 0, 1, "", "uiui"),
            new SemVersion(0, 1, 1),
            new SemVersion(0, 2, 1),
            new SemVersion(1, 0, 0, "alpha"),
            new SemVersion(1, 0, 0, "alpha", "dev.123"),
            new SemVersion(1, 0, 0, "alpha", "😞"),
            new SemVersion(1, 0, 0, "alpha.1"),
            new SemVersion(1, 0, 0, "alpha.-"),
            new SemVersion(1, 0, 0, "alpha.beta"),
            new SemVersion(1, 0, 0, "beta"),
            new SemVersion(1, 0, 0, "beta", "dev.123"),
            new SemVersion(1, 0, 0, "beta.2"),
            new SemVersion(1, 0, 0, "beta.11"),
            new SemVersion(1, 0, 0, "rc.1"),
            new SemVersion(1, 0, 0, "😞"),
            new SemVersion(1),
            new SemVersion(1, 0, 0, "", "CA6B10F"),
            new SemVersion(1, 0, 10, "alpha"),
            new SemVersion(1, 2, 0, "alpha", "dev"),
            new SemVersion(1, 2, 0, "nightly"),
            new SemVersion(1, 2, 0, "nightly", "dev"),
            new SemVersion(1, 2, 0, "nightly2"),
            new SemVersion(1, 2),
            new SemVersion(1, 2, 0, "", "nightly"),
            new SemVersion(1, 2, 1, "-1"), // Doesn't match spec (issue #69)
            new SemVersion(1, 2, 1, "0"),
            new SemVersion(1, 2, 1, "99"),
            new SemVersion(1, 2, 1, "-"),
            new SemVersion(1, 2, 1, "-a"),
            new SemVersion(1, 2, 1, "0A"),
            new SemVersion(1, 2, 1, "A"),
            new SemVersion(1, 2, 1, "a"),
            new SemVersion(1, 2, 1),
            new SemVersion(1, 4),
            new SemVersion(2),
            new SemVersion(2, 1),
            new SemVersion(2, 1, 1),
#pragma warning restore CS0618 // Type or member is obsolete
        }.AsReadOnly();

        public static readonly IReadOnlyList<(SemVersion, SemVersion)> VersionPairs
            = VersionsInSortOrder.AllPairs().ToReadOnlyList();

        #region Equals
        [Fact]
        public void EqualsIdenticalTest()
        {
            foreach (var v in VersionsInSortOrder)
            {
                // Construct an identical version, but different instance
#pragma warning disable CS0618 // Type or member is obsolete
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
#pragma warning restore CS0618 // Type or member is obsolete
                Assert.True(v.Equals(identical), v.ToString());
            }
        }

        [Fact]
        public void EqualsSameTest()
        {
            foreach (var version in VersionsInSortOrder)
                Assert.True(version.Equals(version), version.ToString());
        }

        [Fact]
        public void EqualsDifferentTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                Assert.False(v1.Equals(v2), $"({v1}).Equals({v2})");
        }

        [Theory]
        [InlineData("1.2.3+01", "1.2.3+1")]
        [InlineData("1.2.3+a.01", "1.2.3+a.1")]
        [InlineData("1.2.3+a.000001", "1.2.3+a.1")]
        public void EqualsBuildLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.Equals(v2);

            Assert.False(r, $"({v1}).Equals({v2})");
        }

        [Fact]
        public void EqualsNullTest()
        {
            foreach (var version in VersionsInSortOrder)
                Assert.False(version.Equals(null), version.ToString());
        }

        [Fact]
        public void EqualsNonSemVersionTest()
        {
            var v = new SemVersion(1);
            var r = v.Equals(new object());

            Assert.False(r);
        }

        [Theory]
        [InlineData(null, "1.2.3", false)]
        [InlineData("1.2.3", null, false)]
        [InlineData(null, null, true)]
        public void StaticEqualsTest(string s1, string s2, bool expected)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = SemVersion.Equals(v1, v2);

            Assert.Equal(expected, r);
        }
        #endregion

        #region GetHashCode
        [Fact]
        public void GetHashCodeOfEqualTest()
        {
            foreach (var v in VersionsInSortOrder)
            {
                // Construct an identical version, but different instance
#pragma warning disable CS0618 // Type or member is obsolete
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
#pragma warning restore CS0618 // Type or member is obsolete
                Assert.True(v.GetHashCode() == identical.GetHashCode(), v.ToString());
            }
        }

        [Fact]
        public void GetHashCodeOfDifferentTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                Assert.False(v1.GetHashCode() == v2.GetHashCode(), $"({v1}).GetHashCode() == ({v2}).GetHashCode()");
        }
        #endregion

        #region Operators
        [Fact]
        public void EqualsOperatorIdenticalTest()
        {
            foreach (var v in VersionsInSortOrder)
            {
                // Construct an identical version, but different instance
#pragma warning disable CS0618 // Type or member is obsolete
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
#pragma warning restore CS0618 // Type or member is obsolete
                Assert.True(v == identical, v.ToString());
            }
        }

        [Fact]
        public void EqualsOperatorSameTest()
        {
            foreach (var version in VersionsInSortOrder)
#pragma warning disable CS1718 // Comparison made to same variable
                Assert.True(version == version, version.ToString());
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Fact]
        public void EqualsOperatorDifferentTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                Assert.False(v1 == v2, $"{v1} == {v2}");
        }

        [Theory]
        [InlineData("1.2.3+01", "1.2.3+1")]
        [InlineData("1.2.3+a.01", "1.2.3+a.1")]
        [InlineData("1.2.3+a.000001", "1.2.3+a.1")]
        public void EqualsOperatorBuildLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1 == v2;

            Assert.False(r, $"{v1} == {v2}");
        }

        [Fact]
        public void EqualsOperatorNullTest()
        {
            foreach (var version in VersionsInSortOrder)
                Assert.False(version == null, version.ToString());
        }

        [Theory]
        [InlineData(null, "1.2.3", false)]
        [InlineData("1.2.3", null, false)]
        [InlineData(null, null, true)]
        public void EqualsOperatorTest(string s1, string s2, bool expected)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = v1 == v2;

            Assert.Equal(expected, r);
        }

        [Fact]
        public void NotEqualsOperatorIdenticalTest()
        {
            foreach (var v in VersionsInSortOrder)
            {
                // Construct an identical version, but different instance
#pragma warning disable CS0618 // Type or member is obsolete
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
#pragma warning restore CS0618 // Type or member is obsolete
                Assert.False(v != identical, v.ToString());
            }
        }

        [Fact]
        public void NotEqualsOperatorSameTest()
        {
            foreach (var version in VersionsInSortOrder)
#pragma warning disable CS1718 // Comparison made to same variable
                Assert.False(version != version, version.ToString());
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Fact]
        public void NotEqualsOperatorDifferentTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                Assert.True(v1 != v2, $"{v1} != {v2}");
        }

        [Theory]
        [InlineData("1.2.3+01", "1.2.3+1")]
        [InlineData("1.2.3+a.01", "1.2.3+a.1")]
        [InlineData("1.2.3+a.000001", "1.2.3+a.1")]
        public void NotEqualsOperatorBuildLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1 != v2;

            Assert.True(r, $"{v1} != {v2}");
        }

        [Fact]
        public void NotEqualsOperatorNullTest()
        {
            foreach (var version in VersionsInSortOrder)
                Assert.True(version != null, version.ToString());
        }

        [Theory]
        [InlineData(null, "1.2.3", true)]
        [InlineData("1.2.3", null, true)]
        [InlineData(null, null, false)]
        public void NotEqualsOperatorTest(string s1, string s2, bool expected)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = v1 != v2;

            Assert.Equal(expected, r);
        }
        #endregion
    }
}
