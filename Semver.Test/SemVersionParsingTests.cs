using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Semver.Test.Builders;
using Semver.Test.Helpers;
using Semver.Test.TestCases;
using Xunit;
using static Semver.SemVersionStyles;

namespace Semver.Test
{
    /// <summary>
    /// Tests of the parsing related methods of <see cref="SemVersion"/>.
    /// </summary>
    public class SemVersionParsingTests
    {
        private static readonly BigInteger ULongMaxPlusOne = ulong.MaxValue + BigInteger.One;

        /// <summary>
        /// This a very long but valid version number to test parsing long version numbers. It is
        /// generated using a random number generator seeded with specific value so that the same
        /// version string will be generated each time.
        /// </summary>
        public static readonly string LongValidVersionString = BuildLongVersion();

        private static string BuildLongVersion()
        {
            var s = new StringBuilder(2_000_100);
            s.Append(int.MaxValue);
            s.Append('.');
            s.Append(int.MaxValue);
            s.Append('.');
            s.Append(int.MaxValue);
            s.Append('-');
            var random = new Random(1545743217);
            AppendLabel(s, 1_000_000, random);
            s.Append('+');
            AppendLabel(s, 1_000_000, random);
            return s.ToString();
        }

        private static void AppendLabel(StringBuilder s, int length, Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789-.0";

            for (var i = 0; i < length; i++)
            {
                var startOfIdentifier = s[s.Length - 1] == '.';
                var validChars = startOfIdentifier ? chars.Length - 2 : chars.Length;
                s.Append(chars[random.Next(0, validChars)]);
            }
        }

        public static readonly TheoryData<SemVersionStyles> InvalidSemVersionStyles = new TheoryData<SemVersionStyles>()
        {
            // Optional minor flag without optional patch flag
            OptionalMinorPatch & ~OptionalPatch,
            // Next unused bit flag
            SemVersionStylesExtensions.AllowAll + 1,
        };

        public static readonly TheoryData<int> InvalidMaxLength = new TheoryData<int>()
        {
            -1,
            int.MinValue,
        };

