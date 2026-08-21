using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Matcher;
using XPath.Regex.NET.Internal.Nfa;
using XPath.Regex.NET.Internal.Operations;
using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET;

/// <summary>
/// An immutable compiled XSD/XPath regular expression.
/// Thread-safe: one instance can be shared across threads.
/// </summary>
public sealed class XPathRegex
{
    // -----------------------------------------------------------------------
    // Internal state
    // -----------------------------------------------------------------------

    private readonly NfaProgram _program;

    // -----------------------------------------------------------------------
    // Properties
    // -----------------------------------------------------------------------

    /// <summary>The original pattern string as provided at compile time.</summary>
    public string Pattern { get; }

    /// <summary>The dialect this pattern was compiled against.</summary>
    public RegexDialect Dialect { get; }

    /// <summary>The validated flags active for this pattern.</summary>
    public RegexFlags Flags { get; }

    // -----------------------------------------------------------------------
    // Constructor (private — use factory)
    // -----------------------------------------------------------------------

    private XPathRegex(string pattern, RegexDialect dialect, RegexFlags flags, NfaProgram program)
    {
        Pattern = pattern;
        Dialect = dialect;
        Flags = flags;
        _program = program;
    }

    // -----------------------------------------------------------------------
    // Factory
    // -----------------------------------------------------------------------

    /// <summary>
    /// Compiles <paramref name="pattern"/> for the given <paramref name="dialect"/>
    /// with no flags.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="dialect">Compatibility level. Defaults to <see cref="RegexDialect.XPath30"/>.</param>
    /// <returns>A compiled, immutable <see cref="XPathRegex"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is <see langword="null"/>.</exception>
    /// <exception cref="Forx0002Exception">Pattern is syntactically invalid or uses features
    /// not available in <paramref name="dialect"/>.</exception>
    /// <exception cref="RegexCompilationLimitExceededException">Pattern exceeds a configured
    /// compile-time resource limit (see <see cref="RegexCompileOptions"/>). The pattern is
    /// otherwise syntactically valid.</exception>
    public static XPathRegex Compile(
        string pattern,
        RegexDialect dialect = RegexDialect.XPath30)
        => Compile(pattern, string.Empty, dialect, RegexCompileOptions.Default);

    /// <summary>
    /// Compiles <paramref name="pattern"/> for the given <paramref name="dialect"/> and
    /// <paramref name="options"/>, with no flags.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="dialect">Compatibility level.</param>
    /// <param name="options">
    /// Compile-time resource limits. Use this to lower the library's secure defaults for
    /// untrusted or resource-constrained scenarios; limits cannot be raised above the
    /// library's hard ceilings (see <see cref="RegexCompileOptions"/>).
    /// </param>
    /// <returns>A compiled, immutable <see cref="XPathRegex"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pattern"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="Forx0002Exception">Pattern is syntactically invalid or uses features
    /// not available in <paramref name="dialect"/>.</exception>
    /// <exception cref="RegexCompilationLimitExceededException">Pattern exceeds a configured
    /// compile-time resource limit. The pattern is otherwise syntactically valid.</exception>
    public static XPathRegex Compile(
        string pattern,
        RegexDialect dialect,
        RegexCompileOptions options)
        => Compile(pattern, string.Empty, dialect, options);

    /// <summary>
    /// Compiles <paramref name="pattern"/> for the given <paramref name="dialect"/>
    /// with the specified <paramref name="flags"/>.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="flags">Flags string (e.g. <c>"mix"</c>).</param>
    /// <param name="dialect">Compatibility level. Defaults to <see cref="RegexDialect.XPath30"/>.</param>
    /// <returns>A compiled, immutable <see cref="XPathRegex"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pattern"/> or
    /// <paramref name="flags"/> is <see langword="null"/>.</exception>
    /// <exception cref="Forx0001Exception">
    /// <paramref name="flags"/> contains an unknown character or flags are used in
    /// <see cref="RegexDialect.Xsd"/> mode.
    /// </exception>
    /// <exception cref="Forx0002Exception">Pattern is syntactically invalid or uses features
    /// not available in <paramref name="dialect"/>.</exception>
    /// <exception cref="RegexCompilationLimitExceededException">Pattern exceeds a configured
    /// compile-time resource limit (see <see cref="RegexCompileOptions"/>). The pattern is
    /// otherwise syntactically valid.</exception>
    public static XPathRegex Compile(
        string pattern,
        string flags,
        RegexDialect dialect = RegexDialect.XPath30)
        => Compile(pattern, flags, dialect, RegexCompileOptions.Default);

