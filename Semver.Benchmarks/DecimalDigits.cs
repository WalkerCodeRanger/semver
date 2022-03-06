using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Utility;

namespace Semver.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class DecimalDigits
    {
        [Params(99, 999, 999_999, int.MaxValue)]
        public int Number { get; set; }

        [Benchmark]
        public int Standard() => Number.DecimalDigits();

        [Benchmark]
        public int Logarithm() => LogarithmDecimalDigits(Number);

        [Benchmark]
        public int String() => StringDecimalDigits(Number);

        [Benchmark]
        public int While() => WhileDecimalDigits(Number);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int LogarithmDecimalDigits(int n)
            => (int)Math.Floor(Math.Log10(n) + 1);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int StringDecimalDigits(int n)
            => n.ToString().Length;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int WhileDecimalDigits(int n)
        {
            int digits = 1;
            while ((n /= 10) != 0) ++digits;
            return digits;
        }
    }
}
