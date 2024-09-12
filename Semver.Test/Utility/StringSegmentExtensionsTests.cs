using System.Linq;
using Semver.Utility;
using Xunit;
using StringSegment = Microsoft.Extensions.Primitives.StringSegment;

namespace Semver.Test.Utility;

public class StringSegmentExtensionsTests
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
    [InlineData("0", "0")]
    [InlineData("Hello", "Hello")]
    [InlineData("00123", "123")]
    [InlineData("000", "0")]
    public void TrimLeadingZeros(string value, string expected)
    {
        StringSegment segment = value;

        var trimmed = segment.TrimLeadingZeros();

        Assert.Equal(expected, trimmed.ToString());
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
}
