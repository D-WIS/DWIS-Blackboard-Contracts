using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.FileProviders;

namespace DWIS.Blackboard.Configuration;

/// <summary>One value of the effective configuration with where it came from.</summary>
/// <param name="Key">The configuration key, <c>Section:Key</c>.</param>
/// <param name="Value">The value as shown: masked when the key names a secret.</param>
/// <param name="Source">The layer that supplied the value.</param>
/// <param name="IsSecret">True when the value was masked.</param>
public sealed record ConfigurationEntry(string Key, string? Value, string Source, bool IsSecret);

/// <summary>
/// The effective configuration of a host with the source of each value, secrets masked,
/// printable at start-up (R-CFG-2).
/// </summary>
public static class EffectiveConfiguration
{
    /// <summary>What a masked secret prints as.</summary>
    public const string Mask = "***";

    /// <summary>
    /// Key name endings that mark a secret. Compared case-insensitively against the last segment of the key
    /// (<c>Store:Password</c>, <c>Hub:ApiKey</c>, <c>Tls:PrivateKey</c>). A segment that names a file or path is
    /// not a secret (<c>Tls:PrivateKeyFile</c> is the file that holds one).
    /// </summary>
    public static IReadOnlyList<string> SecretEndings { get; } =
        ["password", "secret", "token", "apikey", "privatekey", "connectionstring", "credential", "credentials"];

    /// <summary>Lists every leaf value with its source, keys sorted, secrets masked.</summary>
    public static IReadOnlyList<ConfigurationEntry> Describe(IConfigurationRoot configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var providers = configuration.Providers.ToList();
        var entries = new List<ConfigurationEntry>();

        foreach (var key in LeafKeys(configuration).Order(StringComparer.OrdinalIgnoreCase))
        {
            var source = "unknown";
            string? value = null;
            // The last provider that holds the key is the one whose value wins.
            foreach (var provider in providers)
            {
                if (provider.TryGet(key, out var candidate))
                {
                    source = Name(provider);
                    value = candidate;
                }
            }

            var secret = IsSecret(key);
            entries.Add(new ConfigurationEntry(key, secret && value is not null ? Mask : value, source, secret));
        }

        return entries;
    }

    /// <summary>The report as text, one line per value: <c>key = value  [source]</c>.</summary>
    public static string Render(IConfigurationRoot configuration)
    {
        var entries = Describe(configuration);
        var width = entries.Count == 0 ? 0 : entries.Max(e => e.Key.Length);
        var text = new StringBuilder();
        text.AppendLine(CultureInfo.InvariantCulture, $"Effective configuration ({entries.Count} values; secrets shown as {Mask}):");
        foreach (var entry in entries)
        {
            text.AppendLine(CultureInfo.InvariantCulture, $"  {entry.Key.PadRight(width)} = {entry.Value ?? "(null)"}  [{entry.Source}]");
        }

        return text.ToString();
    }

    /// <summary>True when the last segment of the key ends with one of <see cref="SecretEndings"/>.</summary>
    public static bool IsSecret(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var separator = key.LastIndexOf(ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
        var last = separator < 0 ? key : key[(separator + 1)..];
        return SecretEndings.Any(ending => last.EndsWith(ending, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> LeafKeys(IConfiguration configuration)
    {
        foreach (var child in configuration.GetChildren())
        {
            var grandChildren = child.GetChildren().ToList();
            if (grandChildren.Count == 0)
            {
                yield return child.Path;
            }
            else
            {
                foreach (var key in LeafKeys(child))
                {
                    yield return key;
                }
            }
        }
    }

    private static string Name(IConfigurationProvider provider) => provider switch
    {
        MemoryConfigurationProvider => "defaults",
        JsonConfigurationProvider json => JsonFilePath(json),
        EnvironmentVariablesConfigurationProvider => $"environment ({DwisConfiguration.EnvironmentPrefix})",
        CommandLineConfigurationProvider => "command line",
        _ => provider.ToString() ?? provider.GetType().Name,
    };

    // A rooted path given to AddJsonFile is split by the framework into a file provider and a file name;
    // put the two back together so the report names the actual file.
    private static string JsonFilePath(JsonConfigurationProvider json)
    {
        var path = json.Source.Path ?? "json";
        return json.Source.FileProvider is PhysicalFileProvider physical ? Path.Combine(physical.Root, path) : path;
    }
}
