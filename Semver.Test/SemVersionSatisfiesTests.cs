using System;
using Semver.Ranges;
using Semver.Test.Helpers;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// Tests of the <see cref="SemVersion.Satisfies(Predicate{SemVersion})"/> overloads. The ranges
    /// are tested extensively, so these tests do not need to test that ranges properly check
    /// containment. They only need to test basic use cases.
    /// </summary>
    public class SemVersionSatisfiesTests
    {
        private static readonly SemVersion FakeVersion = new SemVersion(1, 2, 3);
        private static readonly SemVersion FakePrereleaseVersion = SemVersion.ParsedFrom(1, 2, 3, "rc");
        private const SemVersionRangeOptions InvalidSemVersionRangeOptions = SemVersionRangeOptionsExtensions.AllFlags + 1;

        [Fact]
        public void SatisfiesNullPredicate()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => FakeVersion.Satisfies((Predicate<SemVersion>)null));

            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("predicate", ex.ParamName);
        }

        [Fact]
        public void SatisfiesPredicate()
        {
            Assert.True(FakeVersion.Satisfies(v => true));
            Assert.False(FakeVersion.Satisfies(v => false));
        }

        [Fact]
        public void SatisfiesSemVersionRange()
        {
            var range1 = SemVersionRange.Parse("1.2.3");
            var range2 = SemVersionRange.Parse("2.*");

            Assert.True(FakeVersion.Satisfies(range1));
            Assert.False(FakeVersion.Satisfies(range2));
        }

        [Fact]
        public void SatisfiesUnbrokenSemVersionRange()
        {
            var range1 = UnbrokenSemVersionRange.Equals(FakeVersion);
            var range2 = UnbrokenSemVersionRange.Empty;

            Assert.True(FakeVersion.Satisfies(range1));
            Assert.False(FakeVersion.Satisfies(range2));
        }

        [Fact]
        public void SatisfiesParsedNullRangeWithOptions()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => FakeVersion.Satisfies(null, SemVersionRangeOptions.Strict));

            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("range", ex.ParamName);
        }

        [Fact]
        public void SatisfiesParsedRangeWithInvalidOptions()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => FakeVersion.Satisfies("1.2.3", InvalidSemVersionRangeOptions));

            Assert.StartsWith(ExceptionMessages.InvalidSemVersionRangeOptionsStart, ex.Message);
            Assert.Equal("options", ex.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void SatisfiesParsedRangeWithOptionsAndInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => FakeVersion.Satisfies("1.2.3", SemVersionRangeOptions.Strict, maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
        }

        [Fact]
        public void SatisfiesParsedRangeWithOptions()
        {
            const string range = "1.*";

            Assert.True(FakeVersion.Satisfies(range, SemVersionRangeOptions.Strict));
            Assert.False(FakePrereleaseVersion.Satisfies(range, SemVersionRangeOptions.Strict));
            Assert.True(FakePrereleaseVersion.Satisfies(range, SemVersionRangeOptions.IncludeAllPrerelease));
        }

        [Fact]
        public void SatisfiesParsedNullRangeWithoutOptions()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => FakeVersion.Satisfies((string)null));

            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("range", ex.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void SatisfiesParsedRangeWithoutOptionsAndInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => FakeVersion.Satisfies("1.2.3", maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
        }

        [Fact]
        public void SatisfiesParsedRangeWithoutOptions()
        {
            const string range = "1.*";

            Assert.True(FakeVersion.Satisfies(range));
            Assert.False(FakePrereleaseVersion.Satisfies(range));
        }

        [Fact]
        public void SatisfiesNullNpmRangeWithOptions()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => FakeVersion.SatisfiesNpm(null, true));

            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("range", ex.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void SatisfiesNpmRangeWithOptionsAndInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => FakeVersion.SatisfiesNpm("1.2.3", true, maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
        }

        [Fact]
        public void SatisfiesNpmRangeWithOptions()
        {
            const string range = "1.x";

            Assert.True(FakeVersion.SatisfiesNpm(range, false));
            Assert.False(FakePrereleaseVersion.SatisfiesNpm(range, false));
            Assert.True(FakePrereleaseVersion.SatisfiesNpm(range, true));
        }

        [Fact]
        public void SatisfiesNullNpmRangeWithoutOptions()
        {
            var ex = Assert.Throws<ArgumentNullException>(
                () => FakeVersion.SatisfiesNpm(null));

            Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
            Assert.Equal("range", ex.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void SatisfiesNpmRangeWithoutOptionsAndInvalidMaxLength(int maxLength)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(
                () => FakeVersion.SatisfiesNpm("1.2.3", maxLength));

            Assert.StartsWith(ExceptionMessages.InvalidMaxLengthStart, ex.Message);
            Assert.Equal("maxLength", ex.ParamName);
        }

        [Fact]
        public void SatisfiesNpmRangeWithoutOptions()
        {
            const string range = "1.x";

            Assert.True(FakeVersion.SatisfiesNpm(range));
            Assert.False(FakePrereleaseVersion.SatisfiesNpm(range));
        }
    }
}
