using System;

namespace Semver.Ranges
{
    /// <summary>
    /// The options to use when parsing a range with npm syntax.
    /// </summary>
    public class NpmParseOptions : IEquatable<NpmParseOptions>
    {
        /// <summary>
        /// Gets the default parsing options.
        /// </summary>
        public static readonly NpmParseOptions Default = new NpmParseOptions();

        /// <summary>
        /// Gets if non-explicitly selected prerelease versions should be included.
        /// </summary>
        public readonly bool IncludePreRelease;

        private string cachedStringValue;

        /// <param name="includePreRelease">True if non-explicitly selected prerelease versions should be included.</param>
        public NpmParseOptions(bool includePreRelease = false)
        {
            IncludePreRelease = includePreRelease;
        }

        public NpmParseOptions()
        {
        }

        public override string ToString()
        {
            if (cachedStringValue != null)
                return cachedStringValue;

            cachedStringValue = $"{{ IncludePreRelease: {IncludePreRelease} }}";
            return cachedStringValue;
        }

        public bool Equals(NpmParseOptions other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IncludePreRelease == other.IncludePreRelease;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj is NpmParseOptions opts)
                return Equals(opts);

            return false;
        }

        public override int GetHashCode()
        {
            return IncludePreRelease.GetHashCode();
        }
    }
}
