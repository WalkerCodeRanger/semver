using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Semver.Ranges.Npm
{
    /// <summary>
    /// A range of versions that can be checked against to see if a <see cref="SemVersion"/> is included.
    /// Uses the same syntax as npm.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never), Obsolete("For alpha version only, removed in release")]
    internal class NpmRangeSet : SemVersionRangeSet
    {
        internal readonly NpmComparator[][] Ranges;
        private string cachedStringValue;

        internal NpmRangeSet(IEnumerable<NpmComparator[]> comparators)
        {
            if (comparators is NpmComparator[][] arrayComparators)
                Ranges = arrayComparators;
            else
                Ranges = comparators.ToArray();

            if (Ranges.Length == 0)
                throw new ArgumentException("There must be at least one comparator in the range");
        }

        /// <summary>
        /// Parses the range.
        /// </summary>
        /// <param name="range">The range to parse.</param>
        /// <returns>The parsed range</returns>
        /// <exception cref="ArgumentNullException">Thrown when range is null.</exception>
        /// <exception cref="FormatException">Thrown when the range has invalid syntax or if regex match timed out.</exception>
        public static NpmRangeSet Parse(string range) => Parse(range, false);

        /// <summary>
        ///
        /// </summary>
        /// <param name="range">The range to parse.</param>
        /// <param name="includeAllPrerelease"></param>
        /// <returns>The parsed range.</returns>
        /// <exception cref="ArgumentNullException">Thrown when range or options is null.</exception>
        /// <exception cref="FormatException">Thrown when the range has invalid syntax or if regex match timed out.</exception>
        public static NpmRangeSet Parse(string range, bool includeAllPrerelease)
        {
            if (range == null) throw new ArgumentNullException(nameof(range));

            try
            {
                return RangeParser.ParseRange(range, includeAllPrerelease);
            }
            catch (RegexMatchTimeoutException ex)
            {
                throw new FormatException("Regex match timed out", ex);
            }
        }

        /// <summary>
        /// Tries to parse the range and returns true if successful.
        /// </summary>
        /// <param name="strRange">The range to parse.</param>
        /// <param name="range">The parsed <see cref="NpmRangeSet"/>.</param>
        /// <returns>Returns true if the range was parsed successfully.</returns>
        /// <exception cref="ArgumentNullException">Thrown if strRange is null.</exception>
        public static bool TryParse(string strRange, out NpmRangeSet range)
        {
            if (strRange == null) throw new ArgumentNullException(nameof(strRange));
            return TryParse(strRange, false, out range);
        }

        /// <summary>
        /// Tries to parse the range with the given options and returns true if successful.
        /// </summary>
        /// <param name="strRange">The range to parse.</param>
        /// <param name="includeAllPrerelease"></param>
        /// <param name="range">The parsed range.</param>
        /// <returns>Returns true if the range was parsed successfully.</returns>
        /// <exception cref="ArgumentNullException">Thrown if strRange or options is null.</exception>
        public static bool TryParse(string strRange, bool includeAllPrerelease, out NpmRangeSet range)
        {
            if (strRange == null) throw new ArgumentNullException(nameof(strRange));

            try
            {
                range = Parse(strRange, includeAllPrerelease);
                return true;
            }
            catch (FormatException)
            {
                range = null;
                return false;
            }
        }

        /// <inheritdoc />
        public override bool Contains(SemVersion version)
        {
            foreach (NpmComparator[] comps in Ranges)
            {
                // All comparators in range must succeed
                bool failed = false;

                foreach (NpmComparator comp in comps)
                {
                    if (!comp.Includes(version))
                    {
                        failed = true;
                        break;
                    }
                }

                if (!failed)
                {
                    // Success if at least one range includes the version
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a string of the included versions in this range.
        /// </summary>
        /// <returns>A string of the included versions in this range.</returns>
        public override string ToString()
        {
            if (cachedStringValue != null)
                return cachedStringValue;

            cachedStringValue = string.Join($" {RangeParser.OrSeparator[0]} ", Ranges.Select(comps => string.Join(" ", comps.Select(comp => comp.ToString()))));

            return cachedStringValue;
        }

        public SemVersionRange ToSemVersionRange()
        {
            var unbrokenRanges = new List<UnbrokenSemVersionRange>();

            foreach (NpmComparator[] comps in Ranges)
            {
                var leftBound = LeftBoundedRange.Unbounded;
                var rightBound = RightBoundedRange.Unbounded;
                var includeAllPrerelease = false;
                foreach (var comp in comps)
                {
                    includeAllPrerelease |= comp.IncludeAllPrerelease;
                    if (comp.AnyVersion)
                        break;
                    switch (comp.Operator)
                    {
                        case ComparatorOp.LessThan:
                            rightBound = rightBound.Min(new RightBoundedRange(comp.Version, false));
                            break;
                        case ComparatorOp.LessThanOrEqualTo:
                            rightBound = rightBound.Min(new RightBoundedRange(comp.Version, true));
                            break;
                        case ComparatorOp.GreaterThan:
                            leftBound = leftBound.Max(new LeftBoundedRange(comp.Version, false));
                            break;
                        case ComparatorOp.GreaterThanOrEqualTo:
                            leftBound = leftBound.Max(new LeftBoundedRange(comp.Version, true));
                            break;
                        case ComparatorOp.Equals:
                            leftBound = leftBound.Max(new LeftBoundedRange(comp.Version, true));
                            rightBound = rightBound.Min(new RightBoundedRange(comp.Version, true));
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid enum value {comp.Operator}");
                    }
                }
                unbrokenRanges.Add(UnbrokenSemVersionRange.Create(leftBound, rightBound, includeAllPrerelease));
            }

            return SemVersionRange.Create(unbrokenRanges);
        }
    }
}
