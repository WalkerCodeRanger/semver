using System;
using Semver.Ranges;
using Semver.Test.Builders;
using Xunit;

namespace Semver.Test.Ranges
{
    public class SemVersionRangeParsingTests
    {
        public static readonly TheoryData<RangeParsingTestCase> ParsingTestCases = new TheoryData<RangeParsingTestCase>()
        {
            Valid("=1.2.3", SemVersionRange.Equals(new SemVersion(1,2,3))),
        };

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

                if (ex is ArgumentException argumentException)
                {
                    Assert.StartsWith(testCase.ExceptionMessage, argumentException.Message);
                    Assert.Equal("range", argumentException.ParamName);
                }
                else
                    Assert.Equal(testCase.ExceptionMessage, ex.Message);
            }
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
            => RangeParsingTestCase.Valid(range, SemVersionRangeOptions.Strict, 2048, expected);

        internal static RangeParsingTestCase Valid(string range, SemVersionRangeOptions options, SemVersionRange expected)
            => RangeParsingTestCase.Valid(range, options, 2048, expected);
    }
}
