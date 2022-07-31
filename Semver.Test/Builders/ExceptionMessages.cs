using System;
using System.Globalization;

namespace Semver.Test.Builders
{
    public static class ExceptionMessages
    {
        /// <summary>
        /// Default exception message for <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <remarks>It might be thought it is not necessary to test for this message. However,
        /// the Semver package internally has debug exception checking and the tests need to verify
        /// that an <see cref="ArgumentNullException"/> will be included in the release rather than
        /// is just coming from the debug exception checking.</remarks>
        public const string NotNull = "Value cannot be null.";

        public const string NoMetadata = "Cannot have metadata.";

        public static string InjectValue(string format, string value)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, "{0}", value);
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Could not inject '{value}' into '{format}'", ex);
            }
        }
    }
}
