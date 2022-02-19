using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of any comparison or equality related functionality of <see cref="SemVersion"/>.
    /// This includes both standard comparison and precedence comparison. It also includes
    /// <see cref="SemVersion.GetHashCode()"/> because this is connected to equality.
    ///
    /// Because it is possible to construct invalid semver versions, the comparison
    /// tests must be based of constructing <see cref="SemVersion"/> rather than just
    /// using semver strings. The approach used it to work from a list of versions
    /// in their correct order and then compare versions within the list. To
    /// avoid issues with xUnit serialization of <see cref="SemVersion"/>, this
    /// is done within the test rather than using theory data.
    /// </summary>
    public class SemVersionComparisonTests
    {
        public static readonly IReadOnlyList<SemVersion> VersionsInOrder = new List<SemVersion>()
        {
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
            new SemVersion(0, 0, 1, "gamma.12.87.X"),
            new SemVersion(0, 0, 1, "gamma.12.88"),
            new SemVersion(0, 0, 1, "", "12"),
            new SemVersion(0, 0, 1, "", "."),
            new SemVersion(0, 0, 1, "", ".."),
            new SemVersion(0, 0, 1, "", ".a"),
            new SemVersion(0, 0, 1, "", "bu"),
            new SemVersion(0, 0, 1, "", "build.12"),
            new SemVersion(0, 0, 1, "", "build.12.2"),
            new SemVersion(0, 0, 1, "", "build.13"),
            new SemVersion(0, 0, 1, "", "uiui"),
            new SemVersion(0, 1, 1),
            new SemVersion(0, 2, 1),
            new SemVersion(1, 0, 0, "alpha"),
            new SemVersion(1, 0, 0, "alpha", "dev.123"),
            new SemVersion(1, 0, 0, "alpha", "😞"),
            new SemVersion(1, 0, 0, "alpha.1"),
            new SemVersion(1, 0, 0, "alpha.beta"),
            new SemVersion(1, 0, 0, "beta"),
            new SemVersion(1, 0, 0, "beta", "dev.123"),
            new SemVersion(1, 0, 0, "beta.2"),
            new SemVersion(1, 0, 0, "beta.11"),
            new SemVersion(1, 0, 0, "rc.1"),
            new SemVersion(1, 0, 0, "😞"),
            new SemVersion(1),
            new SemVersion(1, 0, 10, "alpha"),
            new SemVersion(1, 2, 0, "alpha", "dev"),
            new SemVersion(1, 2, 0, "nightly"),
            new SemVersion(1, 2, 0, "nightly", "dev"),
            new SemVersion(1, 2, 0, "nightly2"),
            new SemVersion(1, 2),
            new SemVersion(1, 2, 0, "", "nightly"),
            new SemVersion(1, 2, 1, "0"),
            new SemVersion(1, 2, 1, "99"),
            new SemVersion(1, 2, 1, "-"),
            new SemVersion(1, 2, 1, "0A"),
            new SemVersion(1, 2, 1, "A"),
            new SemVersion(1, 2, 1, "a"),
            new SemVersion(1, 2, 1),
            new SemVersion(1, 4),
            new SemVersion(2),
            new SemVersion(2, 1),
            new SemVersion(2, 1, 1),
        }.AsReadOnly();

        public static readonly IReadOnlyList<(SemVersion, SemVersion)> VersionPairs =
            AllPairs(VersionsInOrder).ToList().AsReadOnly();

        #region Equals
        [Fact]
        public void EqualsIdenticalTest()
        {
            foreach (var v in VersionsInOrder)
            {
                // Construct an identical version, but different instance
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
                Assert.True(v.Equals(identical), v.ToString());
            }
        }

        [Fact]
        public void EqualsSameTest()
        {
            foreach (var version in VersionsInOrder)
                Assert.True(version.Equals(version), version.ToString());
        }

        [Fact]
        public void EqualsDifferentTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                Assert.False(v1.Equals(v2), $"({v1}).Equals({v2})");
        }

        [Theory]
        [InlineData("1.2.3-01", "1.2.3-1")]
        [InlineData("1.2.3-a.01", "1.2.3-a.1")]
        [InlineData("1.2.3-a.000001", "1.2.3-a.1")]
        public void EqualsPrereleaseLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.Equals(v2);

            Assert.False(r, $"({v1}).Equals({v2})");
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
            foreach (var version in VersionsInOrder)
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
            foreach (var v in VersionsInOrder)
            {
                // Construct an identical version, but different instance
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
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

        #region CompareTo
        [Fact]
        public void CompareToIdenticalTest()
        {
            foreach (var v in VersionsInOrder)
            {
                // Construct an identical version, but different instance
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
                Assert.True(v.CompareTo(identical) == 0, v.ToString());
            }
        }

        [Fact]
        public void CompareToSameTest()
        {
            foreach (var v in VersionsInOrder)
                Assert.True(v.CompareTo(v) == 0, v.ToString());
        }

        [Fact]
        public void CompareToGreaterTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                Assert.True(v1.CompareTo(v2) < 0, $"({v1}).CompareTo({v2})");
        }

        [Fact]
        public void CompareToLesserTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                Assert.True(v2.CompareTo(v1) > 0, $"({v2}).CompareTo({v1})");
        }

        [Theory]
        // TODO comparison distinguished by string order currently gives numbers outside -1,0,1 (issue #26)
        [InlineData("1.0.0-beta.11", "1.0.0-rc.1", -16)]
        public void CompareToStringOrderTest(string s1, string s2, int expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var v1 = SemVersion.Parse(s1, true);
            var v2 = SemVersion.Parse(s2, true);
#pragma warning restore CS0618 // Type or member is obsolete

            var r1 = v1.CompareTo(v2);
            var r2 = v2.CompareTo(v1);

            Assert.Equal(expected, r1);
            Assert.Equal(-expected, r2);
        }

        [Theory]
        [InlineData("1.2.3-01", "1.2.3-1")]
        [InlineData("1.2.3-a.01", "1.2.3-a.1")]
        [InlineData("1.2.3-a.000001", "1.2.3-a.1")]
        public void CompareToPrereleaseLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.CompareTo(v2);

            Assert.True(r == 0, $"({v1}).CompareTo({v2})");
        }

        [Theory]
        [InlineData("1.2.3+01", "1.2.3+1")]
        [InlineData("1.2.3+a.01", "1.2.3+a.1")]
        [InlineData("1.2.3+a.000001", "1.2.3+a.1")]
        public void CompareToBuildLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.CompareTo(v2);

            Assert.True(r == 0, $"({v1}).CompareTo({v2})");
        }

        [Fact]
        public void CompareToSemVersionAsObjectTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(1);

            var c = v1.CompareTo((object)v2);

            Assert.Equal(0, c);
        }

        [Fact]
        public void CompareToNonSemVersionTest()
        {
            var v = new SemVersion(1);
            // TODO issue #39 should throw argument exception
            var ex = Assert.Throws<InvalidCastException>(() => v.CompareTo(new object()));

            Assert.Equal("Unable to cast object of type 'System.Object' to type 'Semver.SemVersion'.", ex.Message);
        }

        [Fact]
        public void CompareToNullTest()
        {
            foreach (var version in VersionsInOrder)
                Assert.True(version.CompareTo(null) > 0, version.ToString());
        }

        [Theory]
        [InlineData(null, "1.0.0", -1)]
        [InlineData("1.0.0", null, 1)]
        [InlineData(null, null, 0)]
        public void StaticCompareTest(string s1, string s2, int expected)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = SemVersion.Compare(v1, v2);

            Assert.Equal(expected, r);
        }
        #endregion

        #region Precedence Matches
        [Fact]
        public void PrecedenceMatchesIdenticalTest()
        {
            foreach (var v in VersionsInOrder)
            {
                // Construct an identical version, but different instance
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
                Assert.True(v.PrecedenceMatches(identical), v.ToString());
            }
        }

        [Fact]
        public void PrecedenceMatchesSameTest()
        {
            foreach (var version in VersionsInOrder)
                Assert.True(version.PrecedenceMatches(version), version.ToString());
        }

        [Fact]
        public void PrecedenceMatchesDifferentTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                if (DifferByMetadataOnly(v1, v2))
                    Assert.True(v1.PrecedenceMatches(v2), $"({v1}).PrecedenceMatches({v2})");
                else
                    Assert.False(v1.PrecedenceMatches(v2), $"({v1}).PrecedenceMatches({v2})");
        }

        [Theory]
        [InlineData("1.2.3-01", "1.2.3-1")]
        [InlineData("1.2.3-a.01", "1.2.3-a.1")]
        [InlineData("1.2.3-a.000001", "1.2.3-a.1")]
        public void PrecedenceMatchesPrereleaseLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.PrecedenceMatches(v2);

            Assert.True(r, $"({v1}).PrecedenceMatches({v2})");
        }

        [Theory]
        [InlineData("1.2.3+01", "1.2.3+1")]
        [InlineData("1.2.3+a.01", "1.2.3+a.1")]
        [InlineData("1.2.3+a.000001", "1.2.3+a.1")]
        public void PrecedenceMatchesBuildLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.PrecedenceMatches(v2);

            Assert.True(r, $"({v1}).PrecedenceMatches({v2})");
        }

        [Fact]
        public void PrecedenceMatchesNullTest()
        {
            foreach (var version in VersionsInOrder)
                Assert.False(version.PrecedenceMatches(null), version.ToString());
        }
        #endregion

        #region CompareByPrecedence
        [Fact]
        public void CompareByPrecedenceIdenticalTest()
        {
            foreach (var v in VersionsInOrder)
            {
                // Construct an identical version, but different instance
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
                Assert.True(v.CompareByPrecedence(identical) == 0, v.ToString());
            }
        }

        [Fact]
        public void CompareByPrecedenceSameTest()
        {
            foreach (var v in VersionsInOrder)
                Assert.True(v.CompareByPrecedence(v) == 0, v.ToString());
        }

        [Fact]
        public void CompareByPrecedenceGreaterTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                if (DifferByMetadataOnly(v1, v2))
                    Assert.True(v1.CompareByPrecedence(v2) == 0, $"({v1}).CompareTo({v2})");
                else
                    Assert.True(v1.CompareByPrecedence(v2) < 0, $"({v1}).CompareTo({v2})");
        }

        [Fact]
        public void CompareByPrecedenceLesserTest()
        {
            foreach (var (v1, v2) in VersionPairs)
                if (DifferByMetadataOnly(v1, v2))
                    Assert.True(v2.CompareByPrecedence(v1) == 0, $"({v2}).CompareTo({v1})");
                else
                    Assert.True(v2.CompareByPrecedence(v1) > 0, $"({v2}).CompareTo({v1})");
        }

        [Theory]
        // TODO comparison distinguished by string order currently gives numbers outside -1,0,1 (issue #26)
        [InlineData("1.0.0-beta.11", "1.0.0-rc.1", -16)]
        public void CompareByPrecedenceStringOrderTest(string s1, string s2, int expected)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var v1 = SemVersion.Parse(s1, true);
            var v2 = SemVersion.Parse(s2, true);
