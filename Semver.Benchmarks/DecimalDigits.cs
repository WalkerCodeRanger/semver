using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Semver.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    public class DecimalDigits
    {
        [Params(99, 999, 999_999, int.MaxValue)]
        public int Number { get; set; }

        [Benchmark]
        public int Standard()
        {
            return Number.DecimalDigits();
        }

        [Benchmark]
        public int Logarithm()
        {
            return LogarithmDecimalDigits(Number);
        }

        [Benchmark]
        public int String()
        {
            return StringDecimalDigits(Number);
        }

        [Benchmark]
        public int While()
        {
            return WhileDecimalDigits(Number);
        }

        private static int LogarithmDecimalDigits(int n)
        {
            return (int)Math.Floor(Math.Log10(n) + 1);
        }

        private static int StringDecimalDigits(int n)
        {
            return n.ToString().Length;
        }

        public static int WhileDecimalDigits(int n)
        {
            int digits = 1;
            while ((n /= 10) != 0) ++digits;
            return digits;
        }
    }
}
