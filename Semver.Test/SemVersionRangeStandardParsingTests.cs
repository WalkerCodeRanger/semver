using System;
using Semver.Test.Helpers;
using Semver.Test.TestCases;
using Xunit;
using static Semver.SemVersionRangeOptions;
using static Semver.Test.Builders.UnbrokenSemVersionRangeBuilder;

namespace Semver.Test
{
    public class SemVersionRangeStandardParsingTests
    {
        public static readonly TheoryData<SemVersionRangeOptions> InvalidSemVersionRangeOptions = new TheoryData<SemVersionRangeOptions>()
        {
            // Optional minor flag without optional patch flag
            OptionalMinorPatch & ~OptionalPatch,
            // Next unused bit flag
            SemVersionRangeOptionsExtensions.AllFlags + 1,
        };

        public static readonly TheoryData<int> InvalidMaxLength = new TheoryData<int>()
        {
            -1,
            int.MinValue,
        };

        public static readonly TheoryData<RangeParsingTestCase> ParsingTestCases = new TheoryData<RangeParsingTestCase>()
        {
            Valid("1.2.3", EqualsVersion("1.2.3")),
            Valid("  1.2.3   ", EqualsVersion("1.2.3")),
            Valid("   1.2.3", EqualsVersion("1.2.3")),
            Valid("1.2.3  ", EqualsVersion("1.2.3")),
            Valid("1.2.3 4.5.6", Empty),
            Valid("1.2.3 || 4.5.6", EqualsVersion("1.2.3"), EqualsVersion("4.5.6")),
            Valid("=1.2.3", EqualsVersion("1.2.3")),
            Valid("  =1.2.3   ", EqualsVersion("1.2.3")),
            Valid("=   1.2.3", EqualsVersion("1.2.3")),
            Valid(" =   1.2.3 ", EqualsVersion("1.2.3")),
            Valid("=1.2.3 =4.5.6", Empty),
            Valid("=1.2.3 || =4.5.6", EqualsVersion("1.2.3"), EqualsVersion("4.5.6")),
            // Shows sorting of ranges
            Valid("=4.5.6||=1.2.3", EqualsVersion("1.2.3"), EqualsVersion("4.5.6")),
            Valid(">1.2.3", GreaterThan("1.2.3")),
            Valid("  >1.2.3   ", GreaterThan("1.2.3")),
            Valid(">   1.2.3", GreaterThan("1.2.3")),
            Valid(" >   1.2.3 ", GreaterThan("1.2.3")),
            Valid(">1.2.3 >4.5.6", GreaterThan("4.5.6")),
            Valid(">1.2.3 || >4.5.6", GreaterThan("1.2.3")),
            Valid(">=1.2.3", AtLeast("1.2.3")),
            Valid("  >=1.2.3   ", AtLeast("1.2.3")),
            Valid(">=   1.2.3", AtLeast("1.2.3")),
            Valid(" >=   1.2.3 ", AtLeast("1.2.3")),
            Valid(">=1.2.3 >=4.5.6", AtLeast("4.5.6")),
            Valid(">=1.2.3 || >=4.5.6", AtLeast("1.2.3")),
            Valid("<1.2.3", LessThan("1.2.3")),
            Valid("  <1.2.3   ", LessThan("1.2.3")),
            Valid("<   1.2.3", LessThan("1.2.3")),
            Valid(" <   1.2.3 ", LessThan("1.2.3")),
            Valid("<1.2.3 <4.5.6", LessThan("1.2.3")),
            Valid("<1.2.3 || <4.5.6", LessThan("4.5.6")),
            Valid("<=1.2.3", AtMost("1.2.3")),
            Valid("  <=1.2.3   ", AtMost("1.2.3")),
            Valid("<=   1.2.3", AtMost("1.2.3")),
            Valid(" <=   1.2.3 ", AtMost("1.2.3")),
            Valid("<=1.2.3 <=4.5.6", AtMost("1.2.3")),
            Valid("<=1.2.3 || <=4.5.6", AtMost("4.5.6")),
            Valid("*-* >=2.0.0-0", AtLeast("2.0.0-0", true)),
            Valid("~1.2.3", InclusiveOfStart("1.2.3", "1.3.0-0")),
            Valid("~1.2.3-rc", InclusiveOfStart("1.2.3-rc", "1.3.0-0")),
            Valid("^1.2.3", InclusiveOfStart("1.2.3", "2.0.0-0")),
            Valid("^1.2.3-rc", InclusiveOfStart("1.2.3-rc", "2.0.0-0")),
            Valid("^0.2.3", InclusiveOfStart("0.2.3", "0.3.0-0")),
            Valid("^0.2.3-rc", InclusiveOfStart("0.2.3-rc", "0.3.0-0")),
            Valid("^0.0.3", InclusiveOfStart("0.0.3", "0.0.4-0")),
            Valid("^0.0.3-rc", InclusiveOfStart("0.0.3-rc", "0.0.4-0")),
            Valid(">=4.0.0", IncludeAllPrerelease, AtLeast("4.0.0", true)),

            // Wildcard versions
            Valid("*", AllRelease),
            Valid("*.*", AllRelease),
            Valid("*.*.*", AllRelease),
            Valid("*-*", All),
            Valid("*.*-*", All),
            Valid("*.*.*-*", All),
            Valid("3.*", InclusiveOfStart("3.0.0", "4.0.0-0")),
            Valid("3.*-*", InclusiveOfStart("3.0.0-0", "4.0.0-0", true)),
            Valid("3.*.*", InclusiveOfStart("3.0.0", "4.0.0-0")),
            Valid("3.*.*-*", InclusiveOfStart("3.0.0-0", "4.0.0-0", true)),
            Valid("3.1.*", InclusiveOfStart("3.1.0", "3.2.0-0")),
            Valid("3.1.*-*", InclusiveOfStart("3.1.0-0", "3.2.0-0", true)),
            Valid("3.1.4-*", InclusiveOfStart("3.1.4-0", "3.1.5-0", true)),
            Valid("3.1.4-rc.*", InclusiveOfStart("3.1.4-rc.0", "3.1.4-rc-", true)),
            Valid("3.1.4-5.*", InclusiveOfStart("3.1.4-5.0", "3.1.4-6", true)),
            Valid("3.1.4-2147483647.*", InclusiveOfStart("3.1.4-2147483647.0", "3.1.4-2147483648", true)),
            Valid("3.1.4-l.o.n.g.e.r.rc.*", InclusiveOfStart("3.1.4-l.o.n.g.e.r.rc.0", "3.1.4-l.o.n.g.e.r.rc-", true)),

            // Range containment (or non-containment)
            Valid("1.*||1.2.*", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("1.*-*||1.2.*", InclusiveOfStart("1.0.0-0", "2.0.0-0", true)),
            Valid("1.*||1.2.*-*", InclusiveOfStart("1.0.0", "2.0.0-0"), InclusiveOfStart("1.2.0-0", "1.3.0-0", true)),
            Valid("1.*||2.*", InclusiveOfStart("1.0.0", "3.0.0-0")),
            Valid("<2.0.0||>=2.0.0", AllRelease),
            Valid(">=1.2.3 <=2.5.0||>=2.0.0 <=3.1.4", Inclusive("1.2.3", "3.1.4")),
            Valid(">=1.2.3 <=2.5.0|| *-* >=2.0.0 <=3.1.4", Inclusive("1.2.3", "2.5.0"), Inclusive("2.0.0", "3.1.4", true)),
            Valid(">=1.2.3 <=2.5.0||>=2.0.0-rc <=3.1.4", Inclusive("1.2.3", "2.5.0"), Inclusive("2.0.0-rc", "3.1.4")),
            Valid("*-* >=1.2.3 <=2.5.0||>=2.0.0-rc <=3.1.4", Inclusive("1.2.3", "2.5.0", true), Inclusive("2.0.0-rc", "3.1.4")),

            // Loose Parsing
            Valid("v01+build", Loose, EqualsVersion("1.0.0")),

            // Wildcard before version
            Invalid(">*.2.3", ExceptionMessages.MinorOrPatchMustBeWildcardVersion, "Minor", "*.2.3"),
            Invalid(">1.*.3", ExceptionMessages.MinorOrPatchMustBeWildcardVersion, "Patch", "1.*.3"),
            Invalid(">*.*.3", ExceptionMessages.MinorOrPatchMustBeWildcardVersion, "Patch", "*.*.3"),

            // Wildcard char in major, minor, or patch
            Invalid(">*1.2.3", ExceptionMessages.InvalidWildcardInMajorMinorOrPatch, "Major", "*1.2.3"),
            Invalid(">1.*2.3", ExceptionMessages.InvalidWildcardInMajorMinorOrPatch, "Minor", "1.*2.3"),
            Invalid(">1.2.*3", ExceptionMessages.InvalidWildcardInMajorMinorOrPatch, "Patch", "1.2.*3"),

            // Wildcard char in prerelease identifier
            Invalid("1.2.3-*a", ExceptionMessages.InvalidWildcardInPrerelease),
            Invalid("1.2.3-a*b", ExceptionMessages.InvalidWildcardInPrerelease),
            Invalid("1.2.3-a*", ExceptionMessages.InvalidWildcardInPrerelease),
            Invalid("1.2.3-a.*a", ExceptionMessages.InvalidWildcardInPrerelease),
            Invalid("1.2.3-a.a*b", ExceptionMessages.InvalidWildcardInPrerelease),
            Invalid("1.2.3-a.a*", ExceptionMessages.InvalidWildcardInPrerelease),

            // Prerelease wildcard not last
            Invalid("1.2.3-*.a", ExceptionMessages.PrereleaseWildcardMustBeLast),
            Invalid("1.2.3-a.*.a", ExceptionMessages.PrereleaseWildcardMustBeLast),

            // Prerelease after wildcard version
            Invalid("*-rc", ExceptionMessages.PrereleaseWithWildcardVersion),
            Invalid("*-rc.*", ExceptionMessages.PrereleaseWithWildcardVersion),
            Invalid("1.*-rc", ExceptionMessages.PrereleaseWithWildcardVersion),
            Invalid("1.*-rc.*", ExceptionMessages.PrereleaseWithWildcardVersion),
            Invalid("1.2.*-rc", ExceptionMessages.PrereleaseWithWildcardVersion),
            Invalid("1.2.*-rc.*", ExceptionMessages.PrereleaseWithWildcardVersion),

            // Going past max version
            Valid("~1.2147483647.3", InclusiveOfStart("1.2147483647.3", "1.2147483648.0-0")),
            Valid("^2147483647.2.3", InclusiveOfStart("2147483647.2.3", "2147483648.0.0-0")),
            Valid("^0.2147483647.3", InclusiveOfStart("0.2147483647.3", "0.2147483648.0-0")),
            Valid("^0.0.2147483647", InclusiveOfStart("0.0.2147483647", "0.0.2147483648-0")),
            Valid("2147483647.*", InclusiveOfStart("2147483647.0.0", "2147483648.0.0-0")),
            Valid("3.2147483647.*", InclusiveOfStart("3.2147483647.0", "3.2147483648.0-0")),
            Valid("2147483647.2147483647.*", InclusiveOfStart("2147483647.2147483647.0", "2147483647.2147483648.0-0")),
            Valid("3.1.2147483647-*", InclusiveOfStart("3.1.2147483647-0", "3.1.2147483648-0")),

            // Missing Comparison
            Invalid("", ExceptionMessages.MissingComparison, "0"),
            Invalid("   ", ExceptionMessages.MissingComparison, "3"),
            Invalid("1.2.3||", ExceptionMessages.MissingComparison, "7"),
            Invalid(" ||1.2.3", ExceptionMessages.MissingComparison, "1"),

            // Invalid Whitespace
            Invalid("  \t", ExceptionMessages.InvalidWhitespace, "2"),
            Invalid("\t=1.2.3", ExceptionMessages.InvalidWhitespace, "0"),
            Invalid("=\t1.2.3", ExceptionMessages.InvalidWhitespace, "1"),
            Invalid("=1.2.3\t", ExceptionMessages.InvalidWhitespace, "6"),

            // Invalid Operator
            Invalid("~>1.2.3", ExceptionMessages.InvalidOperator, "~>"),
            Invalid("==1.2.3", ExceptionMessages.InvalidOperator, "=="),
            Invalid("=1.2.3|4.5.6", ExceptionMessages.InvalidOperator, "|"),
            Invalid("@&%1.2.3", ExceptionMessages.InvalidOperator, "@&%"),
            Invalid("≥1.2.3", ExceptionMessages.InvalidOperator, "≥"),

            // Longer than max length
            Invalid("=1.0.0", ExceptionMessages.TooLongRange, "2", maxLength: 2),

            // Wildcard with operator
            Invalid("<1.*", ExceptionMessages.WildcardNotSupportedWithOperator),
            Invalid("<=1.2.*", ExceptionMessages.WildcardNotSupportedWithOperator),
            Invalid(">*", ExceptionMessages.WildcardNotSupportedWithOperator),
            Invalid(">=1.2.3-*", ExceptionMessages.WildcardNotSupportedWithOperator),
            Invalid("=1.2.*", ExceptionMessages.WildcardNotSupportedWithOperator),
            Invalid("^1.2.*", ExceptionMessages.WildcardNotSupportedWithOperator),
            Invalid("~1.2.*", ExceptionMessages.WildcardNotSupportedWithOperator),

            // Null range string
            Invalid<ArgumentNullException>(null, ExceptionMessages.NotNull),
        };

        [Theory]
        [MemberData(nameof(InvalidSemVersionRangeOptions))]
        public void ParseWithInvalidOptions(SemVersionRangeOptions options)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersionRange.Parse("ignored", options));

