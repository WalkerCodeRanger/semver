using System;
using System.Text;
using Semver.Utility;

namespace Semver.Benchmarks.Builders;

public static class RandomExtensions
{
    private const string Alphanumerics = "0123456789"
                                         +"ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                                         +"abcdefghijklmnopqrstruvwxz-";

    public static string PrereleaseIdentifier(this Random random, int maxLength)
    {
        if (random.NextBool())
            // Numeric Identifier
            return random.Next().ToString();

        // Build alphanumeric identifier (which is list metadata except for numeric)
        var value = random.MetadataIdentifier(maxLength);

        // Avoid leading zeros in numeric identifiers by making them alphanumeric
        if (value[0]=='0' && value.IsDigits())
            value += random.Alpha();

        return value;
    }

    public static string MetadataIdentifier(this Random random, int maxLength)
    {
        // Build alphanumeric identifier
        var length = random.Next(1, maxLength);
        var builder = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            builder.Append(random.Alphanumeric());

        return builder.ToString();
    }

    public static string VersionString(
        this Random random,
        int maxVersion = 100,
        int maxPrereleaseIdentifiers = 100,
        int maxPrereleaseIdentifierLength = 100,
        int maxMetadataIdentifiers = 100,
        int maxMetadataIdentifierLength = 100)
    {
        var builder = new StringBuilder();
        builder.Append(random.Next(0, maxVersion));
        builder.Append('.');
        builder.Append(random.Next(0, maxVersion));
        builder.Append('.');
        builder.Append(random.Next(0, maxVersion));

        var prereleaseIdentifiers = random.Next(0, maxPrereleaseIdentifiers);
        if (prereleaseIdentifiers > 0) builder.Append('-');
        for (int i = 0; i < prereleaseIdentifiers; i++)
        {
            if (i != 0) builder.Append('.');
            builder.Append(random.PrereleaseIdentifier(maxPrereleaseIdentifierLength));
        }

        var metadataIdentifiers = random.Next(0, maxMetadataIdentifiers);
        if (metadataIdentifiers > 0) builder.Append('+');
        for (int i = 0; i < metadataIdentifiers; i++)
        {
            if (i != 0) builder.Append('.');
            builder.Append(random.MetadataIdentifier(maxMetadataIdentifierLength));
        }

        return builder.ToString();
    }

    public static char Alphanumeric(this Random random)
    {
        return Alphanumerics[random.Next(0, Alphanumerics.Length)];
    }

    public static char Alpha(this Random random)
    {
        return Alphanumerics[random.Next(10, Alphanumerics.Length)];
    }

    public static bool NextBool(this Random random)
    {
        // Next() returns an int in the range 0 to Int32.MaxValue
        return random.Next() > (int.MaxValue / 2);
    }

    public static string RandomPartialVersion(
        this Random random,
        bool prependOperator,
        int maxMajor = 100,
        int maxMinor = 100,
        int maxPatch = 100,
        int maxPrereleaseIdentifiers = 1,
        int maxMetadataIdentifiers = 1
    )
    {
        var builder = new StringBuilder();

        if (prependOperator && random.NextBool())
        {
            // Prepend ^ or ~
            builder.Append(random.NextBool() ? "^" : "~");
        }

        builder.Append(random.Next(0, maxMajor));

        if (random.NextBool())
        {
            builder.Append(".");
            builder.Append(random.Next(0, maxMinor));
        }

        if (random.NextBool())
        {
            builder.Append(".");
            builder.Append(random.Next(0, maxPatch));
        }

        if (random.NextBool())
        {
            builder.Append("-");
            builder.Append(random.PrereleaseIdentifier(maxPrereleaseIdentifiers));
        }

        if (random.NextBool())
        {
            builder.Append("+");
            builder.Append(random.MetadataIdentifier(maxMetadataIdentifiers));
        }

        return builder.ToString();
    }
}
