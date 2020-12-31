using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Semver
{
    public readonly struct PrereleaseIdentifier : IEquatable<PrereleaseIdentifier>
    {
        public string Value { get; }
        public int? IntValue { get; }

        /// <summary>
        /// Construct a prerelease identifier and be loose in what is accepted
        /// </summary>
        /// <remarks>This should be used only by the <see cref="SemVersion"/> constructor which
        /// still accepts illegal values.</remarks>
        [Obsolete]
        internal static PrereleaseIdentifier CreateLoose(string value)
        {
            if (int.TryParse(value, NumberStyles.None, null, out var intValue))
                return new PrereleaseIdentifier(value, intValue);

            return new PrereleaseIdentifier(value, null);
        }

        /// <summary>
        /// Construct a <see cref="PrereleaseIdentifier"/> without checking that any of the invariants
        /// hold. Used by the parser for performance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PrereleaseIdentifier CreateUnsafe(string value, int? intValue)
        {
            return new PrereleaseIdentifier(value, intValue);
        }

        private PrereleaseIdentifier(string value, int? intValue)
        {
            Value = value;
            IntValue = intValue;
        }

        public PrereleaseIdentifier(string value, bool allowLeadingZeros = false)
        {
            _ = value ?? throw new ArgumentNullException(nameof(value));
            if (value.Length == 0)
                throw new ArgumentException("Cannot be empty string.", nameof(value));
            if (value.IsDigits())
            {
                if (value.Length > 1 && value[0] == '0')
                {
                    if (allowLeadingZeros)
                    {
                        value = value.TrimStart('0');
                        if (value.Length == 0) value = "0";
                    }
                    else
                        throw new ArgumentException($"Leading zeros are not allowed on numeric prerelease identifiers '{value}'.", nameof(value));
                }

                try
                {
                    IntValue = int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
                }
                catch (OverflowException)
                {
                    // Remake the overflow exception to give better message
                    throw new OverflowException($"Prerelease identifier '{value}' was too large for Int32.");
                }
            }
            else
            {
                if (!value.IsAlphanumericOrHyphens())
                    throw new ArgumentException($"A prerelease identifier can contain only ASCII alphanumeric characters and hyphens '{value}'.", nameof(value));
                IntValue = null;
            }

            Value = value;
        }

        public PrereleaseIdentifier(int value)
        {
            if (value < 0)
                throw new ArgumentException($"Numeric prerelease identifiers can't be negative: {value}.", nameof(value));
            Value = value.ToString(CultureInfo.InvariantCulture);
            IntValue = value;
        }

        #region Equality
        public bool Equals(PrereleaseIdentifier other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is PrereleaseIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(PrereleaseIdentifier left, PrereleaseIdentifier right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(PrereleaseIdentifier left, PrereleaseIdentifier right)
        {
            return left.Value != right.Value;
        }
        #endregion

        public static implicit operator string(PrereleaseIdentifier prereleaseIdentifier)
        {
            return prereleaseIdentifier.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
