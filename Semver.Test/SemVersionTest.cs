using System;
#if !NETSTANDARD
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endif
using Xunit;

namespace Semver.Test
{
    public class SemverTests
    {
        [Fact]
        public void CompareTestWithStrings1()
        {
            Assert.True(SemVersion.Equals("1.0.0", "1"));
        }

        [Fact]
        public void CompareTestWithStrings2()
        {
            var v = new SemVersion(1, 0, 0);
            Assert.True(v < "1.1");
        }

        [Fact]
        public void CompareTestWithStrings3()
        {
            var v = new SemVersion(1, 2);
            Assert.True(v > "1.0.0");
        }

        #region Constructors
        [Fact]
        public void ConstructSemVersionDefaultValuesTest()
        {
            var v = new SemVersion(1);

            Assert.Equal(1, v.Major);
            Assert.Equal(0, v.Minor);
            Assert.Equal(0, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Equal("", v.Build);
        }

        [Theory]
        [InlineData(1, 2, 3, "a", "b")]
        [InlineData(1, 2, 3, null, null)]
        // TODO these should be invalid and throw argument exceptions issue #40
        [InlineData(-1, 0, 0, "", "")]
        [InlineData(0, -1, 0, "", "")]
        [InlineData(0, 0, -1, "", "")]
        // TODO these should be invalid and throw argument exceptions issue #41
        [InlineData(1, 2, 3, "😞", "b")]
        [InlineData(1, 2, 3, "a", "😞")]
        [InlineData(1, 2, 3, "01", "b")]
        [InlineData(1, 2, 3, "a.01", "b")]
        [InlineData(1, 2, 3, "a..empty", "b")]
        [InlineData(1, 2, 3, "a", "b..empty")]
        public void ConstructSemVersionTest(int major, int minor, int patch, string prerelease, string build)
        {
            var v = new SemVersion(major, minor, patch, prerelease, build);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease ?? "", v.Prerelease);
            Assert.Equal(build ?? "", v.Build);
        }

        [Theory]
        [InlineData(0, 0, 0, 0)]
        // TODO this is a strange conversion (issue #32)
        [InlineData(1, 2, 0, 3)]
        [InlineData(1, 2, 4, 3)]
        public void ConstructSemVersionFromSystemVersionTest(int major, int minor, int build, int revision)
        {
            var nonSemanticVersion = new Version(major, minor, build, revision);

            var v = new SemVersion(nonSemanticVersion);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(revision, v.Patch);
            Assert.Equal("", v.Prerelease);
            Assert.Equal(build > 0 ? build.ToString() : "", v.Build);
        }

        [Fact]
        public void ConstructSemVersionFromNullSystemVersionTest()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SemVersion(null));

