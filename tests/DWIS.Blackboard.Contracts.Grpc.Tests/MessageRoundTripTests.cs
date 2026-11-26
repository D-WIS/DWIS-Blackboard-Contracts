using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace DWIS.Blackboard.Contracts.Grpc.Tests;

/// <summary>Every message survives the wire (binary) and the JSON mapping unchanged, defaults included.</summary>
public sealed class MessageRoundTripTests
{
    public static TheoryData<IMessage> Messages() =>
    [
        new ManifestRef { Provider = "SmartConnect", Name = "smartconnect-rig-signals" },
        new PrepareRequest { ManifestJson = "{\"ManifestName\":\"x\"}", Origin = "local" },
        new PrepareResult
        {
            Manifest = new ManifestRef { Provider = "SmartConnect", Name = "smartconnect-rig-signals" },
            Accepted = false,
            ReportJson = "{\"accepted\":false}",
            Variables = { new ProvidedVariable { VariableId = "spp", DataType = "double", Rank = 1, Dimensions = { 3 }, InitialValue = "0", ValidityVariableId = "spp_validitySignal" } },
            ContentHash = "sha256:" + new string('0', 64),
            AlreadyLive = true,
        },
        new CommitRequest
        {
            Manifest = new ManifestRef { Provider = "SmartConnect", Name = "smartconnect-rig-signals" },
            Endpoints = { new VariableEndpoint { VariableId = "spp", EndpointUrl = "opc.tcp://rig:4840", NamespaceUri = "http://ddhub.no/SmartConnect/Variables/", NodeId = "SmartConnect.spp" } },
        },
        new CommitResult { Manifest = new ManifestRef { Provider = "a", Name = "b" }, Accepted = true, ReportJson = "{}" },
        new RemoveResult { Removed = true },
        new ManifestList
        {
            Manifests =
            {
                new ManifestSummary
                {
                    Manifest = new ManifestRef { Provider = "a", Name = "b" },
                    ManifestName = "B",
                    Accepted = true,
                    ContentHash = "sha256:1",
                    VocabularyVersion = "2026-10-14",
                    InjectedAt = Timestamp.FromDateTime(new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc)),
                    Variables = { new ProvidedVariable { VariableId = "v" } },
                    Endpoints = { new VariableEndpoint { VariableId = "v", NodeId = "a.v" } },
                },
            },
        },
        new SparqlRequest { Query = "SELECT ?s WHERE { ?s ?p ?o }" },
        new QueryResult { Variables = { "s", "o" }, Rows = { new Row { Values = { "http://d-wis.org/a", "" } } }, Truncated = true },
        new ResourceFile
        {
            Individuals = { new Individual { Iri = "http://d-wis.org/SmartConnect/spp#01", TypeIri = "http://d-wis.org/SPP" } },
            Sentences = { new Sentence { Subject = "http://d-wis.org/SmartConnect/spp#01", Verb = "http://d-wis.org/HasDynamicValue", Object = "http://d-wis.org/SmartConnect/Variables/SmartConnect.spp" } },
        },
        new RegisterRequest { Query = "SELECT ?s WHERE { ?s ?p ?o }", Owner = "session 7", Watcher = "rig-1" },
        new QuerySnapshot { QueryId = 3, Variables = { "s" }, Rows = { new Row { Values = { "http://d-wis.org/a" } } } },
        new UnregisterRequest { Query = "q", Owner = "session 7" },
        new UnregisterResult { Released = true, QueryId = 3 },
        new OwnerRef { Owner = "session 7" },
        new ReleasedQueries { QueryIds = { 3, 4 } },
        new WatchRequest { Watcher = "rig-1", Fresh = true },
        new QueryDiff
        {
            QueryId = 3,
            Variables = { "s" },
            Added = { new Row { Values = { "http://d-wis.org/b" } } },
            Removed = { new Row { Values = { "http://d-wis.org/a" } } },
            At = Timestamp.FromDateTime(DateTime.UtcNow),
        },
        new VocabularyInfo { Version = "2026-10-14", VocabularyGraph = "http://d-wis.org/vocabulary/2026-10-14", UnitsGraph = "http://d-wis.org/units/2026-10-14", Loaded = true, RegisteredQueries = 2 },
    ];

    [Theory]
    [MemberData(nameof(Messages))]
    public void Binary_and_json_round_trips_keep_every_field(IMessage message)
    {
        var parser = (MessageParser)message.GetType().GetProperty("Parser")!.GetValue(null)!;
        parser.ParseFrom(message.ToByteArray()).Should().Be(message, "the binary form is the wire");
        parser.ParseJson(JsonFormatter.Default.Format(message)).Should().Be(message, "the JSON mapping serves tools and logs");
    }

    [Fact]
    public void An_unbound_row_value_is_the_empty_string_and_survives()
    {
        var row = new Row { Values = { "http://d-wis.org/a", "", "literal value" } };
        Row.Parser.ParseFrom(row.ToByteArray()).Values.Should().Equal("http://d-wis.org/a", "", "literal value");
    }
}
