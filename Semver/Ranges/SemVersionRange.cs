using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Semver.Utility;

namespace Semver.Ranges
{
    /// <summary>
    /// A range of <see cref="SemVersion"/> values. A range can have gaps in it and may include only
    /// some prerelease versions between included release versions. For a range that cannot have
    /// gaps see the <see cref="UnbrokenSemVersionRange"/> class.
    /// </summary>
    internal class SemVersionRange : IReadOnlyList<UnbrokenSemVersionRange>
    {
        private readonly IReadOnlyList<UnbrokenSemVersionRange> ranges;

        public SemVersionRange(IEnumerable<UnbrokenSemVersionRange> ranges)
        {
            // TODO order ranges and combine
            this.ranges = ranges.ToReadOnlyList();
        }

        public bool Contains(SemVersion version) => ranges.Any(r => r.Contains(version));

        public static implicit operator Predicate<SemVersion>(SemVersionRange range)
            => range.Contains;

        #region IReadOnlyList<UnbrokenSemVersionRange>
        public int Count => ranges.Count;

        public UnbrokenSemVersionRange this[int index] => ranges[index];

        public IEnumerator<UnbrokenSemVersionRange> GetEnumerator() => ranges.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
