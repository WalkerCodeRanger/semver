using Xunit;
using static Semver.SemVersionStyles;

namespace Semver.Test
{
    public class SemVersionStylesTests
    {
        [Fact]
        public void AnyIsValid()
        {
            Assert.True(Any.IsValid());
        }

        [Fact]
        public void OptionalMinorWithoutOptionalPatchIsInvalid()
        {
            var optionalMinorWithoutOptionalPatch = OptionalMinorPatch & ~OptionalPatch;
            Assert.False(optionalMinorWithoutOptionalPatch.IsValid());
        }
    }
}
