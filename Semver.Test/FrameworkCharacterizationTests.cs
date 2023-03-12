using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
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

            Assert.StartsWith(InvalidNumberStyleMessageStart, ex.Message);
            Assert.Equal("style", ex.ParamName);
        }

        [Fact]
        public void IntParseOfNullAndInvalidNumberStyleThrowsInvalidNumberStyleArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => int.Parse(null, InvalidNumberStyle, CultureInfo.InvariantCulture));

            Assert.StartsWith(InvalidNumberStyleMessageStart, ex.Message);
            Assert.Equal("style", ex.ParamName);
        }

        [Fact]
        public void IntParseInvalidFormat()
        {
            var ex = Assert.Throws<FormatException>(
                () => int.Parse("  42", NumberStyles.None, CultureInfo.InvariantCulture));

            Assert.Equal(InvalidFormatMessage, ex.Message);
        }

        [Fact]
        public void IntParseOverflow()
        {
            var ex = Assert.Throws<OverflowException>(() =>
                int.Parse("99999999999999999999999", NumberStyles.None, CultureInfo.InvariantCulture));

            Assert.Equal(IntOverflowMessage, ex.Message);
        }

        [Fact]
        public void ConstructNegativeVersionThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new Version(-1, 0));
            Assert.StartsWith("Version's parameters must be greater than or equal to zero.", ex.Message);
        }

        [Fact]
        public void IntGetHashCode()
        {
            Assert.Equal(42, 42.GetHashCode());
        }

        [Fact]
        public void HashCodeCombineInt()
        {
            Assert.NotEqual(42, HashCode.Combine(42));
        }

        [Fact]
        public void HashCodeCombineNull()
        {
            Assert.NotEqual(0, HashCode.Combine<object>(null));
        }

        [Fact]
        public void SizeOfBigInteger()
        {
            var bigIntSize = Unsafe.SizeOf<BigInteger>();

            Assert.InRange(bigIntSize, 0, 16);
        }

        private const string InvalidNumberStyleMessageStart = "An undefined NumberStyles value is being used.";
        private const string InvalidFormatMessage = "Input string was not in a correct format.";
        private const string IntOverflowMessage = "Value was either too large or too small for an Int32.";
        private const NumberStyles InvalidNumberStyle = (NumberStyles)int.MaxValue;
    }
}
