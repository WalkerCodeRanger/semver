using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Utility;

namespace Semver.Benchmarks;

[SimpleJob(RuntimeMoniker.Net471)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public class IsDigits
{
    private const string Value = "245413548946516575165756156751323245451984";

    [Benchmark(Baseline = true)]
    public bool Standard() => Value.IsDigits();

    [Benchmark]
    public bool ForeachLoop() => ForeachIsDigits(Value);

    [Benchmark]
    public bool ForLoop() => ForIsDigits(Value);

    public static bool ForeachIsDigits(string value)
    {
        foreach (var c in value)
            if (!c.IsDigit())
                return false;

        return true;
    }

    public static bool ForIsDigits(string value)
    {
        for (var i = 0; i < value.Length; i++)
            if (!value[i].IsDigit())
                return false;

        return true;
    }
}
