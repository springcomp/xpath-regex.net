namespace XPath.Regex.NET;

/// <summary>
/// Immutable set of regex flags validated against a <see cref="RegexDialect"/>.
/// </summary>
public readonly struct RegexFlags : IEquatable<RegexFlags>
{
    /// <summary>The empty flags set.</summary>
    public static readonly RegexFlags None;

    /// <summary>Dot-all: <c>.</c> matches any character including LF and CR.</summary>
    public bool DotAll { get; }

    /// <summary>Multi-line: <c>^</c>/<c>$</c> match around <c>#x0A</c> line boundaries.</summary>
    public bool MultiLine { get; }

    /// <summary>Case-insensitive via Unicode case-folding.</summary>
    public bool IgnoreCase { get; }

    /// <summary>Free-spacing: whitespace and <c>#</c>-comments outside <c>[…]</c> are stripped.</summary>
    public bool FreeSpacing { get; }

    /// <summary>Literal/quote: pattern treated as plain text.</summary>
    public bool Literal { get; }

    private RegexFlags(bool dotAll, bool multiLine, bool ignoreCase, bool freeSpacing, bool literal)
    {
        DotAll = dotAll;
        MultiLine = multiLine;
        IgnoreCase = ignoreCase;
        FreeSpacing = freeSpacing;
        Literal = literal;
    }

    /// <summary>
    /// Parses and validates a flags string against the given dialect.
    /// </summary>
    /// <param name="flags">Flags string (e.g. <c>"mix"</c>). Duplicate characters are idempotent.</param>
    /// <param name="dialect">The dialect to validate against.</param>
    /// <returns>A validated <see cref="RegexFlags"/> instance.</returns>
    /// <exception cref="Errors.Forx0001Exception">
    /// Unknown flag character, or flags supplied in <see cref="RegexDialect.Xsd"/> mode.
    /// </exception>
    public static RegexFlags Parse(string flags, RegexDialect dialect)
    {
        ArgumentNullException.ThrowIfNull(flags);

        if (flags.Length == 0)
            return None;

        if (dialect == RegexDialect.Xsd)
            throw new Errors.Forx0001Exception(
                "Flags are not permitted in XSD dialect.", invalidFlag: null);

        bool dotAll = false, multiLine = false, ignoreCase = false, freeSpacing = false, literal = false;

        foreach (char c in flags)
        {
            switch (c)
            {
                case 's': dotAll = true; break;
                case 'm': multiLine = true; break;
                case 'i': ignoreCase = true; break;
                case 'x': freeSpacing = true; break;
                case 'q':
                    if (dialect == RegexDialect.Xsd)
                        throw new Errors.Forx0001Exception(
                            $"Flag 'q' is not supported in XSD.", invalidFlag: c);
                    literal = true;
                    break;
                default:
                    throw new Errors.Forx0001Exception(
                        $"Unknown flag character '{c}'.", invalidFlag: c);
            }
        }

        return new RegexFlags(dotAll, multiLine, ignoreCase, freeSpacing, literal);
    }

    /// <inheritdoc/>
    public bool Equals(RegexFlags other) =>
        DotAll == other.DotAll &&
        MultiLine == other.MultiLine &&
        IgnoreCase == other.IgnoreCase &&
        FreeSpacing == other.FreeSpacing &&
        Literal == other.Literal;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RegexFlags f && Equals(f);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(DotAll, MultiLine, IgnoreCase, FreeSpacing, Literal);

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder(5);
        if (DotAll) sb.Append('s');
        if (MultiLine) sb.Append('m');
        if (IgnoreCase) sb.Append('i');
        if (FreeSpacing) sb.Append('x');
        if (Literal) sb.Append('q');
        return sb.ToString();
    }

    /// <summary>Equality operator.</summary>
    public static bool operator ==(RegexFlags left, RegexFlags right) => left.Equals(right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(RegexFlags left, RegexFlags right) => !left.Equals(right);
}
