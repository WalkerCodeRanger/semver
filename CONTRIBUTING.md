# Contributing to the `semver` NuGet Package

To avoid wasting your time, it is recommended you discuss proposed changes in an issue before
creating a pull request. If there is not an appropriate issue, create one.

## Testing

All code should be thoroughly covered by unit tests. This includes all exception cases. If you are
changing an existing method without adequate code coverage, submit a separate PR adding tests first.
This ensures that any changes to functionality are clear.

## Backwards Compatibility and Semantic Versioning

The package strictly follows semantic versioning. No breaking changes are to be made in minor or
patch releases. Not all breaking changes are obvious. Please review Microsoft's guide to [changes
that affect compatibility](https://docs.microsoft.com/en-us/dotnet/core/compatibility/). If in
doubt, ask.

## Coding Conventions

* Follow the convention of existing, non-obsolete code.
* This library has many users, make sure to cover all edge cases.
* Provide clear doc comments for all public and protected members using the [recommended XML tags
  for C# documentation
  comments](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags).
* Internal unsafe methods that do not validate arguments should validate them in debug builds.
  * Prefer using `DebugChecks.Something()` which are marked with `[Conditional("DEBUG")]`. These do
    not count against code coverage.
  * When using `#if DEBUG`, use `// dotcover disable` and `// dotcover enable` to disable code
    coverage on the debug checks.
  * Do not use `Debug.Assert()` because it causes problems when run from unit tests.
  * Exception messages should begin "`DEBUG: `" to differentiate them from regular argument
    validation. This ensures that unit tests will detect when a public method argument validation
    only exists in debug.
* Performance matters, but not to the point of unreadable code.
