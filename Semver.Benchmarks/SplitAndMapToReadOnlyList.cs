using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Semver.Utility;

namespace Semver.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net461)]
    [SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class SplitAndMapToReadOnlyList
    {
        /// <remarks>Values of different length. The final one is chosen to be long enough to require
        /// two <see cref="List{T}"/> capacity increases.</remarks>
        [Params("", "hello", "hello.world", "hello.world.how.are.you.today?.I'm.good.thank.you")]
        public string Value { get; set; }

        private static string Identity(string value) => value;

        [Benchmark(Baseline = true)]
        public IReadOnlyList<string> Standard()
        {
            return Value.SplitAndMapToReadOnlyList('.', Identity);
        }

        [Benchmark]
        public IReadOnlyList<string> WithoutCapacity()
        {
            return SplitAndMapToReadOnlyListWithoutCapacity(Value, '.', Identity);
        }

        [Benchmark]
        public IReadOnlyList<string> WithLinqCount()
        {
            return SplitAndMapToReadOnlyListWithoutCapacity(Value, '.', Identity);
        }

        /// <summary>
        /// This is the old Linq code that <see cref="StringExtensions.SplitAndMapToReadOnlyList{T}"/>
        /// replaced.
        /// </summary>
        /// <remarks>This is inlined rather than a separate method because that is how it was in the
        /// original <see cref="SemVersion"/> code.</remarks>
        [Benchmark]
        public IReadOnlyList<string> Linq()
        {
            return Value.Split('.')
                        .Select(Identity)
                        .ToReadOnlyList();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IReadOnlyList<T> SplitAndMapToReadOnlyListWithoutCapacity<T>(
            string value,
            char splitOn,
            Func<string, T> func)
        {
            if (value.Length == 0) return ReadOnlyList<T>.Empty;

            var items = new List<T>();
            int start = 0;
            for (int i = 0; i < value.Length; i++)
                if (value[i] == splitOn)
                {
                    items.Add(func(value.Substring(start, i - start)));
                    start = i + 1;
                }

            // Add the final items from the last separator to the end of the string
            items.Add(func(value.Substring(start, value.Length - start)));

            return items.AsReadOnly();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IReadOnlyList<T> SplitAndMapToReadOnlyListWithLinqCount<T>(
            string value,
            char splitOn,
            Func<string, T> func)
        {
            if (value.Length == 0) return ReadOnlyList<T>.Empty;

            // Figure out how many items the resulting list will have
            int count = 1 + value.Count(t => t == splitOn); // Always one more item than there are separators

            // Allocate enough capacity for the items
            var items = new List<T>(count);
            int start = 0;
            for (int i = 0; i < value.Length; i++)
                if (value[i] == splitOn)
                {
                    items.Add(func(value.Substring(start, i - start)));
                    start = i + 1;
                }

            // Add the final items from the last separator to the end of the string
            items.Add(func(value.Substring(start, value.Length - start)));

            return items.AsReadOnly();
        }
    }
}