            Assert.StartsWith(ExceptionMessages.InvalidSemVersionRangeOptionsStart, ex.Message);
            Assert.Equal("options", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(InvalidMaxLength))]
        public void ParseWithInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => SemVersionRange.Parse("ignored", Strict, maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
            Assert.Equal(maxLength, ex.ActualValue);
        }

        [Theory]
        [MemberData(nameof(ParsingTestCases))]
        public void ParseWithOptionsParsesCorrectly(RangeParsingTestCase testCase)
        {
            if (testCase is null) throw new ArgumentNullException(nameof(testCase));

            if (testCase.IsValid)
            {
                var range = SemVersionRange.Parse(testCase.Range, testCase.Options, testCase.MaxLength);
                Assert.Equal(testCase.ExpectedRange, range);
            }
            else
            {
                var ex = Assert.Throws(testCase.ExceptionType,
                    () => SemVersionRange.Parse(testCase.Range!, testCase.Options, testCase.MaxLength));

                var expected = ExceptionMessages.InjectRange(testCase.ExceptionMessageFormat, testCase.Range);

                if (ex is ArgumentException argumentException)
                {
                    Assert.StartsWith(expected, argumentException.Message);
                    Assert.Equal("range", argumentException.ParamName);
                }
                else
                    Assert.Equal(expected, ex.Message);
            }
        }