    /// <summary>
    /// Compiles <paramref name="pattern"/> for the given <paramref name="dialect"/>,
    /// <paramref name="flags"/>, and <paramref name="options"/>.
    /// </summary>
    /// <param name="pattern">The regular expression pattern.</param>
    /// <param name="flags">Flags string (e.g. <c>"mix"</c>).</param>
    /// <param name="dialect">Compatibility level.</param>
    /// <param name="options">
    /// Compile-time resource limits. Use this to lower the library's secure defaults for
    /// untrusted or resource-constrained scenarios; limits cannot be raised above the
    /// library's hard ceilings (see <see cref="RegexCompileOptions"/>).
    /// </param>
    /// <returns>A compiled, immutable <see cref="XPathRegex"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pattern"/>, <paramref name="flags"/>, or <paramref name="options"/>
    /// is <see langword="null"/>.
    /// </exception>
    /// <exception cref="Forx0001Exception">
    /// <paramref name="flags"/> contains an unknown character or flags are used in
    /// <see cref="RegexDialect.Xsd"/> mode.
    /// </exception>
    /// <exception cref="Forx0002Exception">Pattern is syntactically invalid or uses features
    /// not available in <paramref name="dialect"/>.</exception>
    /// <exception cref="RegexCompilationLimitExceededException">Pattern exceeds a configured
    /// compile-time resource limit. The pattern is otherwise syntactically valid.</exception>
    public static XPathRegex Compile(
        string pattern,
        string flags,
        RegexDialect dialect,
        RegexCompileOptions options)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(flags);
        ArgumentNullException.ThrowIfNull(options);

        RegexFlags parsedFlags = RegexFlags.Parse(flags, dialect);

        PreprocessedPattern preprocessed = Internal.Parser.Preprocessor.Process(pattern, parsedFlags);
        IReadOnlyList<Internal.Parser.RegexToken> tokens = Internal.Parser.RegexLexer.Tokenize(preprocessed, dialect);
        Internal.Parser.ParseResult parsed = Internal.Parser.RegexParser.Parse(tokens, dialect, parsedFlags, Internal.Parser.PermissiveUnicodeNameValidator.Instance, options);
        NfaProgram program = Internal.Compiler.RegexCompiler.Compile(parsed.Root, parsed.CaptureCount, dialect, parsedFlags, options);

