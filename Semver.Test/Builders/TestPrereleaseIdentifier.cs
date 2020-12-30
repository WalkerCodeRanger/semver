using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Semver.Test.Builders
{
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Test Builder")]
    public readonly struct TestPrereleaseIdentifier
    {
        public string Value { get; }
        public int? IntValue { get; }

        public TestPrereleaseIdentifier(string value, int? intValue)
        {
            Value = value;
            IntValue = intValue;
        }

        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Test Builder")]
        public static implicit operator TestPrereleaseIdentifier(string value)
        {
            return new TestPrereleaseIdentifier(value, null);
        }

        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Test Builder")]
        public static implicit operator TestPrereleaseIdentifier(int value)
        {
            return new TestPrereleaseIdentifier(value.ToString(CultureInfo.InvariantCulture), value);
        }

        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Test Builder")]
        public static implicit operator PrereleaseIdentifier(TestPrereleaseIdentifier identifier)
        {
            if (identifier.IntValue is int intValue) return new PrereleaseIdentifier(intValue);

            return new PrereleaseIdentifier(identifier.Value);
        }
    }
}