#pragma warning restore CS0618 // Type or member is obsolete

            var r1 = v1.CompareByPrecedence(v2);
            var r2 = v2.CompareByPrecedence(v1);

            Assert.Equal(expected, r1);
            Assert.Equal(-expected, r2);
        }

        [Theory]
        [InlineData("1.2.3-01", "1.2.3-1")]
        [InlineData("1.2.3-a.01", "1.2.3-a.1")]
        [InlineData("1.2.3-a.000001", "1.2.3-a.1")]
        public void CompareByPrecedencePrereleaseLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.CompareByPrecedence(v2);

            Assert.True(r == 0, $"({v1}).CompareTo({v2})");
        }

        [Theory]
        [InlineData("1.2.3+01", "1.2.3+1")]
        [InlineData("1.2.3+a.01", "1.2.3+a.1")]
        [InlineData("1.2.3+a.000001", "1.2.3+a.1")]
        public void CompareByPrecedenceBuildLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1.CompareByPrecedence(v2);

            Assert.True(r == 0, $"({v1}).CompareTo({v2})");
        }

        [Fact]
        public void CompareByPrecedenceNullTest()
        {
            foreach (var version in VersionsInOrder)
                Assert.True(version.CompareByPrecedence(null) > 0, version.ToString());
        }
        #endregion

        #region Operators
        [Fact]
        public void EqualsOperatorIdenticalTest()
        {
            foreach (var v in VersionsInOrder)
            {
                // Construct an identical version, but different instance
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
                Assert.True(v == identical, v.ToString());
            }
        }

        [Fact]
        public void EqualsOperatorSameTest()
        {
            foreach (var version in VersionsInOrder)
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
        [InlineData("1.2.3-01", "1.2.3-1")]
        [InlineData("1.2.3-a.01", "1.2.3-a.1")]
        [InlineData("1.2.3-a.000001", "1.2.3-a.1")]
        public void EqualsOperatorPrereleaseLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1 == v2;

            Assert.False(r, $"{v1} == {v2}");
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
            foreach (var version in VersionsInOrder)
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
            foreach (var v in VersionsInOrder)
            {
                // Construct an identical version, but different instance
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
                Assert.False(v != identical, v.ToString());
            }
        }

        [Fact]
        public void NotEqualsOperatorSameTest()
        {
            foreach (var version in VersionsInOrder)
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
        [InlineData("1.2.3-01", "1.2.3-1")]
        [InlineData("1.2.3-a.01", "1.2.3-a.1")]
        [InlineData("1.2.3-a.000001", "1.2.3-a.1")]
        public void NotEqualsOperatorPrereleaseLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            var r = v1 != v2;

            Assert.True(r, $"{v1} != {v2}");
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
            foreach (var version in VersionsInOrder)
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

        [Fact]
        public void ComparisonOperatorsIdenticalTest()
        {
            foreach (var v in VersionsInOrder)
            {
                // Construct an identical version, but different instance
                var identical = new SemVersion(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);
                Assert.False(v < identical, $"{v} < {identical}");
                Assert.True(v <= identical, $"{v} <= {identical}");
                Assert.False(v > identical, $"{v} > {identical}");
                Assert.True(v >= identical, $"{v} >= {identical}");
            }
        }

        [Fact]
        public void ComparisonOperatorsSameTest()
        {
            foreach (var v in VersionsInOrder)
            {
#pragma warning disable CS1718 // Comparison made to same variable
                Assert.False(v < v, $"{v} < {v}");
                Assert.True(v <= v, $"{v} <= {v}");
                Assert.False(v > v, $"{v} > {v}");
                Assert.True(v >= v, $"{v} >= {v}");
#pragma warning restore CS1718 // Comparison made to same variable
            }
        }

        [Fact]
        public void ComparisonOperatorsLesserToGreaterTest()
        {
            foreach (var (v1, v2) in VersionPairs)
            {
                Assert.True(v1 < v2, $"{v1} < {v2}");
                Assert.True(v1 <= v2, $"{v1} <= {v2}");
                Assert.False(v1 > v2, $"{v1} > {v2}");
                Assert.False(v1 >= v2, $"{v1} >= {v2}");
            }
        }

        [Fact]
        public void ComparisonOperatorsGreaterToLesserTest()
        {
            foreach (var (v1, v2) in VersionPairs)
            {
                Assert.False(v2 < v1, $"{v2} < {v1}");
                Assert.False(v2 <= v1, $"{v2} <= {v1}");
                Assert.True(v2 > v1, $"{v2} > {v1}");
                Assert.True(v2 >= v1, $"{v2} >= {v1}");
            }
        }

        [Theory]
        [InlineData("1.2.3-01", "1.2.3-1")]
        [InlineData("1.2.3-a.01", "1.2.3-a.1")]
        [InlineData("1.2.3-a.000001", "1.2.3-a.1")]
        public void ComparisonOperatorsPrereleaseLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            // TODO it is a bug that none of these comparisons are true (issue #53)
            Assert.False(v1 < v2, $"{v1} < {v2}");
            Assert.False(v1 <= v2, $"{v1} <= {v2}");
            Assert.False(v1 > v2, $"{v1} > {v2}");
            Assert.False(v1 >= v2, $"{v1} >= {v2}");
        }

        [Theory]
        [InlineData("1.2.3+01", "1.2.3+1")]
        [InlineData("1.2.3+a.01", "1.2.3+a.1")]
        [InlineData("1.2.3+a.000001", "1.2.3+a.1")]
        public void ComparisonOperatorsBuildLeadingZerosTest(string s1, string s2)
        {
            var v1 = SemVersion.Parse(s1);
            var v2 = SemVersion.Parse(s2);

            // TODO it is a bug that none of these comparisons are true (issue #53)
            Assert.False(v1 < v2, $"{v1} < {v2}");
            Assert.False(v1 <= v2, $"{v1} <= {v2}");
            Assert.False(v1 > v2, $"{v1} > {v2}");
            Assert.False(v1 >= v2, $"{v1} >= {v2}");
        }

        [Fact]
        public void ComparisonOperatorsNullToValueTest()
        {
            var v1 = default(SemVersion);
            var v2 = new SemVersion(1);

            Assert.True(v1 < v2, $"{v1} < {v2}");
            Assert.True(v1 <= v2, $"{v1} <= {v2}");
            Assert.False(v1 > v2, $"{v1} > {v2}");
            Assert.False(v1 >= v2, $"{v1} >= {v2}");
        }

        [Fact]
        public void ComparisonOperatorsValueToNullTest()
        {
            var v1 = new SemVersion(1);
            var v2 = default(SemVersion);

            Assert.False(v1 < v2, $"{v1} < {v2}");
            Assert.False(v1 <= v2, $"{v1} <= {v2}");
            Assert.True(v1 > v2, $"{v1} > {v2}");
            Assert.True(v1 >= v2, $"{v1} >= {v2}");
        }

        [Fact]
        public void ComparisonOperatorsNullToNullTest()
        {
            var v1 = default(SemVersion);
            var v2 = default(SemVersion);

            Assert.False(v1 < v2, $"{v1} < {v2}");
            Assert.True(v1 <= v2, $"{v1} <= {v2}");
            Assert.False(v1 > v2, $"{v1} > {v2}");
            Assert.True(v1 >= v2, $"{v1} >= {v2}");
        }
        #endregion

        private static IEnumerable<(SemVersion, SemVersion)> AllPairs(IReadOnlyList<SemVersion> versions)
        {
            for (var i = 0; i < versions.Count; i++)
                for (var j = i + 1; j < versions.Count; j++)
                    yield return (versions[i], versions[j]);
        }

        private static bool DifferByMetadataOnly(SemVersion v1, SemVersion v2)
        {
            // Note: can't use WithoutMetadata because this comparision is performed on invalid SemVersion instances
#pragma warning disable CS0618 // Type or member is obsolete
            return v1.Change(build: "").Equals(v2.Change(build: ""));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
