namespace DWIS.Blackboard.Configuration;

/// <summary>
/// Keeps today's command-line names working (<c>--port</c>, <c>--useHub</c>, <c>--hubURL</c>, <c>--hubGroup</c>)
/// next to the general <c>--Section:Key=value</c> form of the .NET command-line provider.
/// </summary>
public static class LegacyCommandLine
{
    /// <summary>Today's switches and the configuration keys they set.</summary>
    public static IReadOnlyDictionary<string, string> DefaultSwitchMappings { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["--port"] = "RealTime:Port",
            ["--useHub"] = "Hub:Enabled",
            ["--hubURL"] = "Hub:Url",
            ["--hubGroup"] = "Hub:Group",
        };

    /// <summary>Switches that today are given without a value and mean <c>true</c>.</summary>
    public static IReadOnlySet<string> DefaultBooleanSwitches { get; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "--useHub" };

    /// <summary>
    /// Gives every bare boolean switch an explicit <c>true</c>, so that <c>--useHub --hubURL x</c>
    /// does not read <c>--hubURL</c> as the value of <c>--useHub</c>. Other arguments pass through unchanged.
    /// </summary>
    public static IReadOnlyList<string> Normalise(IReadOnlyList<string> arguments, ISet<string> booleanSwitches)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(booleanSwitches);

        var result = new List<string>(arguments.Count + 2);
        for (var i = 0; i < arguments.Count; i++)
        {
            var argument = arguments[i];
            result.Add(argument);

            if (!booleanSwitches.Contains(argument))
            {
                continue;
            }

            var next = i + 1 < arguments.Count ? arguments[i + 1] : null;
            var hasValue = next is not null && !next.StartsWith('-') && !next.StartsWith('/');
            if (!hasValue)
            {
                result.Add("true");
            }
        }

        return result;
    }
}
