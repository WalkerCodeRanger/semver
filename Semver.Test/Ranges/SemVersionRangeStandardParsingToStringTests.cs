using Semver.Ranges;
using Semver.Test.TestCases;
using Xunit;
using static Semver.Ranges.SemVersionRangeOptions;

namespace Semver.Test.Ranges
{
    /// <summary>
    /// It was difficult to ensure that the <see cref="SemVersionRange.ToString"/> would output
    /// reasonable values for all the standard parsed ranges. These tests ensure good round-tripping
    /// for ranges.
    /// </summary>
    /// <remarks>Technically, these tests are more integration tests because they test both parsing
    /// and conversion to string.</remarks>
    public class SemVersionRangeStandardParsingToStringTests
    {
        public static readonly TheoryData<RangeParsingToStringTestCase> ParseToStringTestCases = new TheoryData<RangeParsingToStringTestCase>
        {
            Valid("*", "*"),
            Valid("*", IncludeAllPrerelease, "*-*"),
            Valid("*-*", "*-*"),
            Valid("1.2.3", "1.2.3"),
            Valid("1.2.*", "1.2.*"),
            Valid("1.2.*", IncludeAllPrerelease, "*-* 1.2.*"),
            Valid("1.2.* *-*", "*-* 1.2.*"),
            Valid("1.2.*-*", "1.2.*-*"),
            Valid("1.*.*", "1.*"),
            Valid("1.2.3-*", "1.2.3-*"),
            Valid("1.2.3-rc.*", "1.2.3-rc.*"),
            Valid("1.2.3-5.*", "1.2.3-5.*"),
            Valid("1.2.3-2147483647.*", "1.2.3-2147483647.*"),
        };

        [Theory]
        [MemberData(nameof(ParseToStringTestCases))]
        public void ParseRangeAndToString(RangeParsingToStringTestCase testCase)
        {
            var range = SemVersionRange.Parse(testCase.Range, testCase.Options, testCase.MaxLength);

            Assert.Equal(testCase.ExpectedRange, range.ToString());
        }

        internal static RangeParsingToStringTestCase Valid(
            string range,
            string expected,
            int maxLength = SemVersionRange.MaxRangeLength)
            => RangeParsingToStringTestCase.Valid(range, Strict, maxLength, expected);

        internal static RangeParsingToStringTestCase Valid(
            string range,
            SemVersionRangeOptions options,
            string expected,
            int maxLength = SemVersionRange.MaxRangeLength)
            => RangeParsingToStringTestCase.Valid(range, options, maxLength, expected);
    }
}
