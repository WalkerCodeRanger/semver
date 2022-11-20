using System;
using System.Text;
using Semver.Utility;

namespace Semver.Ranges.Npm
{
    internal class NpmComparator : IEquatable<NpmComparator>
    {
        public readonly ComparatorOp Operator;
        public readonly SemVersion Version;
        public readonly bool AnyVersion;

        public readonly bool IncludeAllPrerelease;
        private NpmComparator[] comparators; // The other comparators in the same AND range
        private string cachedStringValue;

        public NpmComparator(ComparatorOp @operator, SemVersion version, bool includeAllPrerelease)
        {
            if (@operator == ComparatorOp.ReasonablyClose || @operator == ComparatorOp.CompatibleWith)
                throw new ArgumentException("Invalid operator (ReasonablyClose and CompatibleWith are invalid uses in this context)");

            Operator = @operator;
            Version = version;
            IncludeAllPrerelease = includeAllPrerelease;
        }

        /// <summary>
        /// Any version will match when using this constructor
        /// </summary>
        public NpmComparator(bool includeAllPrerelease)
        {
            AnyVersion = true;
            IncludeAllPrerelease = includeAllPrerelease;
        }

        internal void SetRangeComparators(NpmComparator[] comparators)
        {
            this.comparators = comparators ?? throw new ArgumentNullException(nameof(comparators));
        }

        public bool Includes(SemVersion version)
        {
            // Only allow prerelease if either
            // a) options include pre-releases
            // b) this version is a prerelease and main versions match (ignoring prerelease)
            // c) another comparator in range is prerelease and matches main version
            if (!AnyVersion)
            {
                if (version.IsPrerelease && !IncludeAllPrerelease)
                {
                    if (!Version.IsPrerelease || !MainVersionEquals(version))
                    {
                        bool anySuccess = false;

                        if (comparators.Length > 1)
                        {
                            foreach (NpmComparator otherComp in comparators)
                            {
                                if (ReferenceEquals(otherComp, this))
                                    continue;

                                if (!otherComp.AnyVersion && otherComp.Version.IsPrerelease && otherComp.MainVersionEquals(version))
                                {
                                    anySuccess = true;
                                    break;
                                }
                            }
                        }

                        if (!anySuccess)
                            return false;
                    }
                }
            }

            if (AnyVersion)
                return !version.IsPrerelease || IncludeAllPrerelease;

            int comparison = Compare(version);
            bool result;

            switch (Operator)
            {
                case ComparatorOp.LessThan:
                    result = comparison < 0;
                    break;
                case ComparatorOp.GreaterThan:
                    result = comparison > 0;
                    break;
                case ComparatorOp.LessThanOrEqualTo:
                    result = comparison <= 0;
                    break;
                case ComparatorOp.GreaterThanOrEqualTo:
                    result = comparison >= 0;
                    break;
                case ComparatorOp.Equals:
                    result = comparison == 0;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public int Compare(SemVersion version)
        {
            if (AnyVersion)
                return 0;

            int comparison = version.Major.CompareTo(Version.Major);
            if (comparison != 0) return comparison;

            comparison = version.Minor.CompareTo(Version.Minor);
            if (comparison != 0) return comparison;

            comparison = version.Patch.CompareTo(Version.Patch);
            if (comparison != 0) return comparison;

            if ((version.IsPrerelease && Version.IsPrerelease)
                || (version.IsPrerelease && IncludeAllPrerelease))
                comparison = Math.Sign(ComparePreRelease(version));

            return comparison;
        }

        private int ComparePreRelease(SemVersion version)
        {
            var xPrereleaseIdentifiers = version.PrereleaseIdentifiers;
            var yPrereleaseIdentifiers = Version.PrereleaseIdentifiers;

            // Release are higher precedence than prerelease
            var xIsRelease = xPrereleaseIdentifiers.Count == 0;
            var yIsRelease = yPrereleaseIdentifiers.Count == 0;
            if (xIsRelease && yIsRelease) return 0;
            if (xIsRelease) return 1;
            if (yIsRelease) return -1;

            var minLength = Math.Min(xPrereleaseIdentifiers.Count, yPrereleaseIdentifiers.Count);
            for (int i = 0; i < minLength; i++)
            {
                int comparison = xPrereleaseIdentifiers[i].CompareTo(yPrereleaseIdentifiers[i]);
                if (comparison != 0) return comparison;
            }

            return xPrereleaseIdentifiers.Count.CompareTo(yPrereleaseIdentifiers.Count);
        }

        private bool MainVersionEquals(SemVersion version)
        {
            // Not equal if this comparator is * and version is prerelease but options does not include pre-releases
            if (AnyVersion && version.IsPrerelease && !IncludeAllPrerelease)
                return false;

            if (AnyVersion)
                return true;

            if (version.Major != Version.Major)
                return false;

            if (version.Minor != Version.Minor)
                return false;

            if (version.Patch != Version.Patch)
                return false;

            return true;
        }

        public override string ToString()
        {
            if (cachedStringValue != null)
                return cachedStringValue;

            var builder = new StringBuilder();
            if (AnyVersion)
                builder.Append('*');
            else
            {
                builder.Append(ComparatorParser.OperatorsReverse[Operator]);
                builder.Append(Version);
            }

            cachedStringValue = builder.ToString();
            return cachedStringValue;
        }

        public bool Equals(NpmComparator other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Operator == other.Operator
                && AnyVersion == other.AnyVersion
                && IncludeAllPrerelease == other.IncludeAllPrerelease
                && (Version == null && other.Version == null || other.Version != null && Version != null && Version.Equals(other.Version));
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj is NpmComparator comp)
                return Equals(comp);

            return false;
        }

        public override int GetHashCode()
            => CombinedHashCode.Create(Operator, Version, AnyVersion, IncludeAllPrerelease);
    }
}
