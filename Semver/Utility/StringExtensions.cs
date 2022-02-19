namespace Semver.Utility
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Is this string composed entirely of ASCII digits '0' to '9'?
        /// </summary>
        public static bool IsDigits(this string value)
        {
            foreach (var c in value)
                if (!c.IsDigit())
                    return false;

            return true;
        }

        /// <summary>
        /// Is this string composed entirely of ASCII alphanumeric characters and hyphens?
        /// </summary>
        public static bool IsAlphanumericOrHyphens(this string value)
        {
            foreach (var c in value)
                if (!c.IsAlphaOrHyphen() && !c.IsDigit())
                    return false;

            return true;
        }
    }
}
