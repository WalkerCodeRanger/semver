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

        [Fact]
        public void IndexOf()
        {
            var segment = " ^  Hello^World!".Slice(3, 10);
            Assert.Equal(6, segment.IndexOf('^', 0, 10));
            Assert.Equal(6, segment.IndexOf('^', 3, 5));
            Assert.Equal(-1, segment.IndexOf('^', 3, 2));
        }
    }
}
