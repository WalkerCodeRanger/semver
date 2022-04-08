using System;

namespace Semver.Ranges.Comparers.Npm
{
    /// <summary>
    /// The options to use when parsing a range with npm syntax.
    /// </summary>
    public readonly struct NpmParseOptions : IEquatable<NpmParseOptions>
    {
        /// <summary>
        /// Gets if non-explicitly selected prerelease versions should be included.
        /// </summary>
        public readonly bool IncludePreRelease;
        
        private readonly string stringValue;

        /// <param name="includePreRelease">True if non-explicitly selected prerelease versions should be included.</param>
        public NpmParseOptions(bool includePreRelease = false)
        {
            IncludePreRelease = includePreRelease;
            stringValue = $"{{ IncludePreRelease: {IncludePreRelease} }}";
        }

        public override string ToString() => stringValue;

        public bool Equals(NpmParseOptions other)
        {
            return IncludePreRelease == other.IncludePreRelease;
        }

        public override bool Equals(object obj)
        {
            return obj is NpmParseOptions other && Equals(other);
        }

        public override int GetHashCode()
        {
            return IncludePreRelease.GetHashCode();
        }
    }
}
