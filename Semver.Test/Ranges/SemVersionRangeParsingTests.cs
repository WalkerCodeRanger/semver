using Semver.Ranges;
using Semver.Test.Builders;
using Xunit;

namespace Semver.Test.Ranges
{
    public class SemVersionRangeParsingTests
    {
        internal static readonly TheoryData<RangeParsingTestCase> ParsingTestCases = new TheoryData<RangeParsingTestCase>()
        {
            //Valid("=1.0.0",SemVersionRange.)
        };


        internal static RangeParsingTestCase Valid(string range, SemVersionRange expected)
            => RangeParsingTestCase.Valid(range, SemVersionRangeOptions.Strict, expected);

        internal static RangeParsingTestCase Valid(string range, SemVersionRangeOptions options, SemVersionRange expected)
            => RangeParsingTestCase.Valid(range, options, expected);
    }
}
