using System;
using System.Globalization;
using Semver.Ranges;
using Semver.Test.Helpers;
using Semver.Test.TestCases;
using Xunit;
using static Semver.Ranges.SemVersionRangeOptions;
using static Semver.Test.Builders.UnbrokenSemVersionRangeBuilder;

namespace Semver.Test.Ranges
{
    public class SemVersionRangeStandardParsingTests
    {
        private const string InvalidSemVersionRangeOptionsMessageStart
            = "An invalid SemVersionRangeOptions value was used.";
        private const string InvalidMaxLengthMessageStart = "Must not be negative.";
        private const string TooLongRangeMessage = "Exceeded maximum length of {1} for '{0}'.";
        private const string InvalidOperatorMessage = "Invalid operator '{1}'.";
        private const string InvalidWhitespaceMessage =
            "Invalid whitespace character at {1} in '{0}'. Only the ASCII space character is allowed.";
        private const string MissingComparisonMessage
            = "Range is missing a comparison or limit at {1} in '{0}'";
        private const string MaxVersionMessage
            = "Cannot construct range from version '{1}' because version number cannot be incremented beyond max value.";

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
            //Valid("*", AllRelease),
            //Valid("*-*", All),
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
            //Valid(">1.2.3 || >4.5.6", GreaterThan("1.2.3")),
            Valid(">=1.2.3", AtLeast("1.2.3")),
            Valid("  >=1.2.3   ", AtLeast("1.2.3")),
            Valid(">=   1.2.3", AtLeast("1.2.3")),
            Valid(" >=   1.2.3 ", AtLeast("1.2.3")),
            Valid(">=1.2.3 >=4.5.6", AtLeast("4.5.6")),
            //Valid(">=1.2.3 || >=4.5.6", AtLeast("1.2.3")),
            Valid("<1.2.3", LessThan("1.2.3")),
            Valid("  <1.2.3   ", LessThan("1.2.3")),
            Valid("<   1.2.3", LessThan("1.2.3")),
            Valid(" <   1.2.3 ", LessThan("1.2.3")),
            Valid("<1.2.3 <4.5.6", LessThan("1.2.3")),
            //Valid("<1.2.3 || <4.5.6", LessThan("4.5.6")),
            Valid("<=1.2.3", AtMost("1.2.3")),
            Valid("  <=1.2.3   ", AtMost("1.2.3")),
            Valid("<=   1.2.3", AtMost("1.2.3")),
            Valid(" <=   1.2.3 ", AtMost("1.2.3")),
            Valid("<=1.2.3 <=4.5.6", AtMost("1.2.3")),
            //Valid("<=1.2.3 || <=4.5.6", AtMost("4.5.6")),
            //Valid("*-* >=2.0.0-0", AtLeast("2.0.0-0", true)),
            Valid("~1.2.3", InclusiveOfStart("1.2.3", "1.2.4-0")),

            // Already at max version
            Invalid("~1.2.2147483647", MaxVersionMessage, "1.2.2147483647"),

            // Missing Comparison
            Invalid("", MissingComparisonMessage, "0"),
            Invalid("   ", MissingComparisonMessage, "3"),
            Invalid("1.2.3||", MissingComparisonMessage, "7"),

            // Invalid Whitespace
            Invalid("  \t", InvalidWhitespaceMessage, "2"),
            Invalid("\t=1.2.3", InvalidWhitespaceMessage, "0"),
            Invalid("=\t1.2.3", InvalidWhitespaceMessage, "1"),
            Invalid("=1.2.3\t", InvalidWhitespaceMessage, "6"),

            // Invalid Operator
            Invalid("~>1.2.3", InvalidOperatorMessage, "~>"),
            Invalid("==1.2.3", InvalidOperatorMessage, "=="),
            Invalid("=1.2.3|4.5.6", InvalidOperatorMessage, "|"),
            Invalid("@&%1.2.3", InvalidOperatorMessage, "@&%"),
            Invalid("≥1.2.3", InvalidOperatorMessage, "≥"),

            // Longer than max length
            Invalid("=1.0.0", TooLongRangeMessage, "2", maxLength: 2),

            Invalid<ArgumentNullException>(null, ExceptionMessages.NotNull),
        };

        [Theory]
        [MemberData(nameof(InvalidSemVersionRangeOptions))]
        public void ParseWithInvalidOptions(SemVersionRangeOptions options)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVersionRange.Parse("ignored", options));

            Assert.StartsWith(InvalidSemVersionRangeOptionsMessageStart, ex.Message);
            Assert.Equal("options", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(InvalidMaxLength))]
        public void ParseWithInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => SemVersionRange.Parse("ignored", Strict, maxLength));

            Assert.StartsWith(InvalidMaxLengthMessageStart, ex.Message);
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
                    () => SemVersionRange.Parse(testCase.Range, testCase.Options, testCase.MaxLength));

                var expected = string.Format(CultureInfo.InvariantCulture,
                    testCase.ExceptionMessageFormat, testCase.Range.LimitLength());

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
            var ex = Assert.Throws<ArgumentException>(() => SemVersionRange.TryParse("ignored", options, out _));

            Assert.StartsWith(InvalidSemVersionRangeOptionsMessageStart, ex.Message);
            Assert.Equal("options", ex.ParamName);
        }

        [Theory]
        [MemberData(nameof(InvalidMaxLength))]
        public void TryParseWithInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                SemVersionRange.TryParse("ignored", Strict, out _, maxLength));

            Assert.StartsWith(InvalidMaxLengthMessageStart, ex.Message);
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

        internal static RangeParsingTestCase Valid(
            string range,
            SemVersionRange expected,
            int maxLength = SemVersionRange.MaxRangeLength)
            => RangeParsingTestCase.Valid(range, Strict, maxLength, expected);

        internal static RangeParsingTestCase Valid(
            string range,
            UnbrokenSemVersionRange expected,
            int maxLength = SemVersionRange.MaxRangeLength)
            => RangeParsingTestCase.Valid(range, Strict, maxLength, SemVersionRange.Create(expected));

        internal static RangeParsingTestCase Valid(string range, params UnbrokenSemVersionRange[] expectedRanges)
            => RangeParsingTestCase.Valid(range, Strict, SemVersionRange.MaxRangeLength, SemVersionRange.Create(expectedRanges));

        internal static RangeParsingTestCase Valid(string range, SemVersionRangeOptions options, SemVersionRange expected,
            int maxLength = SemVersionRange.MaxRangeLength)
            => RangeParsingTestCase.Valid(range, options, maxLength, expected);

        internal static RangeParsingTestCase Invalid<T>(
            string range,
            string exceptionMessage,
            int maxLength = SemVersionRange.MaxRangeLength)
            => RangeParsingTestCase.Invalid(range, Strict, maxLength, typeof(T), exceptionMessage);

        private static RangeParsingTestCase Invalid(
            string range,
            string exceptionMessage = "",
            string exceptionValue = null,
            int maxLength = SemVersionRange.MaxRangeLength)
        {
            exceptionMessage = ExceptionMessages.InjectValue(exceptionMessage, exceptionValue);
            return RangeParsingTestCase.Invalid(range, Strict, maxLength, typeof(FormatException), exceptionMessage);
        }
    }
}
