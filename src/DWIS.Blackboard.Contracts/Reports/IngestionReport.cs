namespace DWIS.Blackboard.Contracts.Reports;

/// <summary>How serious a finding is.</summary>
public enum Severity
{
    /// <summary>Worth knowing, nothing to fix.</summary>
    Info,

    /// <summary>Accepted, but something was dropped or guessed.</summary>
    Warning,

    /// <summary>Not accepted: the manifest goes live as core only (R-SEM-5).</summary>
    Error,
}

/// <summary>Where a finding comes from (Appendix C.9).</summary>
public enum Stage
{
    /// <summary>The pre-processing pass on manifest plus vocabulary (Appendix C.4).</summary>
    Preprocessing,

    /// <summary>The commit, with the final reasoning on the store (Appendix C.6).</summary>
    Commit,

    /// <summary>The real-time process (OPC UA node creation).</summary>
    RealTime,
}

/// <summary>
/// One thing the provider must know about its manifest. IRIs are canonical (<c>d-wis.org</c>);
/// <see cref="ToLegacy"/> gives the copy for the OPC UA side.
/// </summary>
public sealed record Finding
{
    /// <summary>Where it comes from.</summary>
    public Stage Stage { get; init; } = Stage.Preprocessing;

    /// <summary>How serious it is.</summary>
    public Severity Severity { get; init; } = Severity.Error;

    /// <summary>A sentence for people.</summary>
    public string Message { get; init; } = "";

    /// <summary>A stable code, kebab-case, for tests and tooling (<c>unknown-class</c>, <c>shacl</c>, ...).</summary>
    public string? Code { get; init; }

    /// <summary>The node the finding is about (SHACL focus node), when there is one.</summary>
    public string? Focus { get; init; }

    /// <summary>The property concerned, when there is one.</summary>
    public string? Path { get; init; }

    /// <summary>The offending value, when there is one.</summary>
    public string? Value { get; init; }

    /// <summary>The shape, rule or check that produced the finding.</summary>
    public string? Source { get; init; }

    /// <summary>The same finding with its IRIs in the legacy namespace, for the OPC UA side.</summary>
    public Finding ToLegacy() => this with
    {
        Focus = Focus is null ? null : Namespaces.ToLegacy(Focus),
        Path = Path is null ? null : Namespaces.ToLegacy(Path),
        Value = Value is null ? null : Namespaces.ToLegacy(Value),
        Source = Source is null ? null : Namespaces.ToLegacy(Source),
    };
}

/// <summary>How many findings of each severity.</summary>
/// <param name="Errors">Errors.</param>
/// <param name="Warnings">Warnings.</param>
/// <param name="Infos">Infos.</param>
public sealed record FindingCounts(int Errors, int Warnings, int Infos)
{
    /// <summary>Counts a list of findings.</summary>
    public static FindingCounts Of(IEnumerable<Finding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        int errors = 0, warnings = 0, infos = 0;
        foreach (var finding in findings)
        {
            switch (finding.Severity)
            {
                case Severity.Error: errors++; break;
                case Severity.Warning: warnings++; break;
                default: infos++; break;
            }
        }

        return new FindingCounts(errors, warnings, infos);
    }
}

/// <summary>Which parts of the manifest are live in the store (R-SEM-5).</summary>
/// <param name="Core">The provider, the provided variables, their validity flags and their data points.</param>
/// <param name="Rest">Everything else; false when the manifest is in quarantine.</param>
public sealed record LiveParts(bool Core, bool Rest);

/// <summary>Sizes and timings of an injection.</summary>
public sealed record IngestionStatistics
{
    /// <summary>Triples in the asserted graph.</summary>
    public int Asserted { get; init; }

    /// <summary>Triples in the inferred graph.</summary>
    public int Inferred { get; init; }

    /// <summary>Milliseconds spent in the pre-processing pass.</summary>
    public long PreprocessingMs { get; init; }

    /// <summary>Milliseconds spent in the commit.</summary>
    public long CommitMs { get; init; }
}

/// <summary>The report of an injection (Appendix C.9), the same shape on every path.</summary>
public sealed record IngestionReport
{
    /// <summary><c>{provider}/{manifest name}</c>.</summary>
    public string ManifestId { get; init; } = "";

    /// <summary>True when the whole manifest is live; false when only the core is (R-SEM-5).</summary>
    public bool Accepted { get; init; }

    /// <summary>The vocabulary version the manifest was checked against.</summary>
    public string? VocabularyVersion { get; init; }

    /// <summary>Findings per severity.</summary>
    public FindingCounts Counts { get; init; } = new(0, 0, 0);

    /// <summary>The findings, errors first.</summary>
    public IReadOnlyList<Finding> Findings { get; init; } = [];

    /// <summary>What is live.</summary>
    public LiveParts Live { get; init; } = new(true, true);

    /// <summary>Sizes and timings.</summary>
    public IngestionStatistics Statistics { get; init; } = new();

    /// <summary>
    /// Builds a report from findings: accepted when there is no error, counts computed, findings sorted
    /// errors first, <see cref="Live"/> set to core-only when not accepted.
    /// </summary>
    public static IngestionReport From(string manifestId, string? vocabularyVersion, IEnumerable<Finding> findings, IngestionStatistics? statistics = null)
    {
        ArgumentNullException.ThrowIfNull(manifestId);
        ArgumentNullException.ThrowIfNull(findings);
        var sorted = findings.OrderByDescending(f => f.Severity).ToList();
        var counts = FindingCounts.Of(sorted);
        var accepted = counts.Errors == 0;
        return new IngestionReport
        {
            ManifestId = manifestId,
            Accepted = accepted,
            VocabularyVersion = vocabularyVersion,
            Counts = counts,
            Findings = sorted,
            Live = new LiveParts(true, accepted),
            Statistics = statistics ?? new IngestionStatistics(),
        };
    }

    /// <summary>The same report with every IRI in the legacy namespace, for the OPC UA side.</summary>
    public IngestionReport ToLegacy() => this with { Findings = Findings.Select(f => f.ToLegacy()).ToList() };
}
