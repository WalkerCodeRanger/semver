using System;
using System.Globalization;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// These tests serve to verify or clarify the behavior of standard .NET framework
    /// types which <see cref="SemVersion"/> is modeled on. This helps to ensure the
    /// behavior is consistent with that of the .NET framework types.
    /// </summary>
    public class FrameworkCharacterizationTests
    {
        [Fact]
        public void IntTryParseOfNullReturnsFalse()
        {
            Assert.False(int.TryParse(null, out _));
        }

        [Fact]
        public void IntTryParseOfInvalidNumberStyleThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => int.TryParse("45", InvalidNumberStyle, CultureInfo.InvariantCulture, out _));

            Assert.Equal(InvalidNumberStyleMessage, ex.Message);
        }

        [Fact]
        public void IntParseOfNullAndInvalidNumberStyleThrowsInvalidNumberStyleArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => int.Parse(null, InvalidNumberStyle, CultureInfo.InvariantCulture));

            Assert.Equal(InvalidNumberStyleMessage, ex.Message);
        }

        private const string InvalidNumberStyleMessage = "An undefined NumberStyles value is being used.\r\nParameter name: style";
        private const NumberStyles InvalidNumberStyle = (NumberStyles)int.MaxValue;
    }
}
