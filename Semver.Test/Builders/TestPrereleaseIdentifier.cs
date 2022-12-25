using System.Globalization;

namespace Semver.Test.Builders
{
    public readonly struct TestPrereleaseIdentifier
    {
        public string Value { get; }
        public int? NumericValue { get; }

        public TestPrereleaseIdentifier(string value, int? numericValue)
        {
            Value = value;
            NumericValue = numericValue;
        }

        public static implicit operator TestPrereleaseIdentifier(string value)
            => new TestPrereleaseIdentifier(value, null);

        public static implicit operator TestPrereleaseIdentifier(int value)
            => new TestPrereleaseIdentifier(value.ToString(CultureInfo.InvariantCulture), value);

        public static implicit operator PrereleaseIdentifier(TestPrereleaseIdentifier identifier)
        {
            if (identifier.NumericValue is int numericValue) return new PrereleaseIdentifier(numericValue);

            return new PrereleaseIdentifier(identifier.Value);
        }
    }
}
