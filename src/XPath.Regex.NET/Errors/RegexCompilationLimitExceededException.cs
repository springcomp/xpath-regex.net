using XPath.Regex.NET;

namespace XPath.Regex.NET.Errors;

/// <summary>
/// Identifies the compile-time resource limit that stopped pattern compilation.
/// </summary>
public enum RegexCompilationLimit
{
    /// <summary>Maximum quantifier bound, configured by <c>MaxQuantifierBound</c>.</summary>
    MaxQuantifierBound,

    /// <summary>Maximum number of compiled NFA instructions, configured by <c>MaxProgramInstructions</c>.</summary>
    MaxProgramInstructions
}

/// <summary>
/// Thrown when compiling a pattern would exceed a configured compile-time resource limit
/// (see <see cref="RegexCompileOptions"/>). This is not part of the W3C <c>FORX00xx</c> error
/// family: the pattern is syntactically valid, but exceeds the configured budget.
/// </summary>
public sealed class RegexCompilationLimitExceededException : Exception
{
    /// <summary>The compile-time limit that was exceeded.</summary>
    public RegexCompilationLimit Limit { get; }

    /// <summary>Initializes a new instance with the default limit value.</summary>
    public RegexCompilationLimitExceededException() : this(RegexCompilationLimit.MaxQuantifierBound, string.Empty) { }

    /// <summary>Initializes a new instance with a message and the default limit value.</summary>
    public RegexCompilationLimitExceededException(string message)
        : this(RegexCompilationLimit.MaxQuantifierBound, message) { }

    /// <summary>Initializes a new instance with an inner exception and the default limit value.</summary>
    public RegexCompilationLimitExceededException(string message, Exception innerException)
        : this(RegexCompilationLimit.MaxQuantifierBound, message, innerException) { }

    /// <summary>Initializes a new instance for the specified compile-time limit.</summary>
    public RegexCompilationLimitExceededException(RegexCompilationLimit limit, string message)
        : base(message)
    {
        Limit = limit;
    }

    /// <summary>Initializes a new instance with an inner exception.</summary>
    public RegexCompilationLimitExceededException(
        RegexCompilationLimit limit,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        Limit = limit;
    }
}
