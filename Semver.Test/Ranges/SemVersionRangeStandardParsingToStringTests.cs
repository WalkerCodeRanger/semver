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
            // All and All Release
            Valid("*", "*"),
            Valid("*.*.*", "*"),
            Valid("*", IncludeAllPrerelease, "*-*"),
            Valid("*-*", "*-*"),
            Valid("*.*.*-*", "*-*"),

            // Equality
            Valid("1.2.3", "1.2.3"),
            Valid("1.2.3-rc", "1.2.3-rc"),
            Valid("1.2.3-rc", IncludeAllPrerelease, "1.2.3-rc"),
            Valid("=1.2.3", "1.2.3"),
            Valid("=1.2.3-rc", "1.2.3-rc"),
            Valid("=1.2.3-rc", IncludeAllPrerelease, "1.2.3-rc"),

            // Wildcard Prerelease
            Valid("1.2.3-*", "1.2.3-*"),
            Valid("1.2.3-*", IncludeAllPrerelease, "1.2.3-*"),

            // Wildcard Patch
            Valid("1.2.*", "1.2.*"),
            Valid("1.2.*", IncludeAllPrerelease, "*-* 1.2.*"),
            Valid("1.2.* *-*", "*-* 1.2.*"),
            Valid("1.2.*-*", "1.2.*-*"),

            // Wildcard Minor
            Valid("1.*.*", "1.*"),
            Valid("1.*.*", IncludeAllPrerelease, "*-* 1.*"),
            Valid("1.*.* *-*", "*-* 1.*"),
            Valid("1.*-*", "1.*-*"),

            // Wildcard after prerelease identifiers
            Valid("1.2.3-rc.*", "1.2.3-rc.*"),
            Valid("1.2.3-5.*", "1.2.3-5.*"),
            Valid("1.2.3-2147483647.*", "1.2.3-2147483647.*"),
            Valid("3.1.4-l.o.n.g.e.r.rc.*", "3.1.4-l.o.n.g.e.r.rc.*"),

            // Tilde Ranges
            Valid("~2.5.3", "~2.5.3"),
            Valid("~2.5.3", IncludeAllPrerelease, "*-* ~2.5.3"),
            Valid("~2.5.3-rc", "~2.5.3-rc"),
            Valid("~0.5.3", "~0.5.3"),
            Valid("~0.5.3", IncludeAllPrerelease, "*-* ~0.5.3"),
            Valid("~0.5.3-rc", "~0.5.3-rc"),

            // Caret Ranges
            Valid("^6.2.3", "^6.2.3"),
            Valid("^6.2.3", IncludeAllPrerelease, "*-* ^6.2.3"),
            Valid("^6.2.3-alpha.1", "^6.2.3-alpha.1"),
            Valid("^0.0.7", "^0.0.7"),
            Valid("^0.0.7", IncludeAllPrerelease, "*-* ^0.0.7"),
            Valid("^0.0.7-now", "^0.0.7-now"),

            // Basic Ranges
            Valid(">=1.2.3", ">=1.2.3"),
            Valid("<1.2.3", "<1.2.3"),
            Valid("<6.0.0 >=1.0.0", ">=1.0.0 <6.0.0"),
            Valid(">1.0.0 <=3.2.6", ">1.0.0 <=3.2.6"),
            Valid(">1.0.0 <=3.2.6 || >=1.0.0 <6.0.0", ">=1.0.0 <6.0.0")
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
