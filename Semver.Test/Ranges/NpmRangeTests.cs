using System;
using Semver.Test.Helpers;
using Semver.Test.TestCases;
using Xunit;
using static Semver.Test.Builders.NpmRangeParsingTestCaseBuilder;
using static Semver.Test.Builders.UnbrokenSemVersionRangeBuilder;

namespace Semver.Test.Ranges
{

    /// <summary>
    /// Test cases taken directly from the npm project for versions ranges.
    /// </summary>
    public class NpmRangeTests
    {
        /// <summary>
        /// npm version range includes cases from https://github.com/npm/node-semver/blob/main/test/fixtures/range-include.js
        /// </summary>
        /// <remarks>The loose option is not supported, so loose test cases have been removed if
        /// they otherwise duplicate other test cases or modified to not require the loose option.</remarks>
        public static readonly TheoryData<NpmRangeContainsTestCase> IncludesCases
            = new TheoryData<NpmRangeContainsTestCase>
            {
                Includes("1.0.0 - 2.0.0", "1.2.3"),
                Includes("^1.2.3+build", "1.2.3"),
                Includes("^1.2.3+build", "1.3.0"),
                Includes("1.2.3-pre+asdf - 2.4.3-pre+asdf", "1.2.3"),
                Includes("1.2.3-pre+asdf - 2.4.3-pre+asdf", "1.2.3-pre.2"),
                Includes("1.2.3-pre+asdf - 2.4.3-pre+asdf", "2.4.3-alpha"),
                Includes("1.2.3+asdf - 2.4.3+asdf", "1.2.3"),
                Includes("1.0.0", "1.0.0"),
                Includes(">=*", "0.2.4"),
                Includes("", "1.0.0"),
                Includes("*", "1.2.3"),
                Includes(">=1.0.0", "1.0.0"),
                Includes(">=1.0.0", "1.0.1"),
                Includes(">=1.0.0", "1.1.0"),
                Includes(">1.0.0", "1.0.1"),
                Includes(">1.0.0", "1.1.0"),
                Includes("<=2.0.0", "2.0.0"),
                Includes("<=2.0.0", "1.9999.9999"),
                Includes("<=2.0.0", "0.2.9"),
                Includes("<2.0.0", "1.9999.9999"),
                Includes("<2.0.0", "0.2.9"),
                Includes(">= 1.0.0", "1.0.0"),
                Includes(">=  1.0.0", "1.0.1"),
                Includes(">=   1.0.0", "1.1.0"),
                Includes("> 1.0.0", "1.0.1"),
                Includes(">  1.0.0", "1.1.0"),
                Includes("<=   2.0.0", "2.0.0"),
                Includes("<= 2.0.0", "1.9999.9999"),
                Includes("<=  2.0.0", "0.2.9"),
                Includes("<    2.0.0", "1.9999.9999"),
                Includes("<\t2.0.0", "0.2.9"),
                Includes(">=0.1.97", "0.1.97"),
                Includes("0.1.20 || 1.2.4", "1.2.4"),
                Includes(">=0.2.3 || <0.0.1", "0.0.0"),
                Includes(">=0.2.3 || <0.0.1", "0.2.3"),
                Includes(">=0.2.3 || <0.0.1", "0.2.4"),
                Includes("||", "1.3.4"),
                Includes("2.x.x", "2.1.3"),
                Includes("1.2.x", "1.2.3"),
                Includes("1.2.x || 2.x", "2.1.3"),
                Includes("1.2.x || 2.x", "1.2.3"),
                Includes("x", "1.2.3"),
                Includes("2.*.*", "2.1.3"),
                Includes("1.2.*", "1.2.3"),
                Includes("1.2.* || 2.*", "2.1.3"),
                Includes("1.2.* || 2.*", "1.2.3"),
                Includes("*", "1.2.3"),
                Includes("2", "2.1.2"),
                Includes("2.3", "2.3.1"),
                Includes("~0.0.1", "0.0.1"),
                Includes("~0.0.1", "0.0.2"),
                Includes("~x", "0.0.9"), // >=2.4.0 <2.5.0
                Includes("~2", "2.0.9"), // >=2.4.0 <2.5.0
                Includes("~2.4", "2.4.0"), // >=2.4.0 <2.5.0
                Includes("~2.4", "2.4.5"),
                Includes("~>3.2.1", "3.2.2"), // >=3.2.1 <3.3.0,
                Includes("~1", "1.2.3"), // >=1.0.0 <2.0.0
                Includes("~>1", "1.2.3"),
                Includes("~> 1", "1.2.3"),
                Includes("~1.0", "1.0.2"), // >=1.0.0 <1.1.0,
                Includes("~ 1.0", "1.0.2"),
                Includes("~ 1.0.3", "1.0.12"),
                Includes("~ 1.0.3-alpha", "1.0.12"), // Modified from loose test case
                Includes(">=1", "1.0.0"),
                Includes(">= 1", "1.0.0"),
                Includes("<1.2", "1.1.1"),
                Includes("< 1.2", "1.1.1"),
                Includes("~v0.5.4-pre", "0.5.5"),
                Includes("~v0.5.4-pre", "0.5.4"),
                Includes("=0.7.x", "0.7.2"),
                Includes("<=0.7.x", "0.7.2"),
                Includes(">=0.7.x", "0.7.2"),
                Includes("<=0.7.x", "0.6.2"),
                Includes("~1.2.1 >=1.2.3", "1.2.3"),
                Includes("~1.2.1 =1.2.3", "1.2.3"),
                Includes("~1.2.1 1.2.3", "1.2.3"),
                Includes("~1.2.1 >=1.2.3 1.2.3", "1.2.3"),
                Includes("~1.2.1 1.2.3 >=1.2.3", "1.2.3"),
                Includes("~1.2.1 1.2.3", "1.2.3"),
                Includes(">=1.2.1 1.2.3", "1.2.3"),
                Includes("1.2.3 >=1.2.1", "1.2.3"),
                Includes(">=1.2.3 >=1.2.1", "1.2.3"),
                Includes(">=1.2.1 >=1.2.3", "1.2.3"),
                Includes(">=1.2", "1.2.8"),
                Includes("^1.2.3", "1.8.1"),
                Includes("^0.1.2", "0.1.2"),
                Includes("^0.1", "0.1.2"),
                Includes("^0.0.1", "0.0.1"),
                Includes("^1.2", "1.4.2"),
                Includes("^1.2 ^1", "1.4.2"),
                Includes("^1.2.3-alpha", "1.2.3-pre"),
                Includes("^1.2.0-alpha", "1.2.0-pre"),
                Includes("^0.0.1-alpha", "0.0.1-beta"),
                Includes("^0.0.1-alpha", "0.0.1"),
                Includes("^0.1.1-alpha", "0.1.1-beta"),
                Includes("^x", "1.2.3"),
                Includes("x - 1.0.0", "0.9.7"),
                Includes("x - 1.x", "0.9.7"),
                Includes("1.0.0 - x", "1.9.7"),
                Includes("1.x - x", "1.9.7"),
                Includes("<=7.x", "7.9.9"),
                Includes("2.x", "2.0.0-pre.0", true),
                Includes("2.x", "2.1.0-pre.0", true),
                Includes("1.1.x", "1.1.0-a", true),
                Includes("1.1.x", "1.1.1-a", true),
                Includes("*", "1.0.0-rc1", true),
                Includes("^1.0.0-0", "1.0.1-rc1", true),
                Includes("^1.0.0-rc2", "1.0.1-rc1", true),
                Includes("^1.0.0", "1.0.1-rc1", true),
                Includes("^1.0.0", "1.1.0-rc1", true),
                Includes("1 - 2", "2.0.0-pre", true),
                Includes("1 - 2", "1.0.0-pre", true),
                Includes("1.0 - 2", "1.0.0-pre", true),

                Includes("=0.7.x", "0.7.0-asdf", true),
                Includes(">=0.7.x", "0.7.0-asdf", true),
                Includes("<=0.7.x", "0.7.0-asdf", true),

                Includes(">=1.0.0 <=1.1.0", "1.1.0-pre", true),
            };

