using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Semver.Utility;

namespace Semver.Ranges
{
    /// <summary>
    /// A range of <see cref="SemVersion"/> values. A range can have gaps in it and may include only
    /// some prerelease versions between included release versions. For a range that cannot have
    /// gaps see the <see cref="UnbrokenSemVersionRange"/> class.
    /// </summary>
    public class SemVersionRange : IReadOnlyList<UnbrokenSemVersionRange>
    {
        public static readonly SemVersionRange Empty = new SemVersionRange(ReadOnlyList<UnbrokenSemVersionRange>.Empty);

        public static readonly SemVersionRange AllRelease = new SemVersionRange(UnbrokenSemVersionRange.AllRelease);
        public static readonly SemVersionRange All = new SemVersionRange(UnbrokenSemVersionRange.All);

        public static SemVersionRange GreaterThan(SemVersion version, bool includeAllPrerelease = false)
            => Create(UnbrokenSemVersionRange.GreaterThan(version, includeAllPrerelease));

        public static SemVersionRange AtLeast(SemVersion version, bool includeAllPrerelease = false)
             => Create(UnbrokenSemVersionRange.AtLeast(version, includeAllPrerelease));

        public static SemVersionRange LessThan(SemVersion version, bool includeAllPrerelease = false)
             => Create(UnbrokenSemVersionRange.LessThan(version, includeAllPrerelease));

        public static SemVersionRange AtMost(SemVersion version, bool includeAllPrerelease = false)
            => Create(UnbrokenSemVersionRange.AtMost(version, includeAllPrerelease));

        public static SemVersionRange Inclusive(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(UnbrokenSemVersionRange.Inclusive(start, end, includeAllPrerelease));

        public static SemVersionRange InclusiveOfStart(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(UnbrokenSemVersionRange.InclusiveOfStart(start, end, includeAllPrerelease));

        public static SemVersionRange InclusiveOfEnd(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(UnbrokenSemVersionRange.InclusiveOfEnd(start, end, includeAllPrerelease));

        public static SemVersionRange Exclusive(SemVersion start, SemVersion end, bool includeAllPrerelease = false)
            => Create(UnbrokenSemVersionRange.Exclusive(start, end, includeAllPrerelease));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SemVersionRange Create(UnbrokenSemVersionRange range)
            => UnbrokenSemVersionRange.Empty.Equals(range) ? Empty : new SemVersionRange(range);

        /// <remarks>Ownership of the <paramref name="ranges"/> list must be given to this method.
        /// The list will be mutated and used as the basis of an immutable list. It must not still
        /// be referenced by the caller.</remarks>
        internal static SemVersionRange Create(List<UnbrokenSemVersionRange> ranges)
        {
            throw new NotImplementedException();
        }

        private readonly IReadOnlyList<UnbrokenSemVersionRange> ranges;

        public SemVersionRange(IEnumerable<UnbrokenSemVersionRange> ranges)
            // TODO order ranges and combine
            : this(ranges.ToReadOnlyList())
        {
        }

        /// <remarks>Parameter validation is not performed. The unbroken range must not be empty.</remarks>
        private SemVersionRange(UnbrokenSemVersionRange range)
            : this(new List<UnbrokenSemVersionRange>(1) { range }.AsReadOnly())
        {
        }

        /// <remarks>Parameter validation is not performed. The <paramref name="ranges"/> must be
        /// an immutable list of properly ordered and combined ranges.</remarks>
        private SemVersionRange(IReadOnlyList<UnbrokenSemVersionRange> ranges)
        {
            this.ranges = ranges;
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
