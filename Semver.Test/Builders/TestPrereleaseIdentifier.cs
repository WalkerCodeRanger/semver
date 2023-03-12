using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Semver.Test.Builders
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct TestPrereleaseIdentifier
    {
        public string Value { get; }
        public BigInteger? NumericValue { get; }

        public TestPrereleaseIdentifier(string value, BigInteger? numericValue)
        {
            Value = value;
            NumericValue = numericValue;
        }

        public static implicit operator TestPrereleaseIdentifier(string value)
            => new TestPrereleaseIdentifier(value, null);

        public static implicit operator TestPrereleaseIdentifier(ulong value) =>
            new TestPrereleaseIdentifier(value.ToString(CultureInfo.InvariantCulture), value);

        public static implicit operator TestPrereleaseIdentifier(BigInteger value)
            => new TestPrereleaseIdentifier(value.ToString(CultureInfo.InvariantCulture), value);

        public static implicit operator PrereleaseIdentifier(TestPrereleaseIdentifier identifier)
        {
            if (identifier.NumericValue is BigInteger numericValue) return new PrereleaseIdentifier(numericValue);

            return new PrereleaseIdentifier(identifier.Value);
        }
    }
}
