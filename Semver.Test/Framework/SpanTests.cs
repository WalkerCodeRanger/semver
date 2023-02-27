using System;
using Xunit;

namespace Semver.Test.Framework
{
    /// <summary>
    /// Tests of standard <see cref="ReadOnlySpan{T}"/> behavior to verify it is as expected for
    /// the functionality of <see cref="Semver"/>.
    /// </summary>
    public class SpanTests
    {
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
            var span = value.AsSpan();

            var trimmed = span.TrimEnd();

            Assert.Equal(expected, trimmed.ToString());
        }
    }
}
