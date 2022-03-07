using Semver.Utility;
using Xunit;

namespace Semver.Test.Utility
{
    public class StringExtensionsTests
    {
        [Fact]
        public void SplitAndMapToReadOnlyListOfEmptyList()
        {
            var items = "".SplitAndMapToReadOnlyList('.', int.Parse);

            Assert.Same(ReadOnlyList<int>.Empty, items);
        }

        [Fact]
        public void SplitAndMapToReadOnlyList()
        {
            var items = "42.34.56.76.34.000".SplitAndMapToReadOnlyList('.', int.Parse);

            Assert.Equal(new[] { 42, 34, 56, 76, 34, 0 }, items);
        }

        [Fact]
        public void SplitAndMapToReadOnlyListWithEmptyIdentifiers()
        {
            var items = "..".SplitAndMapToReadOnlyList('.', v => v);

            Assert.Equal(new[] { "", "", "" }, items);
        }

        [Theory]
        [InlineData("01", "1")]
        [InlineData("000001", "1")]
        [InlineData("000000", "0")]
        [InlineData("04020", "4020")]
        [InlineData("0800", "800")]
        public void TrimLeadingZeros(string value, string expected)
        {
            var actual = value.TrimLeadingZeros();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("5")]
        [InlineData("42")]
        [InlineData("4020")]
        public void TrimLeadingZerosWithoutLeadingZeros(string value)
        {
            var actual = value.TrimLeadingZeros();

            Assert.Same(value, actual);
        }
    }
}
