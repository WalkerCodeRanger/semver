using System;
using System.Runtime.Serialization;
using Xunit;

namespace Semver.Test
{
    /// <summary>
    /// These tests help to ensure that every <see cref="SemVersion"/> implements
    /// the correct interfaces on all frameworks.
    /// </summary>
    public class SemVersionImplementsInterfaceTests
    {
        [Fact]
        public void SemVersionImplementsIEquatableSemVersion()
        {
            Assert.IsAssignableFrom<IEquatable<SemVersion>>(new SemVersion(1));
        }

        [Fact]
        public void SemVersionImplementsISerializable()
        {
            Assert.IsAssignableFrom<ISerializable>(new SemVersion(1));
        }
    }
}
