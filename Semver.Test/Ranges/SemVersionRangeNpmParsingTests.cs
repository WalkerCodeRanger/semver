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
            // Basic Operators
            Valid("<1.2.3", LessThan("1.2.3")),
            Valid("<= 1.2.3", AtMost("1.2.3")),
            Valid(">1.2.3", GreaterThan("1.2.3")),
            Valid(">=1.2.3", AtLeast("1.2.3")),
            Valid("=1.2.3", EqualsVersion("1.2.3")),
            Valid("1.2.3", EqualsVersion("1.2.3")),

            // Comparator Sets
            Valid(">=1.2.7 <1.3.0", InclusiveOfStart("1.2.7", "1.3.0")),

            // Alternatives
            Valid("1.2.7 || >=1.2.9 <2.0.0", EqualsVersion("1.2.7"), InclusiveOfStart("1.2.9", "2.0.0")),

            // Prerelease in Version
            Valid(">1.2.3-alpha.3", GreaterThan("1.2.3-alpha.3")),

            // Hyphen Ranges
            Valid("1.2.3 - 2.3.4", Inclusive("1.2.3", "2.3.4")),
            Valid("1.2 - 2.3.4", Inclusive("1.2.0", "2.3.4")),
            Valid("1.2.3 - 2.3", InclusiveOfStart("1.2.3", "2.4.0-0")),
            Valid("1.2.3 - 2", InclusiveOfStart("1.2.3", "3.0.0-0")),

            // X-Ranges
            Valid("*", AllRelease),
            Valid("1.x", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("1.2.x", InclusiveOfStart("1.2.0", "1.3.0-0")),
            Valid("", AllRelease),
            Valid("1", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("1.2", InclusiveOfStart("1.2.0", "1.3.0-0")),
            Valid("1.X", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("1.*", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Invalid("1.2.3-*", ExceptionMessages.InvalidCharacterInPrerelease, "*", "1.2.3-*"),

            // Tilde Ranges
            Valid("~1.2.3", InclusiveOfStart("1.2.3", "1.3.0-0")),
            Valid("~1.2", InclusiveOfStart("1.2.0", "1.3.0-0")),
            Valid("~1", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("~0.2.3", InclusiveOfStart("0.2.3", "0.3.0-0")),
            Valid("~0.2", InclusiveOfStart("0.2.0", "0.3.0-0")),
            Valid("~0", InclusiveOfStart("0.0.0", "1.0.0-0")),
            Valid("~1.2.3-beta.2", InclusiveOfStart("1.2.3-beta.2", "1.3.0-0")),

            // Caret Ranges
            Valid("^1.2.3", InclusiveOfStart("1.2.3", "2.0.0-0")),
            Valid("^0.2.3", InclusiveOfStart("0.2.3", "0.3.0-0")),
            Valid("^0.0.3", InclusiveOfStart("0.0.3", "0.0.4-0")),
            Valid("^1.2.3-beta.2", InclusiveOfStart("1.2.3-beta.2", "2.0.0-0")),
            Valid("^0.0.3-beta", InclusiveOfStart("0.0.3-beta", "0.0.4-0")),
            Valid("^1.2.x", InclusiveOfStart("1.2.0", "2.0.0-0")),
            Valid("^0.0.x", InclusiveOfStart("0.0.0", "0.1.0-0")),
            Valid("^0.0", InclusiveOfStart("0.0.0", "0.1.0-0")),
            Valid("^1.x", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("^0.x", InclusiveOfStart("0.0.0", "1.0.0-0")),
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
