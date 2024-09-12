# Framework Support

This package attempts to maintain support for all versions of .NET Framework, .NET Core, Mono,
Xamarin, and etc. that are still officially supported by their maintainers. As of v3.0.0 of the
package, this is accomplished by multi-targeting the following targets:

* .NET Standard 2.0
* .NET Standard 2.1
* .NET 5

The reason for targeting each is given in the sections below.

## .NET Standard 2.0

.NET Standard 2.0 is the newest version of the .NET Standard API specification that still supports
the .NET Framework. This is necessary to continue to support the Microsoft supported versions of
.NET Framework (e.g. .NET Framework 4.6.1 through 4.8.X). This also includes support for
serialization which .NET Standard 1.X did not support.

## .NET Standard 2.1

.NET Standard 2.1 supports C# 8.0 which includes support for nullable reference types. Including
this target allows users of the package targeting frameworks with nullable reference types to
benefit from type information about which references can or can't be null in the package.

## .NET 5

The `MemberNotNullWhenAttribute` was introduced with .NET 5. This attribute allows the package to
provide better null handling for properties in the range types. To provide better support, the
[Nullable](https://github.com/manuelroemer/Nullable) package is used to provide this attribute in
earlier .NET versions. However, this adds types to the package that aren't needed in .NET 5. Thus
publishing a .NET 5 version of the package allows for a smaller assembly and one that uses the type
built into the framework when using .NET 5+.

## Unit Test Frameworks

In order to unit test each of the three published versions, the tests are run under the following
framework versions below. For .NET Standard versions, the newest framework that tests the library is
used. For .NET versions, the same framework version is used.

* .NET Framework 4.8.1 (tests .NET Standard 2.0)
* .NET Core 3.1 (tests .NET Standard 2.1)
* .NET 5 (tests .NET 5)
