namespace XPath.Regex.NET;

/// <summary>
/// Represents a single successful match of an <see cref="XPathRegex"/> pattern against an input string.
/// </summary>
public sealed class RegexMatch
{
    /// <summary>The input string this match was found in.</summary>
    public string Input { get; }

    /// <summary>The zero-based character offset of the start of this match.</summary>
    public int Index { get; }

    /// <summary>The character length of this match.</summary>
    public int Length { get; }

    /// <summary>The matched substring (<c>$0</c> / group 0).</summary>
    public string Value { get; }

    /// <summary>
    /// Captured substrings, 1-indexed. <c>Groups[0]</c> is the entire match.
    /// <c>Groups[N]</c> is the Nth capturing group.
    /// A group that did not participate has <see cref="RegexGroup.Success"/> = <see langword="false"/>
    /// and <see cref="RegexGroup.Value"/> = <see cref="string.Empty"/>.
    /// </summary>
    public IReadOnlyList<RegexGroup> Groups { get; }

    /// <summary>Initializes a new match result.</summary>
    internal RegexMatch(string input, int index, int length, string value, IReadOnlyList<RegexGroup> groups)
    {
        Input = input;
        Index = index;
        Length = length;
        Value = value;
        Groups = groups;
    }
}
