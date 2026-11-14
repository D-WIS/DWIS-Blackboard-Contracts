namespace DWIS.Blackboard.Contracts;

/// <summary>
/// The one namespace translation of specification 3.6 (R-SEM-9, decisions D-19 and D-30), used in
/// exactly two places: on ingress (manifests, queries, vocabulary) to the canonical name, and on
/// egress through the OPC UA façade back to the legacy name that existing clients compare against.
/// </summary>
public static class Namespaces
{
    /// <summary>The canonical namespace in the store.</summary>
    public const string Canonical = "http://d-wis.org/";

    /// <summary>The legacy namespace accepted on input and returned to existing clients.</summary>
    public const string Legacy = "http://ddhub.no/";

    /// <summary>Rewrites an IRI that starts with the legacy namespace to the canonical one; anything else is returned unchanged.</summary>
    public static string ToCanonical(string iri) => Swap(iri, Legacy, Canonical);

    /// <summary>Rewrites an IRI that starts with the canonical namespace to the legacy one; anything else is returned unchanged.</summary>
    public static string ToLegacy(string iri) => Swap(iri, Canonical, Legacy);

    /// <summary>True when the IRI starts with the legacy namespace.</summary>
    public static bool IsLegacy(string iri) => iri is not null && iri.StartsWith(Legacy, StringComparison.Ordinal);

    /// <summary>True when the IRI starts with the canonical namespace.</summary>
    public static bool IsCanonical(string iri) => iri is not null && iri.StartsWith(Canonical, StringComparison.Ordinal);

    private static string Swap(string iri, string from, string to)
    {
        ArgumentNullException.ThrowIfNull(iri);
        return iri.StartsWith(from, StringComparison.Ordinal) ? string.Concat(to, iri.AsSpan(from.Length)) : iri;
    }
}
