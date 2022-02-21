using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Semver.Utility;

namespace Semver
{
    // TODO Doc Comment
    public readonly struct PrereleaseIdentifier : IEquatable<PrereleaseIdentifier>
    {
        // TODO Doc Comment
        public string Value { get; }

        // TODO Doc Comment
        public int? IntValue { get; }

        /// <summary>
        /// Construct a prerelease identifier and be loose in what is accepted.
        /// </summary>
        /// <remarks>This should be used only by the <see cref="SemVersion"/> constructor which
        /// still accepts illegal values.</remarks>
        [Obsolete]
        internal static PrereleaseIdentifier CreateLoose(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (int.TryParse(value, NumberStyles.None, null, out var intValue))
                return new PrereleaseIdentifier(value, intValue);

            return new PrereleaseIdentifier(value, null);
        }

        /// <summary>
        /// Construct a <see cref="PrereleaseIdentifier"/> without checking that any of the invariants
        /// hold. Used by the parser for performance.
        /// </summary>
        /// <remarks>This is a create method rather than a constructor to clearly indicate uses
        /// of it. The other constructors have not been hidden behind create methods because only
        /// constructors are visible to the package users. So they see a class consistently
        /// using constructors without any create methods.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PrereleaseIdentifier CreateUnsafe(string value, int? intValue)
        {
            return new PrereleaseIdentifier(value, intValue);
        }

        /// <summary>
        /// Private constructor used by <see cref="CreateUnsafe"/>.
        /// </summary>
        private PrereleaseIdentifier(string value, int? intValue)
        {
            Value = value;
            IntValue = intValue;
        }

        // TODO Doc Comment
        public PrereleaseIdentifier(string value, bool allowLeadingZeros = false)
            : this(value, allowLeadingZeros, nameof(value))
        {
        }

        /// <summary>
        /// Internal constructor allows changing the parameter name to enable methods using this
        /// as part of their prerelease identifier validation to match the parameter name to their
        /// parameter name.
        /// </summary>
        internal PrereleaseIdentifier(string value, bool allowLeadingZeros, string paramName)
        {
            _ = value ?? throw new ArgumentNullException(paramName);
            if (value.Length == 0)
                throw new ArgumentException("Prerelease identifier cannot be empty.", paramName);
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
                        throw new ArgumentException($"Leading zeros are not allowed on numeric prerelease identifiers '{value}'.", paramName);
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
                    throw new ArgumentException($"A prerelease identifier can contain only ASCII alphanumeric characters and hyphens '{value}'.", paramName);
                IntValue = null;
            }

            Value = value;
        }

        /// <summary>
        /// Construct a numeric prerelease identifier.
        /// </summary>
        /// <param name="value">The non-negative value of this identifier.</param>
        public PrereleaseIdentifier(int value)
        {
            if (value < 0)
                throw new ArgumentException($"Numeric prerelease identifiers can't be negative: {value}.", nameof(value));
            Value = value.ToString(CultureInfo.InvariantCulture);
            IntValue = value;
        }

        #region Equality
        public bool Equals(PrereleaseIdentifier other) => Value == other.Value;

        public override bool Equals(object obj)
            => obj is PrereleaseIdentifier other && Equals(other);

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public static bool operator ==(PrereleaseIdentifier left, PrereleaseIdentifier right)
            => left.Value == right.Value;

        public static bool operator !=(PrereleaseIdentifier left, PrereleaseIdentifier right)
            => left.Value != right.Value;

        #endregion

        public static implicit operator string(PrereleaseIdentifier prereleaseIdentifier)
            => prereleaseIdentifier.Value;

        public override string ToString() => Value;
    }
}
