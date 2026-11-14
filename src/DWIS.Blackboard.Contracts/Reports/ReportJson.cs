using System.Text.Json;
using System.Text.Json.Serialization;

namespace DWIS.Blackboard.Contracts.Reports;

/// <summary>
/// Reads and writes reports in the JSON of Appendix C.9: camelCase names, enums as lowercase words
/// (<c>"error"</c>, <c>"preprocessing"</c>), absent fields left out.
/// </summary>
public static class ReportJson
{
    /// <summary>The serializer options; reuse them for HTTP.</summary>
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>Parses a report.</summary>
    public static IngestionReport Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<IngestionReport>(json, Options) ?? throw new JsonException("The report JSON is null.");
    }

    /// <summary>Writes a report, indented.</summary>
    public static string Serialize(IngestionReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        return JsonSerializer.Serialize(report, Options);
    }
}
