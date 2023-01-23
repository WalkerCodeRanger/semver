using System;
using System.Collections.Generic;
using Semver.Benchmarks.Builders;
using Semver.Utility;

namespace Semver.Benchmarks.Ranges
{
    public class NpmRangeParsingHyphenRange : NpmRangeParsing
    {
        private const int Seed = 1450160939;

        protected override IReadOnlyList<string> GetRanges()
        {
            var random = new Random(Seed);

            return Enumerables.Generate(RangeCount, () =>
            {
                string a = random.VersionString();
                string b = random.VersionString();

                return $"{a} - {b}";
            }).ToReadOnlyList();
        }
    }
}