namespace DWIS.Blackboard.Contracts.Tests;

/// <summary>Table test of the namespace translation (specification 3.6): both prefixes, and IRIs that must not be touched.</summary>
public sealed class NamespacesTests
{
    [Theory]
    [InlineData("http://ddhub.no/HasDynamicValue", "http://d-wis.org/HasDynamicValue")]
    [InlineData("http://ddhub.no/UnitAndQuantity/CubicMetre", "http://d-wis.org/UnitAndQuantity/CubicMetre")]
    [InlineData("http://ddhub.no/SmartConnect/Variables/SmartConnect.spp", "http://d-wis.org/SmartConnect/Variables/SmartConnect.spp")]
    [InlineData("http://ddhub.no/", "http://d-wis.org/")]
    [InlineData("http://d-wis.org/HasDynamicValue", "http://d-wis.org/HasDynamicValue")]
    [InlineData("http://ddhub.no", "http://ddhub.no")]
    [InlineData("https://ddhub.no/Thing", "https://ddhub.no/Thing")]
    [InlineData("http://ddhub.no.example.com/Thing", "http://ddhub.no.example.com/Thing")]
    [InlineData("http://www.w3.org/1999/02/22-rdf-syntax-ns#type", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type")]
    [InlineData("Nodes", "Nodes")]
    [InlineData("", "")]
    public void To_canonical(string input, string expected)
    {
        Namespaces.ToCanonical(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("http://d-wis.org/HasDynamicValue", "http://ddhub.no/HasDynamicValue")]
    [InlineData("http://d-wis.org/SmartConnect/spp#01", "http://ddhub.no/SmartConnect/spp#01")]
    [InlineData("http://ddhub.no/HasDynamicValue", "http://ddhub.no/HasDynamicValue")]
    [InlineData("http://d-wis.org", "http://d-wis.org")]
    [InlineData("http://www.w3.org/2002/07/owl#Class", "http://www.w3.org/2002/07/owl#Class")]
    public void To_legacy(string input, string expected)
    {
        Namespaces.ToLegacy(input).Should().Be(expected);
    }

    [Fact]
    public void Translation_is_case_sensitive_and_prefix_only()
    {
        Namespaces.ToCanonical("HTTP://DDHUB.NO/Thing").Should().Be("HTTP://DDHUB.NO/Thing");
        Namespaces.ToCanonical("see http://ddhub.no/Thing").Should().Be("see http://ddhub.no/Thing");
        Namespaces.IsLegacy("http://ddhub.no/x").Should().BeTrue();
        Namespaces.IsCanonical("http://ddhub.no/x").Should().BeFalse();
    }

    [Fact]
    public void A_round_trip_restores_the_input()
    {
        const string iri = "http://ddhub.no/SmartConnect/Variables/SmartConnect.spp";

        Namespaces.ToLegacy(Namespaces.ToCanonical(iri)).Should().Be(iri);
    }
}
