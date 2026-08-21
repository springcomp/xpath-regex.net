namespace XPath.Regex.NET.Errors;

/// <summary>
/// Thrown when a compiled pattern can match the zero-length string and the
/// operation does not permit this (applies to <c>fn:replace</c>, <c>fn:tokenize</c>,
/// <c>fn:analyze-string</c>).
/// W3C error code: <c>FORX0003</c>.
/// </summary>
public sealed class Forx0003Exception : ForxException
{
    /// <summary>Initializes a new instance.</summary>
    public Forx0003Exception() : base("FORX0003", "Pattern can match the empty string.") { }

    /// <summary>Initializes a new instance with a message.</summary>
    public Forx0003Exception(string message) : base("FORX0003", message) { }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    public Forx0003Exception(string message, Exception innerException)
        : base("FORX0003", message, innerException) { }
}
