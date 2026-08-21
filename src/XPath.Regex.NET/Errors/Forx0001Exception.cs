namespace XPath.Regex.NET.Errors;

/// <summary>
/// Thrown when the <c>$flags</c> argument contains an invalid character,
/// or when flags are supplied in <see cref="RegexDialect.Xsd"/> mode.
/// W3C error code: <c>FORX0001</c>.
/// </summary>
public sealed class Forx0001Exception : ForxException
{
    /// <summary>
    /// The offending flag character, or <see langword="null"/> if flags were rejected wholesale.
    /// </summary>
    public char? InvalidFlag { get; }

    /// <summary>Initializes a new instance.</summary>
    public Forx0001Exception() : base("FORX0001", "Invalid flags.") { }

    /// <summary>Initializes a new instance with a message.</summary>
    public Forx0001Exception(string message) : base("FORX0001", message) { }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    public Forx0001Exception(string message, Exception innerException)
        : base("FORX0001", message, innerException) { }

    /// <summary>Initializes a new instance with a message and optional invalid flag character.</summary>
    public Forx0001Exception(string message, char? invalidFlag)
        : base("FORX0001", message)
    {
        InvalidFlag = invalidFlag;
    }
}