        return new XPathRegex(pattern, dialect, parsedFlags, program);
    }

    // -----------------------------------------------------------------------
    // Operations
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="input"/> contains (or, in
    /// <see cref="RegexDialect.Xsd"/> mode, equals) a substring that matches this pattern.
    /// Implements <c>fn:matches()</c> semantics.
    /// </summary>
    /// <param name="input">The string to test. <see langword="null"/> is treated as the empty string.</param>
    public bool IsMatch(string? input)
    {
        string s = input ?? string.Empty;
        return PikeVm.IsMatch(_program, Flags, Dialect, s);
    }

    /// <summary>
    /// Returns the first match of this pattern in <paramref name="input"/>,
    /// or <see langword="null"/> if there is no match.
    /// </summary>
    /// <param name="input">The string to search. <see langword="null"/> is treated as the empty string.</param>
    public RegexMatch? Match(string? input)
    {
        string s = input ?? string.Empty;
        MatchContext? ctx = PikeVm.Match(_program, Flags, Dialect, s);
        return ctx is null ? null : MatchProjector.ToRegexMatch(s, ctx, _program);
    }

    /// <summary>
    /// Returns all non-overlapping matches of this pattern in <paramref name="input"/>.
    /// Enumeration is lazy; each next match is found on demand.
    /// </summary>
    /// <param name="input">The string to search. <see langword="null"/> is treated as the empty string.</param>
    public IEnumerable<RegexMatch> Matches(string? input)
    {
        string s = input ?? string.Empty;
        foreach (MatchContext ctx in MatchIterator.EnumerateAll(_program, Flags, Dialect, s))
            yield return MatchProjector.ToRegexMatch(s, ctx, _program);
    }

    /// <summary>
    /// Replaces each non-overlapping match of this pattern in <paramref name="input"/>
    /// with <paramref name="replacement"/>. Implements <c>fn:replace()</c> semantics.
    /// </summary>
    /// <param name="input">The string to process. <see langword="null"/> is treated as the empty string.</param>
    /// <param name="replacement">The replacement string. May contain <c>$0</c>–<c>$N</c>
    /// back-references unless <see cref="RegexFlags.Literal"/> is active.</param>
    /// <exception cref="Forx0003Exception">This pattern can match the empty string.</exception>
    /// <exception cref="Forx0004Exception">
    /// <paramref name="replacement"/> contains an invalid <c>$</c> or <c>\</c> sequence
    /// (when <see cref="RegexFlags.Literal"/> is not active).
    /// </exception>
    public string Replace(string? input, string replacement)
    {
        ArgumentNullException.ThrowIfNull(replacement);

        ThrowIfCanMatchEmpty();

        string s = input ?? string.Empty;

        // Parse replacement once — validates syntax, throws FORX0004 on error.
        ParsedReplacement parsed = ReplacementParser.Parse(replacement, Flags, _program.CaptureCount);

        var sb = new System.Text.StringBuilder(s.Length + replacement.Length);
        int lastIndex = 0;
        foreach (MatchContext ctx in MatchIterator.EnumerateAll(_program, Flags, Dialect, s))
        {
            sb.Append(s, lastIndex, ctx.Start - lastIndex);
            parsed.Apply(ctx, s, sb);
            lastIndex = ctx.EndExclusive;
        }

        sb.Append(s, lastIndex, s.Length - lastIndex);
        return sb.ToString();
    }

    /// <summary>
    /// Splits <paramref name="input"/> on each non-overlapping match of this pattern.
    /// Implements <c>fn:tokenize()</c> semantics.
    /// </summary>
    /// <param name="input">The string to split. <see langword="null"/> or empty returns an empty sequence.</param>
    /// <returns>
    /// The sequence of substrings between matches. Leading and trailing empty strings
    /// are included when the match occurs at the start or end of input.
    /// </returns>
    /// <exception cref="Forx0003Exception">This pattern can match the empty string.</exception>
    public IReadOnlyList<string> Tokenize(string? input)
    {
        ThrowIfCanMatchEmpty();

        string s = input ?? string.Empty;
        if (s.Length == 0)
            return [];

        var parts = new List<string>();
        int lastIndex = 0;
        foreach (MatchContext ctx in MatchIterator.EnumerateAll(_program, Flags, Dialect, s))
        {
            parts.Add(s.Substring(lastIndex, ctx.Start - lastIndex));
            lastIndex = ctx.EndExclusive;
        }

        parts.Add(s.Substring(lastIndex));
        return parts;
    }

    /// <summary>
    /// Partitions <paramref name="input"/> into matched and non-matched regions,
    /// with captured group detail. Implements <c>fn:analyze-string()</c> semantics.
    /// </summary>
    /// <param name="input">The string to analyze. <see langword="null"/> is treated as the empty string.</param>
    /// <returns>An <see cref="AnalyzeStringResult"/> describing the partition.</returns>
    /// <exception cref="Forx0003Exception">This pattern can match the empty string.</exception>
    public AnalyzeStringResult AnalyzeString(string? input)
    {
        ThrowIfCanMatchEmpty();

        string s = input ?? string.Empty;
        var regions = new List<AnalyzeStringRegion>();
        int lastIndex = 0;
        foreach (MatchContext ctx in MatchIterator.EnumerateAll(_program, Flags, Dialect, s))
        {
            if (ctx.Start > lastIndex)
                regions.Add(new NonMatchRegion(s.Substring(lastIndex, ctx.Start - lastIndex)));

            regions.Add(MatchProjector.ToMatchRegion(s, ctx, _program));
            lastIndex = ctx.EndExclusive;
        }

        if (lastIndex < s.Length)
            regions.Add(new NonMatchRegion(s.Substring(lastIndex)));
        else if (regions.Count == 0)
            regions.Add(new NonMatchRegion(s)); // entire input is non-match (includes empty input)

        return new AnalyzeStringResult(s, regions);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void ThrowIfCanMatchEmpty()
    {
        if (_program.CanMatchEmpty)
            throw new Forx0003Exception(
                "The pattern can match the empty string, which is not permitted for this operation.");
    }
}
