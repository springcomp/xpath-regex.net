using System.Globalization;

namespace XPath.Regex.NET;

/// <summary>
/// Immutable compile-time resource limits for <see cref="XPathRegex.Compile(string, RegexDialect, RegexCompileOptions)"/>.
/// Callers may lower the secure defaults but cannot raise a limit above its hard ceiling;
/// exceeding either results in an <see cref="ArgumentOutOfRangeException"/> at construction time.
/// </summary>
public sealed class RegexCompileOptions
{
    /// <summary>
    /// The non-bypassable upper bound for <see cref="MaxQuantifierBound"/>. Equal to the secure
    /// default: callers may lower this limit but never raise it.
    /// </summary>
    public const int HardCeilingMaxQuantifierBound = 1_000;

    /// <summary>
    /// The non-bypassable upper bound for <see cref="MaxProgramInstructions"/>. Equal to the
    /// secure default: callers may lower this limit but never raise it.
    /// </summary>
    public const int HardCeilingMaxProgramInstructions = 100_000;

    /// <summary>The default options, using the library's secure defaults.</summary>
    public static RegexCompileOptions Default { get; } = new();

    /// <summary>
    /// The maximum value permitted for either bound of a <c>{min,max}</c> quantifier.
    /// Defaults to 1,000; cannot exceed <see cref="HardCeilingMaxQuantifierBound"/>.
    /// </summary>
    public int MaxQuantifierBound { get; }

    /// <summary>
    /// The maximum number of NFA instructions a compiled program may contain.
    /// Defaults to 100,000; cannot exceed <see cref="HardCeilingMaxProgramInstructions"/>.
    /// </summary>
    public int MaxProgramInstructions { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="RegexCompileOptions"/>.
    /// </summary>
    /// <param name="maxQuantifierBound">
    /// Maximum value permitted for either bound of a <c>{min,max}</c> quantifier. Defaults to 1,000.
    /// </param>
    /// <param name="maxProgramInstructions">
    /// Maximum number of NFA instructions a compiled program may contain. Defaults to 100,000.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxQuantifierBound"/> or <paramref name="maxProgramInstructions"/> is
    /// less than 1 or greater than its respective hard ceiling.
    /// </exception>
    public RegexCompileOptions(
        int maxQuantifierBound = 1_000,
        int maxProgramInstructions = 100_000)
    {
        if (maxQuantifierBound < 1 || maxQuantifierBound > HardCeilingMaxQuantifierBound)
            throw new ArgumentOutOfRangeException(
                nameof(maxQuantifierBound),
                maxQuantifierBound,
                $"Must be between 1 and {HardCeilingMaxQuantifierBound.ToString(CultureInfo.InvariantCulture)} (the library's hard ceiling).");

        if (maxProgramInstructions < 1 || maxProgramInstructions > HardCeilingMaxProgramInstructions)
            throw new ArgumentOutOfRangeException(
                nameof(maxProgramInstructions),
                maxProgramInstructions,
                $"Must be between 1 and {HardCeilingMaxProgramInstructions.ToString(CultureInfo.InvariantCulture)} (the library's hard ceiling).");

        MaxQuantifierBound = maxQuantifierBound;
        MaxProgramInstructions = maxProgramInstructions;
    }
}
