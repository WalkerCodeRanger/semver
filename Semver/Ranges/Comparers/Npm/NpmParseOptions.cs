namespace Semver.Ranges.Comparers.Npm
{
    public readonly struct NpmParseOptions
    {
        public readonly bool IncludePreRelease;
        
        private readonly string stringValue;

        public NpmParseOptions(bool includePreRelease = false)
        {
            IncludePreRelease = includePreRelease;
            stringValue = $"{{ IncludePreRelease: {IncludePreRelease} }}";
        }

        public override string ToString() => stringValue;
    }
}
