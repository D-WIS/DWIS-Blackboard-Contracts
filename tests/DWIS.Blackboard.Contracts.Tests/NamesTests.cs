namespace DWIS.Blackboard.Contracts.Tests;

/// <summary>Table test of the name rule of decision D-62: the characters an IRI cannot carry become underscores, nothing else changes.</summary>
public sealed class NamesTests
{
    [Theory]
    [InlineData("DDHub CLI", "DDHub_CLI")]
    [InlineData("SmartConnect", "SmartConnect")]
    [InlineData("PumpStartupManifestPWDa:", "PumpStartupManifestPWDa:")]
    [InlineData("mainwellCirculationCommon_pressureStandPipe#01", "mainwellCirculationCommon_pressureStandPipe#01")]
    [InlineData("a<b>|c", "a_b__c")]
    [InlineData("tab\tand\nnewline", "tab_and_newline")]
    [InlineData("quote\"back\\slash^caret`tick{brace}", "quote_back_slash_caret_tick_brace_")]
    [InlineData("  two  blanks  ", "__two__blanks__")]
    [InlineData("Ñandú.Ωmega", "Ñandú.Ωmega")]
    [InlineData("percent%20kept?query=1", "percent%20kept?query=1")]
    [InlineData("", "")]
    public void Iri_safe(string input, string expected)
    {
        Names.IriSafe(input).Should().Be(expected);
        Names.IsIriSafe(input).Should().Be(input == expected);
        Names.IriSafe(expected).Should().Be(expected, "the rule is idempotent");
    }

    [Fact]
    public void A_safe_name_comes_back_as_the_same_instance()
    {
        const string name = "WellARig";
        ReferenceEquals(Names.IriSafe(name), name).Should().BeTrue();
    }
}
