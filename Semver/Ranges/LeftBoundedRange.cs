using System;
using System.Linq;
using System.Runtime.InteropServices;
using Semver.Comparers;
using Semver.Utility;

namespace Semver.Ranges
{
    /// <summary>
    /// A range of versions that is bounded only on the left. That is a range defined by some version
    /// <c>x</c> such that <c>x &lt; v</c> or <c>x &lt;= v</c> depending on whether it is inclusive.
    /// A left-bounded range forms the lower limit for a version range.
    /// </summary>
    /// <remarks>An "unbounded" left-bounded range is represented by a lower bound of
    /// <see langword="null"/> since <see langword="null"/> compares as less than all versions.
    /// However, it does not allow such ranges to be inclusive because a range cannot contain null.
    /// The <see cref="SemVersion.Min"/> (i.e. <c>0.0.0-0</c>) cannot be used instead
    /// because it would be inclusive of prerelease.</remarks>
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct LeftBoundedRange : IEquatable<LeftBoundedRange>
    {
        public static readonly LeftBoundedRange Unbounded = new LeftBoundedRange(null, false);

        public LeftBoundedRange(SemVersion version, bool inclusive)
        {
#if DEBUG
            if (version is null && inclusive)
                throw new ArgumentException("DEBUG: Cannot be inclusive of start without start value.", nameof(inclusive));
            if (version?.MetadataIdentifiers.Any() ?? false)
                throw new ArgumentException("DEBUG: Cannot have metadata.", nameof(version));
#endif
            Version = version;
            Inclusive = inclusive;
        }

        public SemVersion Version { get; }
        public bool Inclusive { get; }

        public bool Contains(SemVersion version)
        {
            var comparison = SemVersion.ComparePrecedence(Version, version);
            return Inclusive ? comparison <= 0 : comparison < 0;
        }

        public LeftBoundedRange Max(LeftBoundedRange other)
        {
            var comparison = SemVersion.ComparePrecedence(Version, other.Version);
            if (comparison < 0) return other;
            if (comparison == 0)
                // If the versions are equal, then non-inclusive will be max
                return new LeftBoundedRange(Version, Inclusive && other.Inclusive);
            return this;
        }

        #region Equality
        public bool Equals(LeftBoundedRange other)
            => Equals(Version, other.Version) && Inclusive == other.Inclusive;

        public override bool Equals(object obj)
            => obj is LeftBoundedRange other && Equals(other);

        public override int GetHashCode()
            => CombinedHashCode.Create(Version, Inclusive);

        public static bool operator ==(LeftBoundedRange left, LeftBoundedRange right)
            => left.Equals(right);

        public static bool operator !=(LeftBoundedRange left, LeftBoundedRange right)
            => !left.Equals(right);
        #endregion

        public int CompareTo(RightBoundedRange other)
        {
            var comparison = SemVersion.PrecedenceComparer.Compare(Version, other.Version);
            if (comparison != 0) return comparison;
            return Inclusive && other.Inclusive ? 0 : 1;
        }

        public int CompareTo(LeftBoundedRange other)
        {
            var comparison = PrecedenceComparer.Instance.Compare(Version, other.Version);
            if (comparison != 0) return comparison;
            return -Inclusive.CompareTo(other.Inclusive);
        }
    }
}
