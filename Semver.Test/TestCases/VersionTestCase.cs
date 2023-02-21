namespace Semver.Test.TestCases
{
    public class VersionTestCase
    {
        public VersionTestCase(string version)
        {
            Version = version;
        }
        public string Version { get; }

        public override string ToString() => Version;
    }
}
