using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Semver.Benchmarks
{
    public class ParsingConfig : ManualConfig
    {
        public ParsingConfig()
        {
            //AddJob(new Job("Foo", RunMode.Default).Wit);
        }
    }
}
