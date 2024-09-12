using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using Semver.Test.Helpers;
using Xunit;

namespace Semver.Test;

public class PrereleaseIdentifierTests
{
    /// <summary>
    /// Microsoft Guidelines are that structs should be 16 bytes or less. Since
    /// <see cref="PrereleaseIdentifier"/> contains a <see cref="BigInteger"/> that is composed
    /// of a <see cref="Int32"/> and a <see cref="T:UInt32[]"/> and a <see cref="String"/>, its
    /// size depends on the architecture. On a 32-bit system it should be 12 bytes which may get
    /// padded to 16. However, on 64-bit systems it should be 20 bytes which may get padded out
    /// to 24 or 32 bytes. Since <see cref="PrereleaseIdentifier"/> will be rarely passed and
    /// copied by typically held in a collection, this has been deemed acceptable.
    /// </summary>
    [Fact]
    public void SizeIsAcceptable()
    {
        var size = Unsafe.SizeOf<PrereleaseIdentifier>();

        Assert.InRange(size, 0, 32);
    }

    [Theory]
    [InlineData("ident", null)]
    [InlineData("42", 42)]
    [InlineData("0-1", null)]
    [InlineData("0A", null)]
    [InlineData("-1", null)]
    public void CreateUnsafe(string value, int? numericValue)
    {
        var identifier = PrereleaseIdentifier.CreateUnsafe(value, numericValue);

        Assert.Equal(value, identifier.Value);
        Assert.Equal(numericValue, identifier.NumericValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConstructWithNullString(bool allowLeadingZeros)
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new PrereleaseIdentifier(null!, allowLeadingZeros));

        Assert.StartsWith(ExceptionMessages.NotNull, ex.Message);
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
    [InlineData("0", false, "0", 0uL)]
    [InlineData("1", false, "1", 1uL)]
    [InlineData("00", true, "0", 0uL)]
    [InlineData("01", true, "1", 1uL)]
    // int.MaxValue + 1
    [InlineData("2147483648", false, "2147483648", 2147483648uL)]
    // long.MaxValue + 1
    [InlineData("9223372036854775808", false, "9223372036854775808", 9223372036854775808)]
    public void ConstructWithString(string value, bool allowLeadingZeros,
        string expectedValue, ulong? expectedNumericValue)
    {
        var identifier = new PrereleaseIdentifier(value, allowLeadingZeros);

        Assert.Equal(expectedValue, identifier.Value);
        Assert.Equal(expectedNumericValue, identifier.NumericValue);
    }

    [Fact]
    public void ConstructWithStringThatWouldOverflow()
    {
        // ulong.MaxValue + 1
        var identifier = new PrereleaseIdentifier("18446744073709551616");

        Assert.Equal("18446744073709551616", identifier.Value);
        BigInteger expected = 18446744073709551615;
        expected += 1;
        Assert.Equal(expected, identifier.NumericValue);
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
        Assert.Equal(value, identifier.NumericValue);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ConstructWithNegativeInt(int value)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new PrereleaseIdentifier(value));

        Assert.StartsWith($"Numeric prerelease identifiers can't be negative: {value}.", ex.Message);
        Assert.Equal("value", ex.ParamName);
    }

    #region Equality
    public static readonly TheoryData<PrereleaseIdentifier, PrereleaseIdentifier, bool> EqualityCases
        = new TheoryData<PrereleaseIdentifier, PrereleaseIdentifier, bool>()
        {
            {new PrereleaseIdentifier("hello"), new PrereleaseIdentifier("hello"), true},
            {new PrereleaseIdentifier("hello"), new PrereleaseIdentifier("nope"), false},
            {new PrereleaseIdentifier(42), new PrereleaseIdentifier(42), true},
            {new PrereleaseIdentifier(42), new PrereleaseIdentifier(0), false},
            {new PrereleaseIdentifier("hello"), new PrereleaseIdentifier(42), false},
            {default, default, true},
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
    #endregion

    #region Comparison
    [Theory]
    [InlineData("a", "a", 0)]
    [InlineData("a", "b", -1)]
    [InlineData("b", "a", 1)]
    [InlineData(null, null, 0)]
    [InlineData("a", "aa", -1)]
    [InlineData("aa", "aa", 0)]
    [InlineData("aa", "ab", -1)]
    [InlineData("ab", "aa", 1)]
    [InlineData("1", "1", 0)]
    [InlineData("1", "42", -1)]
    [InlineData("beta", "rc", -1)] // Case that causes -16 for string comparison
                                   // Numeric identifiers always have lower precedence than alphanumeric (even though '-' < '0')
    [InlineData("0", "a", -1)]
    [InlineData("9", "A", -1)]
    [InlineData("9", "-", -1)]
    [InlineData("-", "9", 1)]
    [InlineData("0", "-100", -1)]
    public void CompareTo(string? left, string? right, int expected)
    {
        var leftIdentifier = CreateOrDefault(left);
        var rightIdentifier = CreateOrDefault(right);

        Assert.Equal(expected, leftIdentifier.CompareTo(rightIdentifier));
    }

    [Theory]
    [MemberData(nameof(EqualityCases))]
    public void CompareToMatchesEquality(PrereleaseIdentifier left, PrereleaseIdentifier right, bool equal)
    {
        var comparison = left.CompareTo(right);

        if (equal)
            Assert.Equal(0, comparison);
        else
            Assert.NotEqual(0, comparison);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("42")]
    [InlineData(null)]
    public void CompareToNullObject(string? value)
    {
        var comparison = CreateOrDefault(value).CompareTo(null);

        Assert.Equal(1, comparison);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("42")]
    [InlineData(null)]
    public void CompareToObject(string? value)
    {
        var ex = Assert.Throws<ArgumentException>(()
            => CreateOrDefault(value).CompareTo(new object()));

        Assert.StartsWith("Object must be of type PrereleaseIdentifier.", ex.Message);
        Assert.Equal("value", ex.ParamName);
    }
    #endregion

    [Theory]
    [InlineData("0A")]
    [InlineData("123")]
    [InlineData("1-2")]
    [InlineData(null)]
    public void ImplicitConversionToString(string? value)
    {
        var identifier = CreateOrDefault(value);

        string convertedValue = identifier;

        Assert.Equal(value, convertedValue);
    }

    [Theory]
    [InlineData("0A")]
    [InlineData("123")]
    [InlineData("1-2")]
    [InlineData(null)]
    public void ToStringTest(string? value)
    {
        var identifier = CreateOrDefault(value);

        string convertedValue = identifier.ToString();

        Assert.Equal(value, convertedValue);
    }

    private static PrereleaseIdentifier CreateOrDefault(string? value)
    {
        if (value is null) return default;
        return new PrereleaseIdentifier(value);
    }
}
