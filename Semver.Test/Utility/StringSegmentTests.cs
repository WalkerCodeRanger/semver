using System.Linq;
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
        [InlineData("   \t  ", "")]
        [InlineData("Hello", "Hello")]
        [InlineData("  Hello", "Hello")]
        [InlineData("Bye  ", "Bye  ")]
        [InlineData("  foo!   ", "foo!   ")]
        [InlineData("  \tsomething\r   ", "something\r   ")]
        public void TrimStartWhitespace(string value, string expected)
        {
            StringSegment segment = value;

            var trimmed = segment.TrimStartWhitespace();

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
        public void TrimEndWhitespace(string value, string expected)
        {
            StringSegment segment = value;

            var trimmed = segment.TrimEndWhitespace();

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

        [Theory]
        [InlineData("", 1)]
        [InlineData(".", 2)]
        [InlineData("hello.world", 2)]
        [InlineData("hello..world", 3)]
        public void SplitCount(string value, int expected)
        {
            StringSegment segment = value;

            Assert.Equal(expected, segment.SplitCount('.'));
        }

        [Fact]
        public void Split()
        {
            StringSegment segment = ".a..hello.";

            var expected = new[] { "", "a", "", "hello", "" };
            Assert.Equal(expected, segment.Split('.').Select(s => s.ToString()));
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("1.2.0-rc", "1.2.0", "-rc")]
        [InlineData("1.2.0", "1.2.0", "")]
        [InlineData("-rc", "", "-rc")]
        public void SplitBeforeFirst(string value, string expectedLeft, string expectedRight)
        {
            StringSegment segment = value;

            segment.SplitBeforeFirst('-', out var left, out var right);

            Assert.Equal(expectedLeft, left.ToString());
            Assert.Equal(expectedRight, right.ToString());
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("1.2.0-rc", "1.2.0", "-rc")]
        [InlineData("1.2.0", "1.2.0", "")]
        [InlineData("-rc", "", "-rc")]
        public void SplitBeforeFirstOutToSameVariable(string value, string expectedLeft, string expectedRight)
        {
            StringSegment segment = value;

            segment.SplitBeforeFirst('-', out segment, out var right);

            Assert.Equal(expectedLeft, segment.ToString());
            Assert.Equal(expectedRight, right.ToString());
        }
    }
}
