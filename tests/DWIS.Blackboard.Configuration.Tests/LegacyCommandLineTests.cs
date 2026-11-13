using Microsoft.Extensions.Configuration;

namespace DWIS.Blackboard.Configuration.Tests;

/// <summary>Today's command-line names keep working (specification section 8, item 5).</summary>
public sealed class LegacyCommandLineTests
{
    private static DwisConfigurationOptions Options(params string[] arguments) => new()
    {
        BasePath = Path.GetTempPath(),
        SharedFilePath = Path.Combine(Path.GetTempPath(), "dwis-cfg-absent-" + Guid.NewGuid().ToString("N") + ".json"),
        CommandLineArguments = arguments,
    };

    [Fact]
    public void Todays_switches_map_to_configuration_keys()
    {
        var configuration = DwisConfiguration.Build(Options("--useHub", "--hubURL", "https://hub.example/applications", "--hubGroup", "rig-1", "--port", "48030"));

        configuration["Hub:Enabled"].Should().Be("true");
        configuration["Hub:Url"].Should().Be("https://hub.example/applications");
        configuration["Hub:Group"].Should().Be("rig-1");
        configuration["RealTime:Port"].Should().Be("48030");
    }

    [Fact]
    public void A_bare_boolean_switch_at_the_end_means_true()
    {
        DwisConfiguration.Build(Options("--port", "1", "--useHub"))["Hub:Enabled"].Should().Be("true");
    }

    [Fact]
    public void A_boolean_switch_with_an_explicit_value_keeps_it()
    {
        DwisConfiguration.Build(Options("--useHub", "false"))["Hub:Enabled"].Should().Be("false");
    }

    [Fact]
    public void The_general_form_works_next_to_the_short_names()
    {
        var configuration = DwisConfiguration.Build(Options("--Semantic:Address=http://localhost:5080", "--port", "48030"));

        configuration["Semantic:Address"].Should().Be("http://localhost:5080");
        configuration["RealTime:Port"].Should().Be("48030");
    }

    [Fact]
    public void Normalise_leaves_other_arguments_untouched()
    {
        var normalised = LegacyCommandLine.Normalise(["--a", "1", "--useHub", "--b=2"], new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "--useHub" });

        normalised.Should().Equal("--a", "1", "--useHub", "true", "--b=2");
    }
}
