extern alias previous;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Previous = previous::Semver;

namespace Semver.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class SemVersionParsedFrom
    {
        private const string Prerelease = "asdf.24534634.sdfdg.02343sd";
        private const string Metadata = "some.random.metadata.35435345.03534634";

        [Benchmark]
        public string Simple_Previous() => Previous.SemVersion.ParsedFrom(1, 2, 3).Prerelease;

        [Benchmark]
        public string Full_Previous()
            => Previous.SemVersion.ParsedFrom(1, 2, 3, Prerelease, Metadata).Prerelease;

        [Benchmark]
        public string Simple_Current() => SemVersion.ParsedFrom(1, 2, 3).Prerelease;

        [Benchmark]
        public string Full_Current()
            => SemVersion.ParsedFrom(1, 2, 3, Prerelease, Metadata).Prerelease;
    }
}
