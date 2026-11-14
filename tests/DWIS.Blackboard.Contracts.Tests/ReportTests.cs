using System.Text.Json;
using DWIS.Blackboard.Contracts.Reports;

namespace DWIS.Blackboard.Contracts.Tests;

/// <summary>The report of Appendix C.9: computed fields, JSON shape, legacy copy.</summary>
public sealed class ReportTests
{
    private static readonly Finding Kilogram = new()
    {
        Stage = Stage.Preprocessing,
        Severity = Severity.Error,
        Code = "shacl",
        Message = "Unit Kilogram is not a unit for PressureDrillingQuantity",
        Focus = "http://d-wis.org/SmartConnect/mainwellCirculationCommon_pressureStandPipe#01",
        Path = "http://d-wis.org/HasUnitOfMeasure",
        Value = "http://d-wis.org/UnitAndQuantity/Kilogram",
        Source = "http://d-wis.org/shapes/UnitMatchesQuantityShape",
    };

    private static readonly Finding Dropped = new() { Severity = Severity.Warning, Code = "injected-variable-dropped", Message = "Injected variable mirror1 was dropped." };

    private static readonly Finding Flag = new() { Severity = Severity.Info, Code = "validity-flag-added", Message = "Validity flag added." };

    [Fact]
    public void From_computes_acceptance_counts_order_and_live_parts()
    {
        var report = IngestionReport.From("SmartConnect/smartconnect", "2026-10-14", [Flag, Dropped, Kilogram]);

        report.Accepted.Should().BeFalse();
        report.Counts.Should().Be(new FindingCounts(1, 1, 1));
        report.Findings.Select(f => f.Severity).Should().Equal(Severity.Error, Severity.Warning, Severity.Info);
        report.Live.Should().Be(new LiveParts(true, false));

        var clean = IngestionReport.From("SmartConnect/smartconnect", "2026-10-14", [Flag, Dropped]);
        clean.Accepted.Should().BeTrue();
        clean.Live.Should().Be(new LiveParts(true, true));
    }

    [Fact]
    public void The_json_has_the_shape_of_C9()
    {
        var report = IngestionReport.From("SmartConnect/smartconnect", "2026-10-14", [Kilogram], new IngestionStatistics { Asserted = 3124, PreprocessingMs = 120 });

        var json = ReportJson.Serialize(report);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        root.GetProperty("manifestId").GetString().Should().Be("SmartConnect/smartconnect");
        root.GetProperty("accepted").GetBoolean().Should().BeFalse();
        root.GetProperty("counts").GetProperty("errors").GetInt32().Should().Be(1);
        var finding = root.GetProperty("findings")[0];
        finding.GetProperty("stage").GetString().Should().Be("preprocessing");
        finding.GetProperty("severity").GetString().Should().Be("error");
        finding.GetProperty("source").GetString().Should().Be("http://d-wis.org/shapes/UnitMatchesQuantityShape");
        root.GetProperty("live").GetProperty("core").GetBoolean().Should().BeTrue();
        root.GetProperty("statistics").GetProperty("asserted").GetInt32().Should().Be(3124);
        json.Should().NotContain("\"code\": null", "absent fields are left out");
    }

    [Fact]
    public void A_report_survives_a_round_trip()
    {
        var report = IngestionReport.From("p/m", null, [Kilogram, Dropped, Flag]);

        ReportJson.Parse(ReportJson.Serialize(report)).Should().BeEquivalentTo(report);
    }

    [Fact]
    public void The_legacy_copy_translates_every_iri_and_nothing_else()
    {
        var legacy = IngestionReport.From("p/m", null, [Kilogram]).ToLegacy().Findings.Single();

        legacy.Focus.Should().Be("http://ddhub.no/SmartConnect/mainwellCirculationCommon_pressureStandPipe#01");
        legacy.Path.Should().Be("http://ddhub.no/HasUnitOfMeasure");
        legacy.Value.Should().Be("http://ddhub.no/UnitAndQuantity/Kilogram");
        legacy.Source.Should().Be("http://ddhub.no/shapes/UnitMatchesQuantityShape");
        legacy.Message.Should().Be(Kilogram.Message);
        Dropped.ToLegacy().Should().Be(Dropped);
    }
}
