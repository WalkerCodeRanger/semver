using Semver.Utility;
using Xunit;

namespace Semver.Test.Utility
{
    public class StringSegmentTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("     ", "")]
        [InlineData("Hello", "Hello")]
        [InlineData("  Hello", "Hello")]
        [InlineData("Bye  ", "Bye")]
        [InlineData("  foo!   ", "foo!")]
        [InlineData("  \tsomething\r   ", "\tsomething\r")]
        public void TrimSpaces(string value, string expected)
        {
            StringSegment segment = value;

            var trimmed = segment.TrimSpaces();

            Assert.Equal(expected, trimmed.ToString());
        }
    }
}
