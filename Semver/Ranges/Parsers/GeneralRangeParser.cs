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
               || (char.IsPunctuation(c) && !char.IsWhiteSpace(c) && c != '*' && c != '.')
               || (char.IsSymbol(c) && c != '*');

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPossibleOperatorOrSpaceChar(char c) => c == ' ' || IsPossibleOperatorChar(c);
    }
}
