using Microsoft.Extensions.Configuration;

namespace DWIS.Blackboard.Configuration.Tests;

/// <summary>T-CFG-1: each of the five layers overrides the previous one.</summary>
public sealed class PrecedenceTests : IDisposable
{
    private const string Key = "Probe:Value";
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "dwis-cfg-" + Guid.NewGuid().ToString("N"));
    private readonly string _environmentVariable = "DWIS__Probe__Value";

    public PrecedenceTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(_environmentVariable, null);
        Directory.Delete(_folder, recursive: true);
    }

    [Theory]
    [InlineData(1, "defaults")]
    [InlineData(2, "appsettings")]
    [InlineData(3, "shared file")]
    [InlineData(4, "environment")]
    [InlineData(5, "command line")]
    public void The_highest_present_layer_wins(int layers, string expected)
    {
        var options = new DwisConfigurationOptions { BasePath = _folder, SharedFilePath = Path.Combine(_folder, "dwis.json") };
        options.Defaults[Key] = "defaults";
        if (layers >= 2)
        {
            File.WriteAllText(Path.Combine(_folder, "appsettings.json"), """{ "Probe": { "Value": "appsettings" } }""");
        }

        if (layers >= 3)
        {
            File.WriteAllText(options.SharedFilePath, """{ "Probe": { "Value": "shared file" } }""");
        }

        if (layers >= 4)
        {
            Environment.SetEnvironmentVariable(_environmentVariable, "environment");
        }

        if (layers >= 5)
        {
            options.CommandLineArguments = ["--Probe:Value=command line"];
        }

        var configuration = DwisConfiguration.Build(options);

        configuration[Key].Should().Be(expected);
    }

    [Fact]
    public void The_environment_file_overrides_appsettings_json()
    {
        File.WriteAllText(Path.Combine(_folder, "appsettings.json"), """{ "Probe": { "Value": "base" } }""");
        File.WriteAllText(Path.Combine(_folder, "appsettings.Lab.json"), """{ "Probe": { "Value": "lab" } }""");
        var options = new DwisConfigurationOptions { BasePath = _folder, EnvironmentName = "Lab", SharedFilePath = Path.Combine(_folder, "none.json") };

        DwisConfiguration.Build(options)[Key].Should().Be("lab");
    }

    [Fact]
    public void A_missing_shared_file_is_not_an_error()
    {
        var options = new DwisConfigurationOptions { BasePath = _folder, SharedFilePath = Path.Combine(_folder, "absent.json") };
        options.Defaults[Key] = "defaults";

        DwisConfiguration.Build(options)[Key].Should().Be("defaults");
    }

    [Fact]
    public void The_shared_file_path_comes_from_the_option_then_the_variable_then_the_default()
    {
        var variable = DwisConfiguration.ConfigFileVariable;
        var before = Environment.GetEnvironmentVariable(variable);
        try
        {
            Environment.SetEnvironmentVariable(variable, null);
            DwisConfiguration.ResolveSharedFilePath(new DwisConfigurationOptions()).Should().Be(DwisConfiguration.DefaultConfigFile);

            Environment.SetEnvironmentVariable(variable, "/mnt/other.json");
            DwisConfiguration.ResolveSharedFilePath(new DwisConfigurationOptions()).Should().Be("/mnt/other.json");

            DwisConfiguration.ResolveSharedFilePath(new DwisConfigurationOptions { SharedFilePath = "/opt/explicit.json" }).Should().Be("/opt/explicit.json");
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, before);
        }
    }
}
