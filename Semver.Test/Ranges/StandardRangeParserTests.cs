using Semver.Ranges.Parsers;
using Xunit;

namespace Semver.Test.Ranges
{
    public class StandardRangeParserTests
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
        [InlineData('*', false)]
        [InlineData('.', false)]
        [InlineData(' ', false)]
        [InlineData('\t', false)]
        public void IsPossibleOperatorChar(char c, bool expected)
        {
            Assert.Equal(expected, StandardRangeParser.IsPossibleOperatorChar(c));
        }
    }
}
