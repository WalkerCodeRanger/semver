using System.Diagnostics.CodeAnalysis;

namespace Semver.Test.Helpers
{
    public static class StringExtensions
    {
        private const int DisplayLimit = 100;

        [return: NotNullIfNotNull("value")]
        public static string? LimitLength(this string? value)
        {
            if (value?.Length > DisplayLimit)
                value = value[..(DisplayLimit - 3)] + "...";

            return value;
        }
    }
}
