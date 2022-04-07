using System;

namespace Semver.Ranges
{
    /// <summary>
    /// The exception that is thrown when a range's syntax could not be parsed.
    /// </summary>
    public class RangeParseException : Exception
    {
        public RangeParseException(string message) : base(message)
        {
        }

        public RangeParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
