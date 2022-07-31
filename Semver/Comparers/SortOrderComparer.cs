using System;
using System.Collections.Generic;
using Semver.Utility;

namespace Semver.Comparers
{
    internal class SortOrderComparer : Comparer<SemVersion>, ISemVersionComparer
    {
        #region Singleton
        public static readonly ISemVersionComparer Instance = new SortOrderComparer();

        private SortOrderComparer() { }
        #endregion

        public bool Equals(SemVersion x, SemVersion y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            return x.Major == y.Major && x.Minor == y.Minor && x.Patch == y.Patch
                   // Compare prerelease identifiers by their string value so that "01" != "1"
                   && string.Equals(x.Prerelease, y.Prerelease, StringComparison.Ordinal)
                   && string.Equals(x.Metadata, y.Metadata, StringComparison.Ordinal);
        }

        public int GetHashCode(SemVersion v)
            // Using v.Prerelease handles leading zero so "01" != "1"
            => CombinedHashCode.Create(v.Major, v.Minor, v.Patch, v.Prerelease, v.Metadata);

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
