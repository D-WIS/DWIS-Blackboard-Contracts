using Microsoft.Extensions.Configuration;

namespace DWIS.Blackboard.Configuration.Tests;

/// <summary>T-CFG-2: the effective configuration is printed with sources and masked secrets.</summary>
public sealed class EffectiveConfigurationTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "dwis-cfg-" + Guid.NewGuid().ToString("N"));

    public EffectiveConfigurationTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Fact]
    public void Every_value_names_the_layer_it_came_from()
    {
        var shared = Path.Combine(_folder, "dwis.json");
        File.WriteAllText(Path.Combine(_folder, "appsettings.json"), """{ "Semantic": { "Address": "http://semantic:8080", "Timeout": "30" } }""");
        File.WriteAllText(shared, """{ "Semantic": { "Timeout": "60" }, "Store": { "Password": "hunter2" } }""");
        var options = new DwisConfigurationOptions
        {
            BasePath = _folder,
            SharedFilePath = shared,
            CommandLineArguments = ["--Semantic:Address=http://localhost:5080"],
        };
        options.Defaults["Semantic:Address"] = "http://unused";
        options.Defaults["Store:Url"] = "http://fuseki:3030/dwis";

        var entries = EffectiveConfiguration.Describe(DwisConfiguration.Build(options));

        entries.Should().ContainEquivalentOf(new ConfigurationEntry("Semantic:Address", "http://localhost:5080", "command line", false));
        entries.Should().ContainEquivalentOf(new ConfigurationEntry("Semantic:Timeout", "60", shared, false));
        entries.Should().ContainEquivalentOf(new ConfigurationEntry("Store:Url", "http://fuseki:3030/dwis", "defaults", false));
        entries.Should().ContainEquivalentOf(new ConfigurationEntry("Store:Password", EffectiveConfiguration.Mask, shared, true));
        entries.Select(e => e.Key).Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void The_rendered_report_shows_sources_and_never_the_secret()
    {
        var options = new DwisConfigurationOptions { BasePath = _folder, SharedFilePath = Path.Combine(_folder, "none.json") };
        options.Defaults["Hub:Url"] = "https://hub.example";
        options.Defaults["Hub:ApiKey"] = "s3cr3t";

        var text = EffectiveConfiguration.Render(DwisConfiguration.Build(options));

        text.Should().Contain("Hub:Url").And.Contain("https://hub.example").And.Contain("[defaults]");
        text.Should().Contain("Hub:ApiKey").And.Contain(EffectiveConfiguration.Mask).And.NotContain("s3cr3t");
    }

    [Theory]
    [InlineData("Store:Password", true)]
    [InlineData("Hub:ApiKey", true)]
    [InlineData("Tls:PrivateKey", true)]
    [InlineData("Auth:Token", true)]
    [InlineData("Tls:PrivateKeyFile", false)]
    [InlineData("Store:PasswordFile", false)]
    [InlineData("Semantic:Address", false)]
    [InlineData("Port", false)]
    public void Secret_detection_looks_at_the_last_segment(string key, bool secret)
    {
        EffectiveConfiguration.IsSecret(key).Should().Be(secret);
    }
}
