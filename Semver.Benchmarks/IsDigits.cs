using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Semver.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    public class IsDigits
    {
        private const string Value = "245413548946516575165756156751323245451984";

        [Benchmark]
        public bool ForeachLoop() => ForeachIsDigit(Value);

        [Benchmark]
        public bool ForLoop() => ForIsDigit(Value);

        public static bool ForeachIsDigit(string value)
        {
            foreach (var c in value)
                if (!c.IsDigit())
                    return false;

            return true;
        }

        public static bool ForIsDigit(string value)
        {
            for (var i = 0; i < value.Length; i++)
                if (!value[i].IsDigit())
                    return false;

            return true;
        }
    }
}
