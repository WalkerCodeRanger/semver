using System;
#if !NETSTANDARD
using System.Runtime.Serialization;
#endif
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
        public void SemVersionImplementsIComparable()
        {
            Assert.IsAssignableFrom<IComparable>(new SemVersion(1));
        }

        [Fact]
        public void SemVersionImplementsIComparableSemVersion()
        {
            Assert.IsAssignableFrom<IComparable<SemVersion>>(new SemVersion(1));
        }

        [Fact]
        public void SemVersionImplementsIEquatableSemVersion()
        {
            Assert.IsAssignableFrom<IEquatable<SemVersion>>(new SemVersion(1));
        }

#if !NETSTANDARD
        [Fact]
        public void SemVersionImplementsISerializable()
        {
            Assert.IsAssignableFrom<ISerializable>(new SemVersion(1));
        }
#endif
    }
}
