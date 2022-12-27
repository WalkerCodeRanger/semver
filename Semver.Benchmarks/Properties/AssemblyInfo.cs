using System.Diagnostics.CodeAnalysis;

#if NETCOREAPP
// Prior to .NET core ExcludeFromCodeCoverage could not be applied to an assembly
[assembly: ExcludeFromCodeCoverage]
#endif
