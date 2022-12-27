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

            // X-Ranges
            Valid("*", AllRelease),
            Valid("*", true, All),
            Valid("1.x", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("1.2.x", InclusiveOfStart("1.2.0", "1.3.0-0")),
            Valid("", AllRelease),
            Valid("", true, All),
            Valid("   ", AllRelease),
            Valid("1.2.3||", AllRelease, EqualsVersion("1.2.3")),
            Valid(" ||1.2.3", AllRelease, EqualsVersion("1.2.3")),
            Valid("1", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("1.2", InclusiveOfStart("1.2.0", "1.3.0-0")),
            Valid("1.X", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("1.*", InclusiveOfStart("1.0.0", "2.0.0-0")),
            // '1.x.x-rc' is allowed by npm but doesn't work right (shorter wildcards are invalid) npm issue #510
            Invalid("1.x.x-rc", ExceptionMessages.PrereleaseWithWildcardVersion), // accepted by npm issue #509
            Invalid("1.x-rc", ExceptionMessages.PrereleaseWithWildcardVersion),
            Invalid("1-rc", ExceptionMessages.PrereleaseWithWildcardVersion),
            Invalid("1.2.3-*", ExceptionMessages.InvalidCharacterInPrerelease, "*", "1.2.3-*"),
            Valid("1.x.x+build", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("1.x+build", InclusiveOfStart("1.0.0", "2.0.0-0")), // rejected by npm issue #509
            Valid("1+build", InclusiveOfStart("1.0.0", "2.0.0-0")), // rejected by npm issue #509
            Invalid("1.x.3", ExceptionMessages.MinorOrPatchMustBeWildcardVersion, "Patch", "1.x.3"), // accepted by npm issue #511
            Invalid("x.x.3", ExceptionMessages.MinorOrPatchMustBeWildcardVersion, "Patch", "x.x.3"), // accepted by npm issue #511
            Invalid("x.2.3", ExceptionMessages.MinorOrPatchMustBeWildcardVersion, "Minor", "x.2.3"), // accepted by npm issue #511

            // Wildcards with basic operators
            Valid("<1.2.*", LessThan("1.2.0-0")),
            Valid("<1.2.*", true, LessThan("1.2.0-0", true)),
            Valid("<1.*", LessThan("1.0.0-0")),
            Valid("<1.*", true, LessThan("1.0.0-0", true)),
            Valid("<*", Empty),
            Valid("<*", true, Empty),
            Valid("<=1.2.*", LessThan("1.3.0-0")),
            Valid("<=1.2.*", true, LessThan("1.3.0-0", true)),
            Valid("<=1.*", LessThan("2.0.0-0")),
            Valid("<=1.*", true, LessThan("2.0.0-0", true)),
            Valid("<=*", AllRelease),
            Valid("<=*", true, All),
            Valid(">1.2.*", AtLeast("1.3.0")),
            Valid(">1.2.*", true, AtLeast("1.3.0-0", true)),
            Valid(">1.*", AtLeast("2.0.0")),
            Valid(">1.*", true, AtLeast("2.0.0-0", true)),
            Valid(">*", Empty),
            Valid(">*", true, Empty),
            Valid(">=1.2.*", AtLeast("1.2.0")),
            Valid(">=1.2.*", true, AtLeast("1.2.0-0", true)),
            Valid(">=1.*", AtLeast("1.0.0")),
            Valid(">=1.*", true, AtLeast("1.0.0-0", true)),
            Valid(">=*", AllRelease),
            Valid(">=*", true, All),
            Valid("=1.2.*", InclusiveOfStart("1.2.0", "1.3.0-0")),
            Valid("=1.2.*", true, InclusiveOfStart("1.2.0-0", "1.3.0-0", true)),
            Valid("=1.*", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("=1.*", true, InclusiveOfStart("1.0.0-0", "2.0.0-0", true)),
            Valid("=*", AllRelease),
            Valid("=*", true, All),

            // Hyphen Ranges
            Valid("1.2.3 - 2.3.4", Inclusive("1.2.3", "2.3.4")),
            Valid("1.2 - 2.3.4", Inclusive("1.2.0", "2.3.4")),
            Valid("1.2.3 - 2.3", InclusiveOfStart("1.2.3", "2.4.0-0")),
            Valid("1.2.3 - 2", InclusiveOfStart("1.2.3", "3.0.0-0")),

            // Tilde Ranges
            Valid("~1.2.3", InclusiveOfStart("1.2.3", "1.3.0-0")),
            Valid("~1.2.3", true, InclusiveOfStart("1.2.3", "1.3.0-0", true)),
            Valid("~1.2.*", InclusiveOfStart("1.2.0", "1.3.0-0")),
            Valid("~1.2.*", true, InclusiveOfStart("1.2.0-0", "1.3.0-0", true)), // npm issue #512 parses as >=1.2.0 <1.3.0-0
            Valid("~1.*", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("~1.*", true, InclusiveOfStart("1.0.0-0", "2.0.0-0", true)), // npm issue #512 parses as >=1.0.0 <2.0.0-0
            Valid("~0.2.3", InclusiveOfStart("0.2.3", "0.3.0-0")),
            Valid("~0.2.*", InclusiveOfStart("0.2.0", "0.3.0-0")),
            Valid("~0.2.*", true, InclusiveOfStart("0.2.0-0", "0.3.0-0", true)), // npm issue #512 parses as >=0.2.0 <0.3.0-0
            Valid("~0.*", InclusiveOfStart("0.0.0", "1.0.0-0")),
            Valid("~0.*", true, InclusiveOfStart("0.0.0-0", "1.0.0-0", true)), // npm issue #512 parses as >=0.0.0 <1.0.0-0
            Valid("~1.2.3-beta.2", InclusiveOfStart("1.2.3-beta.2", "1.3.0-0")),
            Valid("~1.2.3-beta.2", true, InclusiveOfStart("1.2.3-beta.2", "1.3.0-0", true)),
            Valid("~*", AllRelease),
            Valid("~*", true, All),

            // Caret Ranges
            Valid("^1.2.3", InclusiveOfStart("1.2.3", "2.0.0-0")),
            Valid("^1.2.3", true, InclusiveOfStart("1.2.3", "2.0.0-0", true)),
            Valid("^0.2.3", InclusiveOfStart("0.2.3", "0.3.0-0")),
            Valid("^0.0.3", InclusiveOfStart("0.0.3", "0.0.4-0")),
            Valid("^1.2.3-beta.2", InclusiveOfStart("1.2.3-beta.2", "2.0.0-0")),
            Valid("^1.2.3-beta.2", true, InclusiveOfStart("1.2.3-beta.2", "2.0.0-0", true)),
            Valid("^0.0.3-beta", InclusiveOfStart("0.0.3-beta", "0.0.4-0")),
            Valid("^1.2.x", InclusiveOfStart("1.2.0", "2.0.0-0")),
            Valid("^1.2.x", true, InclusiveOfStart("1.2.0-0", "2.0.0-0", true)),
            Valid("^0.0.x", InclusiveOfStart("0.0.0", "0.1.0-0")),
            Valid("^0.0", InclusiveOfStart("0.0.0", "0.1.0-0")),
            Valid("^1.x", InclusiveOfStart("1.0.0", "2.0.0-0")),
            Valid("^1.x", true, InclusiveOfStart("1.0.0-0", "2.0.0-0", true)),
            Valid("^0.x", InclusiveOfStart("0.0.0", "1.0.0-0")),
            Valid("^*", AllRelease),
            Valid("^*", true, All),

            // Already at max version
            Invalid(">2147483647.*.*", ExceptionMessages.MaxVersion, version: "2147483647.*.*"),
            Invalid(">1.2147483647.*", ExceptionMessages.MaxVersion, version: "1.2147483647.*"),
            Invalid("~2147483647.*.*", ExceptionMessages.MaxVersion, version: "2147483647.*.*"),
            Invalid("~1.2147483647.3", ExceptionMessages.MaxVersion, version: "1.2147483647.3"),
            Invalid("^2147483647.2.3", ExceptionMessages.MaxVersion, version: "2147483647.2.3"),
            Invalid("^0.2147483647.3", ExceptionMessages.MaxVersion, version: "0.2147483647.3"),
            Invalid("^0.0.2147483647", ExceptionMessages.MaxVersion, version: "0.0.2147483647"),
            Invalid("2147483647.*", ExceptionMessages.MaxVersion, version: "2147483647.*"),
            Invalid("3.2147483647.*", ExceptionMessages.MaxVersion, version: "3.2147483647.*"),
            Invalid("2147483647.2147483647.*", ExceptionMessages.MaxVersion, version: "2147483647.2147483647.*"),

            // Invalid Operator
            Invalid("~<1.2.3", ExceptionMessages.InvalidOperator, "~<"),
            Invalid("~=1.2.3", ExceptionMessages.InvalidOperator, "~="),
            Invalid("==1.2.3", ExceptionMessages.InvalidOperator, "=="),
            Invalid("=1.2.3|4.5.6", ExceptionMessages.InvalidOperator, "|"),
            Invalid("@&%1.2.3", ExceptionMessages.InvalidOperator, "@&%"),
            Invalid("≥1.2.3", ExceptionMessages.InvalidOperator, "≥"),

            // Longer than max length
            Invalid("=1.0.0", ExceptionMessages.TooLongRange, "2", maxLength: 2),

            // Null range string
            Invalid<ArgumentNullException>(null, ExceptionMessages.NotNull),
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
