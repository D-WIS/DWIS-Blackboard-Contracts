using Microsoft.Extensions.Configuration;

namespace DWIS.Blackboard.Configuration;

/// <summary>
/// Builds the configuration of a Rig Edition host from five layers, lowest precedence first
/// (specification section 8, R-CFG-1):
/// defaults in code; <c>appsettings.json</c> plus <c>appsettings.{Environment}.json</c>;
/// a shared file named by <c>DWIS_CONFIG_FILE</c>; environment variables with the <c>DWIS__</c> prefix;
/// command-line arguments.
/// </summary>
public static class DwisConfiguration
{
    /// <summary>Prefix of the environment variables that are read; <c>__</c> separates sections.</summary>
    public const string EnvironmentPrefix = "DWIS__";

    /// <summary>Environment variable that names the shared configuration file.</summary>
    public const string ConfigFileVariable = "DWIS_CONFIG_FILE";

    /// <summary>Path of the shared configuration file when <see cref="ConfigFileVariable"/> is not set.</summary>
    public const string DefaultConfigFile = "/config/dwis.json";

    /// <summary>Builds a configuration from the five layers alone.</summary>
    public static IConfigurationRoot Build(DwisConfigurationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new ConfigurationBuilder().AddDwisLayers(options).Build();
    }

    /// <summary>Adds the five layers, in order, to an existing builder (for example a host's).</summary>
    public static IConfigurationBuilder AddDwisLayers(this IConfigurationBuilder builder, DwisConfigurationOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        // 1. Defaults in code.
        builder.AddInMemoryCollection(options.Defaults);

        // 2. Files shipped in the image.
        builder.SetBasePath(options.BasePath);
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        builder.AddJsonFile($"appsettings.{options.EnvironmentName}.json", optional: true, reloadOnChange: false);

        // 3. The shared file mounted into the container, read once.
        builder.AddJsonFile(ResolveSharedFilePath(options), optional: true, reloadOnChange: false);

        // 4. Environment variables DWIS__Section__Key.
        builder.AddEnvironmentVariables(EnvironmentPrefix);

        // 5. Command line, with today's short names accepted.
        var arguments = LegacyCommandLine.Normalise(options.CommandLineArguments, options.CommandLineBooleanSwitches);
        builder.AddCommandLine([.. arguments], options.CommandLineSwitchMappings);

        return builder;
    }

    /// <summary>The shared file path a host will read: the option, else the environment variable, else the default.</summary>
    public static string ResolveSharedFilePath(DwisConfigurationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!string.IsNullOrWhiteSpace(options.SharedFilePath))
        {
            return options.SharedFilePath;
        }

        var fromEnvironment = Environment.GetEnvironmentVariable(ConfigFileVariable);
        return string.IsNullOrWhiteSpace(fromEnvironment) ? DefaultConfigFile : fromEnvironment;
    }
}