        [Theory]
        [MemberData(nameof(InvalidSemVersionRangeOptions))]
        public void TryParseWithInvalidOptions(SemVersionRangeOptions options)
        {
            var ex = Assert.Throws<ArgumentException>(
                () => SemVersionRange.TryParse("ignored", options, out _));

            Assert.StartsWith(ExceptionMessages.InvalidSemVersionRangeOptionsStart, ex.Message);
            Assert.Equal("options", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(InvalidMaxLength))]
        public void TryParseWithInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => SemVersionRange.TryParse("ignored", Strict, out _, maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
            Assert.Equal(maxLength, ex.ActualValue);
        }

        [Theory]
        [MemberData(nameof(ParsingTestCases))]
        public void TryParseWithOptionsParsesCorrectly(RangeParsingTestCase testCase)
        {
            if (testCase is null) throw new ArgumentNullException(nameof(testCase));

            var result = SemVersionRange.TryParse(testCase.Range, testCase.Options, out var semverRange, testCase.MaxLength);

            Assert.Equal(testCase.IsValid, result);

            if (testCase.IsValid)
                Assert.Equal(testCase.ExpectedRange, semverRange);
            else
                Assert.Null(semverRange);
        }

        /// <summary>
        /// This doesn't need to check all cases, just that this overload forwards.
        /// </summary>
        [Fact]
        public void TryParseWithoutOptions()
        {
            Assert.True(SemVersionRange.TryParse("1.*", out var range));

            Assert.Equal(SemVersionRange.Create(InclusiveOfStart("1.0.0", "2.0.0-0")), range);
        }

