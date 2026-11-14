using System.Text.Json;
using System.Text.Json.Serialization;

namespace DWIS.Blackboard.Contracts.Manifests;

/// <summary>
/// Reads and writes manifests in the JSON shape of <c>DWIS.API.DTO</c> 2.x: property names as declared
/// (PascalCase), case-insensitive on input, nulls written, unknown properties ignored.
/// </summary>
public static class ManifestJson
{
    /// <summary>The serializer options; reuse them for streams and HTTP.</summary>
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>Parses a manifest. Throws <see cref="JsonException"/> on malformed JSON.</summary>
    public static Manifest Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<Manifest>(json, Options) ?? throw new JsonException("The manifest JSON is null.");
    }

    /// <summary>Parses a manifest from a stream.</summary>
    public static async Task<Manifest> ParseAsync(Stream json, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(json);
        return await JsonSerializer.DeserializeAsync<Manifest>(json, Options, cancellationToken).ConfigureAwait(false)
            ?? throw new JsonException("The manifest JSON is null.");
    }

    /// <summary>Writes a manifest, indented.</summary>
    public static string Serialize(Manifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        return JsonSerializer.Serialize(manifest, Options);
    }
}
