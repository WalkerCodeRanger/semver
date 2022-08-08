using Semver.Ranges.Parsers;
using Xunit;

namespace Semver.Test.Ranges.Parsers
{
    public class GeneralRangeParserTests
    {
        [Theory]
        [InlineData('$', true)]
        [InlineData('|', true)]
        [InlineData('>', true)]
        [InlineData('=', true)]
        [InlineData('<', true)]
        [InlineData('^', true)]
        [InlineData('(', true)]
        [InlineData('~', true)]
        [InlineData('`', true)]
        [InlineData('"', true)]
        // Could be an operator even though it can be part of a version
        [InlineData('-', true)]
        [InlineData('.', true)]
        // Can't be an operator
        [InlineData('0', false)]
        [InlineData('1', false)]
        [InlineData('9', false)]
        [InlineData('a', false)]
        [InlineData('z', false)]
        [InlineData('A', false)]
        [InlineData('Z', false)]
        [InlineData('*', false)]
        [InlineData(' ', false)]
        [InlineData('\t', false)]
        public void IsPossibleOperatorChar(char c, bool expected)
        {
            Assert.Equal(expected, GeneralRangeParser.IsPossibleOperatorChar(c));
        }

        [Theory]
        [InlineData('$', false)]
        [InlineData('|', false)]
        [InlineData('>', false)]
        [InlineData('=', false)]
        [InlineData('<', false)]
        [InlineData('^', false)]
        [InlineData('(', false)]
        [InlineData('~', false)]
        [InlineData('`', false)]
        [InlineData('"', false)]
        [InlineData('-', true)]
        [InlineData('.', true)]
        [InlineData('0', true)]
        [InlineData('1', true)]
        [InlineData('9', true)]
        [InlineData('a', true)]
        [InlineData('z', true)]
        [InlineData('A', true)]
        [InlineData('Z', true)]
        [InlineData('*', true)]
        [InlineData(' ', false)]
        [InlineData('\t', false)]
        public void IsPossibleVersionChar(char c, bool expected)
        {
            Assert.Equal(expected, GeneralRangeParser.IsPossibleVersionChar(c));
        }
    }
}
