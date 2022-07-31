using System;
using System.Linq;
using System.Runtime.InteropServices;
using Semver.Comparers;
using Semver.Utility;

namespace Semver.Ranges
{
    /// <summary>
    /// A range of versions that is bounded only on the right. That is a range defined by some version
    /// <c>x</c> such that <c>v &lt; x</c> or <c>v &lt;= x</c> depending on whether it is inclusive.
    /// A right-bounded range forms the upper limit for a version range.
    /// </summary>
    /// <remarks>An "unbounded" right-bounded range is represented by an inclusive upper bound of
    /// <see cref="SemVersion.Max"/>.</remarks>
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct RightBoundedRange : IEquatable<RightBoundedRange>
    {
        public static readonly RightBoundedRange Unbounded
            = new RightBoundedRange(SemVersion.Max, true);

        public RightBoundedRange(SemVersion version, bool inclusive)
        {
#if DEBUG
            if (version is null) throw new ArgumentNullException(nameof(version), "DEBUG: Value cannot be null.");
            if (version.MetadataIdentifiers.Any())
                throw new ArgumentException("DEBUG: Cannot have metadata.", nameof(version));
#endif
            Version = version;
            Inclusive = inclusive;
        }

        public SemVersion Version { get; }
        public bool Inclusive { get; }

        public bool Contains(SemVersion version)
        {
            var comparison = SemVersion.ComparePrecedence(version, Version);
            return Inclusive ? comparison <= 0 : comparison < 0;
        }

        public RightBoundedRange Min(RightBoundedRange other)
        {
            var comparison = SemVersion.ComparePrecedence(Version, other.Version);
            if (comparison < 0) return this;
            if (comparison == 0)
                // If the versions are equal, then non-inclusive will be min
                return new RightBoundedRange(Version, Inclusive && other.Inclusive);
            return other;
        }

        #region Equality
        public bool Equals(RightBoundedRange other)
            => Equals(Version, other.Version) && Inclusive == other.Inclusive;

        public override bool Equals(object obj)
            => obj is RightBoundedRange other && Equals(other);

        public override int GetHashCode()
            => CombinedHashCode.Create(Version, Inclusive);

        public static bool operator ==(RightBoundedRange left, RightBoundedRange right)
            => left.Equals(right);

        public static bool operator !=(RightBoundedRange left, RightBoundedRange right)
            => !left.Equals(right);
        #endregion

        public int CompareTo(RightBoundedRange other)
        {
            var comparison = PrecedenceComparer.Instance.Compare(Version, other.Version);
            if (comparison != 0) return comparison;
            return Inclusive.CompareTo(other.Inclusive);
        }

        public override string ToString() => (Inclusive ? "<=" : "<") + Version;
    }
}
