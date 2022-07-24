using System;

namespace Semver.Utility
{
    internal static class Parsing
    {
        /// <remarks>
        /// This exception is used with the <see cref="Semver.SemVersionParser.Parse(string,Semver.SemVersionStyles,System.Exception,int,out Semver.SemVersion)"/>
        /// method to indicate parse failure without constructing a new exception.
        /// This exception should never be thrown or exposed outside of this
        /// package.
        /// </remarks>
        public static readonly Exception FailedException = new Exception("Parse Failed");
    }
}
