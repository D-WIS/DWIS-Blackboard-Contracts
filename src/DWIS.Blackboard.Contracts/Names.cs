namespace DWIS.Blackboard.Contracts;

/// <summary>
/// The one rule that makes a name from a manifest fit for an IRI, a namespace URI and a node id (decision D-62):
/// every character an IRI cannot carry becomes an underscore, nothing else changes. The semantic service applies
/// it to the provider name, the unique names, the variable ids and the injected terms when it forms IRIs and the
/// manifest id; the real-time process applies the same to the namespaces and node ids it claims, so that both
/// sides name one thing the same way. Browse names and display names stay as written. The old server dropped
/// the space of <c>DDHub CLI</c> in its node ids and left every other character alone.
/// </summary>
public static class Names
{
    /// <summary>The character that replaces one an IRI cannot carry.</summary>
    public const char Replacement = '_';

    /// <summary>
    /// The name with every character an IRI cannot carry replaced by <see cref="Replacement"/>: blanks and
    /// control characters, and the delimiters RFC 3987 keeps out of an IRI (<c>"</c>, <c>&lt;</c>, <c>&gt;</c>,
    /// <c>\</c>, <c>^</c>, <c>`</c>, <c>{</c>, <c>|</c>, <c>}</c>). A name without any is returned as it is.
    /// </summary>
    public static string IriSafe(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (IsIriSafe(name))
        {
            return name;
        }

        return string.Create(name.Length, name, static (span, source) =>
        {
            for (var i = 0; i < source.Length; i++)
            {
                span[i] = NeedsReplacing(source[i]) ? Replacement : source[i];
            }
        });
    }

    /// <summary>True when <see cref="IriSafe"/> would return the name unchanged.</summary>
    public static bool IsIriSafe(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        foreach (var c in name)
        {
            if (NeedsReplacing(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool NeedsReplacing(char c) => c == ' ' || char.IsControl(c) || c is '"' or '<' or '>' or '\\' or '^' or '`' or '{' or '|' or '}';
}
