namespace XPath.Regex.NET.Errors;

/// <summary>
/// Base class for all XPath/XSD regex dynamic errors (FORX00xx).
/// </summary>
public abstract class ForxException : Exception
{
    /// <summary>The W3C error code, e.g. <c>"FORX0002"</c>.</summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Zero-based character offset in the pattern where the error was detected,
    /// or <c>-1</c> if not applicable.
    /// </summary>
    public int PatternOffset { get; }

    /// <summary>Initializes a new instance with default error code.</summary>
    protected ForxException() : base() { ErrorCode = string.Empty; }

    /// <summary>Initializes a new instance with a message.</summary>
    protected ForxException(string message) : base(message) { ErrorCode = string.Empty; }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    protected ForxException(string message, Exception innerException)
        : base(message, innerException) { ErrorCode = string.Empty; }

    /// <summary>Initializes a new instance.</summary>
    protected ForxException(string errorCode, string message, int patternOffset = -1)
        : this(errorCode, message, null, patternOffset) { }

    /// <summary>Initializes a new instance with an inner exception.</summary>
    protected ForxException(string errorCode, string message, Exception? inner, int patternOffset = -1)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        PatternOffset = patternOffset;
    }
}
