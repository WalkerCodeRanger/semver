using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Semver.Ranges.Comparers.Npm
{
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

        public static NpmRange Parse(string range)
        {
            return Parse(range, default);
        }

        public static NpmRange Parse(string range, NpmParseOptions options)
        {
            try
            {
                return new RangeParser().ParseRange(range, options);
            }
            catch (RegexMatchTimeoutException ex)
            {
                throw new RangeParseException("Regex match timed out", ex);
            }
        }

        public static bool TryParse(string strRange, out NpmRange range)
        {
            return TryParse(strRange, default, out range);
        }

        public static bool TryParse(string strRange, NpmParseOptions options, out NpmRange range)
        {
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

        public override string ToString()
        {
            if (cachedStringValue != null)
                return cachedStringValue;

            cachedStringValue = string.Join($" {RangeParser.OrSeparator[0]} ", Ranges.Select(comps => string.Join(" ", comps.Select(comp => comp.ToString()))));
            
            return cachedStringValue;
        }
    }
}
