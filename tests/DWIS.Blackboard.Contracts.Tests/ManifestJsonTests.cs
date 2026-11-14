using System.Text.Json;
using DWIS.Blackboard.Contracts.Manifests;

namespace DWIS.Blackboard.Contracts.Tests;

/// <summary>The records read and write the JSON shape of DWIS.API.DTO 2.x (decision D-43).</summary>
public sealed class ManifestJsonTests
{
    // Copy of DWIS-Blackboard-Tests/fixtures/manifests/smartconnect.manifest.json (DWIS-SmartConnect d559ea6).
    private static readonly string FixturePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "smartconnect.manifest.json");

    [Fact]
    public void The_SmartConnect_manifest_parses_with_the_known_counts()
    {
        var manifest = ManifestJson.Parse(File.ReadAllText(FixturePath));

        manifest.ManifestName.Should().Be("SmartConnect rig signals");
        manifest.Provider.Should().Be(new Provider { Name = "SmartConnect", Company = "Equinor" });
        manifest.InjectionInformation.InjectedNodesNamespaceAlias.Should().Be("Nodes");
        manifest.InjectionInformation.ProvidedVariablesNamespaceAlias.Should().Be("ProvidedVariables");
        manifest.InjectionInformation.ServerName.Should().BeNull();
        manifest.InjectedVariables.Should().BeEmpty();
        manifest.InjectedNodes.Should().HaveCount(391);
        manifest.InjectedReferences.Should().HaveCount(1537);
        manifest.ProvidedVariables.Should().HaveCount(254);
        manifest.InjectedVocabulary.Should().BeNull();
    }

    [Fact]
    public void Nodes_references_and_variables_keep_their_values()
    {
        var manifest = ManifestJson.Parse(File.ReadAllText(FixturePath));

        manifest.InjectedNodes[0].Should().Be(new InjectedNode
        {
            TypeDictionaryURI = "Measurement",
            BrowseName = "circulationBaseOil10s_volume#01",
            UniqueName = "circulationBaseOil10s_volume#01",
            DisplayName = "circulationBaseOil10s_volume#01",
            Fields = null,
        });
        manifest.InjectedNodes.Single(n => n.UniqueName == "wobMaxWindow").Fields.Should()
            .BeEquivalentTo([new Field { FieldName = "Duration", FieldValue = "180.0" }]);
        manifest.InjectedReferences[0].Should().Be(new InjectedReference
        {
            Subject = new NodeIdentifier { NameSpace = "Nodes", ID = "circulationBaseOil10s_volume#01" },
            VerbURI = "http://ddhub.no/BelongsToClass",
            Object = new NodeIdentifier { NameSpace = "http://ddhub.no/", ID = "http://ddhub.no/ContinuousDataType" },
        });
        manifest.ProvidedVariables[0].Should().BeEquivalentTo(new ProvidedVariable
        {
            VariableID = "circulationBaseOil10s_volume",
            Rank = 0,
            Dimensions = null,
            DataType = "double",
            InitialValue = "",
        });
    }

    [Fact]
    public void Serialising_and_parsing_again_gives_the_same_manifest()
    {
        var original = ManifestJson.Parse(File.ReadAllText(FixturePath));

        var again = ManifestJson.Parse(ManifestJson.Serialize(original));

        again.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void The_written_json_keeps_the_old_property_names_and_nulls()
    {
        var json = ManifestJson.Serialize(new Manifest { ManifestName = "m", Provider = new Provider { Name = "p", Company = "c" } });

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        root.GetProperty("ManifestName").GetString().Should().Be("m");
        root.GetProperty("Provider").GetProperty("Company").GetString().Should().Be("c");
        root.GetProperty("InjectionInformation").GetProperty("ServerName").ValueKind.Should().Be(JsonValueKind.Null);
        root.GetProperty("InjectionInformation").GetProperty("InjectedNodesNamespaceAlias").GetString().Should().Be("Nodes");
        root.GetProperty("InjectedVariables").GetArrayLength().Should().Be(0);
        root.GetProperty("InjectedVocabulary").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void Missing_lists_read_as_empty_and_unknown_properties_are_ignored()
    {
        var manifest = ManifestJson.Parse("""{ "manifestName": "x", "Provider": { "Name": "p" }, "InjectedNodes": null, "Extra": 1 }""");

        manifest.ManifestName.Should().Be("x");
        manifest.Provider.Company.Should().BeEmpty();
        manifest.InjectedNodes.Should().BeEmpty();
        manifest.InjectedReferences.Should().BeEmpty();
        manifest.ProvidedVariables.Should().BeEmpty();
    }

    [Fact]
    public void Malformed_json_throws_a_json_exception()
    {
        var act = () => ManifestJson.Parse("{ not json");

        act.Should().Throw<JsonException>();
    }
}
