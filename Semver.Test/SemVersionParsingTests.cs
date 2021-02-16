﻿using System;
using System.Text;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of any parsing related methods of <see cref="SemVersion"/>.
    ///
    /// Parsing tests are structured around a number of lists of version strings
    /// which each parsing method should either parse or reject.
    /// </summary>
    public class SemVersionParsingTests
    {
        /// <summary>
        /// This a very long but valid version number to test parsing long version numbers. It is
        /// generated using a random number generator seeded with specific value so that the same
        /// version string will be generated each time.
        /// </summary>
        private static readonly string LongValidVersionString = BuildLongVersion();

        private static string BuildLongVersion()
        {
            var s = new StringBuilder(2_000_100);
            s.Append(int.MaxValue);
            s.Append('.');
            s.Append(int.MaxValue);
            s.Append('.');
            s.Append(int.MaxValue);
            s.Append("-pre");
            var random = new Random(1545743217);
            AppendLabel(s, 1_000_000, random);
            s.Append("+build");
            AppendLabel(s, 1_000_000, random);
            return s.ToString();
        }

        private static void AppendLabel(StringBuilder s, int length, Random random1)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-.";
            var random = random1;

            var identifierBuilder = new StringBuilder(".");
            
            for (var i = 0; i < length; i++)
            {
                var nextChar = chars[random.Next(0, chars.Length)];

                if (identifierBuilder.Length == 1 && (nextChar == '.' || nextChar == '0'))
                {
                    // Don't start with a leading zero, and don't insert an empty group.
                    continue;
                }
                
                if (nextChar == '.')
                {
                    s.Append(identifierBuilder);
                    identifierBuilder.Clear();
                }
                
                identifierBuilder.Append(nextChar);
            }
        }

        /// <summary>
        /// These are version numbers given with the link in the spec to a regex for semver versions
        /// </summary>
        public static readonly TheoryData<string, int, int, int, string, string> RegexValidExamples =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"0.0.4", 0, 0, 4,"",""},
                {"1.2.3", 1, 2, 3, "", ""},
                {"10.20.30", 10, 20, 30, "", ""},
                {"1.1.2-prerelease+meta", 1, 1, 2, "prerelease", "meta"},
                {"1.1.2+meta", 1, 1, 2, "", "meta"},
                {"1.1.2+meta-valid", 1, 1, 2, "", "meta-valid"},
                {"1.0.0-alpha", 1, 0, 0, "alpha", ""},
                {"1.0.0-beta", 1, 0, 0, "beta", ""},
                {"1.0.0-alpha.beta", 1, 0, 0, "alpha.beta", ""},
                {"1.0.0-alpha.beta.1", 1, 0, 0, "alpha.beta.1", ""},
                {"1.0.0-alpha.1", 1, 0, 0, "alpha.1", ""},
                {"1.0.0-alpha0.valid", 1, 0, 0, "alpha0.valid", ""},
                {"1.0.0-alpha.0valid", 1, 0, 0, "alpha.0valid", ""},
                {"1.0.0-alpha-a.b-c-somethinglong+build.1-aef.1-its-okay", 1, 0, 0, "alpha-a.b-c-somethinglong", "build.1-aef.1-its-okay"},
                {"1.0.0-rc.1+build.1", 1, 0, 0, "rc.1", "build.1"},
                {"2.0.0-rc.1+build.123", 2, 0, 0, "rc.1", "build.123"},
                {"1.2.3-beta", 1, 2, 3, "beta", ""},
                {"10.2.3-DEV-SNAPSHOT", 10, 2, 3, "DEV-SNAPSHOT", ""},
                {"1.2.3-SNAPSHOT-123", 1, 2, 3, "SNAPSHOT-123", ""},
                {"1.0.0", 1, 0, 0, "", ""},
                {"2.0.0", 2, 0, 0, "", ""},
                {"1.1.7", 1, 1, 7, "", ""},
                {"2.0.0+build.1848", 2, 0, 0, "", "build.1848"},
                {"2.0.1-alpha.1227", 2, 0, 1, "alpha.1227", ""},
                {"1.0.0-alpha+beta", 1, 0, 0, "alpha", "beta"},
                {"1.2.3----RC-SNAPSHOT.12.9.1--.12+788", 1, 2, 3, "---RC-SNAPSHOT.12.9.1--.12", "788"},
                {"1.2.3----R-S.12.9.1--.12+meta", 1, 2, 3, "---R-S.12.9.1--.12", "meta"},
                {"1.2.3----RC-SNAPSHOT.12.9.1--.12", 1, 2, 3, "---RC-SNAPSHOT.12.9.1--.12", ""},
                {"1.0.0+0.build.1-rc.10000aaa-kk-0.1", 1, 0, 0, "", "0.build.1-rc.10000aaa-kk-0.1"},
                {"1.0.0-0A.is.legal", 1, 0, 0, "0A.is.legal", ""},
                {"1.0.0-pre.0.1", 1, 0, 0, "pre.0.1", ""},
            };

        /// <summary>
        /// These are invalid version numbers given with the link in the spec to a regex for semver versions
        /// </summary>
        /// <remarks>To keep these together, they have not been broken down by what exception they throw.
        /// These examples can only be used with strict parsing. However, the parsing currently
        /// accepts invalid strings so even that doesn't work yet.</remarks>
        public static readonly TheoryData<string> RegexInvalidExamples =
            new TheoryData<string>()
            {
                {"1"},
                {"1.2"},
                {"1.2.3-0123"},
                {"1.2.3-0123.0123"},
                {"1.1.2+.123"},
                {"+invalid"},
                {"-invalid"},
                {"-invalid+invalid"},
                {"-invalid.01"},
                {"alpha"},
                {"alpha.beta"},
                {"alpha.beta.1"},
                {"alpha.1"},
                {"alpha+beta"},
                {"alpha_beta"},
                {"alpha."},
                {"alpha.."},
                {"beta"},
                {"1.0.0-alpha_beta"},
                {"-alpha."},
                {"1.0.0-alpha.."},
                {"1.0.0-alpha..1"},
                {"1.0.0-alpha...1"},
                {"1.0.0-alpha....1"},
                {"1.0.0-alpha.....1"},
                {"1.0.0-alpha......1"},
                {"1.0.0-alpha.......1"},
                {"01.1.1"},
                {"1.01.1"},
                {"1.1.01"},
                {"1.2"},
                {"1.2.3.DEV"},
                {"1.2-SNAPSHOT"},
                {"1.2.31.2.3----RC-SNAPSHOT.12.09.1--..12+788"},
                {"1.2-RC-SNAPSHOT"},
                {"-1.0.3-gamma+b7718"},
                {"+justmeta"},
                {"9.8.7+meta+meta"},
                {"9.8.7-whatever+meta+meta"},
                {"99999999999999999999999.999999999999999999.99999999999999999----RC-SNAPSHOT.12.09.1--------------------------------..12"},
                // This was given as a valid example, but isn't supported by the semver package because of overflow
                {"99999999999999999999999.999999999999999999.99999999999999999"},
            };

        public static readonly TheoryData<string, int, int, int, string, string> BasicValid =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1.2.3-a+b", 1, 2, 3, "a", "b"},
                {"1.2.3-a", 1, 2, 3, "a", ""},
                {"1.2.3+b", 1, 2, 3, "", "b"},
                {"1.2.3", 1, 2, 3, "", ""},
                // Letter Limits
                {"1.2.3-A-Z.a-z.0-9+A-Z.a-z.0-9", 1, 2, 3, "A-Z.a-z.0-9", "A-Z.a-z.0-9"},
            };

        public static readonly TheoryData<string, int, int, int, string, string> MiscValid =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1.2.45-alpha-beta+nightly.23.43-bla", 1, 2, 45, "alpha-beta", "nightly.23.43-bla"},
                {"1.2.45-alpha+nightly.23", 1, 2, 45, "alpha", "nightly.23"},
                {"3.2.1-beta", 3, 2, 1, "beta", ""},
                {"2.0.0+nightly.23.43-bla", 2, 0, 0, "", "nightly.23.43-bla"},
                {"5.6.7", 5, 6, 7, "", ""},
                // Valid unusual versions
                {"1.0.0--ci.1", 1, 0, 0, "-ci.1", ""},
                {"1.0.0-0A", 1, 0, 0, "0A", ""},
            };

        public static readonly TheoryData<string, int, int, int, string, string> DashInStrangePlace =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1.2.3--+b", 1, 2, 3, "-", "b"},
                {"1.2.3---+b", 1, 2, 3, "--", "b"},
                {"1.2.3---", 1, 2, 3, "--", ""},
                {"1.2.3-a+-", 1, 2, 3, "a", "-"},
                {"1.2.3-a+--", 1, 2, 3, "a", "--"},
                {"1.2.3--a+b", 1, 2, 3, "-a", "b"},
                {"1.2.3---a+b", 1, 2, 3, "--a", "b"},
                {"1.2.3-a+-b", 1, 2, 3, "a", "-b"},
                {"1.2.3-a+--b", 1, 2, 3, "a", "--b"},
                {"1.2.3-a-+b", 1, 2, 3, "a-", "b"},
                {"1.2.3-a--+b", 1, 2, 3, "a--", "b"},
                {"1.2.3-a+b-", 1, 2, 3, "a", "b-"},
                {"1.2.3-a+b--", 1, 2, 3, "a", "b--"},
                {"1.2.3--.a+b", 1, 2, 3, "-.a", "b"},
                {"1.2.3-a+-.b", 1, 2, 3, "a", "-.b"},
                {"1.2.3-a.-+b", 1, 2, 3, "a.-", "b"},
                {"1.2.3-a.-.c+b", 1, 2, 3, "a.-.c", "b"},
                {"1.2.3-a+b.-", 1, 2, 3, "a", "b.-"},
                {"1.2.3-a+b.-.c", 1, 2, 3, "a", "b.-.c"},
            };

        public static readonly TheoryData<string, int, int, int, string, string> MissingPatchValid =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1.6-zeta.5+nightly.23.43-bla", 1, 6, 0, "zeta.5", "nightly.23.43-bla"},
                {"2.0+nightly.23.43-bla", 2, 0, 0, "", "nightly.23.43-bla"},
                {"2.1-alpha", 2, 1, 0, "alpha", ""},
                {"5.6+nightly.23.43-bla", 5, 6, 0, "", "nightly.23.43-bla"},
                {"3.2", 3, 2, 0, "", ""},
                {"1.3", 1, 3, 0, "", ""},
                {"1.3-alpha", 1, 3, 0, "alpha", ""},
                {"1.3+build", 1, 3, 0, "", "build"},
            };

        public static readonly TheoryData<string> MissingPatchInvalid = RemoveExpected(MissingPatchValid);

        public static readonly TheoryData<string, int, int, int, string, string> MissingMinorPatchValid =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1-beta+dev.123", 1, 0, 0, "beta", "dev.123"},
                {"7-rc.1", 7, 0, 0, "rc.1", ""},
                {"6+sha.a3456b", 6, 0, 0, "", "sha.a3456b"},
                {"64", 64, 0, 0, "", ""},
            };

        public static readonly TheoryData<string> MissingMinorPatchInvalid = RemoveExpected(MissingMinorPatchValid);

        public static readonly TheoryData<string, int, int, int, string, string> LeadingZeros =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"01.2.3", 1, 2, 3, "", ""},
                {"1.02.3", 1, 2, 3, "", ""},
                {"1.2.03", 1, 2, 3, "", ""},
            };

        public static readonly TheoryData<string, int, int, int, string, string> LeadingZerosPrereleaseAlphanumeric =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1.2.3-0a", 1, 2, 3, "0a", ""},
                {"1.2.3-00000a", 1, 2, 3, "00000a", ""},
                {"1.2.3-a.0c", 1, 2, 3, "a.0c", ""},
                {"1.2.3-a.00000c", 1, 2, 3, "a.00000c", ""},
            };

        // TODO these should have leading zeros removed if they are accepted
        public static readonly TheoryData<string, int, int, int, string, string> LeadingZerosPrerelease =
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1.2.3-01", 1, 2, 3, "01", ""},
                {"1.2.3-a.01", 1, 2, 3, "a.01", ""},
                {"1.2.3-a.01.c", 1, 2, 3, "a.01.c", ""},
                {"1.2.3-a.00001.c", 1, 2, 3, "a.00001.c", ""},
            };

        public static readonly TheoryData<string, int, int, int, string, string> EmptyPrereleaseIdentities = 
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1.0.0-pre..1", 1, 0, 0, "pre..1", ""},
                {"1.0.0-pre.2..1", 1, 0, 0, "pre.2..1", ""},
                {"1.0.0-pre.1..2+alpha", 1, 0, 0, "pre.1..2", "alpha"},
                {"1.0.0-pre.1..2+alpha.3..1", 1, 0, 0, "pre.1..2", "alpha.3..1"},
            };

        public static readonly TheoryData<string, int, int, int, string, string> EmptyBuildIdentities = 
            new TheoryData<string, int, int, int, string, string>()
            {
                {"1.0.0-pre.1+alpha..1", 1, 0, 0, "pre.1", "alpha..1"},
                {"1.0.0-pre.1.2+alpha.1..3", 1, 0, 0, "pre.1.2", "alpha.1..3"},
                {"1.0.0+alpha.1..3", 1, 0, 0, "", "alpha.1..3"},
                {"1.0.0+1..3", 1, 0, 0, "", "1..3"},
            };

        public static readonly TheoryData<string> Overflow = new TheoryData<string>()
            {
                // int.Max+1
                {"2147483648.2.3"},
                {"1.2147483648.3"},
                {"1.2.2147483648"},
            };

        public static readonly TheoryData<string> IllegalCharacters = new TheoryData<string>()
        {
            {"1.2.3-😞+b"},
            {"1.2.3-a+😞"},
            {"1.0.0-a@"},
            {"1.0.0-á"},
            {"1.0.0+a@"},
            {"1.0.0+á"},
        };

        public static readonly TheoryData<string> EmptyOrWhitespace = new TheoryData<string>()
        {
            {""}, {" "}, {"\t"},
        };

        public static readonly TheoryData<string> LeadingV = new TheoryData<string>()
        {
            {"v1.2.3"}, {"V1.2.3"},
        };

        public static readonly TheoryData<string> FourthNumber = new TheoryData<string>()
        {
            {"1.2.3.4"}, {"1.2.3.0"}, {"1.2.3.0-alpha"}, {"1.2.3.0+build"}, {"1.2.3.0-beta+b23"},
        };

        public static readonly TheoryData<string> BadFormat = new TheoryData<string>()
        {
            {"ui-2.1-alpha"},
        };

        [Fact]
        public void ParseLooseLongTest()
        {
            SemVersion.Parse(LongValidVersionString);
        }

        [Theory]
        [MemberData(nameof(RegexValidExamples))]
        [MemberData(nameof(BasicValid))]
        [MemberData(nameof(MiscValid))]
        [MemberData(nameof(DashInStrangePlace))]
        [MemberData(nameof(MissingPatchValid))]
        [MemberData(nameof(MissingMinorPatchValid))]
        [MemberData(nameof(LeadingZeros))]
        [MemberData(nameof(LeadingZerosPrereleaseAlphanumeric))]
        [MemberData(nameof(LeadingZerosPrerelease))]
        [MemberData(nameof(EmptyPrereleaseIdentities))]
        [MemberData(nameof(EmptyBuildIdentities))]
        public void ParseLooseValidTest(string versionString, int major, int minor, int patch, string prerelease, string build)
        {
            var v = SemVersion.Parse(versionString);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease, v.Prerelease);
            Assert.Equal(build, v.Build);
        }

        // TODO These exceptions should be FormatException etc.
        [Theory]
        [MemberData(nameof(IllegalCharacters))]
        [MemberData(nameof(EmptyOrWhitespace))]
        [MemberData(nameof(LeadingV))]
        [MemberData(nameof(FourthNumber))]
        [MemberData(nameof(BadFormat))]
        public void ParseLooseInvalidThrowsArgumentExceptionTest(string versionString)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersion.Parse(versionString));
            Assert.Equal($"Invalid version '{versionString}'.{Environment.NewLine}Parameter name: version", ex.Message);
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
            Assert.Equal($"Value cannot be null.{Environment.NewLine}Parameter name: input", ex.Message);
        }

        [Fact]
        public void ParseStrictLongTest()
        {
            SemVersion.Parse(LongValidVersionString, true);
        }

        [Theory]
        [MemberData(nameof(RegexValidExamples))]
        [MemberData(nameof(BasicValid))]
        [MemberData(nameof(MiscValid))]
        [MemberData(nameof(DashInStrangePlace))]
        public void ParseStrictValidTest(string versionString, int major, int minor, int patch, string prerelease, string build)
        {
            var v = SemVersion.Parse(versionString, true);

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease, v.Prerelease);
            Assert.Equal(build, v.Build);
        }

        [Theory(Skip = "Strict parsing still accepts invalids")]
        [MemberData(nameof(RegexInvalidExamples))]
        public void ParseStrictInvalidExamples(string versionString)
        {
            Assert.ThrowsAny<Exception>(() => SemVersion.Parse(versionString, true));
        }

        [Theory]
        [MemberData(nameof(LeadingZerosPrerelease))]
        [MemberData(nameof(EmptyPrereleaseIdentities))]
