using System;
using Xunit;

namespace Semver.Test
{
    public class SemVersionParsingTests
    {
        // TODO use examples given with sample regexs

        public static readonly TheoryData<string, int, int, int, string, string> Valid =
            new TheoryData<string, int, int, int, string, string>()
            {
                // Major, Minor, Patch
                {"1.2.45-alpha-beta+nightly.23.43-bla", 1, 2, 45, "alpha-beta", "nightly.23.43-bla"},
                {"1.2.45-alpha+nightly.23", 1, 2, 45, "alpha", "nightly.23"},
                {"3.2.1-beta", 3, 2, 1, "beta", ""},
                {"2.0.0+nightly.23.43-bla", 2, 0, 0, "", "nightly.23.43-bla"},
                {"5.6.7", 5, 6, 7, "", ""},
                // Valid unusual versions
                {"1.0.0--ci.1", 1, 0, 0, "-ci.1", ""},
                {"1.0.0-0A", 1, 0, 0, "0A", ""},
            };

        public static readonly TheoryData<string, int, int, int, string, string> MissingPatch =
            new TheoryData<string, int, int, int, string, string>()
            {
                // Major, Minor
                {"1.6-zeta.5+nightly.23.43-bla", 1, 6, 0, "zeta.5", "nightly.23.43-bla"},
                {"2.0+nightly.23.43-bla", 2, 0, 0, "", "nightly.23.43-bla"},
                {"2.1-alpha", 2, 1, 0, "alpha", ""},
                {"5.6+nightly.23.43-bla", 5, 6, 0, "", "nightly.23.43-bla"},
                {"3.2", 3, 2, 0, "", ""},
            };

        public static readonly TheoryData<string, int, int, int, string, string> MissingMinorPatch =
            new TheoryData<string, int, int, int, string, string>()
            {
                // Major
                {"1-beta+dev.123", 1, 0, 0, "beta", "dev.123"},
                {"7-rc.1", 7, 0, 0, "rc.1", ""},
                {"6+sha.a3456b", 6, 0, 0, "", "sha.a3456b"},
                {"64", 64, 0, 0, "", ""},
            };
        public static readonly TheoryData<string, int, int, int, string, string> LeadingZeros =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"01.2.3", 1, 2, 3, "", ""},
                {"1.02.3", 1, 2, 3, "", ""},
                {"1.2.03", 1, 2, 3, "", ""},
                // TODO these should have leading zeros removed
                {"1.2.3-01", 1, 2, 3, "01", ""},
                {"1.2.3-a.01", 1, 2, 3, "a.01", ""},
            };

        public static readonly TheoryData<string> Overflow =
            new TheoryData<string>()
            {
                // int.Max+1
                {"2147483648.2.3"},
                {"1.2147483648.3"},
                {"1.2.2147483648"},
            };

        [Theory]
        [MemberData(nameof(Valid))]
        [MemberData(nameof(MissingPatch))]
        [MemberData(nameof(MissingMinorPatch))]
        [MemberData(nameof(LeadingZeros))]
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
        [MemberData(nameof(Overflow))]
        public void ParseLooseInvalidThrowsOverflowExceptionTest(string versionString)
        {
            var ex = Assert.Throws<OverflowException>(() => SemVersion.Parse(versionString));
            Assert.Equal("Value was either too large or too small for an Int32.", ex.Message);
        }

        [Fact]
        public void ParseLooseNullTest()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersion.Parse(null));
            // TODO that is a strange error message, should be version
            Assert.Equal("Value cannot be null.\r\nParameter name: input", ex.Message);
        }

        [Theory]
        [MemberData(nameof(Valid))]
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

        [Theory]
        [MemberData(nameof(Overflow))]
        public void ParseStringInvalidThrowsOverflowExceptionTest(string versionString)
        {
            var ex = Assert.Throws<OverflowException>(() => SemVersion.Parse(versionString));
            Assert.Equal("Value was either too large or too small for an Int32.", ex.Message);
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
        [MemberData(nameof(Valid))]
        [MemberData(nameof(MissingPatch))]
        [MemberData(nameof(MissingMinorPatch))]
        [MemberData(nameof(Valid))]
        [MemberData(nameof(LeadingZeros))]
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
        [MemberData(nameof(Overflow))]
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
    }
}