        /// <summary>
        /// npm version range excludes cases from https://github.com/npm/node-semver/blob/main/test/fixtures/range-exclude.js
        /// </summary>
        /// <remarks>The loose option is not supported, so loose test cases have been removed if
        /// they otherwise duplicate other test cases or modified to not require the loose option.</remarks>
        public static readonly TheoryData<NpmRangeContainsTestCase> ExcludeCases
            = new TheoryData<NpmRangeContainsTestCase>
            {
                Excludes("1.0.0 - 2.0.0", "2.2.3"),
                Excludes("1.2.3+asdf - 2.4.3+asdf", "1.2.3-pre.2"),
                Excludes("1.2.3+asdf - 2.4.3+asdf", "2.4.3-alpha"),
                Excludes("^1.2.3+build", "2.0.0"),
                Excludes("^1.2.3+build", "1.2.0"),
                Excludes("^1.2.3", "1.2.3-pre"),
                Excludes("^1.2", "1.2.0-pre"),
                Excludes(">1.2", "1.3.0-beta"),
                Excludes("<=1.2.3", "1.2.3-beta"),
                Excludes("^1.2.3", "1.2.3-beta"),
                Excludes("=0.7.x", "0.7.0-asdf"),
                Excludes(">=0.7.x", "0.7.0-asdf"),
                Excludes("<=0.7.x", "0.7.0-asdf"),
                Excludes("1", "1.0.0-beta"),
                Excludes("<1", "1.0.0-beta"),
                Excludes("< 1", "1.0.0-beta"),
                Excludes("1.0.0", "1.0.1"),
                Excludes(">=1.0.0", "0.0.0"),
                Excludes(">=1.0.0", "0.0.1"),
                Excludes(">=1.0.0", "0.1.0"),
                Excludes(">1.0.0", "0.0.1"),
                Excludes(">1.0.0", "0.1.0"),
                Excludes("<=2.0.0", "3.0.0"),
                Excludes("<=2.0.0", "2.9999.9999"),
                Excludes("<=2.0.0", "2.2.9"),
                Excludes("<2.0.0", "2.9999.9999"),
                Excludes("<2.0.0", "2.2.9"),
                Excludes(">=0.1.97", "0.1.93"),
                Excludes("0.1.20 || 1.2.4", "1.2.3"),
                Excludes(">=0.2.3 || <0.0.1", "0.0.3"),
                Excludes(">=0.2.3 || <0.0.1", "0.2.2"),
                Excludes("2.x.x", "1.1.3"),
                Excludes("2.x.x", "3.1.3"),
                Excludes("1.2.x", "1.3.3"),
                Excludes("1.2.x || 2.x", "3.1.3"),
                Excludes("1.2.x || 2.x", "1.1.3"),
                Excludes("2.*.*", "1.1.3"),
                Excludes("2.*.*", "3.1.3"),
                Excludes("1.2.*", "1.3.3"),
                Excludes("1.2.* || 2.*", "3.1.3"),
                Excludes("1.2.* || 2.*", "1.1.3"),
                Excludes("2", "1.1.2"),
                Excludes("2.3", "2.4.1"),
                Excludes("~0.0.1", "0.1.0-alpha"),
                Excludes("~0.0.1", "0.1.0"),
                Excludes("~2.4", "2.5.0"), // >=2.4.0 <2.5.0
                Excludes("~2.4", "2.3.9"),
                Excludes("~>3.2.1", "3.3.2"), // >=3.2.1 <3.3.0
                Excludes("~>3.2.1", "3.2.0"), // >=3.2.1 <3.3.0
                Excludes("~1", "0.2.3"), // >=1.0.0 <2.0.0
                Excludes("~>1", "2.2.3"),
                Excludes("~1.0", "1.1.0"), // >=1.0.0 <1.1.0
                Excludes("<1", "1.0.0"),
                Excludes(">=1.2", "1.1.1"),
                Excludes("1", "2.0.0-beta"),
                Excludes("~v0.5.4-beta", "0.5.4-alpha"),
                Excludes("=0.7.x", "0.8.2"),
                Excludes(">=0.7.x", "0.6.2"),
                Excludes("<0.7.x", "0.7.2"),
                Excludes("<1.2.3", "1.2.3-beta"),
                Excludes("=1.2.3", "1.2.3-beta"),
                Excludes(">1.2", "1.2.8"),
                Excludes("^0.0.1", "0.0.2-alpha"),
                Excludes("^0.0.1", "0.0.2"),
                Excludes("^1.2.3", "2.0.0-alpha"),
                Excludes("^1.2.3", "1.2.2"),
                Excludes("^1.2", "1.1.9"),
                Excludes("*", "1.2.3-foo"),

                Excludes("2.x", "3.0.0-pre.0", true),
                Excludes("^1.0.0", "1.0.0-rc1", true),
                Excludes("^1.0.0", "2.0.0-rc1", true),
                Excludes("^1.2.3-rc2", "2.0.0", true),
                Excludes("^1.0.0", "2.0.0-rc1", true),
                Excludes("^1.0.0", "2.0.0-rc1"),

                Excludes("1 - 2", "3.0.0-pre", true),
                Excludes("1 - 2", "2.0.0-pre"),
                Excludes("1 - 2", "1.0.0-pre"),
                Excludes("1.0 - 2", "1.0.0-pre"),

                Excludes("1.1.x", "1.0.0-a"),
                Excludes("1.1.x", "1.1.0-a"),
                Excludes("1.1.x", "1.2.0-a"),
                Excludes("1.1.x", "1.2.0-a", true),
                Excludes("1.1.x", "1.0.0-a", true),
                Excludes("1.x", "1.0.0-a"),
                Excludes("1.x", "1.1.0-a"),
                Excludes("1.x", "1.2.0-a"),
                Excludes("1.x", "0.0.0-a", true),
                Excludes("1.x", "2.0.0-a", true),

                Excludes(">=1.0.0 <1.1.0", "1.1.0"),
                Excludes(">=1.0.0 <1.1.0", "1.1.0", true),
                Excludes(">=1.0.0 <1.1.0", "1.1.0-pre"),
                Excludes(">=1.0.0 <1.1.0-pre", "1.1.0-pre")
            };