        public static readonly TheoryData<ParsingTestCase> ParsingTestCases = ExpandTestCases(
            // version numbers given with the link in the spec to a regex for semver versions
            Valid("0.0.4", 0, 0, 4),
            Valid("1.2.3", 1, 2, 3),
            Valid("10.20.30", 10, 20, 30),
            Valid("1.1.2-prerelease+meta", 1, 1, 2, Pre("prerelease"), Meta("meta")),
            Valid("1.1.2+meta", 1, 1, 2, Pre(), Meta("meta")),
            Valid("1.1.2+meta-valid", 1, 1, 2, Pre(), Meta("meta-valid")),
            Valid("1.0.0-alpha", 1, 0, 0, Pre("alpha")),
            Valid("1.0.0-beta", 1, 0, 0, Pre("beta")),
            Valid("1.0.0-alpha.beta", 1, 0, 0, Pre("alpha", "beta")),
            Valid("1.0.0-alpha.beta.1", 1, 0, 0, Pre("alpha", "beta", "1")),
            Valid("1.0.0-alpha.1", 1, 0, 0, Pre("alpha", 1)),
            Valid("1.0.0-alpha0.valid", 1, 0, 0, Pre("alpha0", "valid")),
            Valid("1.0.0-alpha.0valid", 1, 0, 0, Pre("alpha", "0valid")),
            Valid("1.0.0-alpha-a.b-c-somethinglong+build.1-aef.1-its-okay", 1, 0, 0,
                Pre("alpha-a", "b-c-somethinglong"), Meta("build", "1-aef", "1-its-okay")),
            Valid("1.0.0-rc.1+build.1", 1, 0, 0, Pre("rc", 1), Meta("build", "1")),
            Valid("2.0.0-rc.1+build.123", 2, 0, 0, Pre("rc", 1), Meta("build", "123")),
            Valid("1.2.3-beta", 1, 2, 3, Pre("beta")),
            Valid("10.2.3-DEV-SNAPSHOT", 10, 2, 3, Pre("DEV-SNAPSHOT")),
            Valid("1.2.3-SNAPSHOT-123", 1, 2, 3, Pre("SNAPSHOT-123")),
            Valid("1.0.0", 1),
            Valid("2.0.0", 2),
            Valid("1.1.7", 1, 1, 7),
            Valid("2.0.0+build.1848", 2, 0, 0, Pre(), Meta("build", "1848")),
            Valid("2.0.1-alpha.1227", 2, 0, 1, Pre("alpha", 1227)),
            Valid("1.0.0-alpha+beta", 1, 0, 0, Pre("alpha"), Meta("beta")),
            Valid("1.2.3----RC-SNAPSHOT.12.9.1--.12+788", 1, 2, 3, Pre("---RC-SNAPSHOT", 12, 9, "1--", 12), Meta("788")),
            Valid("1.2.3----R-S.12.9.1--.12+meta", 1, 2, 3, Pre("---R-S", 12, 9, "1--", 12), Meta("meta")),
            Valid("1.2.3----RC-SNAPSHOT.12.9.1--.12", 1, 2, 3, Pre("---RC-SNAPSHOT", 12, 9, "1--", 12)),
            Valid("1.0.0+0.build.1-rc.10000aaa-kk-0.1", 1, 0, 0, Pre(), Meta("0", "build", "1-rc", "10000aaa-kk-0", "1")),
            Valid("1.0.0-0A.is.legal", 1, 0, 0, Pre("0A", "is", "legal")),
            // This was given as a valid example, but isn't supported by the semver package because of overflow
            Invalid<OverflowException>("99999999999999999999999.999999999999999999.99999999999999999", ExceptionMessages.MajorOverflow, "99999999999999999999999"),

            // These are invalid version numbers given with the link in the spec to a regex for semver versions
            Invalid("1", ExceptionMessages.MissingMinor),
            Invalid("1.2", ExceptionMessages.MissingPatch),
            Invalid("1.2.3-0123", ExceptionMessages.LeadingZeroInPrerelease, "0123"),
            Invalid("1.2.3-0123.0123", ExceptionMessages.LeadingZeroInPrerelease, "0123"),
            Invalid("1.1.2+.123", ExceptionMessages.MissingMetadataIdentifier),
            Invalid("+invalid", ExceptionMessages.EmptyMajor),
            Invalid("-invalid", ExceptionMessages.EmptyMajor),
            Invalid("-invalid+invalid", ExceptionMessages.EmptyMajor),
            Invalid("-invalid.01", ExceptionMessages.EmptyMajor),
            Invalid("alpha", ExceptionMessages.InvalidCharacterInMajor, "a"),
            Invalid("alpha.beta", ExceptionMessages.InvalidCharacterInMajor, "a"),
            Invalid("alpha.beta.1", ExceptionMessages.InvalidCharacterInMajor, "a"),
            Invalid("alpha.1", ExceptionMessages.InvalidCharacterInMajor, "a"),
            Invalid("alpha+beta", ExceptionMessages.InvalidCharacterInMajor, "a"),
            Invalid("alpha_beta", ExceptionMessages.InvalidCharacterInMajor, "a"),
            Invalid("alpha.", ExceptionMessages.InvalidCharacterInMajor, "a"),
            Invalid("alpha..", ExceptionMessages.InvalidCharacterInMajor, "a"),
            Invalid("beta", ExceptionMessages.InvalidCharacterInMajor, "b"),
            Invalid("1.0.0-alpha_beta", ExceptionMessages.InvalidCharacterInPrerelease, "_"),
            Invalid("-alpha.", ExceptionMessages.EmptyMajor),
            Invalid("1.0.0-alpha..", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.0.0-alpha..1", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.0.0-alpha...1", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.0.0-alpha....1", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.0.0-alpha.....1", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.0.0-alpha......1", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.0.0-alpha.......1", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("01.1.1", ExceptionMessages.LeadingZeroInMajor),
            Invalid("1.01.1", ExceptionMessages.LeadingZeroInMinor),
            Invalid("1.1.01", ExceptionMessages.LeadingZeroInPatch),
            Invalid("1.2.3.DEV", ExceptionMessages.PrereleasePrefixedByDot),
            Invalid("1.2-SNAPSHOT", ExceptionMessages.MissingPatch),
            Invalid("1.2.31.2.3----RC-SNAPSHOT.12.09.1--..12+788", ExceptionMessages.FourthVersionNumber),
            Invalid("1.2-RC-SNAPSHOT", ExceptionMessages.MissingPatch),
            Invalid("-1.0.3-gamma+b7718", ExceptionMessages.EmptyMajor),
            Invalid("+justmeta", ExceptionMessages.EmptyMajor),
            Invalid("9.8.7+meta+meta", ExceptionMessages.InvalidCharacterInMetadata, "+"),
            Invalid("9.8.7-whatever+meta+meta", ExceptionMessages.InvalidCharacterInMetadata, "+"),
            Invalid<OverflowException>(
                "99999999999999999999999.999999999999999999.99999999999999999----RC-SNAPSHOT.12.09.1--------------------------------..12",
                ExceptionMessages.MajorOverflow, "99999999999999999999999"),

            // Basic valid versions
            Valid("1.2.3-a+b", 1, 2, 3, Pre("a"), Meta("b")),
            Valid("1.2.3-a", 1, 2, 3, Pre("a")),
            Valid("1.2.3+b", 1, 2, 3, Pre(), Meta("b")),
            Valid("4.5.6", 4, 5, 6),

            // Valid letter Limits
            Valid("1.2.3-A-Z.a-z.0-9+A-Z.a-z.0-9", 1, 2, 3, Pre("A-Z", "a-z", "0-9"), Meta("A-Z", "a-z", "0-9")),

            // Misc valid
            Valid("1.2.45-alpha-beta+nightly.23.43-bla", 1, 2, 45, Pre("alpha-beta"), Meta("nightly", "23", "43-bla")),
            Valid("1.2.45-alpha+nightly.23", 1, 2, 45, Pre("alpha"), Meta("nightly", "23")),
            Valid("3.2.1-beta", 3, 2, 1, Pre("beta")),
            Valid("2.0.0+nightly.23.43-bla", 2, 0, 0, Pre(), Meta("nightly", "23", "43-bla")),
            Valid("5.6.7", 5, 6, 7),

            // Valid unusual versions
            Valid("1.0.0--ci.1", 1, 0, 0, Pre("-ci", 1)),
            Valid("1.0.0-0A", 1, 0, 0, Pre("0A")),

            // Hyphen in strange place
            Valid("1.2.3--+b", 1, 2, 3, Pre("-"), Meta("b")),
            Valid("1.2.3---+b", 1, 2, 3, Pre("--"), Meta("b")),
            Valid("1.2.3---", 1, 2, 3, Pre("--")),
            Valid("1.2.3-a+-", 1, 2, 3, Pre("a"), Meta("-")),
            Valid("1.2.3-a+--", 1, 2, 3, Pre("a"), Meta("--")),
            Valid("1.2.3--a+b", 1, 2, 3, Pre("-a"), Meta("b")),
            Valid("1.2.3--1+b", 1, 2, 3, Pre("-1"), Meta("b")),
            Valid("1.2.3---a+b", 1, 2, 3, Pre("--a"), Meta("b")),
            Valid("1.2.3---1+b", 1, 2, 3, Pre("--1"), Meta("b")),
            Valid("1.2.3-a+-b", 1, 2, 3, Pre("a"), Meta("-b")),
            Valid("1.2.3-a+--b", 1, 2, 3, Pre("a"), Meta("--b")),
            Valid("1.2.3-a-+b", 1, 2, 3, Pre("a-"), Meta("b")),
            Valid("1.2.3-1-+b", 1, 2, 3, Pre("1-"), Meta("b")),
            Valid("1.2.3-a--+b", 1, 2, 3, Pre("a--"), Meta("b")),
            Valid("1.2.3-1--+b", 1, 2, 3, Pre("1--"), Meta("b")),
            Valid("1.2.3-a+b-", 1, 2, 3, Pre("a"), Meta("b-")),
            Valid("1.2.3-a+b--", 1, 2, 3, Pre("a"), Meta("b--")),
            Valid("1.2.3--.a+b", 1, 2, 3, Pre("-", "a"), Meta("b")),
            Valid("1.2.3-a+-.b", 1, 2, 3, Pre("a"), Meta("-", "b")),
            Valid("1.2.3-a.-+b", 1, 2, 3, Pre("a", "-"), Meta("b")),
            Valid("1.2.3-a.-.c+b", 1, 2, 3, Pre("a", "-", "c"), Meta("b")),
            Valid("1.2.3-a+b.-", 1, 2, 3, Pre("a"), Meta("b", "-")),
            Valid("1.2.3-a+b.-.c", 1, 2, 3, Pre("a"), Meta("b", "-", "c")),

            // Missing patch number, but otherwise valid
            Valid("1.6-zeta.5+nightly.23.43-bla", OptionalPatch, 1, 6, 0, Pre("zeta", "5"), Meta("nightly", "23", "43-bla")),
            Valid("2.0+nightly.23.43-bla", OptionalPatch, 2, 0, 0, Pre(), Meta("nightly", "23", "43-bla")),
            Valid("2.1-alpha", OptionalPatch, 2, 1, 0, Pre("alpha")),
            Valid("5.6+nightly.23.43-bla", OptionalPatch, 5, 6, 0, Pre(), Meta("nightly", "23", "43-bla")),
            Valid("3.2", OptionalPatch, 3, 2),
            Valid("1.3", OptionalPatch, 1, 3),
            Valid("1.3-alpha", OptionalPatch, 1, 3, 0, Pre("alpha")),
            Valid("1.3+build", OptionalPatch, 1, 3, 0, Pre(), Meta("build")),

            // Missing minor and patch number, but otherwise valid
            Valid("1-beta+dev.123", OptionalMinorPatch, 1, 0, 0, Pre("beta"), Meta("dev", "123")),
            Valid("7-rc.1", OptionalMinorPatch, 7, 0, 0, Pre("rc", 1)),
            Valid("6+sha.a3456b", OptionalMinorPatch, 6, 0, 0, Pre(), Meta("sha", "a3456b")),
            Valid("64", OptionalMinorPatch, 64),

            // Leading zeros in major, minor, or patch, but otherwise valid
            Valid("01.2.3", AllowLeadingZeros, 1, 2, 3),
            Valid("00.2.3", AllowLeadingZeros, 0, 2, 3),
            Valid("1.02.3", AllowLeadingZeros, 1, 2, 3),
            Valid("1.00.3", AllowLeadingZeros, 1, 0, 3),
            Valid("1.2.03", AllowLeadingZeros, 1, 2, 3),
            Valid("1.2.00", AllowLeadingZeros, 1, 2),

            // Leading zeros in alphanumeric prerelease identifiers
            Valid("1.2.3-0a", 1, 2, 3, Pre("0a")),
            Valid("1.2.3-00000a", 1, 2, 3, Pre("00000a")),
            Valid("1.2.3-a.0c", 1, 2, 3, Pre("a", "0c")),
            Valid("1.2.3-a.00000c", 1, 2, 3, Pre("a", "00000c")),

            // Leading zeros in numeric prerelease identifiers, but otherwise valid
            Valid("1.2.3-01", AllowLeadingZeros, 1, 2, 3, Pre(1)),
            Valid("1.2.3-a.01", AllowLeadingZeros, 1, 2, 3, Pre("a", 1)),
            Valid("1.2.3-a.01.c", AllowLeadingZeros, 1, 2, 3, Pre("a", 1, "c")),
            Valid("1.2.3-a.050.c", AllowLeadingZeros, 1, 2, 3, Pre("a", 50, "c")),
            Valid("1.2.3-a.00001.c", AllowLeadingZeros, 1, 2, 3, Pre("a", 1, "c")),

            // Simple zero in prerelease identifiers (leading zero check is correct)
            Valid("1.2.3-0", 1, 2, 3, Pre(0)),
            Valid("1.2.3-pre.0", 1, 2, 3, Pre("pre", 0)),
            Valid("1.2.3-pre.0.42", 1, 2, 3, Pre("pre", 0, 42)),

            // Longer than max length
            Invalid("1.0.0-length", ExceptionMessages.TooLongVersion, "2", maxLength: 2),

            // Overflow at int.Max+1
            Invalid<OverflowException>("2147483648.2.3", ExceptionMessages.MajorOverflow, "2147483648"),
            Invalid<OverflowException>("1.2147483648.3", ExceptionMessages.MinorOverflow, "2147483648"),
            Invalid<OverflowException>("1.2.2147483648", ExceptionMessages.PatchOverflow, "2147483648"),

            // Supports ulong.Max+1
            Valid("1.2.3-18446744073709551616", 1, 2, 3, Pre(ULongMaxPlusOne)),

            // Invalid characters in major, minor, or patch
            Invalid("1@.2.3", ExceptionMessages.InvalidCharacterInMajor, "@"),
            Invalid("1.2@.3", ExceptionMessages.InvalidCharacterInMinor, "@"),
            Invalid("1.0.0@", ExceptionMessages.InvalidCharacterInPatch, "@"),
            Invalid("1.0.0@.alpha", ExceptionMessages.InvalidCharacterInPatch, "@"),
            Invalid("1.0.0@-alpha", ExceptionMessages.InvalidCharacterInPatch, "@"),
            Invalid("1.0.0@+build", ExceptionMessages.InvalidCharacterInPatch, "@"),

            // Invalid characters in prerelease and metadata
            Invalid("1.2.3-😞+b", ExceptionMessages.InvalidCharacterInPrerelease, "\ud83d"), // High part of surrogate pair
            Invalid("1.2.3-a+😞", ExceptionMessages.InvalidCharacterInMetadata, "\ud83d"), // High part of surrogate pair
            Invalid("1.0.0-a@", ExceptionMessages.InvalidCharacterInPrerelease, "@"),
            Invalid("1.0.0-á", ExceptionMessages.InvalidCharacterInPrerelease, "á"),
            Invalid("1.0.0+a@", ExceptionMessages.InvalidCharacterInMetadata, "@"),
            Invalid("1.0.0+á", ExceptionMessages.InvalidCharacterInMetadata, "á"),

            // Empty or whitespace
            Invalid("", ExceptionMessages.EmptyVersion),
            Invalid(" ", ExceptionMessages.AllWhitespaceVersion),
            Invalid("\t", ExceptionMessages.AllWhitespaceVersion),

            // Leading 'v', but otherwise valid
            Valid("v14.5.6", AllowLowerV, 14, 5, 6),
            Valid("V14.5.6", AllowUpperV, 14, 5, 6),

            // Leading whitespace, but otherwise valid
            Valid(" 12.2.3", AllowLeadingWhitespace, 12, 2, 3),
            Valid("\t12.2.3", AllowLeadingWhitespace, 12, 2, 3),

            // Trailing whitespace, but otherwise valid
            Valid("11.2.3 ", AllowTrailingWhitespace, 11, 2, 3),
            Valid("11.2.3\t", AllowTrailingWhitespace, 11, 2, 3),
            Valid("11.2.3-a ", AllowTrailingWhitespace, 11, 2, 3, Pre("a")),
            Valid("11.2.3-a\t", AllowTrailingWhitespace, 11, 2, 3, Pre("a")),
            Valid("11.2.3+b ", AllowTrailingWhitespace, 11, 2, 3, Pre(), Meta("b")),
            Valid("11.2.3+b\t", AllowTrailingWhitespace, 11, 2, 3, Pre(), Meta("b")),

            // Whitespace in middle
            Invalid("1 .2.3-alpha+build", ExceptionMessages.InvalidCharacterInMajor, " "),
            Invalid("1. 2.3-alpha+build", ExceptionMessages.InvalidCharacterInMinor, " "),
            Invalid("1.2 .3-alpha+build", ExceptionMessages.InvalidCharacterInMinor, " "),
            Invalid("1.2. 3-alpha+build", ExceptionMessages.InvalidCharacterInPatch, " "),
            Invalid("1.2.3 -alpha+build", ExceptionMessages.InvalidCharacterInPatch, " "),
            Invalid("1.2.3- alpha+build", ExceptionMessages.InvalidCharacterInPrerelease, " "),
            Invalid("1.2.3-al pha+build", ExceptionMessages.InvalidCharacterInPrerelease, " "),
            Invalid("1.2.3-alpha +build", ExceptionMessages.InvalidCharacterInPrerelease, " "),
            Invalid("1.2.3-alpha+ build", ExceptionMessages.InvalidCharacterInMetadata, " "),
            Invalid("1.2.3-alpha+bu ild", ExceptionMessages.InvalidCharacterInMetadata, " "),
            Invalid("1.2.3-alpha+build .2", ExceptionMessages.InvalidCharacterInMetadata, " "),

            // Fourth number
            Invalid("1.2.3.4", ExceptionMessages.FourthVersionNumber),
            Invalid("1.2.3.0", ExceptionMessages.FourthVersionNumber),
            Invalid("1.2.3.0-alpha", ExceptionMessages.FourthVersionNumber),
            Invalid("1.2.3.0+build", ExceptionMessages.FourthVersionNumber),
            Invalid("1.2.3.0-beta+b23", ExceptionMessages.FourthVersionNumber),

            // Ends in dot after number
            Invalid("1.2.3.", ExceptionMessages.PrereleasePrefixedByDot),
            Invalid("1.2.", OptionalPatch, ExceptionMessages.EmptyPatch),
            Invalid("1.", OptionalMinorPatch, ExceptionMessages.EmptyMinor),

            // No or improper separator for prerelease version
            Invalid("1.2.3.alpha", ExceptionMessages.PrereleasePrefixedByDot),
            Invalid("1.2.3alpha", ExceptionMessages.InvalidCharacterInPatch, "a"),
            Invalid("1.2.3.12alpha", ExceptionMessages.FourthVersionNumber),

            // Missing major version
            Invalid("ui-2.1-alpha", ExceptionMessages.InvalidCharacterInMajor, "u"),

            // Missing prerelease identifier
            Invalid("1.2.3-.a", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.2.3-a.", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.2.3-a..a", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.2.3-", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.2.3-  ", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.2.3-a.+b", ExceptionMessages.MissingPrereleaseIdentifier),
            Invalid("1.2.3-+b", ExceptionMessages.MissingPrereleaseIdentifier),

            // Missing metadata identifier
            Invalid("1.2.3+", ExceptionMessages.MissingMetadataIdentifier),
            Invalid("1.2.3+  ", ExceptionMessages.MissingMetadataIdentifier),
            Invalid("1.2.3+.b", ExceptionMessages.MissingMetadataIdentifier),
            Invalid("1.2.3+b.", ExceptionMessages.MissingMetadataIdentifier),
            Invalid("1.2.3+b..b", ExceptionMessages.MissingMetadataIdentifier),

            // Multiple prerelease identifiers
            Valid("1.2.3-alpha.beta.gamma", 1, 2, 3, Pre("alpha", "beta", "gamma")),

            // Some long versions to test parsing big version number (parameter is random seed)
            ValidLongVersion(21575113),
            ValidLongVersion(23),
            ValidLongVersion(984567098),
            ValidLongVersion(8187),
            ValidLongVersion(3218987),
            ValidLongVersion(1445670),
            ValidLongVersion(5646),

            // Invalid versions around the version display limit to test exception message formatting
            Invalid("1.2.3.length097.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.t", ExceptionMessages.PrereleasePrefixedByDot),
            Invalid("1.2.3.length098.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.to", ExceptionMessages.PrereleasePrefixedByDot),
            Invalid("1.2.3.length099.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.tip", ExceptionMessages.PrereleasePrefixedByDot),
            Invalid("1.2.3.length100.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test", ExceptionMessages.PrereleasePrefixedByDot),
            Invalid("1.2.3.length101.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.test.tests", ExceptionMessages.PrereleasePrefixedByDot),

            Invalid<ArgumentNullException>(null, ExceptionMessages.NotNull));

        [Fact]
        public void CanConstructParsingTestCases()
        {
            // Also checks that ToString() doesn't throw an exception
            _ = ParsingTestCases.Select(t => t[0].ToString()).ToList();
        }

        [Fact]
        public void NoDuplicateParsingTestCases()
        {
            var duplicates = ParsingTestCases
                             .Select(data => data[0]).Cast<ParsingTestCase>()
                             .GroupBy(c => (c.Version, c.Styles), (k, g) => g.ToList())
                             .Where(g => g.Count > 1)
                             .Select(g => g.First()).ToList();
            Assert.Empty(duplicates);
        }

        [Theory]
        [MemberData(nameof(InvalidSemVersionStyles))]
        public void ParseWithInvalidStyle(SemVersionStyles styles)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersion.Parse("ignored", styles));

            Assert.StartsWith(ExceptionMessages.InvalidSemVersionStylesStart, ex.Message);
            Assert.Equal("style", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(InvalidMaxLength))]
        public void ParseWithInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SemVersion.Parse("ignored", Strict, maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(ParsingTestCases))]
        public void ParseWithStyleParsesCorrectly(ParsingTestCase testCase)
        {
            _ = testCase ?? throw new ArgumentNullException(nameof(testCase));

            if (testCase.IsValid)
            {
                var version = SemVersion.Parse(testCase.Version, testCase.Styles, testCase.MaxLength);

                AssertVersionEqual(version, testCase);
            }
            else
            {
                var ex = Assert.Throws(testCase.ExceptionType,
                    () => SemVersion.Parse(testCase.Version!, testCase.Styles, testCase.MaxLength));

                var expected = ExceptionMessages.InjectVersion(testCase.ExceptionMessageFormat, testCase.Version);

                if (ex is ArgumentException argumentException)
                {
                    Assert.StartsWith(expected, argumentException.Message);
                    Assert.Equal("version", argumentException.ParamName);
                }
                else
                    Assert.Equal(expected, ex.Message);
            }
        }

