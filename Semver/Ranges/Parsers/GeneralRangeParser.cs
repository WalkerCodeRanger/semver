using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Semver.Utility;

namespace Semver.Ranges.Parsers
{
    internal static class GeneralRangeParser
    {
        public static int CountSplitOnOrOperator(string range)
        {
#if DEBUG
            if (range is null) throw new ArgumentNullException(nameof(range), "DEBUG: Value cannot be null.");
#endif
            int count = 1; // Always one more item than there are separators
            bool possiblyInSeparator = false;
            // Use `for` instead of `foreach` to ensure performance
            for (int i = 0; i < range.Length; i++)
            {
                var isSeparatorChar = range[i] == '|';
                if (possiblyInSeparator && isSeparatorChar)
                {
                    count++;
                    possiblyInSeparator = false;
                }
                else
                    possiblyInSeparator = isSeparatorChar;
            }

            return count;
        }

        public static IEnumerable<StringSegment> SplitOnOrOperator(string range)
        {
#if DEBUG
            if (range is null) throw new ArgumentNullException(nameof(range), "DEBUG: Value cannot be null.");
#endif
            var possiblyInSeparator = false;
            int start = 0;
            // Use `for` instead of `foreach` to ensure performance
            for (int i = 0; i < range.Length; i++)
            {
                var isSeparatorChar = range[i] == '|';
                if (possiblyInSeparator && isSeparatorChar)
                {
                    possiblyInSeparator = false;
                    yield return range.Slice(start, i - 1 - start);
                    start = i + 1;
                }
                else
                    possiblyInSeparator = isSeparatorChar;
            }

            // The final segment from the last separator to the end of the string
            yield return range.Slice(start, range.Length - start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPossibleOperatorChar(char c)
            => c == '=' || c == '<' || c == '>' || c == '~' || c == '^'
               || (char.IsPunctuation(c) && !char.IsWhiteSpace(c) && c != '*')
               || (char.IsSymbol(c) && c != '*');

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPossibleVersionChar(char c)
            => !char.IsWhiteSpace(c) && (!IsPossibleOperatorChar(c) || c == '-' || c == '.');

        /// <summary>
        /// Parse optional spaces from the beginning of the segment.
        /// </summary>
        public static Exception ParseSpaces(ref StringSegment segment, Exception ex)
        {
            segment = segment.TrimStartSpaces();

            if (segment.Length > 0 && char.IsWhiteSpace(segment[0]))
                return ex ?? RangeError.InvalidWhitespace(segment.Offset, segment.Source);
            return null;
        }

        /// <summary>
        /// Parse optional whitespace from the beginning of the segment.
        /// </summary>
        public static Exception ParseWhitespace(ref StringSegment segment, Exception ex)
        {
            segment = segment.TrimStartWhitespace();
            return null;
        }

        /// <summary>
        /// Parse a version number from the beginning of the segment.
        /// </summary>
        public static Exception ParseVersion(
            ref StringSegment segment,
            SemVersionRangeOptions rangeOptions,
            SemVersionParsingOptions parseOptions,
            Exception ex,
            int maxLength,
            out SemVersion semver,
            out WildcardVersion wildcardVersion)
        {
            // The SemVersionParser assumes there is nothing following the version number. To reuse
            // its parsing, the appropriate end must be found.
            var end = 0;
            while (end < segment.Length && IsPossibleVersionChar(segment[end])) end++;
            var version = segment.Subsegment(0, end);
            segment = segment.Subsegment(end);

            return SemVersionParser.Parse(version, rangeOptions.ToStyles(), parseOptions, ex,
                maxLength, out semver, out wildcardVersion);
        }
    }
}
