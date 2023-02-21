using System.Text;

namespace Semver.Utility
{
    internal static class IntExtensions
    {
        /// <summary>
        /// The number of digits in a non-negative number. Returns 1 for all
        /// negative numbers. That is ok because we are using it to calculate
        /// string length for a <see cref="StringBuilder"/> for numbers that
        /// aren't supposed to be negative, but when they are it is just a little
        /// slower.
        /// </summary>
        /// <remarks>
        /// This approach is based on https://stackoverflow.com/a/51099524/268898
        /// where the poster offers performance benchmarks showing this is the
        /// fastest way to get a number of digits.
        /// </remarks>
        public static int DecimalDigits(this int n)
        {
            return n switch
            {
                < 10 => 1,
                < 100 => 2,
                < 1_000 => 3,
                < 10_000 => 4,
                < 100_000 => 5,
                < 1_000_000 => 6,
                < 10_000_000 => 7,
                < 100_000_000 => 8,
                < 1_000_000_000 => 9,
                _ => 10
            };
        }
    }
}
