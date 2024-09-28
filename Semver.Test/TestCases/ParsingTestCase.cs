using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Semver.Utility;

namespace Semver.Test.TestCases;

/// <summary>
/// Represents an individual parsing test case.
/// </summary>
public class ParsingTestCase
{
    public static ParsingTestCase Valid(
        string version,
        SemVersionStyles requiredStyles,
        BigInteger major, BigInteger minor, BigInteger patch,
        IEnumerable<PrereleaseIdentifier> prereleaseIdentifiers,
        IEnumerable<MetadataIdentifier> metadataIdentifiers,
        int maxLength = SemVersion.MaxVersionLength)
        => new(version, requiredStyles, major, minor, patch,
            prereleaseIdentifiers, metadataIdentifiers, maxLength);

    public static ParsingTestCase Invalid(
        string? version,
        SemVersionStyles styles,
        Type exceptionType,
        string exceptionMessage,
        int maxLength = SemVersion.MaxVersionLength)
        => new(version, styles, exceptionType, exceptionMessage, maxLength);

    private ParsingTestCase(
        string version,
        SemVersionStyles requiredStyles,
        BigInteger major,
        BigInteger minor,
        BigInteger patch,
        IEnumerable<PrereleaseIdentifier> prereleaseIdentifiers,
        IEnumerable<MetadataIdentifier> metadataIdentifiers,
        int maxLength)
    {
        Version = version;
        Styles = requiredStyles;
        IsValid = true;
        Major = major;
        Minor = minor;
        Patch = patch;
        MaxLength = maxLength;
        PrereleaseIdentifiers = prereleaseIdentifiers.ToReadOnlyList();
        MetadataIdentifiers = metadataIdentifiers.ToReadOnlyList();
    }

    private ParsingTestCase(
        string? version,
        SemVersionStyles styles,
        Type exceptionType,
        string exceptionMessageFormat,
        int maxLength)
    {
        Version = version;
        Styles = styles;
        IsValid = false;
        ExceptionType = exceptionType;
        ExceptionMessageFormat = exceptionMessageFormat;
        MaxLength = maxLength;
    }

    private ParsingTestCase(
        bool isValid,
        string? version,
        SemVersionStyles requiredStyles,
        BigInteger? major,
        BigInteger? minor,
        BigInteger? patch,
        IReadOnlyList<PrereleaseIdentifier>? prereleaseIdentifiers,
        IReadOnlyList<MetadataIdentifier>? metadataIdentifiers,
        Type? exceptionType,
        string? exceptionMessageFormat,
        int maxLength)
    {
        IsValid = isValid;
        Version = version;
        Styles = requiredStyles;
        Major = major;
        Minor = minor;
        Patch = patch;
        PrereleaseIdentifiers = prereleaseIdentifiers;
        MetadataIdentifiers = metadataIdentifiers;
        ExceptionType = exceptionType;
        ExceptionMessageFormat = exceptionMessageFormat;
        MaxLength = maxLength;
    }

    public ParsingTestCase Change(string? version = null, SemVersionStyles? styles = null)
        => new(IsValid, version ?? Version, styles ?? Styles,
            Major, Minor, Patch,
            PrereleaseIdentifiers, MetadataIdentifiers,
            ExceptionType, ExceptionMessageFormat, MaxLength);

    public string? Version { get; }
    public SemVersionStyles Styles { get; }
    public int MaxLength { get; }

    [MemberNotNullWhen(true, nameof(Version), nameof(Major), nameof(Minor), nameof(Patch), nameof(PrereleaseIdentifiers), nameof(MetadataIdentifiers))]
    [MemberNotNullWhen(false, nameof(ExceptionType), nameof(ExceptionMessageFormat))]
    public bool IsValid { get; }

    #region Valid Values
    public BigInteger? Major { get; }
    public BigInteger? Minor { get; }
    public BigInteger? Patch { get; }

    public IReadOnlyList<PrereleaseIdentifier>? PrereleaseIdentifiers { get; }
    public IReadOnlyList<MetadataIdentifier>? MetadataIdentifiers { get; }
    #endregion

    #region Invalid Values
    public Type? ExceptionType { get; }
    public string? ExceptionMessageFormat { get; }
    #endregion

    public override string ToString()
    {
        if (Version is null) return $"null as {Styles}";
        return Version.Length > 200
            ? $"Long #{Version.GetHashCode()} as {Styles}"
            : $"'{Version}' as {Styles}";
    }
}