        [Theory]
        [MemberData(nameof(InvalidSemVersionStyles))]
        public void TryParseWithInvalidStyle(SemVersionStyles styles)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersion.TryParse("ignored", styles, out _));

            Assert.StartsWith(ExceptionMessages.InvalidSemVersionStylesStart, ex.Message);
            Assert.Equal("style", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(InvalidMaxLength))]
        public void TryParseWithInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SemVersion.TryParse("ignored", Strict, out _, maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(ParsingTestCases))]
        public void TryParseWithStyleParsesCorrectly(ParsingTestCase testCase)
        {
            _ = testCase ?? throw new ArgumentNullException(nameof(testCase));

            var result = SemVersion.TryParse(testCase.Version, testCase.Styles, out var version, testCase.MaxLength);

            Assert.Equal(testCase.IsValid, result);
            if (testCase.IsValid)
                AssertVersionEqual(version, testCase);
            else
                Assert.Null(version);
        }

        /// <summary>
        /// Tests that a very long valid version number can be parsed in a reasonable time.
        /// </summary>
        [Fact]
        public void ParseLongVersionTest()
        {
            SemVersion.Parse(LongValidVersionString, Strict, maxLength: int.MaxValue);
            SemVersion.TryParse(LongValidVersionString, Strict, out _, maxLength: int.MaxValue);
        }

        private static void AssertVersionEqual(SemVersion? version, ParsingTestCase testCase)
        {
            Assert.NotNull(version);
            Assert.True(testCase.IsValid);

            Assert.Equal(testCase.Major, version.Major);
            Assert.Equal(testCase.Minor, version.Minor);
            Assert.Equal(testCase.Patch, version.Patch);
            Assert.Equal(testCase.PrereleaseIdentifiers, version.PrereleaseIdentifiers);
            Assert.Equal(string.Join(".", testCase.PrereleaseIdentifiers), version.Prerelease);
            var isPrerelease = testCase.PrereleaseIdentifiers.Any();
            Assert.Equal(isPrerelease, version.IsPrerelease);
            Assert.Equal(!isPrerelease, version.IsRelease);
            Assert.Equal(testCase.MetadataIdentifiers, version.MetadataIdentifiers);
            Assert.Equal(string.Join(".", testCase.MetadataIdentifiers), version.Metadata);
        }

        /// <summary>
        /// Expands out parsing test cases by creating variations of each test case
        /// with different issues.
        /// </summary>
        private static TheoryData<ParsingTestCase> ExpandTestCases(params ParsingTestCase[] testCaseParams)
        {
            var testCases = testCaseParams.ToList();
            var validTestCases = testCases.Where(c => c.IsValid).ToList();

            // Versions needing optional patch should error if that is taken away
            foreach (var testCase in validTestCases.Where(c => c.Styles.HasStyle(OptionalPatch)
                                                               && !c.Styles.HasStyle(OptionalMinorPatch)))
                testCases.Add(Invalid<FormatException>(testCase.Version,
                    testCase.Styles & ~OptionalPatch, ExceptionMessages.MissingPatch, maxLength: testCase.MaxLength));

            // Versions needing optional minor should error if that is taken away
            foreach (var testCase in validTestCases.Where(c => c.Styles.HasStyle(OptionalMinorPatch)))
                testCases.Add(Invalid<FormatException>(testCase.Version,
                    testCase.Styles & ~OptionalMinorPatch, ExceptionMessages.MissingMinor, maxLength: testCase.MaxLength));

            // Versions needing leading zeros should error if that is taken away
            foreach (var testCase in validTestCases.Where(c => c.Styles.HasStyle(AllowLeadingZeros)))
            {
                // Determine whether the error should be about leading zeros in major, minor, or patch
                if (!SemVersion.TryParse(testCase.Version, testCase.Styles, out var expectedSemVersion))
                    continue;

                var expectedVersion = expectedSemVersion.ToString();
                var expectedMajorMinorPatch = expectedVersion.Split('-')[0];
                var actualMajorMinorPatch = testCase.Version!.Split('-')[0];
                var leadingZeroInMajorMinorPatch = expectedMajorMinorPatch != actualMajorMinorPatch;
                string message;
                if (leadingZeroInMajorMinorPatch)
                {
                    var expectedParts = expectedMajorMinorPatch.Split('.');
                    var actualParts = actualMajorMinorPatch.Split('.');
                    if (actualParts[0] != expectedParts[0])
                        message = ExceptionMessages.LeadingZeroInMajor;
                    else if (actualParts[1] != expectedParts[1])
                        message = ExceptionMessages.LeadingZeroInMinor;
                    else
                        message = ExceptionMessages.LeadingZeroInPatch;
                }
                else
                    message = ExceptionMessages.LeadingZeroInPrerelease;

                testCases.Add(Invalid<FormatException>(testCase.Version,
                    testCase.Styles & ~AllowLeadingZeros, message, maxLength: testCase.MaxLength));
            }

            // Versions needing allow leading lower v should error if that is taken away
            foreach (var testCase in validTestCases.Where(c => c.Styles.HasStyle(AllowLowerV)))
                testCases.Add(Invalid<FormatException>(testCase.Version,
                    testCase.Styles & ~AllowLowerV, ExceptionMessages.LeadingLowerV, maxLength: testCase.MaxLength));

            // Versions needing allow leading upper v should error if that is taken away
            foreach (var testCase in validTestCases.Where(c => c.Styles.HasStyle(AllowUpperV)))
                testCases.Add(Invalid<FormatException>(testCase.Version,
                    testCase.Styles & ~AllowUpperV, ExceptionMessages.LeadingUpperV, maxLength: testCase.MaxLength));

            // Versions needing allow leading whitespace should error if that is taken away
            foreach (var testCase in validTestCases.Where(c => c.Styles.HasStyle(AllowLeadingWhitespace)))
                testCases.Add(Invalid<FormatException>(testCase.Version,
                    testCase.Styles & ~AllowLeadingWhitespace, ExceptionMessages.LeadingWhitespace, maxLength: testCase.MaxLength));

            // Versions needing allow trailing whitespace should error if that is taken away
            foreach (var testCase in validTestCases.Where(c => c.Styles.HasStyle(AllowTrailingWhitespace)))
                testCases.Add(Invalid<FormatException>(testCase.Version,
                    testCase.Styles & ~AllowTrailingWhitespace, ExceptionMessages.TrailingWhitespace, maxLength: testCase.MaxLength));

            // Construct cases with leading 'v' and 'V' added
            foreach (var testCase in testCases.Where(CanBePrefixedWithV).ToList())
            {
                testCases.Add(testCase.Change("v" + testCase.Version, testCase.Styles | AllowLowerV));
                testCases.Add(testCase.Change("V" + testCase.Version, testCase.Styles | AllowUpperV));
            }

            // Construct cases with leading whitespace added
            foreach (var testCase in testCases.Where(c => !c.Styles.HasStyle(AllowLeadingWhitespace)
                                                          && !string.IsNullOrWhiteSpace(c.Version)
                                                          && c.ExceptionMessageFormat != ExceptionMessages.LeadingWhitespace).ToList())
                testCases.Add(testCase.Change(" " + testCase.Version, testCase.Styles | AllowLeadingWhitespace));

            // Construct cases with trailing whitespace added
            foreach (var testCase in testCases.Where(c => !c.Styles.HasStyle(AllowTrailingWhitespace)
                                                          && !string.IsNullOrWhiteSpace(c.Version)
                                                          && c.ExceptionMessageFormat != ExceptionMessages.TrailingWhitespace).ToList())
                testCases.Add(testCase.Change(testCase.Version + " ", testCase.Styles | AllowTrailingWhitespace));

            var theoryData = new TheoryData<ParsingTestCase>();
            foreach (var testCase in testCases)
                theoryData.Add(testCase);
            return theoryData;
        }

        private static bool CanBePrefixedWithV(ParsingTestCase c)
        {
            if (string.IsNullOrWhiteSpace(c.Version)) return false;

            if (c.Version is null
                || c.Version.StartsWith("v", StringComparison.Ordinal)
                || c.Version.StartsWith("V", StringComparison.Ordinal)
                || c.Version.StartsWith(" ", StringComparison.Ordinal)
                || c.Version.StartsWith("\t", StringComparison.Ordinal))
                return false;

            return true;
        }

        private static ParsingTestCase Valid(
            string version,
            SemVersionStyles requiredStyles,
            int major,
            int minor = 0,
            int patch = 0,
            IEnumerable<PrereleaseIdentifier>? prerelease = null,
            IEnumerable<MetadataIdentifier>? metadata = null)
        {
            return ParsingTestCase.Valid(version, requiredStyles, major, minor, patch,
                prerelease ?? Enumerable.Empty<PrereleaseIdentifier>(),
                metadata ?? Enumerable.Empty<MetadataIdentifier>());
        }

        private static ParsingTestCase Valid(
            string version,
            int major,
            int minor = 0,
            int patch = 0,
            IEnumerable<PrereleaseIdentifier>? prerelease = null,
            IEnumerable<MetadataIdentifier>? metadata = null)
        {
            return ParsingTestCase.Valid(version, Strict, major, minor, patch,
            prerelease ?? Enumerable.Empty<PrereleaseIdentifier>(),
            metadata ?? Enumerable.Empty<MetadataIdentifier>());
        }

        private static ParsingTestCase ValidLongVersion(int seed)
        {
            var random = new Random(seed);
            var v = new StringBuilder();
            var major = random.Next(0, int.MaxValue);
            v.Append(major);
            v.Append('.');
            var minor = random.Next(0, int.MaxValue);
            v.Append(minor);
            v.Append('.');
            var patch = random.Next(0, int.MaxValue);
            v.Append(patch);
            v.Append('-');
            var identifierCount = random.Next(1_000);
            var prereleaseIdentifiers = new List<PrereleaseIdentifier>(identifierCount);
            for (int i = 0; i < identifierCount; i++)
                prereleaseIdentifiers.Add(ValidPrereleaseIdentifier(random));
            v.Append(string.Join(".", prereleaseIdentifiers));

            v.Append('+');
            identifierCount = random.Next(1_000);
            var metadataIdentifiers = new List<MetadataIdentifier>(identifierCount);
            for (int i = 0; i < identifierCount; i++)
                metadataIdentifiers.Add(ValidMetadataIdentifier(random));
            v.Append(string.Join(".", metadataIdentifiers));

            return ParsingTestCase.Valid(v.ToString(), Strict, major, minor, patch,
                prereleaseIdentifiers, metadataIdentifiers, maxLength: int.MaxValue);
        }

        private const string ValidIdentifierChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-";

        private static PrereleaseIdentifier ValidPrereleaseIdentifier(Random random)
        {
            if (random.NextBool())
            {
                var value = random.Next();
                return new PrereleaseIdentifier(value);
            }

            var charCount = random.Next(1, 1_000);
            var identifier = new StringBuilder(charCount);
            for (int i = 0; i < charCount; i++)
                identifier.Append(ValidIdentifierChars[random.Next(ValidIdentifierChars.Length)]);

            return new PrereleaseIdentifier(identifier.ToString());
        }

        private static MetadataIdentifier ValidMetadataIdentifier(Random random)
        {
            var charCount = random.Next(1, 1_000);
            var identifier = new StringBuilder(charCount);
            for (int i = 0; i < charCount; i++)
                identifier.Append(ValidIdentifierChars[random.Next(ValidIdentifierChars.Length)]);

            return new MetadataIdentifier(identifier.ToString());
        }

        private static ParsingTestCase Invalid<T>(
            string? version,
            SemVersionStyles requiredStyles,
            string exceptionMessage = "",
            string? exceptionValue = null,
            int maxLength = SemVersion.MaxVersionLength)
            where T : Exception
        {
            exceptionMessage = ExceptionMessages.InjectValue(exceptionMessage, exceptionValue);
            return ParsingTestCase.Invalid(version, requiredStyles, typeof(T), exceptionMessage, maxLength);
        }

        private static ParsingTestCase Invalid(
            string? version,
            SemVersionStyles requiredStyles,
            string exceptionMessage = "",
            string? exceptionValue = null,
            int maxLength = SemVersion.MaxVersionLength)
        {
            exceptionMessage = ExceptionMessages.InjectValue(exceptionMessage, exceptionValue);
            return ParsingTestCase.Invalid(version, requiredStyles, typeof(FormatException),
                exceptionMessage, maxLength);
        }

        private static ParsingTestCase Invalid<T>(
            string? version,
            string exceptionMessage = "",
            string? exceptionValue = null,
            int maxLength = SemVersion.MaxVersionLength)
            where T : Exception
        {
            exceptionMessage = ExceptionMessages.InjectValue(exceptionMessage, exceptionValue);
            return ParsingTestCase.Invalid(version, Strict, typeof(T), exceptionMessage, maxLength);
        }

        private static ParsingTestCase Invalid(
            string? version,
            string exceptionMessage = "",
            string? exceptionValue = null,
            int maxLength = SemVersion.MaxVersionLength)
        {
            exceptionMessage = ExceptionMessages.InjectValue(exceptionMessage, exceptionValue);
            return ParsingTestCase.Invalid(version, Strict, typeof(FormatException),
                exceptionMessage, maxLength);
        }

        public static IEnumerable<PrereleaseIdentifier> Pre(params TestPrereleaseIdentifier[] identifiers)
            => identifiers.Select(i => (PrereleaseIdentifier)i);

        public static IEnumerable<MetadataIdentifier> Meta(params string[] identifiers)
            => identifiers.Select(MetadataIdentifier.CreateUnsafe);
    }
}
