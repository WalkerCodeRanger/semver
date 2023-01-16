namespace Semver.Test.Builders
{
    public static class VersionBuilder
    {
        public static SemVersion Version(string version) => SemVersion.Parse(version);
    }
}
