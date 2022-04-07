using System;

namespace Semver.Ranges.Comparers.Npm
{
    public readonly struct NpmParseOptions : IEquatable<NpmParseOptions>
    {
        public readonly bool IncludePreRelease;
        
        private readonly string stringValue;

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
