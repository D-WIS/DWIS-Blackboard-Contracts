using Google.Protobuf.Reflection;
using Grpc.Core;

namespace DWIS.Blackboard.Contracts.Grpc.Tests;

/// <summary>The service of specification 3.3 as the proto file defines it: its calls, and the stubs the package generates.</summary>
public sealed class ServiceShapeTests
{
    [Fact]
    public void The_service_has_the_calls_of_the_specification()
    {
        var service = Semantic.Descriptor;
        service.FullName.Should().Be("dwis.blackboard.semantic.v1.Semantic");
        service.Methods.Select(m => m.Name).Should().Equal(
            "PrepareManifest", "CommitManifest", "AbortManifest", "RemoveManifest", "ListManifests",
            "Query", "GetResources", "RegisterQuery", "UnregisterQuery", "ReleaseOwner", "WatchQueries", "GetVocabulary");
        var watch = service.FindMethodByName("WatchQueries");
        watch.IsServerStreaming.Should().BeTrue("one stream per real-time process carries the diffs");
        watch.IsClientStreaming.Should().BeFalse();
        service.Methods.Where(m => m.Name != "WatchQueries").Should().OnlyContain(m => !m.IsServerStreaming && !m.IsClientStreaming);
    }

    [Fact]
    public void The_messages_carry_the_typed_fields_the_real_time_process_acts_on()
    {
        FieldNames(PrepareResult.Descriptor).Should().Equal("manifest", "accepted", "report_json", "variables", "content_hash", "already_live");
        FieldNames(CommitRequest.Descriptor).Should().Equal("manifest", "endpoints");
        FieldNames(VariableEndpoint.Descriptor).Should().Equal("variable_id", "endpoint_url", "namespace_uri", "node_id");
        FieldNames(ProvidedVariable.Descriptor).Should().Equal("variable_id", "data_type", "rank", "dimensions", "initial_value", "validity_variable_id");
        FieldNames(QueryDiff.Descriptor).Should().Equal("query_id", "variables", "added", "removed", "at");
        FieldNames(ManifestSummary.Descriptor).Should().Contain(["manifest", "variables", "endpoints"], "the rebuild after a restart needs them (R-SEM-7)");
    }

    [Fact]
    public void A_server_and_a_client_are_generated()
    {
        // A server derives from the base class and overrides the calls it implements.
        typeof(TestServer).Should().BeDerivedFrom<Semantic.SemanticBase>();
        // A client has one method per call, the streaming one returning a server-streaming call.
        typeof(Semantic.SemanticClient).GetMethod("PrepareManifestAsync", [typeof(PrepareRequest), typeof(CallOptions)]).Should().NotBeNull();
        typeof(Semantic.SemanticClient).GetMethod("WatchQueries", [typeof(WatchRequest), typeof(CallOptions)])!
            .ReturnType.Should().Be(typeof(AsyncServerStreamingCall<QueryDiff>));
        // The binding of a server implementation produces the service definition the host registers.
        Semantic.BindService(new TestServer()).Should().NotBeNull();
    }

    private static List<string> FieldNames(MessageDescriptor descriptor) => descriptor.Fields.InFieldNumberOrder().Select(f => f.Name).ToList();

    private sealed class TestServer : Semantic.SemanticBase
    {
        public override Task<VocabularyInfo> GetVocabulary(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context) =>
            Task.FromResult(new VocabularyInfo { Version = "test", Loaded = true });
    }
}
