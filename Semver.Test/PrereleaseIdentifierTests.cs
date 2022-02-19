using System;
using System.Globalization;
using Xunit;

namespace Semver.Test
{
    public class PrereleaseIdentifierTests
    {
        [Theory]
        [InlineData("ident", null)]
        [InlineData("42", 42)]
        [InlineData("042", 42)]
        [InlineData("hello", null)]
        [InlineData("2147483648", null)] // int.MaxValue + 1
        [InlineData("hello@", null)]
        public void CreateLoose(string value, int? intValue)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            var identifier = PrereleaseIdentifier.CreateLoose(value);
#pragma warning restore CS0612 // Type or member is obsolete

            Assert.Equal(value, identifier.Value);
            Assert.Equal(intValue, identifier.IntValue);
        }

        [Theory]
        [InlineData("ident", null)]
        [InlineData("42", 12)]
        [InlineData("042", 13)]
        [InlineData("hello", -1)]
        [InlineData("2147483648", 42)] // int.MaxValue + 1
        [InlineData("hello@", null)]
        public void CreateUnsafe(string value, int? intValue)
        {
            var identifier = PrereleaseIdentifier.CreateUnsafe(value, intValue);

            Assert.Equal(value, identifier.Value);
            Assert.Equal(intValue, identifier.IntValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConstructWithNullString(bool allowLeadingZeros)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new PrereleaseIdentifier(null, allowLeadingZeros));

            Assert.StartsWith("Value cannot be null.", ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConstructWithEmptyString(bool allowLeadingZeros)
        {
            var ex = Assert.Throws<ArgumentException>(() => new PrereleaseIdentifier("", allowLeadingZeros));

            Assert.StartsWith("Prerelease identifier cannot be empty.", ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [InlineData("ident", false, "ident", null)]
        [InlineData("0A", false, "0A", null)]
        [InlineData("a0", false, "a0", null)]
        [InlineData("0-1", false, "0-1", null)]
        [InlineData("-1", false, "-1", null)]
        [InlineData("0", false, "0", 0)]
        [InlineData("1", false, "1", 1)]
        [InlineData("00", true, "0", 0)]
        [InlineData("01", true, "1", 1)]
        public void ConstructWithString(string value, bool allowLeadingZeros, string expectedValue, int? expectedIntValue)
        {
            var identifier = new PrereleaseIdentifier(value, allowLeadingZeros);

            Assert.Equal(expectedValue, identifier.Value);
            Assert.Equal(expectedIntValue, identifier.IntValue);
        }

        [Fact]
        public void ConstructWithStringThrowsOverflowException()
        {
            // int.MaxValue + 1
            var ex = Assert.Throws<OverflowException>(() => new PrereleaseIdentifier("2147483648"));

            Assert.Equal("Prerelease identifier '2147483648' was too large for Int32.", ex.Message);
        }

        [Theory]
        [InlineData("01")]
        [InlineData("00")]
        public void ConstructWithStringThrowsExceptionForLeadingZero(string value)
        {
            var ex = Assert.Throws<ArgumentException>(() => new PrereleaseIdentifier(value));

            Assert.StartsWith($"Leading zeros are not allowed on numeric prerelease identifiers '{value}'.",
                ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [InlineData("sdfá")]
        [InlineData("34@")]
        [InlineData("+")]
        [InlineData("+2")]
        [InlineData(".")]
        [InlineData("😞")]
        public void ConstructWithStringWithInvalidCharacters(string value)
        {
            var ex = Assert.Throws<ArgumentException>(() => new PrereleaseIdentifier(value));

            Assert.StartsWith($"A prerelease identifier can contain only ASCII alphanumeric characters and hyphens '{value}'.",
                ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void ConstructWithInt(int value)
        {
            var identifier = new PrereleaseIdentifier(value);

            Assert.Equal(value.ToString(CultureInfo.InvariantCulture), identifier.Value);
            Assert.Equal(value, identifier.IntValue);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void ConstructWithNegativeInt(int value)
        {
            var ex = Assert.Throws<ArgumentException>(() => new PrereleaseIdentifier(value));

            Assert.StartsWith($"Numeric prerelease identifiers can't be negative: {value}.", ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        public static readonly TheoryData<PrereleaseIdentifier, PrereleaseIdentifier, bool> EqualityCases
            = new TheoryData<PrereleaseIdentifier, PrereleaseIdentifier, bool>()
            {
                {new PrereleaseIdentifier("hello"), new PrereleaseIdentifier("hello"), true},
                {new PrereleaseIdentifier("hello"), new PrereleaseIdentifier("nope"), false},
                {new PrereleaseIdentifier(42), new PrereleaseIdentifier(42), true},
                {new PrereleaseIdentifier(42), new PrereleaseIdentifier(0), false},
                {new PrereleaseIdentifier("hello"), new PrereleaseIdentifier(42), false},
                {PrereleaseIdentifier.CreateUnsafe("", null), PrereleaseIdentifier.CreateUnsafe("", null), true},
                {default, default, true},
                // Equality is based only on the string value
                {PrereleaseIdentifier.CreateUnsafe("896", null), PrereleaseIdentifier.CreateUnsafe("896", 896), true},
#pragma warning disable CS0612 // Type or member is obsolete
                {PrereleaseIdentifier.CreateLoose("loose"), new PrereleaseIdentifier("loose"), true},
                {PrereleaseIdentifier.CreateLoose("045"), PrereleaseIdentifier.CreateLoose("45"), false},
                {PrereleaseIdentifier.CreateLoose("10053"), new PrereleaseIdentifier(10053), true},
                {PrereleaseIdentifier.CreateLoose("2147483648"), PrereleaseIdentifier.CreateLoose("2147483648"), true}, // int.MaxValue + 1
#pragma warning restore CS0612 // Type or member is obsolete
            };

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualsPrereleaseIdentifier(PrereleaseIdentifier left, PrereleaseIdentifier right, bool expected)
        {
            var equal = left.Equals(right);

            Assert.Equal(expected, equal);
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualsObject(PrereleaseIdentifier left, PrereleaseIdentifier right, bool expected)
        {
            var equal = left.Equals((object)right);

            Assert.Equal(expected, equal);
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualOperator(PrereleaseIdentifier left, PrereleaseIdentifier right, bool expected)
        {
            var equal = left == right;

            Assert.Equal(expected, equal);
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void NotEqualOperator(PrereleaseIdentifier left, PrereleaseIdentifier right, bool expectedEqual)
        {
            var equal = left != right;

            Assert.Equal(!expectedEqual, equal);
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void GetHashCodeTest(PrereleaseIdentifier left, PrereleaseIdentifier right, bool expectEqual)
        {
            var leftHashcode = left.GetHashCode();
            var rightHashcode = right.GetHashCode();

            if (expectEqual)
                Assert.Equal(leftHashcode, rightHashcode);
            else
                Assert.NotEqual(leftHashcode, rightHashcode);
        }

        [Theory]
        [InlineData("042")]
        [InlineData("0A")]
        [InlineData("123")]
        [InlineData("1-2")]
        [InlineData("@")]
        [InlineData(null)]
        public void ImplicitConversionToString(string value)
        {
            var identifier = PrereleaseIdentifier.CreateUnsafe(value, null);

            string convertedValue = identifier;

            Assert.Equal(value, convertedValue);
        }

        [Theory]
        [InlineData("042")]
        [InlineData("0A")]
        [InlineData("123")]
        [InlineData("1-2")]
        [InlineData("@")]
        [InlineData(null)]
        public void ToStringTest(string value)
        {
            var identifier = PrereleaseIdentifier.CreateUnsafe(value, null);

            string convertedValue = identifier.ToString();

            Assert.Equal(value, convertedValue);
        }
    }
}
