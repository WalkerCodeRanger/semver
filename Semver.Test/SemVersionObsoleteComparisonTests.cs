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
        // TODO this could use string now
        public static readonly IReadOnlyList<SemVersion> VersionsInSortOrder = new List<SemVersion>()
        {
            SemVersion.ParsedFrom(0),
            SemVersion.ParsedFrom(0, 0, 1, "13"),
            SemVersion.ParsedFrom(0, 0, 1, "b"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87.1"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87.99"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87.-"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.87.X"),
            SemVersion.ParsedFrom(0, 0, 1, "gamma.12.88"),
            SemVersion.ParsedFrom(0, 0, 1),
            SemVersion.ParsedFrom(0, 0, 1, "", "12"),
            SemVersion.ParsedFrom(0, 0, 1, "", "bu"),
            SemVersion.ParsedFrom(0, 0, 1, "", "build.12"),
            SemVersion.ParsedFrom(0, 0, 1, "", "build.12.2"),
            SemVersion.ParsedFrom(0, 0, 1, "", "build.13"),
            SemVersion.ParsedFrom(0, 0, 1, "", "build.-"),
            SemVersion.ParsedFrom(0, 0, 1, "", "uiui"),
            SemVersion.ParsedFrom(0, 1, 1),
            SemVersion.ParsedFrom(0, 2, 1),
            SemVersion.ParsedFrom(1, 0, 0, "alpha"),
            SemVersion.ParsedFrom(1, 0, 0, "alpha", "dev.123"),
            SemVersion.ParsedFrom(1, 0, 0, "alpha.1"),
            SemVersion.ParsedFrom(1, 0, 0, "alpha.-"),
            SemVersion.ParsedFrom(1, 0, 0, "alpha.beta"),
            SemVersion.ParsedFrom(1, 0, 0, "beta"),
            SemVersion.ParsedFrom(1, 0, 0, "beta", "dev.123"),
            SemVersion.ParsedFrom(1, 0, 0, "beta.2"),
            SemVersion.ParsedFrom(1, 0, 0, "beta.11"),
            SemVersion.ParsedFrom(1, 0, 0, "rc.1"),
            SemVersion.ParsedFrom(1),
            SemVersion.ParsedFrom(1, 0, 0, "", "CA6B10F"),
            SemVersion.ParsedFrom(1, 0, 10, "alpha"),
            SemVersion.ParsedFrom(1, 2, 0, "alpha", "dev"),
            SemVersion.ParsedFrom(1, 2, 0, "nightly"),
            SemVersion.ParsedFrom(1, 2, 0, "nightly", "dev"),
            SemVersion.ParsedFrom(1, 2, 0, "nightly2"),
            SemVersion.ParsedFrom(1, 2),
            SemVersion.ParsedFrom(1, 2, 0, "", "nightly"),
            SemVersion.ParsedFrom(1, 2, 1, "-1"), // Doesn't match spec (issue #69)
            SemVersion.ParsedFrom(1, 2, 1, "0"),
            SemVersion.ParsedFrom(1, 2, 1, "99"),
            SemVersion.ParsedFrom(1, 2, 1, "-"),
            SemVersion.ParsedFrom(1, 2, 1, "-a"),
            SemVersion.ParsedFrom(1, 2, 1, "0A"),
            SemVersion.ParsedFrom(1, 2, 1, "A"),
            SemVersion.ParsedFrom(1, 2, 1, "a"),
            SemVersion.ParsedFrom(1, 2, 1),
            SemVersion.ParsedFrom(1, 4),
            SemVersion.ParsedFrom(2),
            SemVersion.ParsedFrom(2, 1),
            SemVersion.ParsedFrom(2, 1, 1),
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
                var identical = new SemVersion(v.Major, v.Minor, v.Patch,
                    v.PrereleaseIdentifiers, v.MetadataIdentifiers);
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
                var identical = new SemVersion(v.Major, v.Minor, v.Patch,
                    v.PrereleaseIdentifiers, v.MetadataIdentifiers);
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
                var identical = new SemVersion(v.Major, v.Minor, v.Patch,
                    v.PrereleaseIdentifiers, v.MetadataIdentifiers);
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
                var identical = new SemVersion(v.Major, v.Minor, v.Patch,
                    v.PrereleaseIdentifiers, v.MetadataIdentifiers);
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
