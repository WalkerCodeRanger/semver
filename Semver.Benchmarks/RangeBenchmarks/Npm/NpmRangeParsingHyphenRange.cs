using System;
using System.Collections.Generic;
using Semver.Benchmarks.Builders;
using Semver.Utility;

namespace Semver.Benchmarks.RangeBenchmarks.Npm
{
    public class NpmRangeParsingHyphenRange : NpmRangeParsing
    {
        protected override IReadOnlyList<string> GetRanges()
        {
            var random = new Random();

            return Enumerables.Generate(NumRanges, () =>
            {
                string a = random.VersionString();
                string b = random.VersionString();

                return $"{a} - {b}";
            }).ToReadOnlyList();
        }
    }
}