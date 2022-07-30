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
        [InlineData("Bye  ", "Bye  ")]
        [InlineData("  foo!   ", "foo!   ")]
        [InlineData("  \tsomething\r   ", "\tsomething\r   ")]
        public void TrimStartSpaces(string value, string expected)
        {
            StringSegment segment = value;

            var trimmed = segment.TrimStartSpaces();

            Assert.Equal(expected, trimmed.ToString());
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("     ", "")]
        [InlineData("Hello", "Hello")]
        [InlineData("  Hello", "  Hello")]
        [InlineData("Bye  ", "Bye")]
        [InlineData("  foo!   ", "  foo!")]
        [InlineData("  \tsomething\r   ", "  \tsomething\r")]
        public void TrimEndSpaces(string value, string expected)
        {
            StringSegment segment = value;

            var trimmed = segment.TrimEndSpaces();

            Assert.Equal(expected, trimmed.ToString());
        }
    }
}
