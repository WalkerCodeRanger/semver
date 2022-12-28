using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Utility;

namespace Semver.Benchmarks
{
    /// <remarks>Shows that a for loop through the characters of a string segment is much faster
    /// than enumerating them since the compiler has no loop optimizer for this case.</remarks>
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class LoopThroughStringSegment
    {
        private static readonly StringSegment Value = "245413548946516575165756156751323245451984";

        /// <remarks>This is the baseline because this is what is done in the package.</remarks>
        [Benchmark(Baseline = true)]
        public int ForLoop()
        {
            // The accumulator ensures the loop isn't dead code with minimal overhead
            int accumulator = 0;
            for (int i = 0; i < Value.Length; i++)
                accumulator += Value[i];

            return accumulator;
        }

        [Benchmark]
        public int ForeachLoop()
        {
            // The accumulator ensures the loop isn't dead code with minimal overhead
            int accumulator = 0;
            foreach (char c in GetEnumerator(Value))
                accumulator += c;

            return accumulator;
        }

        [Benchmark]
        public int ForeachLambda()
        {
            // The accumulator ensures the loop isn't dead code with minimal overhead
            int accumulator = 0;
            Foreach(Value, c => accumulator += c);
            return accumulator;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<char> GetEnumerator(StringSegment segment)
        {
            var end = segment.Offset + segment.Length;
            for (int i = segment.Offset; i < end; i++)
                yield return segment.Source[i];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Foreach(StringSegment segment, Action<char> action)
        {
            var end = segment.Offset + segment.Length;
            for (int i = segment.Offset; i < end; i++)
                action(segment.Source[i]);
        }
    }
}
