[![Build status](https://ci.appveyor.com/api/projects/status/kek3h7gflo3qqidb?svg=true)](https://ci.appveyor.com/project/maxhauser/semver)
[![NuGet](https://img.shields.io/nuget/v/semver.svg)](https://www.nuget.org/packages/semver/)

A semantic version library for .Net
===================================

This class library implements the SemVersion class, that
complies to v2.0.0 of the spec from http://semver.org.

**Installation**

With the nuget console:

     Install-Package semver

**Parsing**

     var version = SemVersion.Parse("1.1.0-rc.1+nightly.2345");

**Comparing**

     if(version >= "1.0")
         Console.WriteLine("released version {0}!", version)
