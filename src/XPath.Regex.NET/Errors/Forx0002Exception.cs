namespace XPath.Regex.NET.Errors;

/// <summary>
/// Thrown when the regular expression pattern is syntactically invalid, uses a
/// feature unavailable at the configured dialect, or contains an invalid back-reference
/// or Unicode block name.
/// W3C error code: <c>FORX0002</c>.
/// </summary>
public sealed class Forx0002Exception : ForxException
{
    /// <summary>Initializes a new instance.</summary>
    public Forx0002Exception() : base("FORX0002", "Invalid pattern.") { }

    /// <summary>Initializes a new instance with a message.</summary>
    public Forx0002Exception(string message) : base("FORX0002", message) { }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    public Forx0002Exception(string message, Exception innerException)
        : base("FORX0002", message, innerException) { }

    /// <summary>Initializes a new instance with a message and pattern offset.</summary>
    public Forx0002Exception(string message, int patternOffset)
        : base("FORX0002", message, patternOffset) { }

}
