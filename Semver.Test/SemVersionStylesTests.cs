using Xunit;

namespace Semver.Test
{
    public class SemVersionStylesTests
    {
        [Fact]
        public void StrictIsTheSameAsSemVer2()
        {
            Assert.Equal(SemVersionStyles.SemVer2, SemVersionStyles.Strict);
        }
    }
}
