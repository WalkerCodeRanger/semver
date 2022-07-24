using System;
using System.Linq;
using System.Runtime.InteropServices;

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
    /// The <see cref="UnbrokenSemVersionRange.MinVersion"/> (i.e. <c>0.0.0-0</c>) cannot be used instead
    /// because it would be inclusive of prerelease.</remarks>
    [StructLayout(LayoutKind.Auto)]
    internal readonly struct LeftBoundedRange
    {
        public static readonly LeftBoundedRange Unbounded = new LeftBoundedRange(null, false);

        public LeftBoundedRange(SemVersion version, bool inclusive)
        {
#if DEBUG
            if (version is null && inclusive)
                throw new ArgumentException("Cannot be inclusive of start without start value.", nameof(inclusive));
            if (version?.MetadataIdentifiers.Any() ?? false)
                throw new ArgumentException("Cannot have metadata", nameof(version));
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
    }
}