        /// <summary>
        /// npm version range parsing cases from https://github.com/npm/node-semver/blob/main/test/fixtures/range-parse.js
        /// </summary>
        /// <remarks>The loose option is not supported, so loose test cases have been removed if
        /// they otherwise duplicate other test cases or modified to not require the loose option.</remarks>
        public static readonly TheoryData<NpmRangeParsingTestCase> ParsingCases
            = new TheoryData<NpmRangeParsingTestCase>()
            {
                Valid("1.0.0 - 2.0.0", Inclusive("1.0.0", "2.0.0")),
                Valid("1.0.0 - 2.0.0", true, Inclusive("1.0.0", "2.0.0", true)), // Corrected, npm has >=1.0.0-0 <2.0.1-0
                Valid("1 - 2", InclusiveOfStart("1.0.0","3.0.0-0")),
                Valid("1 - 2", true, InclusiveOfStart("1.0.0-0", "3.0.0-0", true)),
                Valid("1.0 - 2.0", InclusiveOfStart("1.0.0", "2.1.0-0")),
                Valid("1.0 - 2.0", true, InclusiveOfStart("1.0.0-0", "2.1.0-0", true)),
                Valid("1.0.0", EqualsVersion("1.0.0")),
                Valid(">=*", AllRelease),
                Valid("", AllRelease),
                Valid("*", AllRelease),
                Valid(">=1.0.0", AtLeast("1.0.0")),
                Valid(">1.0.0", GreaterThan("1.0.0")),
                Valid("<=2.0.0", AtMost("2.0.0")),
                Valid("1", InclusiveOfStart("1.0.0","2.0.0-0")),
                Valid("<2.0.0", LessThan("2.0.0")),
                Valid(">= 1.0.0", AtLeast("1.0.0")),
                Valid(">=  1.0.0", AtLeast("1.0.0")),
                Valid(">=   1.0.0", AtLeast("1.0.0")),
                Valid("> 1.0.0", GreaterThan("1.0.0")),
                Valid(">  1.0.0", GreaterThan("1.0.0")),
                Valid("<=   2.0.0", AtMost("2.0.0")),
                Valid("<= 2.0.0", AtMost("2.0.0")),
                Valid("<=  2.0.0", AtMost("2.0.0")),
                Valid("<    2.0.0", LessThan("2.0.0")),
                Valid("<\t2.0.0", LessThan("2.0.0")),
                Valid(">=0.1.97", AtLeast("0.1.97")),
                Valid("0.1.20 || 1.2.4", EqualsVersion("0.1.20"), EqualsVersion("1.2.4")),
                Valid(">=0.2.3 || <0.0.1", AtLeast("0.2.3"), LessThan("0.0.1")),
                Valid("||", AllRelease),
                Valid("2.x.x", InclusiveOfStart("2.0.0", "3.0.0-0")),
                Valid("1.2.x", InclusiveOfStart("1.2.0", "1.3.0-0")),
                Valid("1.2.x || 2.x", InclusiveOfStart("1.2.0", "1.3.0-0"), InclusiveOfStart("2.0.0", "3.0.0-0")),
                Valid("x", AllRelease),
                Valid("2.*.*", InclusiveOfStart("2.0.0", "3.0.0-0")),
                Valid("1.2.*", InclusiveOfStart("1.2.0", "1.3.0-0")),
                Valid("1.2.* || 2.*", InclusiveOfStart("1.2.0", "1.3.0-0"), InclusiveOfStart("2.0.0", "3.0.0-0")),
                Valid("2", InclusiveOfStart("2.0.0", "3.0.0-0")),
                Valid("2.3", InclusiveOfStart("2.3.0", "2.4.0-0")),
                Valid("~2.4", InclusiveOfStart("2.4.0", "2.5.0-0")),
                Valid("~>3.2.1", InclusiveOfStart("3.2.1", "3.3.0-0")),
                Valid("~1", InclusiveOfStart("1.0.0", "2.0.0-0")),
                Valid("~>1", InclusiveOfStart("1.0.0", "2.0.0-0")),
                Valid("~> 1", InclusiveOfStart("1.0.0", "2.0.0-0")),
                Valid("~1.0", InclusiveOfStart("1.0.0", "1.1.0-0")),
                Valid("~ 1.0", InclusiveOfStart("1.0.0", "1.1.0-0")),
                Valid("^0", InclusiveOfStart("0.0.0", "1.0.0-0")), // Corrected, npm has <1.0.0-0
                Valid("^ 1", InclusiveOfStart("1.0.0", "2.0.0-0")),
                Valid("^0.1", InclusiveOfStart("0.1.0", "0.2.0-0")),
                Valid("^1.0", InclusiveOfStart("1.0.0", "2.0.0-0")),
                Valid("^1.2", InclusiveOfStart("1.2.0", "2.0.0-0")),
                Valid("^0.0.1", InclusiveOfStart("0.0.1", "0.0.2-0")),
                Valid("^0.0.1-beta", InclusiveOfStart("0.0.1-beta", "0.0.2-0")),
                Valid("^0.1.2", InclusiveOfStart("0.1.2", "0.2.0-0")),
                Valid("^1.2.3", InclusiveOfStart("1.2.3", "2.0.0-0")),
                Valid("^1.2.3-beta.4", InclusiveOfStart("1.2.3-beta.4", "2.0.0-0")),
                Valid("<1", LessThan("1.0.0-0")),
                Valid("< 1", LessThan("1.0.0-0")),
                Valid(">=1", AtLeast("1.0.0")),
                Valid(">= 1", AtLeast("1.0.0")),
                Valid("<1.2", LessThan("1.2.0-0")),
                Valid("< 1.2", LessThan("1.2.0-0")),
                Valid("~1.2.3-beta", InclusiveOfStart("1.2.3-beta", "1.3.0-0")),
                Valid("^ 1.2 ^ 1", InclusiveOfStart("1.2.0", "2.0.0-0")),
                Valid("1.2 - 3.4.5", Inclusive("1.2.0", "3.4.5")),
                Valid("1.2.3 - 3.4", InclusiveOfStart("1.2.3", "3.5.0-0")),
                Valid("1.2 - 3.4", InclusiveOfStart("1.2.0", "3.5.0-0")),
                Valid(">1", AtLeast("2.0.0")),
                Valid(">1.2", AtLeast("1.3.0")),
                Valid(">X", Empty),
                Valid("<X", Empty),
                Valid("<x <* || >* 2.x", Empty),
                Valid(">x 2.x || * || <x", AllRelease),
                Valid($"={int.MaxValue}.0.0", EqualsVersion($"{int.MaxValue}.0.0")),
                Valid($"^{int.MaxValue - 1}.0.0", InclusiveOfStart($"{int.MaxValue - 1}.0.0", $"{int.MaxValue}.0.0-0")),
                Valid($"^{int.MaxValue}.0.0", InclusiveOfStart($"{int.MaxValue}.0.0", $"{int.MaxValue + 1L}.0.0-0")),

                Invalid(">01.02.03", ExceptionMessages.LeadingZeroInMajor, version: "01.02.03"),
                Invalid(">=09090", ExceptionMessages.LeadingZeroInMajor, version: "09090"),
                Invalid(">=09090-0", ExceptionMessages.LeadingZeroInMajor, version: "09090-0"),
                Invalid<ArgumentNullException>(null, ExceptionMessages.NotNull),
            };

        [Theory]
        [MemberData(nameof(IncludesCases))]
        public void RangeIncludes(NpmRangeContainsTestCase testCase)
        {
            testCase.Assert();
        }

        [Theory]
        [MemberData(nameof(ExcludeCases))]
        public void RangeExcludes(NpmRangeContainsTestCase testCase)
        {
            testCase.Assert();
        }

        [Theory]
        [MemberData(nameof(ParsingCases))]
        public void ParseTests(NpmRangeParsingTestCase testCase)
        {
            testCase.AssertParseNpm();
        }

        private static NpmRangeContainsTestCase Includes(string range, string version, bool includeAllPrerelease = false)
            => NpmRangeContainsTestCase.NpmIncludes(range, version, includeAllPrerelease);

        private static NpmRangeContainsTestCase Excludes(string range, string version, bool includeAllPrerelease = false)
            => NpmRangeContainsTestCase.NpmExcludes(range, version, includeAllPrerelease);
    }
}
