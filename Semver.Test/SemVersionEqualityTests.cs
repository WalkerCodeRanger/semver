using Semver.Test.Comparers;
using Semver.Test.Helpers;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of any equality related functionality of <see cref="SemVersion"/>. It also includes
    /// <see cref="SemVersion.GetHashCode()"/> because this is connected to equality.
    /// </summary>
    public class SemVersionEqualityTests
    {
        public static readonly TheoryData<string> Versions = new TheoryData<string>()
        {
            "0.0.0",
            "0.0.1-13",
            "0.0.1-b",
            "0.0.1-gamma.12.87",
            "0.0.1-gamma.12.87.1",
            "0.0.1-gamma.12.87.99",
            "0.0.1-gamma.12.87.-",
            "0.0.1-gamma.12.87.X",
            "0.0.1-gamma.12.88",
            "0.0.1",
            "0.0.1+12",
            "0.0.1+b",
            "0.0.1+build.-",
            "0.0.1+build.12",
            "0.0.1+build.12.2",
            "0.0.1+build.13",
            "0.0.1+uiui",
            "0.1.1",
            "0.2.1",
            "1.0.0-alpha",
            "1.0.0-alpha+dev.123",
            "1.0.0-beta",
            "1.0.0",
            "1.0.0+CA6B10F",
            "1.2.1-0",
            "1.2.1-1",
            "1.2.1--",
            "1.2.1--1", // Is alphanumeric instead of numeric
            "1.2.1-a",
            "1.2.3+a.000001",
            "1.2.3+a.01",
            "1.2.3+a.1",
        };

        public static readonly TheoryData<string, string> VersionPairs = Versions.AllPairs();

        #region Equals
        [Theory]
        [MemberData(nameof(Versions))]
        public void EqualsIdenticalTest(string version)
        {
            var v = SemVersion.Parse(version);
            // Construct an identical version, but different instance
            var identical = new SemVersion(v.Major, v.Minor, v.Patch,
                    v.PrereleaseIdentifiers, v.MetadataIdentifiers);
            Assert.True(v.Equals(identical), v.ToString());
        }

        [Theory]
        [MemberData(nameof(Versions))]
        public void EqualsSameTest(string version)
        {
            var v = SemVersion.Parse(version);
            Assert.True(v.Equals(v), v.ToString());
        }

        [Theory]
        [MemberData(nameof(VersionPairs))]
        public void EqualsDifferentTest(string left, string right)
        {
            var v1 = SemVersion.Parse(left);
            var v2 = SemVersion.Parse(right);
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

        [Theory]
        [MemberData(nameof(Versions))]
        public void EqualsNullTest(string version)
        {
            var v = SemVersion.Parse(version);
            Assert.False(v.Equals(null), v.ToString());
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
        public void StaticEqualsTest(string? s1, string? s2, bool expected)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = SemVersion.Equals(v1, v2);

            Assert.Equal(expected, r);
        }
        #endregion

        #region GetHashCode
        [Theory]
        [MemberData(nameof(Versions))]
        public void GetHashCodeOfEqualTest(string version)
        {
            var v = SemVersion.Parse(version);
            // Construct an identical version, but different instance
            var identical = new SemVersion(v.Major, v.Minor, v.Patch,
                v.PrereleaseIdentifiers, v.MetadataIdentifiers);
            Assert.True(v.GetHashCode() == identical.GetHashCode(), v.ToString());
        }

        [Theory]
        [MemberData(nameof(VersionPairs))]
        public void GetHashCodeOfDifferentTest(string left, string right)
        {
            var v1 = SemVersion.Parse(left);
            var v2 = SemVersion.Parse(right);
            Assert.False(v1.GetHashCode() == v2.GetHashCode(), $"({v1}).GetHashCode() == ({v2}).GetHashCode()");
        }
        #endregion

        #region Operators
        [Theory]
        [MemberData(nameof(Versions))]
        public void EqualsOperatorIdenticalTest(string version)
        {
            var v = SemVersion.Parse(version);
            // Construct an identical version, but different instance
            var identical = new SemVersion(v.Major, v.Minor, v.Patch,
                v.PrereleaseIdentifiers, v.MetadataIdentifiers);
            Assert.True(v == identical, v.ToString());
        }

        [Theory]
        [MemberData(nameof(Versions))]
        public void EqualsOperatorSameTest(string version)
        {
            var v = SemVersion.Parse(version);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(v == v, v.ToString());
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Theory]
        [MemberData(nameof(VersionPairs))]
        public void EqualsOperatorDifferentTest(string left, string right)
        {
            var v1 = SemVersion.Parse(left);
            var v2 = SemVersion.Parse(right);
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

        [Theory]
        [MemberData(nameof(Versions))]
        public void EqualsOperatorNullTest(string version)
        {
            var v = SemVersion.Parse(version);
            Assert.False(v == null, v.ToString());
        }

        [Theory]
        [InlineData(null, "1.2.3", false)]
        [InlineData("1.2.3", null, false)]
        [InlineData(null, null, true)]
        public void EqualsOperatorTest(string? s1, string? s2, bool expected)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = v1 == v2;

            Assert.Equal(expected, r);
        }

        [Theory]
        [MemberData(nameof(Versions))]
        public void NotEqualsOperatorIdenticalTest(string version)
        {
            var v = SemVersion.Parse(version);
            // Construct an identical version, but different instance
            var identical = new SemVersion(v.Major, v.Minor, v.Patch,
                v.PrereleaseIdentifiers, v.MetadataIdentifiers);
            Assert.False(v != identical, v.ToString());
        }

        [Theory]
        [MemberData(nameof(Versions))]
        public void NotEqualsOperatorSameTest(string version)
        {
            var v = SemVersion.Parse(version);
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.False(v != v, v.ToString());
#pragma warning restore CS1718 // Comparison made to same variable
        }

        [Theory]
        [MemberData(nameof(VersionPairs))]
        public void NotEqualsOperatorDifferentTest(string left, string right)
        {
            var v1 = SemVersion.Parse(left);
            var v2 = SemVersion.Parse(right);
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

        [Theory]
        [MemberData(nameof(Versions))]
        public void NotEqualsOperatorNullTest(string version)
        {
            var v = SemVersion.Parse(version);
            Assert.True(v != null, v.ToString());
        }

        [Theory]
        [InlineData(null, "1.2.3", true)]
        [InlineData("1.2.3", null, true)]
        [InlineData(null, null, false)]
        public void NotEqualsOperatorTest(string? s1, string? s2, bool expected)
        {
            var v1 = s1 is null ? null : SemVersion.Parse(s1);
            var v2 = s2 is null ? null : SemVersion.Parse(s2);

            var r = v1 != v2;

            Assert.Equal(expected, r);
        }
        #endregion
    }
}
