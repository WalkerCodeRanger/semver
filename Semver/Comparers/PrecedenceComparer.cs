using System;
using System.Collections.Generic;
using Semver.Utility;

namespace Semver.Comparers
{
    internal class PrecedenceComparer : Comparer<SemVersion>, ISemVersionComparer
    {
        #region Singleton
        public static readonly ISemVersionComparer Instance = new PrecedenceComparer();

        private PrecedenceComparer() { }
        #endregion

        public bool Equals(SemVersion x, SemVersion y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return x.Major == y.Major && x.Minor == y.Minor && x.Patch == y.Patch
                   && Equals(x.PrereleaseIdentifiers, y.PrereleaseIdentifiers);
        }

        private static bool Equals(
            IReadOnlyList<PrereleaseIdentifier> xIdentifiers,
            IReadOnlyList<PrereleaseIdentifier> yIdentifiers)
        {
            if (xIdentifiers.Count != yIdentifiers.Count) return false;

            for (int i = 0; i < xIdentifiers.Count; i++)
                if (xIdentifiers[i] != yIdentifiers[i])
                    return false;

            return true;
        }

        public int GetHashCode(SemVersion v)
        {
            var hash = CombinedHashCode.Create(v.Major, v.Minor, v.Patch);
            foreach (var identifier in v.PrereleaseIdentifiers)
                hash.Add(identifier);

            return hash;
        }

        public override int Compare(SemVersion x, SemVersion y)
        {
            if (ReferenceEquals(x, y)) return 0; // covers both null case
            if (x is null) return -1;
            if (y is null) return 1;

            var comparison = x.Major.CompareTo(y.Major);
            if (comparison != 0) return comparison;

            comparison = x.Minor.CompareTo(y.Minor);
            if (comparison != 0) return comparison;

            comparison = x.Patch.CompareTo(y.Patch);
            if (comparison != 0) return comparison;

            var xPrereleaseIdentifiers = x.PrereleaseIdentifiers;
            var yPrereleaseIdentifiers = y.PrereleaseIdentifiers;

            // Release are higher precedence than prerelease
            var xIsRelease = xPrereleaseIdentifiers.Count == 0;
            var yIsRelease = yPrereleaseIdentifiers.Count == 0;
            if (xIsRelease && yIsRelease) return 0;
            if (xIsRelease) return 1;
            if (yIsRelease) return -1;

            var minLength = Math.Min(xPrereleaseIdentifiers.Count, yPrereleaseIdentifiers.Count);
            for (int i = 0; i < minLength; i++)
            {
                comparison = xPrereleaseIdentifiers[i].CompareTo(yPrereleaseIdentifiers[i]);
                if (comparison != 0) return comparison;
            }

            return xPrereleaseIdentifiers.Count.CompareTo(yPrereleaseIdentifiers.Count);
        }
    }
}