        internal static RangeParsingTestCase Valid(
            string range,
            UnbrokenSemVersionRange expected,
            int maxLength = SemVersionRange.MaxRangeLength)
            => RangeParsingTestCase.Valid(range, Strict, maxLength, SemVersionRange.Create(expected));

        internal static RangeParsingTestCase Valid(string range, params UnbrokenSemVersionRange[] expectedRanges)
            => RangeParsingTestCase.Valid(range, Strict, SemVersionRange.MaxRangeLength, SemVersionRange.Create(expectedRanges));

        internal static RangeParsingTestCase Valid(
            string range,
            SemVersionRangeOptions options,
            params UnbrokenSemVersionRange[] expectedRanges)
            => RangeParsingTestCase.Valid(range, options, SemVersionRange.MaxRangeLength, SemVersionRange.Create(expectedRanges));

        internal static RangeParsingTestCase Invalid<T>(
            string? range,
            string message,
            int maxLength = SemVersionRange.MaxRangeLength)
            => RangeParsingTestCase.Invalid(range, Strict, maxLength, typeof(T), message);

        private static RangeParsingTestCase Invalid(
            string range,
            string message = "",
            string? value = null,
            string? version = null,
            int maxLength = SemVersionRange.MaxRangeLength)
        {
            message = ExceptionMessages.InjectValue(message, value);
            message = ExceptionMessages.InjectVersion(message, version);
            return RangeParsingTestCase.Invalid(range, Strict, maxLength, typeof(FormatException), message);
        }
    }
}
