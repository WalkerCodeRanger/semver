using System;
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
            DebugChecks.IsNotNull(version, nameof(version));
            DebugChecks.NoMetadata(version, nameof(version));

            Version = version;
            Inclusive = inclusive;
        }

        public SemVersion Version { get; }
        public bool Inclusive { get; }

        /// <summary>
        /// Whether this bound actually includes any prerelease versions.
        /// </summary>
        /// <remarks>
        /// A non-inclusive bound of X.Y.Z-0 doesn't actually include any prerelease versions.
        /// </remarks>
        public bool IncludesPrerelease
            => Version.IsPrerelease && !(!Inclusive && Version.PrereleaseIsZero);

        public bool Contains(SemVersion version)
        {
            var comparison = SemVersion.ComparePrecedence(version, Version);
            return Inclusive ? comparison <= 0 : comparison < 0;
        }

        public RightBoundedRange Min(RightBoundedRange other)
        {
            var comparison = SemVersion.ComparePrecedence(Version, other.Version);
            if (comparison == 0)
                // If the versions are equal, then non-inclusive will be min
                return new RightBoundedRange(Version, Inclusive && other.Inclusive);
            return comparison < 0 ? this : other;
        }

        public RightBoundedRange Max(RightBoundedRange other)
        {
            var comparison = SemVersion.ComparePrecedence(Version, other.Version);
            if (comparison == 0)
                // If the versions are equal, then inclusive will be max
                return new RightBoundedRange(Version, Inclusive || other.Inclusive);
            return comparison < 0 ? other : this;
        }

        #region Equality
        public bool Equals(RightBoundedRange other)
            => Equals(Version, other.Version) && Inclusive == other.Inclusive;

        public override bool Equals(object? obj)
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
