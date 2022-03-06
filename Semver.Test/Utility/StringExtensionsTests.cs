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
    }
}
