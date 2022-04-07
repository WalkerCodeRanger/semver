using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Semver.Ranges.Comparers.Npm
{
    /// <summary>
    /// A range of versions that can be checked against to see if a <see cref="SemVersion"/> is included.
    /// Uses the same syntax as npm.
    /// </summary>
    public class NpmRange
    {
        internal readonly NpmComparator[][] Ranges;
        private string cachedStringValue;

        internal NpmRange(IEnumerable<NpmComparator[]> comparators)
        {
            if (comparators is NpmComparator[][] arrayComparators)
                Ranges = arrayComparators;
            else
                Ranges = comparators.ToArray();

            if (Ranges.Length == 0)
                throw new ArgumentException("There must be atleast one comparator in the range");
        }

        /// <summary>
        /// Parses the range.
        /// </summary>
        /// <param name="range">The range to parse.</param>
        /// <returns>The parsed range</returns>
        /// <exception cref="ArgumentNullException">Thrown when range is null.</exception>
        /// <exception cref="RangeParseException">Thrown when the range has invalid syntax or if regex match timed out.</exception>
        public static NpmRange Parse(string range)
        {
            return Parse(range, default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range">The range to parse.</param>
        /// <param name="options">The options to use when parsing.</param>
        /// <returns>The parsed range.</returns>
        /// <exception cref="ArgumentNullException">Thrown when range is null.</exception>
        /// <exception cref="RangeParseException">Thrown when the range has invalid syntax or if regex match timed out.</exception>
        public static NpmRange Parse(string range, NpmParseOptions options)
        {
            if (range == null) throw new ArgumentNullException(nameof(range));
            
            try
            {
                return new RangeParser().ParseRange(range, options);
            }
            catch (RegexMatchTimeoutException ex)
            {
                throw new RangeParseException("Regex match timed out", ex);
            }
        }

        /// <summary>
        /// Tries to parse the range and returns true if successful.
        /// </summary>
        /// <param name="strRange">The range to parse.</param>
        /// <param name="range">The parsed <see cref="NpmRange"/>.</param>
        /// <returns>Returns true if the range was parsed successfully.</returns>
        /// <exception cref="ArgumentNullException">Thrown if strRange is null.</exception>
        public static bool TryParse(string strRange, out NpmRange range)
        {
            if (strRange == null) throw new ArgumentNullException(nameof(strRange));
            return TryParse(strRange, default, out range);
        }

        /// <summary>
        /// Tries to parse the range with the given options and returns true if successful.
        /// </summary>
        /// <param name="strRange">The range to parse.</param>
        /// <param name="options">The options to use when parsing.</param>
        /// <param name="range">The parsed range.</param>
        /// <returns>Returns true if the range was parsed successfully.</returns>
        /// <exception cref="ArgumentNullException">Thrown if strRange is null.</exception>
        public static bool TryParse(string strRange, NpmParseOptions options, out NpmRange range)
        {
            if (strRange == null) throw new ArgumentNullException(nameof(strRange));
            
            try
            {
                range = Parse(strRange, options);
                return true;
            }
            catch (RangeParseException)
            {
                range = null;
                return false;
            }
            catch (RegexMatchTimeoutException)
            {
                range = null;
                return false;
            }
        }

        /// <summary>
        /// Returns whether the specified version is included in this range.
        /// </summary>
        /// <param name="version">The version to check if it's included in this range.</param>
        /// <returns>True if the version is included in this range.</returns>
        public bool Includes(SemVersion version)
        {
            bool anySuccess = false;
            
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
                    anySuccess = true;
                    break;
                }
            }

            return anySuccess;
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
    }
}
