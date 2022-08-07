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
        [InlineData('*', false)]
        [InlineData('.', false)]
        [InlineData(' ', false)]
        [InlineData('\t', false)]
        public void IsPossibleOperatorChar(char c, bool expected)
        {
            Assert.Equal(expected, GeneralRangeParser.IsPossibleOperatorChar(c));
        }
    }
}
