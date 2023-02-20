using Semver.Ranges.Parsers;
using Xunit;
using static Semver.SemVersionRangeOptions;

namespace Semver.Test.Ranges.Parsers
{
    public class GeneralRangeParserTests
    {
        [Theory]
        [InlineData('$', Strict, true)]
        [InlineData('|', Strict, true)]
        [InlineData('>', Strict, true)]
        [InlineData('=', Strict, true)]
        [InlineData('<', Strict, true)]
        [InlineData('^', Strict, true)]
        [InlineData('(', Strict, true)]
        [InlineData('~', Strict, true)]
        [InlineData('`', Strict, true)]
        [InlineData('"', Strict, true)]
        // Could be an operator even though it can be part of a version
        [InlineData('-', Strict, true)]
        [InlineData('.', Strict, true)]
        // Can't be an operator
        [InlineData('0', Strict, false)]
        [InlineData('1', Strict, false)]
        [InlineData('9', Strict, false)]
        [InlineData('a', Strict, false)]
        [InlineData('z', Strict, false)]
        [InlineData('A', Strict, false)]
        [InlineData('Z', Strict, false)]
        [InlineData('*', Strict, false)]
        [InlineData('+', Strict, true)]
        [InlineData('+', AllowMetadata, false)]
        [InlineData(' ', Strict, false)]
        [InlineData('\t', Strict, false)]
        public void IsPossibleOperatorChar(char c, SemVersionRangeOptions rangeOptions, bool expected)
        {
            Assert.Equal(expected, GeneralRangeParser.IsPossibleOperatorChar(c, rangeOptions));
        }

        [Theory]
        [InlineData('$', Strict, false)]
        [InlineData('|', Strict, false)]
        [InlineData('>', Strict, false)]
        [InlineData('=', Strict, false)]
        [InlineData('<', Strict, false)]
        [InlineData('^', Strict, false)]
        [InlineData('(', Strict, false)]
        [InlineData('~', Strict, false)]
        [InlineData('`', Strict, false)]
        [InlineData('"', Strict, false)]
        [InlineData('-', Strict, true)]
        [InlineData('.', Strict, true)]
        [InlineData('0', Strict, true)]
        [InlineData('1', Strict, true)]
        [InlineData('9', Strict, true)]
        [InlineData('a', Strict, true)]
        [InlineData('z', Strict, true)]
        [InlineData('A', Strict, true)]
        [InlineData('Z', Strict, true)]
        [InlineData('*', Strict, true)]
        [InlineData('+', Strict, false)]
        [InlineData('+', AllowMetadata, true)]
        [InlineData(' ', Strict, false)]
        [InlineData('\t', Strict, false)]
        public void IsPossibleVersionChar(char c, SemVersionRangeOptions rangeOptions, bool expected)
        {
            Assert.Equal(expected, GeneralRangeParser.IsPossibleVersionChar(c, rangeOptions));
        }
    }
}
