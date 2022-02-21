using System;
using System.Runtime.CompilerServices;
using Semver.Utility;

namespace Semver
{
    // TODO Doc Comment
    public readonly struct MetadataIdentifier : IEquatable<MetadataIdentifier>, IComparable<MetadataIdentifier>, IComparable
    {
        // TODO Doc Comment
        public string Value { get; }

        /// <summary>
        /// Construct a metadata identifier and be loose in what is accepted.
        /// </summary>
        /// <remarks>This should be used only by the <see cref="SemVersion"/> constructor which
        /// still accepts illegal values.</remarks>
        [Obsolete]
        internal static MetadataIdentifier CreateLoose(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return new MetadataIdentifier(value, UnsafeOverload.Marker);
        }

        /// <summary>
        /// Construct a <see cref="MetadataIdentifier"/> without checking that any of the invariants
        /// hold. Used by the parser for performance.
        /// </summary>
        /// <remarks>This is a create method rather than a constructor to clearly indicate uses
        /// of it. The other constructors have not been hidden behind create methods because only
        /// constructors are visible to the package users. So they see a class consistently
        /// using constructors without any create methods.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MetadataIdentifier CreateUnsafe(string value)
        {
#if DEBUG
            if (value is null) throw new ArgumentNullException(nameof(value), "DEBUG: Value cannot be null.");
            if (value.Length == 0) throw new ArgumentException("DEBUG: Metadata identifier cannot be empty.", nameof(value));
            if (!value.IsAlphanumericOrHyphens())
                throw new ArgumentException($"DEBUG: A metadata identifier can contain only ASCII alphanumeric characters and hyphens '{value}'.", nameof(value));
#endif
            return new MetadataIdentifier(value, UnsafeOverload.Marker);
        }

        /// <summary>
        /// Private constructor used by <see cref="CreateUnsafe"/>.
        /// </summary>
        /// <param name="value">The value for the identifier. Not validated.</param>
        /// <param name="_">Unused parameter that differentiates this from the
        /// constructor that performs validation.</param>
        private MetadataIdentifier(string value, UnsafeOverload _)
        {
            Value = value;
        }

        // TODO Doc Comment
        public MetadataIdentifier(string value)
            : this(value, nameof(value))
        {
        }

        /// <summary>
        /// Internal constructor allows changing the parameter name to enable methods using this
        /// as part of their prerelease identifier validation to match the parameter name to their
        /// parameter name.
        /// </summary>
        internal MetadataIdentifier(string value, string paramName)
        {
            if (value is null)
                throw new ArgumentNullException(paramName);
            if (value.Length == 0)
                throw new ArgumentException("Metadata identifier cannot be empty.", paramName);
            if (!value.IsAlphanumericOrHyphens())
                throw new ArgumentException($"A metadata identifier can contain only ASCII alphanumeric characters and hyphens '{value}'.", paramName);

            Value = value;
        }

        #region Equality
        public bool Equals(MetadataIdentifier other) => Value == other.Value;

        public override bool Equals(object obj)
            => obj is MetadataIdentifier other && Equals(other);

        public override int GetHashCode() => HashCodes.Combine(Value);

        public static bool operator ==(MetadataIdentifier left, MetadataIdentifier right)
            => left.Value == right.Value;

        public static bool operator !=(MetadataIdentifier left, MetadataIdentifier right)
            => left.Value != right.Value;
        #endregion

        #region Comparison
        public int CompareTo(MetadataIdentifier other)
            => IdentifierString.Compare(Value, other.Value);

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;
            return obj is MetadataIdentifier other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(MetadataIdentifier)}.", nameof(obj));
        }

        public static bool operator <(MetadataIdentifier left, MetadataIdentifier right)
            => left.CompareTo(right) < 0;

        public static bool operator >(MetadataIdentifier left, MetadataIdentifier right)
            => left.CompareTo(right) > 0;

        public static bool operator <=(MetadataIdentifier left, MetadataIdentifier right)
            => left.CompareTo(right) <= 0;

        public static bool operator >=(MetadataIdentifier left, MetadataIdentifier right)
            => left.CompareTo(right) >= 0;
        #endregion

        public static implicit operator string(MetadataIdentifier metadataIdentifier)
            => metadataIdentifier.Value;

        public override string ToString() => Value;
    }
}
