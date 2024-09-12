using System.Linq;
using BenchmarkDotNet.Running;

namespace Semver.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //var random = new Random();
            //var version = BasicPrereleaseParsing.RandomVersion(random);
            //var seed = random.Next(int.MinValue, int.MaxValue);
            _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args).ToList();
        }
    }
}
