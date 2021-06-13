using System.Globalization;

namespace Semver.Test.Builders
{
    public readonly struct TestPrereleaseIdentifier
    {
        public string Value { get; }
        public int? IntValue { get; }

        public TestPrereleaseIdentifier(string value, int? intValue)
        {
            Value = value;
            IntValue = intValue;
        }

        public static implicit operator TestPrereleaseIdentifier(string value)
        {
            return new TestPrereleaseIdentifier(value, null);
        }

        public static implicit operator TestPrereleaseIdentifier(int value)
        {
            return new TestPrereleaseIdentifier(value.ToString(CultureInfo.InvariantCulture), value);
        }

        public static implicit operator PrereleaseIdentifier(TestPrereleaseIdentifier identifier)
        {
            if (identifier.IntValue is int intValue) return new PrereleaseIdentifier(intValue);

            return new PrereleaseIdentifier(identifier.Value);
        }
    }
}
