using System;
using System.Collections.Generic;
using System.Linq;
using Semver.Utility;

namespace Semver.Comparers
{
    internal class SortComparer : Comparer<SemVersion>, ISemVersionComparer
    {
        #region Singleton
        public static readonly ISemVersionComparer Instance = new SortComparer();

        private SortComparer() { }
        #endregion

        public bool Equals(SemVersion x, SemVersion y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return x.Major == y.Major && x.Minor == y.Minor && x.Patch == y.Patch
                   && Equals(x.PrereleaseIdentifiers, y.PrereleaseIdentifiers)
                   && Equals(x.MetadataIdentifiers, y.MetadataIdentifiers);
        }

        /// <summary>
        /// Compare prerelease identifiers by their string value so that "01" != "1"
        /// </summary>
        private static bool Equals(
            IReadOnlyList<PrereleaseIdentifier> xIdentifiers,
            IReadOnlyList<PrereleaseIdentifier> yIdentifiers)
        {
            if (xIdentifiers.Count != yIdentifiers.Count) return false;

            for (int i = 0; i < xIdentifiers.Count; i++)
                if (xIdentifiers[i].Value != yIdentifiers[i].Value)
                    return false;

            return true;
        }

        private static bool Equals(
            IReadOnlyList<MetadataIdentifier> xIdentifiers,
            IReadOnlyList<MetadataIdentifier> yIdentifiers)
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
            {
                hash.Add(identifier);
                // TODO remove the next `if` in v3.0.0 when prerelease identifiers can't have leading zeros
                // Change the hash for leading zero so "01" != "1"
                if (identifier.NumericValue != null && identifier.Value[0] == '0')
                    hash.Add(identifier.Value);
            }
            // Mark the start of metadata so 1.0.0-a and 1.0.0+a have different hashes
            if (v.MetadataIdentifiers.Any())
                hash.Add("+");
            foreach (var identifier in v.MetadataIdentifiers)
                hash.Add(identifier);

            return hash;
        }

        public override int Compare(SemVersion x, SemVersion y)
        {
            if (ReferenceEquals(x, y)) return 0; // covers both null case

            var comparison = PrecedenceComparer.Instance.Compare(x, y);
            if (comparison != 0) return comparison;

            var xMetadataIdentifiers = x.MetadataIdentifiers;
            var yMetadataIdentifiers = y.MetadataIdentifiers;
            var minLength = Math.Min(xMetadataIdentifiers.Count, yMetadataIdentifiers.Count);
            for (int i = 0; i < minLength; i++)
            {
                comparison = xMetadataIdentifiers[i].CompareTo(yMetadataIdentifiers[i]);
                if (comparison != 0) return comparison;
            }

            comparison = xMetadataIdentifiers.Count.CompareTo(yMetadataIdentifiers.Count);
            if (comparison != 0) return comparison;

            // TODO remove the next section in v3.0.0 when prerelease identifiers can't have leading zeros
            // Now must sort based on leading zeros in prerelease identifiers.
            // Doing this after metadata so that 1.0.0-1+a < 1.0.0-01+a.b (i.e. leading zeros are the
            // least significant difference to sort on).
            var xPrereleaseIdentifiers = x.PrereleaseIdentifiers;
            var yPrereleaseIdentifiers = y.PrereleaseIdentifiers;
            minLength = Math.Min(xPrereleaseIdentifiers.Count, yPrereleaseIdentifiers.Count);
            for (int i = 0; i < minLength; i++)
                if (xPrereleaseIdentifiers[i].NumericValue != null) // Skip alphanumeric identifiers
                {
                    comparison = IdentifierString.Compare(
                        xPrereleaseIdentifiers[i].Value, yPrereleaseIdentifiers[i].Value);
                    if (comparison != 0) return comparison;
                }

            return 0;
        }
    }
}
