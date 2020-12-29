using System.Runtime.CompilerServices;

namespace Semver
{
    internal static class CharExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDigit(this char c)
        {
            return c >= '0' && c <= '9';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaOrHyphen(this char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >='a' && c <= 'z') || c == '-';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHandledCharacter(this char c)
        {
            return (c >= 'A' && c <= 'Z')
                   || (c >= 'a' && c <= 'z')
                   || c >= '0' && c <= '9'
                   || c == '-' || c == '.' || c == '+'
                   || char.IsWhiteSpace(c);
        }
    }
}
