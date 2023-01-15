using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Ranges;

namespace Semver.Benchmarks.Ranges
{
    /// <remarks>
    /// This benchmark shows the impact of using <see cref="Enumerable.Any{TSource}(IEnumerable{TSource})"/>
    /// on the performance of <see cref="SemVersionRange.Contains"/>. In .NET Core, even using a
    /// <c>for</c> loop rather than <c>foreach</c> helps.
    /// </remarks>
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class RangeContains
    {
        /// <summary>
        /// A version not contained in the range so every unbroken range will have to be tested.
        /// </summary>
        private static readonly SemVersion Version = SemVersion.ParsedFrom(45, 2, 3, "rc.0", "af541cd");

        /// <summary>
        /// A range with multiple unbroken ranges to highlight the loop performance.
        /// </summary>
        private static readonly SemVersionRange Range = SemVersionRange.Parse("1.* || >=2.5.6-rc < 6.0.0 || 7.2.3-*");

        [Benchmark(Baseline = true)]
        public bool Standard() => Range.Contains(Version);

        [Benchmark]
        public bool Any() => Range.Any(r => r.Contains(Version));

        [Benchmark]
        public bool Foreach()
        {
            foreach (var range in Range)
                if (range.Contains(Version))
                    return true;

            return false;
        }

        [Benchmark]
        public bool For()
        {
            for (var i = 0; i < Range.Count; i++)
                if (Range[i].Contains(Version))
                    return true;

            return false;
        }
    }
}
