namespace Semver.Test.Helpers
{
    public static class StringExtensions
    {
        private const int DisplayLimit = 100;

        public static string LimitLength(this string value)
        {
            if (value?.Length > DisplayLimit)
                value = value.Substring(0, DisplayLimit - 3) + "...";

            return value;
        }
    }
}
