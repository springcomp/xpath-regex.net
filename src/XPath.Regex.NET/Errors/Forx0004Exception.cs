namespace XPath.Regex.NET.Errors;

/// <summary>
/// Thrown when the replacement string passed to <c>fn:replace</c> contains an
/// unescaped <c>$</c> or <c>\</c> (when flag <c>q</c> is not active).
/// W3C error code: <c>FORX0004</c>.
/// </summary>
public sealed class Forx0004Exception : ForxException
{
    /// <summary>Zero-based offset of the offending character in the replacement string.</summary>
    public int ReplacementOffset { get; }

    /// <summary>Initializes a new instance.</summary>
    public Forx0004Exception() : base("FORX0004", "Invalid replacement string.") { }

    /// <summary>Initializes a new instance with a message.</summary>
    public Forx0004Exception(string message) : base("FORX0004", message) { }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    public Forx0004Exception(string message, Exception innerException)
        : base("FORX0004", message, innerException) { }

    /// <summary>Initializes a new instance with a message and replacement offset.</summary>
    public Forx0004Exception(string message, int replacementOffset)
        : base("FORX0004", message)
    {
        ReplacementOffset = replacementOffset;
    }
}
