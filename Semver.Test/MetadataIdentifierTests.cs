using System;
using Xunit;

namespace Semver.Test
{
    public class MetadataIdentifierTests
    {
        [Theory]
        [InlineData("ident")]
        [InlineData("42")]
        [InlineData("042")]
        [InlineData("hello")]
        [InlineData("2147483648")] // int.MaxValue + 1
        [InlineData("hello@")]
        [InlineData("")]
        public void CreateLoose(string value)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            var identifier = MetadataIdentifier.CreateLoose(value);
#pragma warning restore CS0612 // Type or member is obsolete

            Assert.Equal(value, identifier.Value);
        }

        [Fact]
        public void CreateLooseNull()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            var ex = Assert.Throws<ArgumentNullException>(() => MetadataIdentifier.CreateLoose(null));
#pragma warning restore CS0612 // Type or member is obsolete

            Assert.StartsWith("Value cannot be null.", ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [InlineData("ident")]
        [InlineData("42")]
        [InlineData("042")]
        [InlineData("hello")]
        [InlineData("2147483648")] // int.MaxValue + 1
        [InlineData("hello@")]
        [InlineData("")]
        [InlineData(null)]
        public void CreateUnsafe(string value)
        {
            var identifier = MetadataIdentifier.CreateUnsafe(value);

            Assert.Equal(value, identifier.Value);
        }

        [Fact]
        public void ConstructWithNullString()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new MetadataIdentifier(null));

            Assert.StartsWith("Value cannot be null.", ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        [Fact]
        public void ConstructWithEmptyString()
        {
            var ex = Assert.Throws<ArgumentException>(() => new MetadataIdentifier(""));

            Assert.StartsWith("Metadata identifier cannot be empty.", ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [InlineData("ident")]
        [InlineData("0A")]
        [InlineData("a0")]
        [InlineData("0-1")]
        [InlineData("-1")]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("00")]
        [InlineData("01")]
        [InlineData("2147483648")] // int.MaxValue + 1
        public void ConstructWithString(string value)
        {
            var identifier = new MetadataIdentifier(value);

            Assert.Equal(value, identifier.Value);
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
            var ex = Assert.Throws<ArgumentException>(() => new MetadataIdentifier(value));

            Assert.StartsWith($"A metadata identifier can contain only ASCII alphanumeric characters and hyphens '{value}'.",
                ex.Message);
            Assert.Equal("value", ex.ParamName);
        }

        public static readonly TheoryData<MetadataIdentifier, MetadataIdentifier, bool> EqualityCases
            = new TheoryData<MetadataIdentifier, MetadataIdentifier, bool>()
            {
                {new MetadataIdentifier("hello"), new MetadataIdentifier("hello"), true},
                {new MetadataIdentifier("hello"), new MetadataIdentifier("nope"), false},
                {new MetadataIdentifier("42"), new MetadataIdentifier("42"), true},
                {new MetadataIdentifier("42"), new MetadataIdentifier("0"), false},
                {new MetadataIdentifier("hello"), new MetadataIdentifier("42"), false},
                {MetadataIdentifier.CreateUnsafe(""), MetadataIdentifier.CreateUnsafe(""), true},
#pragma warning disable CS0612 // Type or member is obsolete
                {MetadataIdentifier.CreateLoose("loose"), new MetadataIdentifier("loose"), true},
                {MetadataIdentifier.CreateLoose("045"), MetadataIdentifier.CreateLoose("45"), false},
                {MetadataIdentifier.CreateLoose("10053"), new MetadataIdentifier("10053"), true},
                {MetadataIdentifier.CreateLoose("2147483648"), MetadataIdentifier.CreateLoose("2147483648"), true}, // int.MaxValue + 1
#pragma warning restore CS0612 // Type or member is obsolete
            };

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualsMetadataIdentifier(MetadataIdentifier left, MetadataIdentifier right, bool expected)
        {
            var equal = left.Equals(right);

            Assert.Equal(expected, equal);
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualsObject(MetadataIdentifier left, MetadataIdentifier right, bool expected)
        {
            var equal = left.Equals((object)right);

            Assert.Equal(expected, equal);
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void EqualOperator(MetadataIdentifier left, MetadataIdentifier right, bool expected)
        {
            var equal = left == right;

            Assert.Equal(expected, equal);
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void NotEqualOperator(MetadataIdentifier left, MetadataIdentifier right, bool expectedEqual)
        {
            var equal = left != right;

            Assert.Equal(!expectedEqual, equal);
        }

        [Theory]
        [MemberData(nameof(EqualityCases))]
        public void GetHashCodeTest(MetadataIdentifier left, MetadataIdentifier right, bool expectEqual)
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
            var identifier = MetadataIdentifier.CreateUnsafe(value);

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
            var identifier = MetadataIdentifier.CreateUnsafe(value);

            string convertedValue = identifier.ToString();

            Assert.Equal(value, convertedValue);
        }
    }
}
