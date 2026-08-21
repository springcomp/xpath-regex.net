namespace XPath.Regex.NET;

/// <summary>
/// Represents the result of a single capturing group within a <see cref="RegexMatch"/>.
/// </summary>
public sealed class RegexGroup
{
    /// <summary>Whether this group participated in the match.</summary>
    public bool Success { get; }

    /// <summary>
    /// Zero-based start offset in the input string,
    /// or <c>-1</c> if <see cref="Success"/> is <see langword="false"/>.
    /// </summary>
    public int Index { get; }

    /// <summary>Character length, or <c>0</c> if <see cref="Success"/> is <see langword="false"/>.</summary>
    public int Length { get; }

    /// <summary>
    /// Captured substring,
    /// or <see cref="string.Empty"/> if <see cref="Success"/> is <see langword="false"/>.
    /// </summary>
    public string Value { get; }

    /// <summary>Initializes a participating group.</summary>
    internal RegexGroup(bool success, int index, int length, string value)
    {
        Success = success;
        Index = index;
        Length = length;
        Value = value;
    }
}