            Assert.Equal("Value cannot be null.\r\nParameter name: version", ex.Message);
        }
        #endregion

        #region Parsing
        [Theory]
        // Major, Minor, Patch
        [InlineData("1.2.45-alpha-beta+nightly.23.43-bla", 1, 2, 45, "alpha-beta", "nightly.23.43-bla")]
        [InlineData("1.2.45-alpha+nightly.23", 1, 2, 45, "alpha", "nightly.23")]
        [InlineData("3.2.1-beta", 3, 2, 1, "beta", "")]
        [InlineData("2.0.0+nightly.23.43-bla", 2, 0, 0, "", "nightly.23.43-bla")]
        [InlineData("5.6.7", 5, 6, 7, "", "")]
        // Major, Minor
        [InlineData("1.6-zeta.5+nightly.23.43-bla", 1, 6, 0, "zeta.5", "nightly.23.43-bla")]
        [InlineData("2.0+nightly.23.43-bla", 2, 0, 0, "", "nightly.23.43-bla")]
        [InlineData("2.1-alpha", 2, 1, 0, "alpha", "")]
        [InlineData("5.6+nightly.23.43-bla", 5, 6, 0, "", "nightly.23.43-bla")]
        [InlineData("3.2", 3, 2, 0, "", "")]
        // Major
        [InlineData("1-beta+dev.123", 1, 0, 0, "beta", "dev.123")]
        [InlineData("7-rc.1", 7, 0, 0, "rc.1", "")]
        [InlineData("6+sha.a3456b", 6, 0, 0, "", "sha.a3456b")]
        [InlineData("64", 64, 0, 0, "", "")]
        // Leading Zeros
        [InlineData("01.2.3", 1, 2, 3, "", "")]
        [InlineData("1.02.3", 1, 2, 3, "", "")]
        [InlineData("1.2.03", 1, 2, 3, "", "")]
        // TODO these should have leading zeros removed
        [InlineData("1.2.3-01", 1, 2, 3, "01", "")]
        [InlineData("1.2.3-a.01", 1, 2, 3, "a.01", "")]
        // Valid unusual versions
        [InlineData("1.0.0--ci.1", 1, 0, 0, "-ci.1", "")]
        [InlineData("1.0.0-0A", 1, 0, 0, "0A", "")]
        public void ParseLooseValidTest(string versionString, int major, int minor, int patch, string prerelease, string build)
        {
            var v = SemVersion.Parse(versionString);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease, v.Prerelease);
            Assert.Equal(build, v.Build);
        }

        [Theory]
        [InlineData("ui-2.1-alpha", "Invalid version.\r\nParameter name: version")]
        [InlineData("v1.2.3", "Invalid version.\r\nParameter name: version")]
        [InlineData("V1.2.3", "Invalid version.\r\nParameter name: version")]
        [InlineData("", "Invalid version.\r\nParameter name: version")]
        [InlineData("1.0.0-a@", "Invalid version.\r\nParameter name: version")]
        [InlineData("1.0.0-á", "Invalid version.\r\nParameter name: version")]
        [InlineData("1.0.0+a@", "Invalid version.\r\nParameter name: version")]
        [InlineData("1.0.0+á", "Invalid version.\r\nParameter name: version")]
        public void ParseLooseInvalidThrowsArgumentExceptionTest(string versionString, string expectedMsg)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersion.Parse(versionString));
            Assert.Equal(expectedMsg, ex.Message);
        }

        [Theory]
        // int.Max+1
        [InlineData("2147483648.2.3", "Value was either too large or too small for an Int32.")]
        [InlineData("1.2147483648.3", "Value was either too large or too small for an Int32.")]
        [InlineData("1.2.2147483648", "Value was either too large or too small for an Int32.")]
        public void ParseLooseInvalidThrowsOverflowExceptionTest(string versionString, string expectedMsg)
        {
            var ex = Assert.Throws<OverflowException>(() => SemVersion.Parse(versionString));
            Assert.Equal(expectedMsg, ex.Message);
        }

        [Fact]
        public void ParseLooseNullTest()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersion.Parse(null));
            // TODO that is a strange error message, should be version
            Assert.Equal("Value cannot be null.\r\nParameter name: input", ex.Message);
        }

        [Theory]
        [InlineData("1.3.4", 1, 3, 4, "", "")]
        // TODO these invalid versions are accepted (issue #16)
        [InlineData("01.2.3", 1, 2, 3, "", "")]
        [InlineData("1.02.3", 1, 2, 3, "", "")]
        [InlineData("1.2.03", 1, 2, 3, "", "")]
        [InlineData("1.0.0-01", 1, 0, 0, "01", "")]
        [InlineData("1.0.0-alpha.01", 1, 0, 0, "alpha.01", "")]
        public void ParseStrictValidTest(string versionString, int major, int minor, int patch, string prerelease, string build)
        {
            var v = SemVersion.Parse(versionString, true);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease, v.Prerelease);
            Assert.Equal(build, v.Build);
        }

        // TODO These exceptions should be FormatException etc.
        [Theory]
        [InlineData("1.0.0-a@", "Invalid version.\r\nParameter name: version")]
        [InlineData("1.0.0-á", "Invalid version.\r\nParameter name: version")]
        [InlineData("1.0.0+a@", "Invalid version.\r\nParameter name: version")]
        [InlineData("1.0.0+á", "Invalid version.\r\nParameter name: version")]
        public void ParseStrictInvalidThrowsArgumentExceptionTest(string versionString, string expectedMsg)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersion.Parse(versionString, true));
            Assert.Equal(expectedMsg, ex.Message);
        }

        // TODO These exceptions should be FormatException etc.
        [Theory]
        [InlineData("1", "Invalid version (no minor version given in strict mode)")]
        [InlineData("1.3", "Invalid version (no patch version given in strict mode)")]
        [InlineData("1.3-alpha", "Invalid version (no patch version given in strict mode)")]
        public void ParseStrictInvalidThrowsInvalidOperationTest(string versionString, string expectedMsg)
        {
            var ex = Assert.Throws<InvalidOperationException>(() => SemVersion.Parse(versionString, true));
            Assert.Equal(expectedMsg, ex.Message);
        }

        [Theory]
        // Major, Minor, Patch
        [InlineData("1.2.45-alpha-beta+nightly.23.43-bla", 1, 2, 45, "alpha-beta", "nightly.23.43-bla")]
        [InlineData("1.2.45-alpha+nightly.23", 1, 2, 45, "alpha", "nightly.23")]
        [InlineData("3.2.1-beta", 3, 2, 1, "beta", "")]
        [InlineData("2.0.0+nightly.23.43-bla", 2, 0, 0, "", "nightly.23.43-bla")]
        [InlineData("5.6.7", 5, 6, 7, "", "")]
        // Major, Minor
        [InlineData("1.6-zeta.5+nightly.23.43-bla", 1, 6, 0, "zeta.5", "nightly.23.43-bla")]
        [InlineData("2.0+nightly.23.43-bla", 2, 0, 0, "", "nightly.23.43-bla")]
        [InlineData("2.1-alpha", 2, 1, 0, "alpha", "")]
        [InlineData("5.6+nightly.23.43-bla", 5, 6, 0, "", "nightly.23.43-bla")]
        [InlineData("3.2", 3, 2, 0, "", "")]
        // Major
        [InlineData("1-beta+dev.123", 1, 0, 0, "beta", "dev.123")]
        [InlineData("7-rc.1", 7, 0, 0, "rc.1", "")]
        [InlineData("6+sha.a3456b", 6, 0, 0, "", "sha.a3456b")]
        [InlineData("64", 64, 0, 0, "", "")]
        // Leading Zeros
        [InlineData("01.2.3", 1, 2, 3, "", "")]
        [InlineData("1.02.3", 1, 2, 3, "", "")]
        [InlineData("1.2.03", 1, 2, 3, "", "")]
        // TODO these should have leading zeros removed
        [InlineData("1.2.3-01", 1, 2, 3, "01", "")]
        [InlineData("1.2.3-a.01", 1, 2, 3, "a.01", "")]
        public void TryParseLooseValidTest(string versionString, int major, int minor, int patch, string prerelease, string build)
        {
            Assert.True(SemVersion.TryParse(versionString, out var v));

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease, v.Prerelease);
            Assert.Equal(build, v.Build);
        }

        [Theory]
        [InlineData("ui-2.1-alpha")]
        [InlineData("v1.2.3")]
        [InlineData("V1.2.3")]
        [InlineData("")]
        [InlineData(null)]
        // int.Max+1
        [InlineData("2147483648.2.3")]
        [InlineData("1.2147483648.3")]
        [InlineData("1.2.2147483648")]
        // Illegal characters
        [InlineData("1.0.0-a@")]
        [InlineData("1.0.0-á")]
        [InlineData("1.0.0+a@")]
        [InlineData("1.0.0+á")]
        public void TryParseLooseInvalidTest(string versionString)
        {
            Assert.False(SemVersion.TryParse(versionString, out _));
        }

        [Theory]
        [InlineData("1.3.4", 1, 3, 4, "", "")]
        // TODO these invalid versions are accepted (issue #16)
        [InlineData("01.2.3", 1, 2, 3, "", "")]
        [InlineData("1.02.3", 1, 2, 3, "", "")]
        [InlineData("1.2.03", 1, 2, 3, "", "")]
        [InlineData("1.0.0-01", 1, 0, 0, "01", "")]
        [InlineData("1.0.0-alpha.01", 1, 0, 0, "alpha.01", "")]
        public void TryParseStrictValidTest(string versionString, int major, int minor, int patch, string prerelease, string build)
        {
            Assert.True(SemVersion.TryParse(versionString, out var v));

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease, v.Prerelease);
            Assert.Equal(build, v.Build);
        }

        [Theory]
        [InlineData("1.2")]
        [InlineData("1.0.0-a@")]
        [InlineData("1.0.0-á")]
        [InlineData("1.0.0+a@")]
        [InlineData("1.0.0+á")]
        [InlineData("1")]
        [InlineData("1.3")]
        [InlineData("1.3-alpha")]
        [InlineData(null)]
        public void TryParseStrictInvalidTest(string versionString)
        {
            Assert.False(SemVersion.TryParse(versionString, out _, true));
        }
        #endregion

        [Fact]
        public void ToStringTest()
        {
            var version = new SemVersion(1, 2, 3, "beta-x.2", "dev-mha.120");

            Assert.Equal("1.2.3-beta-x.2+dev-mha.120", version.ToString());
        }

        #region Equality 
        [Fact]
        public void EqualTest1()
        {
            var v1 = new SemVersion(1, 2, build: "nightly");
            var v2 = new SemVersion(1, 2, build: "nightly");

            var r = v1.Equals(v2);
            Assert.True(r);
        }

        [Fact]
        public void EqualTest2()
        {
            var v1 = new SemVersion(1, 2, prerelease: "alpha", build: "dev");
            var v2 = new SemVersion(1, 2, prerelease: "alpha", build: "dev");

            var r = v1.Equals(v2);
            Assert.True(r);
        }

        [Fact]
        public void EqualTest3()
        {
            var v1 = SemVersion.Parse("1.2-nightly+dev");
            var v2 = SemVersion.Parse("1.2.0-nightly");

            var r = v1.Equals(v2);
            Assert.False(r);
        }

        [Fact]
        public void EqualTest4()
        {
            var v1 = SemVersion.Parse("1.2-nightly");
            var v2 = SemVersion.Parse("1.2.0-nightly2");

            var r = v1.Equals(v2);
            Assert.False(r);
        }

        [Fact]
        public void EqualTest5()
        {
            var v1 = SemVersion.Parse("1.2.1");
            var v2 = SemVersion.Parse("1.2.0");

            var r = v1.Equals(v2);
            Assert.False(r);
        }

        [Fact]
        public void EqualTest6()
        {
            var v1 = SemVersion.Parse("1.4.0");
            var v2 = SemVersion.Parse("1.2.0");

            var r = v1.Equals(v2);
            Assert.False(r);
        }

        [Fact]
        public void EqualByReferenceTest()
        {
            var v1 = SemVersion.Parse("1.2-nightly");

            var r = v1.Equals(v1);
            Assert.True(r);
        }

        [Fact]
        public void StaticEqualsTest1()
        {
            var v1 = new SemVersion(1, 2, 3);
            var v2 = new SemVersion(1, 2, 3);

            var r = SemVersion.Equals(v1, v2);
            Assert.True(r);
        }

        [Fact]
        public void StaticEqualsTest2()
        {
            var r = SemVersion.Equals(null, null);
            Assert.True(r);
        }

        [Fact]
        public void StaticEqualsTest3()
        {
            var v1 = new SemVersion(1);

            var r = SemVersion.Equals(v1, null);
            Assert.False(r);
        }
        #endregion

        #region Comparison
        [Theory]
        [InlineData("1.0.0-alpha+dev.123", "1.0.0-beta+dev.123", -1)]
        [InlineData("1.0.0", "1.0.1-alpha", -1)]
        [InlineData("0.0.1", "0.0.1+build.12", -1)]
        [InlineData("0.0.1+build.13", "0.0.1+build.12.2", 1)]
        [InlineData("0.0.1-13", "0.0.1-b", -1)]
        [InlineData("0.0.1+uiui", "0.0.1+12", 1)]
        [InlineData("0.0.1+bu", "0.0.1", 1)]
        [InlineData("0.1.1+bu", "0.2.1", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.88", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.87.1", -1)]
        [InlineData("0.1.1-gamma.12.87.99", "0.1.1-gamma.12.87.X", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.87.X", -1)]
        // Examples from spec
        [InlineData("1.0.0", "2.0.0", -1)]
        [InlineData("2.0.0", "2.1.0", -1)]
        [InlineData("2.1.0", "2.1.1", -1)]
        [InlineData("1.0.0-alpha", "1.0.0", -1)]
        [InlineData("1.0.0-alpha", "1.0.0-alpha.1", -1)]
        [InlineData("1.0.0-alpha.1", "1.0.0-alpha.beta", -1)]
        [InlineData("1.0.0-alpha.beta", "1.0.0-beta", -1)]
        [InlineData("1.0.0-beta", "1.0.0-beta.2", -1)]
        [InlineData("1.0.0-beta.2", "1.0.0-beta.11", -1)]
        // TODO comparison distinguished by string order currently gives numbers outside -1,0,1 (issue #26)
        [InlineData("1.0.0-beta.11", "1.0.0-rc.1", -16)]
        [InlineData("1.0.0-rc.1", "1.0.0", -1)]
        public void TestCompareTo(string s1, string s2, int expected)
        {
            var v1 = SemVersion.Parse(s1, true);
            var v2 = SemVersion.Parse(s2, true);

            var r1 = v1.CompareTo(v2);
            var r2 = v2.CompareTo(v1);

            Assert.Equal(expected, r1);
            Assert.Equal(-expected, r2);
        }

        [Fact]
        public void TestCompareToNull()
        {
            var v1 = SemVersion.Parse("0.0.1+bu");
            var r = v1.CompareTo(null);
            Assert.Equal(1, r);
        }
        #endregion

        #region Precedence
        [Theory]
        [InlineData("1.0.0-alpha+dev.123", "1.0.0-beta+dev.123", -1)]
        [InlineData("1.0.0", "1.0.1-alpha", -1)]
        [InlineData("0.0.1", "0.0.1+build.12", 0)]
        [InlineData("0.0.1+build.13", "0.0.1+build.12.2", 0)]
        [InlineData("0.0.1-13", "0.0.1-b", -1)]
        [InlineData("0.0.1+uiui", "0.0.1+12", 0)]
        [InlineData("0.0.1+bu", "0.0.1", 0)]
        [InlineData("0.1.1+bu", "0.2.1", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.88", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.87.1", -1)]
        [InlineData("0.1.1-gamma.12.87.99", "0.1.1-gamma.12.87.X", -1)]
        [InlineData("0.1.1-gamma.12.87", "0.1.1-gamma.12.87.X", -1)]
        // Examples from spec
        [InlineData("1.0.0", "2.0.0", -1)]
        [InlineData("2.0.0", "2.1.0", -1)]
        [InlineData("2.1.0", "2.1.1", -1)]
        [InlineData("1.0.0-alpha", "1.0.0", -1)]
        [InlineData("1.0.0-alpha", "1.0.0-alpha.1", -1)]
        [InlineData("1.0.0-alpha.1", "1.0.0-alpha.beta", -1)]
        [InlineData("1.0.0-alpha.beta", "1.0.0-beta", -1)]
        [InlineData("1.0.0-beta", "1.0.0-beta.2", -1)]
        [InlineData("1.0.0-beta.2", "1.0.0-beta.11", -1)]
        // TODO comparison distinguished by string order currently gives numbers outside -1,0,1 (issue #26)
        [InlineData("1.0.0-beta.11", "1.0.0-rc.1", -16)]
        [InlineData("1.0.0-rc.1", "1.0.0", -1)]
        public void TestCompareByPrecedence(string s1, string s2, int expected)
        {
            var v1 = SemVersion.Parse(s1, true);
            var v2 = SemVersion.Parse(s2, true);

            var r1 = v1.CompareByPrecedence(v2);
            var r2 = v2.CompareByPrecedence(v1);

            Assert.Equal(expected, r1);
            Assert.Equal(-expected, r2);
        }

        [Fact]
        public void TestCompareByPrecedenceToNull()
        {
            var v1 = SemVersion.Parse("0.0.1+bu");
            var r = v1.CompareByPrecedence(null);
            Assert.Equal(1, r);
        }
        #endregion

        [Fact]
        public void GetHashCodeTest()
        {
            var v1 = SemVersion.Parse("1.0.0-1+b");
            var v2 = SemVersion.Parse("1.0.0-1+c");

            var h1 = v1.GetHashCode();
            var h2 = v2.GetHashCode();

            Assert.NotEqual(h1, h2);
        }

        [Fact]
        public void ImplicitConversionFromStringTest()
        {
            SemVersion v = "1.0.0";
            Assert.Equal(1, v.Major);
        }

        [Fact]
        public void CompareToObjectTest()
        {
            var v1 = new SemVersion(1);
            var c = v1.CompareTo((object)v1);

            Assert.Equal(0, c);
        }

        [Fact]
        public void StaticCompareTest1()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(2);

            var r = SemVersion.Compare(v1, v2);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void StaticCompareTest2()
        {
            var v1 = new SemVersion(1);

            var r = SemVersion.Compare(v1, null);
            Assert.Equal(1, r);
        }

        [Fact]
        public void StaticCompareTest3()
        {
            var v1 = new SemVersion(1);

            var r = SemVersion.Compare(null, v1);
            Assert.Equal(-1, r);
        }

        [Fact]
        public void StaticCompareTest4()
        {
            var r = SemVersion.Compare(null, null);
            Assert.Equal(0, r);
        }

        #region Operators
        [Fact]
        public void EqualsOperatorTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(1);

            var r = v1 == v2;
            Assert.True(r);
        }

        [Fact]
        public void NotEqualOperatorTest()
        {
            var v1 = new SemVersion(1);
            var v2 = new SemVersion(2);

            var r = v1 != v2;
            Assert.True(r);
        }

        [Theory]
        [InlineData("1.0.0", "2.0.0")]
        [InlineData("1.0.0-alpha", "1.0.0-rc")]
        [InlineData("1.0.0-alpha", "1.0.0-ci.1")]
        public void ComparisonOperatorTest(string v1String, string v2String)
        {
            var v1 = SemVersion.Parse(v1String);
            var v2 = SemVersion.Parse(v2String);

            Assert.True(v1 < v2, "v1 < v2");
            Assert.True(v2 > v1, "v2 > v1");
        }

        [Theory]
        [InlineData("1.0.0", "2.0.0")]
        [InlineData("1.0.0-alpha", "1.0.0-rc")]
        [InlineData("1.0.0-alpha", "1.0.0-ci.1")]
        public void CompareOrEqualOperatorTest(string v1String, string v2String)
        {
            var v1 = SemVersion.Parse(v1String);
            var v2 = SemVersion.Parse(v2String);

            Assert.True(v1 <= v2, "v1 <= v2");
            Assert.True(v2 >= v1, "v2 >= v1");
        }

        [Theory]
        [InlineData("1.0.0", "1.0.0")]
        [InlineData("1.0.0-alpha", "1.0.0-alpha")]
        [InlineData("1.0.0-rc.1", "1.0.0-rc.1")]
        [InlineData("1.0.0-beta-34+0f45", "1.0.0-beta-34+0f45")]
        public void CompareOrEqualOperatorWhenEqualTest(string v1String, string v2String)
        {
            var v1 = SemVersion.Parse(v1String);
            var v2 = SemVersion.Parse(v2String);

            Assert.True(v1 <= v2, "v1 <= v2");
            Assert.True(v1 >= v2, "v1 >= v2");
        }
        #endregion

        #region Change
        [Fact]
        public void ChangeMajorTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(major: 5);

            Assert.Equal(5, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }

        [Fact]
        public void ChangeMinorTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(minor: 5);

            Assert.Equal(1, v2.Major);
            Assert.Equal(5, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }

        [Fact]
        public void ChangePatchTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(patch: 5);

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(5, v2.Patch);
            Assert.Equal("alpha", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }

        [Fact]
        public void ChangePrereleaseTest()
        {
            var v1 = new SemVersion(1, 2, 3, "alpha", "dev");
            var v2 = v1.Change(prerelease: "beta");

            Assert.Equal(1, v2.Major);
            Assert.Equal(2, v2.Minor);
            Assert.Equal(3, v2.Patch);
            Assert.Equal("beta", v2.Prerelease);
            Assert.Equal("dev", v2.Build);
        }
        #endregion

#if !NETSTANDARD
        [Fact]
        public void TestSerialization()
        {
            var semVer = new SemVersion(1, 2, 3, "alpha", "dev");
            SemVersion semVerSerializedDeserialized;
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, semVer);
                ms.Position = 0;
                semVerSerializedDeserialized = (SemVersion)bf.Deserialize(ms);
            }
            Assert.Equal(semVer, semVerSerializedDeserialized);
        }
#endif
    }
}