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
        [InlineData(4_569)]
        [InlineData(96_854)]
        [InlineData(565_627)]
        [InlineData(6_615_776)]
        [InlineData(78_415_675)]
        [InlineData(787_415_757)]
        [InlineData(int.MaxValue)]
        public void DigitsTest(int n)
        {
            Assert.Equal(n.ToString(CultureInfo.InvariantCulture).Length, n.DecimalDigits());
        }
    }
}
