using System;
using Semver.Ranges;
using Semver.Test.Helpers;
using Semver.Test.TestCases;
using Xunit;
using static Semver.Test.Builders.NpmRangeParsingTestCaseBuilder;
using static Semver.Test.Builders.UnbrokenSemVersionRangeBuilder;

namespace Semver.Test.Ranges
{
    public class SemVersionRangeNpmParsingTests
    {
        public static readonly TheoryData<int> InvalidMaxLength = new TheoryData<int>()
        {
            -1,
            int.MinValue,
        };

        public static readonly TheoryData<NpmRangeParsingTestCase> ParsingTestCases = new TheoryData<NpmRangeParsingTestCase>()
        {
            Valid("1.2.3", EqualsVersion("1.2.3")),
        };

        [Theory]
        [MemberData(nameof(InvalidMaxLength))]
        public void ParseWithInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => SemVersionRange.ParseNpm("ignored", false, maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
            Assert.Equal(maxLength, ex.ActualValue);
        }

        [Theory]
        [MemberData(nameof(ParsingTestCases))]
        public void ParseWithOptionsParsesCorrectly(NpmRangeParsingTestCase testCase)
        {
            testCase.AssertParse();
        }

        [Theory]
        [MemberData(nameof(InvalidMaxLength))]
        public void TryParseWithInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => SemVersionRange.TryParseNpm("ignored", false, out _, maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
            Assert.Equal(maxLength, ex.ActualValue);
        }

        [Theory]
        [MemberData(nameof(ParsingTestCases))]
        public void TryParseWithOptionsParsesCorrectly(NpmRangeParsingTestCase testCase)
        {
            testCase.AssertTryParse();
        }
    }
}
