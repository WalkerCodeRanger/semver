using System;

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
    }
}
