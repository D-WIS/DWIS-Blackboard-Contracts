namespace DWIS.Blackboard.Configuration;

/// <summary>
/// What a host hands to <see cref="DwisConfiguration"/> to build its five layers
/// (specification section 8). Every property has a default that suits a container.
/// </summary>
public sealed class DwisConfigurationOptions
{
    /// <summary>Layer 1: defaults in code, as configuration keys (<c>Section:Key</c>) and values.</summary>
    public IDictionary<string, string?> Defaults { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Layer 2: the folder that holds <c>appsettings.json</c> and <c>appsettings.{Environment}.json</c>.
    /// Default: the folder of the running application.
    /// </summary>
    public string BasePath { get; set; } = AppContext.BaseDirectory;

    /// <summary>Layer 2: the environment name for <c>appsettings.{Environment}.json</c>. Default: <c>Production</c>.</summary>
    public string EnvironmentName { get; set; } = "Production";

    /// <summary>
    /// Layer 3: the path of the shared configuration file. When null, the environment variable
    /// <see cref="DwisConfiguration.ConfigFileVariable"/> is used, then <see cref="DwisConfiguration.DefaultConfigFile"/>.
    /// The file is optional and read once at start-up.
    /// </summary>
    public string? SharedFilePath { get; set; }

    /// <summary>Layer 5: the command-line arguments of the process.</summary>
    public IReadOnlyList<string> CommandLineArguments { get; set; } = [];

    /// <summary>
    /// Layer 5: short switches accepted for compatibility with today's Blackboard, mapped to
    /// configuration keys. Starts with <see cref="LegacyCommandLine.DefaultSwitchMappings"/>.
    /// </summary>
    public IDictionary<string, string> CommandLineSwitchMappings { get; } =
        new Dictionary<string, string>(LegacyCommandLine.DefaultSwitchMappings, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Layer 5: switches that may appear without a value and then mean <c>true</c>
    /// (today's <c>--useHub</c>). Starts with <see cref="LegacyCommandLine.DefaultBooleanSwitches"/>.
    /// </summary>
    public ISet<string> CommandLineBooleanSwitches { get; } =
        new HashSet<string>(LegacyCommandLine.DefaultBooleanSwitches, StringComparer.OrdinalIgnoreCase);
}
