# Publishing a Release

## Preparing for the Release

* Make all changes for the release on `master` (except for the readme).
* Have a `vX.Y.Z-readme` branch ready with the readme for the next version.
* Prepare a branch in the docs repo with updated docs for the new version and verify the doc
  comments for the new version.

## Steps to Publish Release

1. Update the NuGet package release notes link to the URL where notes for this version will be. This
   will be `https://github.com/WalkerCodeRanger/semver/releases/tag/vX.Y.Z`.
2. Merge the `vX.Y.Z-readme` branch into `master`.
3. Create an *annotated* tag of the version number being published prefixed with "v". The tag
   description should match the tag name.
4. Wait for the build of that tag to complete in Appveyor.
5. Download the nupkg and snupkg files from Appveyor's assets tab.
6. Upload those to NuGet.org. The readme should be populated from the package.

## After Publishing a Release

These steps only need to be done for a non-prerelease version.

1. Create a release from the tag on GitHub.
   * Write up the changes in that version.
   * Attach the nupkg and snupkg files to the release in GitHub.
2. In GitHub, mark the milestone and any included issues as done with the current date.
3. Update `appveyor.yml` with the new version number so builds will be assigned proper versions.
4. Update the `PackageValidationBaselineVersion` in the `Semver.csproj` to change the baseline
   package version to the newly released version. It may be necessary to update the
   `CompatibilitySuppressions.xml` file after that. (see [Detecting breaking changes between two
   versions of a NuGet package at packaging time](https://www.meziantou.net/detecting-breaking-changes-between-two-versions-of-a-nuget-package-at-packaging.htm)
   for more info). Note that the `CP0003` error about the version number should be higher occurs
   locally, but on the build server suppressing it causes the build to fail.
5. Update the `PublicAPI` files to reflect the newly published APIs.
6. Update the benchmark to reference the new version as the previous version.
7. Create a new `vX.Y.Z-readme` branch for the next version.
