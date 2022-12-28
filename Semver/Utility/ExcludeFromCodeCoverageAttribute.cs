#if NETSTANDARD1_1
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Specifies that the attributed code should be excluded from code coverage information.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor
        | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event,
        Inherited = false)]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute { }
}
#endif
