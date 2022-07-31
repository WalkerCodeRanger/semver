using System;
using System.Globalization;
using Semver.Ranges;
using Semver.Test.Builders;
using Xunit;
using static Semver.Ranges.SemVersionRangeOptions;

namespace Semver.Test.Ranges
{
    public class SemVersionRangeParsingTests
    {
        private const string InvalidSemVersionRangeOptionsMessageStart = "An invalid SemVersionRangeOptions value was used.";
        private const string InvalidMaxLengthMessageStart = "Must not be negative.";
        private const string TooLongRangeMessage = "Exceeded maximum length of {1} for '{0}'.";

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
            Valid("=1.2.3", Equals("1.2.3")),
            Valid("  =1.2.3   ", Equals("1.2.3")),
            Valid("=   1.2.3", Equals("1.2.3")),
            Valid(" =   1.2.3 ", Equals("1.2.3")),
            Valid("=1.2.3 || =4.5.6", Equals("1.2.3"), Equals("4.5.6")),
            Valid(">1.2.3", GreaterThan("1.2.3")),
            Valid("  >1.2.3   ", GreaterThan("1.2.3")),
            Valid(">   1.2.3", GreaterThan("1.2.3")),
            Valid(" >   1.2.3 ", GreaterThan("1.2.3")),
            //Valid(">1.2.3 || >4.5.6", GreaterThan("1.2.3")),
            Valid(">=1.2.3", AtLeast("1.2.3")),
            Valid("  >=1.2.3   ", AtLeast("1.2.3")),
            Valid(">=   1.2.3", AtLeast("1.2.3")),
            Valid(" >=   1.2.3 ", AtLeast("1.2.3")),
            //Valid(">=1.2.3 || >=4.5.6", AtLeast("1.2.3")),
            Valid("<1.2.3", LessThan("1.2.3")),
            Valid("  <1.2.3   ", LessThan("1.2.3")),
            Valid("<   1.2.3", LessThan("1.2.3")),
            Valid(" <   1.2.3 ", LessThan("1.2.3")),
            //Valid("<1.2.3 || <4.5.6", LessThan("4.5.6")),
            Valid("<=1.2.3", AtMost("1.2.3")),
            Valid("  <=1.2.3   ", AtMost("1.2.3")),
            Valid("<=   1.2.3", AtMost("1.2.3")),
            Valid(" <=   1.2.3 ", AtMost("1.2.3")),
            //Valid("<=1.2.3 || <=4.5.6", AtMost("4.5.6")),

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

        internal static RangeParsingTestCase Valid(string range, SemVersionRange expected)
            => RangeParsingTestCase.Valid(range, Strict, 2048, expected);

        internal static RangeParsingTestCase Valid(string range, UnbrokenSemVersionRange expected)
            => RangeParsingTestCase.Valid(range, Strict, 2048, new SemVersionRange(expected));

        internal static RangeParsingTestCase Valid(string range, params UnbrokenSemVersionRange[] expectedRanges)
            => RangeParsingTestCase.Valid(range, Strict, 2048, new SemVersionRange(expectedRanges));

        internal static RangeParsingTestCase Valid(string range, SemVersionRangeOptions options, SemVersionRange expected)
            => RangeParsingTestCase.Valid(range, options, 2048, expected);

        internal static RangeParsingTestCase Invalid<T>(string range, string exceptionMessage)
            => RangeParsingTestCase.Invalid(range, Strict, 2048, typeof(T), exceptionMessage);

        private static RangeParsingTestCase Invalid(
            string range,
            string exceptionMessage = "",
            string exceptionValue = null,
            int maxLength = SemVersionRange.MaxRangeLength)
        {
            exceptionMessage = ExceptionMessages.InjectValue(exceptionMessage, exceptionValue);
            return RangeParsingTestCase.Invalid(range, Strict, maxLength, typeof(FormatException), exceptionMessage);
        }

        internal static UnbrokenSemVersionRange Equals(string version)
            => UnbrokenSemVersionRange.Equals(SemVersion.Parse(version, SemVersionStyles.Strict));

        internal static UnbrokenSemVersionRange GreaterThan(string version)
            => UnbrokenSemVersionRange.GreaterThan(SemVersion.Parse(version, SemVersionStyles.Strict));

        internal static UnbrokenSemVersionRange AtLeast(string version)
            => UnbrokenSemVersionRange.AtLeast(SemVersion.Parse(version, SemVersionStyles.Strict));

        internal static UnbrokenSemVersionRange LessThan(string version)
            => UnbrokenSemVersionRange.LessThan(SemVersion.Parse(version, SemVersionStyles.Strict));

        internal static UnbrokenSemVersionRange AtMost(string version)
            => UnbrokenSemVersionRange.AtMost(SemVersion.Parse(version, SemVersionStyles.Strict));
    }
}
