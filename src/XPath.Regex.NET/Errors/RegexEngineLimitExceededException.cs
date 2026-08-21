namespace XPath.Regex.NET.Errors;

/// <summary>
/// Identifies the runtime safety limit that stopped regex matching.
/// </summary>
public enum RegexEngineLimit
{
    /// <summary>Maximum elapsed matching time, configured by <c>TimeoutMs</c>.</summary>
    TimeoutMs,

    /// <summary>Maximum number of VM threads, configured by <c>MaxThreadCount</c>.</summary>
    MaxThreadCount,

    /// <summary>Maximum epsilon-expansion stack depth, configured by <c>MaxStackDepth</c>.</summary>
    MaxStackDepth
}

/// <summary>
/// Thrown when the regex matching engine reaches a runtime safety limit.
/// This is not part of the W3C <c>FORX00xx</c> error family and does not
/// indicate that the compiled pattern is invalid.
/// </summary>
public sealed class RegexEngineLimitExceededException : Exception
{
    /// <summary>The runtime engine limit that was exceeded.</summary>
    public RegexEngineLimit Limit { get; }

    /// <summary>Initializes a new instance with the default limit value.</summary>
    public RegexEngineLimitExceededException() : this(RegexEngineLimit.TimeoutMs, string.Empty) { }

    /// <summary>Initializes a new instance with a message and the default limit value.</summary>
    public RegexEngineLimitExceededException(string message)
        : this(RegexEngineLimit.TimeoutMs, message) { }

    /// <summary>Initializes a new instance with an inner exception and the default limit value.</summary>
    public RegexEngineLimitExceededException(string message, Exception innerException)
        : this(RegexEngineLimit.TimeoutMs, message, innerException) { }

    /// <summary>Initializes a new instance for the specified runtime limit.</summary>
    public RegexEngineLimitExceededException(RegexEngineLimit limit, string message)
        : base(message)
    {
        Limit = limit;
    }

    /// <summary>Initializes a new instance with an inner exception.</summary>
    public RegexEngineLimitExceededException(
        RegexEngineLimit limit,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        Limit = limit;
    }
}
