using System.Text.Json;

namespace DWIS.Blackboard.Contracts.Manifests;

/// <summary>
/// A manifest as a provider sends it: the JSON shape of <c>DWIS.API.DTO.ManifestFile</c> 2.x
/// (property names and nesting unchanged, decision D-43), as immutable records.
/// Lists are never null; a missing or null list in the JSON reads as empty.
/// </summary>
public sealed record Manifest
{
    /// <summary>Free name of the manifest.</summary>
    public string ManifestName { get; init; } = "";

    /// <summary>Who provides the data.</summary>
    public Provider Provider { get; init; } = new();

    /// <summary>Namespace aliases and OPC UA settings used by the references.</summary>
    public InjectionInformation InjectionInformation { get; init; } = new();

    private readonly IReadOnlyList<InjectedVariable> _injectedVariables = [];
    private readonly IReadOnlyList<InjectedNode> _injectedNodes = [];
    private readonly IReadOnlyList<InjectedReference> _injectedReferences = [];
    private readonly IReadOnlyList<ProvidedVariable> _providedVariables = [];

    /// <summary>Variables mirrored from another OPC UA server. Dropped by the Rig Edition with a warning (decision D-26).</summary>
    public IReadOnlyList<InjectedVariable> InjectedVariables { get => _injectedVariables; init => _injectedVariables = value ?? []; }

    /// <summary>The semantic nodes of the manifest.</summary>
    public IReadOnlyList<InjectedNode> InjectedNodes { get => _injectedNodes; init => _injectedNodes = value ?? []; }

    /// <summary>The references (subject, verb, object) among nodes, variables and vocabulary terms.</summary>
    public IReadOnlyList<InjectedReference> InjectedReferences { get => _injectedReferences; init => _injectedReferences = value ?? []; }

    /// <summary>The variables the provider will write.</summary>
    public IReadOnlyList<ProvidedVariable> ProvidedVariables { get => _providedVariables; init => _providedVariables = value ?? []; }

    /// <summary>
    /// A vocabulary extension carried by the manifest (the old <c>DWIS.Vocabulary.Development.Vocabulary</c>).
    /// Kept as raw JSON; the vocabulary extension is a later project (decision D-27).
    /// </summary>
    public JsonElement? InjectedVocabulary { get; init; }
}

/// <summary>The data provider.</summary>
public sealed record Provider
{
    /// <summary>Provider name; also the first segment of every IRI the manifest creates.</summary>
    public string Name { get; init; } = "";

    /// <summary>Company behind the provider.</summary>
    public string Company { get; init; } = "";
}

/// <summary>Settings that tell how the references name things.</summary>
public sealed record InjectionInformation
{
    /// <summary>OPC UA server name of the provider, when it mirrors variables (unused by the Rig Edition).</summary>
    public string? ServerName { get; init; }

    /// <summary>OPC UA endpoint of the provider, when it mirrors variables (unused by the Rig Edition).</summary>
    public string? EndPointURL { get; init; }

    /// <summary>Publishing interval for mirrored variables (unused by the Rig Edition).</summary>
    public double PublishingIntervalInMS { get; init; }

    /// <summary>Namespace alias that a reference uses to name an injected variable.</summary>
    public string InjectedVariablesNamespaceAlias { get; init; } = "Variables";

    /// <summary>Namespace alias that a reference uses to name an injected node.</summary>
    public string InjectedNodesNamespaceAlias { get; init; } = "Nodes";

    /// <summary>Namespace alias that a reference uses to name a provided variable.</summary>
    public string ProvidedVariablesNamespaceAlias { get; init; } = "ProvidedVariables";

    /// <summary>Namespace alias that a reference uses to name a term of the injected vocabulary.</summary>
    public string InjectedVocabularyNamespaceAlias { get; init; } = "Vocabulary";
}

/// <summary>A variable the provider writes to the Blackboard.</summary>
public sealed record ProvidedVariable
{
    /// <summary>Identifier, unique within the provider.</summary>
    public string VariableID { get; init; } = "";

    /// <summary>OPC UA value rank: 0 scalar, 1 array, and so on.</summary>
    public int Rank { get; init; }

    /// <summary>Array dimensions when the rank is above 0.</summary>
    public int[]? Dimensions { get; init; }

    /// <summary>Data type name as the manifest gives it (<c>double</c>, <c>bool</c>, ...).</summary>
    public string DataType { get; init; } = "";

    /// <summary>Initial value as text.</summary>
    public string InitialValue { get; init; } = "";
}

/// <summary>A semantic node of the manifest.</summary>
public sealed record InjectedNode
{
    /// <summary>The vocabulary class of the node: a bare name (<c>Measurement</c>) or a full IRI.</summary>
    public string TypeDictionaryURI { get; init; } = "";

    /// <summary>OPC UA browse name (kept for the old address space; unused by the Rig Edition).</summary>
    public string BrowseName { get; init; } = "";

    /// <summary>Identifier, unique within the provider; the last segment of the node IRI.</summary>
    public string UniqueName { get; init; } = "";

    /// <summary>Display name.</summary>
    public string DisplayName { get; init; } = "";

    /// <summary>Attribute values of the node, typed by the vocabulary.</summary>
    public IReadOnlyList<Field>? Fields { get; init; }
}

/// <summary>One attribute value of a node.</summary>
public sealed record Field
{
    /// <summary>The attribute name in the vocabulary.</summary>
    public string FieldName { get; init; } = "";

    /// <summary>The value as text.</summary>
    public string FieldValue { get; init; } = "";
}

/// <summary>A reference: subject, verb, object.</summary>
public sealed record InjectedReference
{
    /// <summary>The subject.</summary>
    public NodeIdentifier Subject { get; init; } = new();

    /// <summary>The verb as a full IRI (<c>http://ddhub.no/HasDynamicValue</c> or its d-wis.org form).</summary>
    public string VerbURI { get; init; } = "";

    /// <summary>The object.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The JSON property is named Object in DWIS.API.DTO 2.x.")]
    public NodeIdentifier Object { get; init; } = new();
}

/// <summary>
/// Names a node, a variable or a vocabulary term: <see cref="NameSpace"/> is an alias from
/// <see cref="InjectionInformation"/> or a full namespace IRI; <see cref="ID"/> is the local name or,
/// for vocabulary classes, the full IRI.
/// </summary>
public sealed record NodeIdentifier
{
    /// <summary>Alias or namespace IRI.</summary>
    public string NameSpace { get; init; } = "";

    /// <summary>Local name or full IRI.</summary>
    public string ID { get; init; } = "";
}

/// <summary>A variable mirrored from another OPC UA server. Parsed for compatibility, then dropped (decision D-26).</summary>
public sealed record InjectedVariable
{
    /// <summary>Namespace of the source node.</summary>
    public string? NativeAddressSpaceNameSpace { get; init; }

    /// <summary>Node id of the source node.</summary>
    public string? NativeAddressSpaceNodeID { get; init; }

    /// <summary>Name of the mirrored variable in the Blackboard.</summary>
    public string InjectedName { get; init; } = "";

    /// <summary>Sampling interval.</summary>
    public double SamplingIntervalInMS { get; init; }

    /// <summary>OPC UA value rank.</summary>
    public int Rank { get; init; }

    /// <summary>Array dimensions.</summary>
    public int[]? Dimensions { get; init; }

    /// <summary>Data type name.</summary>
    public string DataType { get; init; } = "";
}
