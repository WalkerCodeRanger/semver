using System.Globalization;
using Xunit;

namespace Semver.Test
{
    public class IntExtensionsTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(986)]
        [InlineData(4569)]
        [InlineData(96854)]
        [InlineData(565627)]
        [InlineData(6615776)]
        [InlineData(78415675)]
        [InlineData(787_415_757)]
        [InlineData(int.MaxValue)]
        public void DigitsTest(int n)
        {
            Assert.Equal(n.ToString(CultureInfo.InvariantCulture).Length, n.Digits());
        }
    }
}
