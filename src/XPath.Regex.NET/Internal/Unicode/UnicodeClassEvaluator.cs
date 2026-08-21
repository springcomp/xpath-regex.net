using System.Collections.Immutable;
using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.Internal.Unicode;

/// <summary>
/// Evaluates Unicode character class expressions by resolving property names
/// and shortcut escapes through <see cref="UnicodeAdapter"/>.
/// </summary>
internal sealed class UnicodeClassEvaluator
{
    private readonly RegexDialect _dialect;

    /// <summary>
    /// Initializes a new evaluator for the specified dialect.
    /// </summary>
    public UnicodeClassEvaluator(RegexDialect dialect)
    {
        _dialect = dialect;
    }

    /// <summary>
    /// Resolves a Unicode property escape (<c>\p{Name}</c>) to sorted, merged range tuples.
    /// </summary>
    /// <param name="propertyName">The property/category/block name (e.g. "Lu", "IsBasicLatin").</param>
    /// <param name="negated">If <see langword="true"/>, complement the set (<c>\P{Name}</c>).</param>
    /// <returns>Sorted, merged ranges suitable for <c>CharClassNode</c>.</returns>
    /// <exception cref="Forx0002Exception">The property name could not be resolved.</exception>
    public ImmutableArray<(int Lo, int Hi)> ResolvePropertyEscape(string propertyName, bool negated)
    {
        try
        {
            return UnicodeAdapter.ResolveProperty(propertyName, negated, _dialect);
        }
        catch (InvalidOperationException ex)
        {
            throw new Forx0002Exception(ex.Message, ex);
        }
    }

    /// <summary>
    /// Resolves multi-character escapes (\d, \s, \w, \i, \c and complements) to tuple ranges.
    /// </summary>
    /// <param name="escape">The escape character.</param>
    /// <returns>Sorted, merged ranges suitable for <c>CharClassNode</c>.</returns>
    public static ImmutableArray<(int Lo, int Hi)> ResolveMultiCharEscape(char escape)
    {
        return UnicodeAdapter.ResolveShortcut(escape);
    }


}
