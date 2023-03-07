using Microsoft.Extensions.Primitives;
using Semver.Utility;
using Xunit;

namespace Semver.Test.Utility
{
    /// <summary>
    /// These tests serve to verify the behavior of <see cref="StringSegment"/>.
    /// </summary>
    public class StringSegmentTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("     ", "")]
        [InlineData("   \t  ", "")]
        [InlineData("Hello", "Hello")]
        [InlineData("  Hello", "Hello")]
        [InlineData("Bye  ", "Bye  ")]
        [InlineData("  foo!   ", "foo!   ")]
        [InlineData("  \tsomething\r   ", "something\r   ")]
        public void TrimStart(string value, string expected)
        {
            StringSegment segment = value;

            var trimmed = segment.TrimStart();

            Assert.Equal(expected, trimmed.ToString());
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("   \t  ", "")]
        [InlineData("Hello", "Hello")]
        [InlineData("  Hello", "  Hello")]
        [InlineData("Bye  ", "Bye")]
        [InlineData("  foo!   ", "  foo!")]
        [InlineData("  \tsomething\r   ", "  \tsomething")]
        public void TrimEnd(string value, string expected)
        {
            StringSegment segment = value;

            var trimmed = segment.TrimEnd();

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