// Ignoring extra parameters which are only used for the "Loose" tests
#pragma warning disable
        public void ParseStrictInvalidPreReleaseExamples(string versionString, int major, int minor, int patch, string prerelease, string build)
#pragma warning restore
        {
            var ex = Assert.Throws<InvalidOperationException>(() => SemVersion.Parse(versionString, true));
            Assert.StartsWith(
                "Invalid version (The pre-release version does not comply with the specification. Invalid identifier:", 
                ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }
        
        [Theory]
        [MemberData(nameof(EmptyBuildIdentities))]
// Ignoring extra parameters which are only used for the "Loose" tests
#pragma warning disable
        public void ParseStrictInvalidBuildExamples(string versionString, int major, int minor, int patch, string prerelease, string build)
#pragma warning restore
        {
            var ex = Assert.Throws<InvalidOperationException>(() => SemVersion.Parse(versionString, true));
            Assert.StartsWith(
                "Invalid version (The build metadata does not comply with the specification. Invalid identifier:", 
                ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }
        
        // TODO These exceptions should be FormatException etc.
        [Theory]
        [MemberData(nameof(IllegalCharacters))]
        [MemberData(nameof(EmptyOrWhitespace))]
        [MemberData(nameof(LeadingV))]
        [MemberData(nameof(FourthNumber))]
        [MemberData(nameof(BadFormat))]
        public void ParseStrictInvalidThrowsArgumentExceptionTest(string versionString)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersion.Parse(versionString, true));
            Assert.Equal($"Invalid version '{versionString}'.{Environment.NewLine}Parameter name: version", ex.Message);
        }

        [Theory]
        [MemberData(nameof(Overflow))]
        public void ParseStrictInvalidThrowsOverflowExceptionTest(string versionString)
        {
            var ex = Assert.Throws<OverflowException>(() => SemVersion.Parse(versionString));
            Assert.Equal("Value was either too large or too small for an Int32.", ex.Message);
        }

        // TODO These exceptions should be FormatException etc.
        [Theory]
        [MemberData(nameof(MissingPatchInvalid))]
        public void ParseStrictInvalidThrowsInvalidOperationMissingPatchTest(string versionString)
        {
            var ex = Assert.Throws<InvalidOperationException>(() => SemVersion.Parse(versionString, true));
            Assert.Equal("Invalid version (no patch version given in strict mode)", ex.Message);
        }

        // TODO These exceptions should be FormatException etc.
        [Theory]
        [MemberData(nameof(MissingMinorPatchInvalid))]
        public void ParseStrictInvalidThrowsInvalidOperationMissingMinorPatchTest(string versionString)
        {
            var ex = Assert.Throws<InvalidOperationException>(() => SemVersion.Parse(versionString, true));
            Assert.Equal("Invalid version (no minor version given in strict mode)", ex.Message);
        }

        [Fact]
        public void ParseStrictNullTest()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => SemVersion.Parse(null, true));
            // TODO that is a strange error message, should be version
            Assert.Equal($"Value cannot be null.{Environment.NewLine}Parameter name: input", ex.Message);
        }

        [Theory]
        [MemberData(nameof(RegexValidExamples))]
        [MemberData(nameof(BasicValid))]
        [MemberData(nameof(MiscValid))]
        [MemberData(nameof(DashInStrangePlace))]
        [MemberData(nameof(MissingPatchValid))]
        [MemberData(nameof(MissingMinorPatchValid))]
        [MemberData(nameof(LeadingZeros))]
        [MemberData(nameof(LeadingZerosPrereleaseAlphanumeric))]
        [MemberData(nameof(LeadingZerosPrerelease))]
        [MemberData(nameof(EmptyPrereleaseIdentities))]
        [MemberData(nameof(EmptyBuildIdentities))]
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
        [MemberData(nameof(IllegalCharacters))]
        [MemberData(nameof(EmptyOrWhitespace))]
        [MemberData(nameof(LeadingV))]
        [MemberData(nameof(FourthNumber))]
        [MemberData(nameof(BadFormat))]
        [MemberData(nameof(Overflow))]
        [InlineData(null)]
        public void TryParseLooseInvalidTest(string versionString)
        {
            Assert.False(SemVersion.TryParse(versionString, out _));
        }

        [Theory]
        [MemberData(nameof(RegexValidExamples))]
        [MemberData(nameof(BasicValid))]
        [MemberData(nameof(MiscValid))]
        [MemberData(nameof(DashInStrangePlace))]
        // TODO leading zero versions are accepted and shouldn't be (issue #16)
        [MemberData(nameof(LeadingZeros))]
        public void TryParseStrictValidTest(string versionString, int major, int minor, int patch, string prerelease, string build)
        {
            Assert.True(SemVersion.TryParse(versionString, out var v));

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease, v.Prerelease);
            Assert.Equal(build, v.Build);
        }

        [Theory(Skip = "Strict parsing still accepts invalids")]
        [MemberData(nameof(RegexInvalidExamples))]
        public void TryParseStrictInvalidExamples(string versionString)
        {
            Assert.ThrowsAny<Exception>(() => SemVersion.TryParse(versionString, out _, true));
        }

        [Theory]
        [MemberData(nameof(IllegalCharacters))]
        [MemberData(nameof(EmptyOrWhitespace))]
        [MemberData(nameof(LeadingV))]
        [MemberData(nameof(FourthNumber))]
        [MemberData(nameof(BadFormat))]
        [MemberData(nameof(Overflow))]
        [MemberData(nameof(MissingPatchInvalid))]
        [MemberData(nameof(MissingMinorPatchInvalid))]
        [InlineData(null)]
        public void TryParseStrictInvalidTest(string versionString)
        {
            Assert.False(SemVersion.TryParse(versionString, out _, true));
        }

        /// <summary>
        /// Implicit conversion from string is supposed to be equivalent to lenient parse
        /// </summary>
        [Theory]
        [MemberData(nameof(RegexValidExamples))]
        [MemberData(nameof(BasicValid))]
        [MemberData(nameof(MiscValid))]
        [MemberData(nameof(DashInStrangePlace))]
        [MemberData(nameof(MissingPatchValid))]
        [MemberData(nameof(MissingMinorPatchValid))]
        [MemberData(nameof(LeadingZeros))]
        public void ImplicitConversionFromValidStringTest(string versionString, int major, int minor, int patch, string prerelease, string build)
        {
            SemVersion v = versionString;

            Assert.Equal(major, v.Major);
            Assert.Equal(minor, v.Minor);
            Assert.Equal(patch, v.Patch);
            Assert.Equal(prerelease, v.Prerelease);
            Assert.Equal(build, v.Build);
        }

        [Theory]
        [MemberData(nameof(IllegalCharacters))]
        [MemberData(nameof(EmptyOrWhitespace))]
        [MemberData(nameof(LeadingV))]
        [MemberData(nameof(FourthNumber))]
        [MemberData(nameof(BadFormat))]
        public void ImplicitConversionFromInvalidStringThrowsArgumentExceptionTest(string versionString)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                SemVersion _ = versionString;
            });
            Assert.Equal($"Invalid version '{versionString}'.{Environment.NewLine}Parameter name: version", ex.Message);
        }

        [Theory]
        [MemberData(nameof(Overflow))]
        public void ImplicitConversionFromInvalidStringThrowsOverflowExceptionTest(string versionString)
        {
            var ex = Assert.Throws<OverflowException>(() =>
            {
                SemVersion _ = versionString;
            });
            Assert.Equal("Value was either too large or too small for an Int32.", ex.Message);
        }

        [Fact]
        public void ImplicitConversionFromNullStringTest()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                // Must use default otherwise it thinks `null` is of type SemVersion
                SemVersion _ = default(string);
            });
            // TODO that is a strange error message, should be version
            Assert.Equal($"Value cannot be null.{Environment.NewLine}Parameter name: input", ex.Message);
        }

        /// <summary>
        /// Construct invalid test cases for strict from valid ones for loose
        /// </summary>
        private static TheoryData<string> RemoveExpected(TheoryData<string, int, int, int, string, string> data)
        {
            var result = new TheoryData<string>();
            foreach (var testCase in data)
                result.Add((string)testCase[0]);

            return result;
        }
    }
}
