using XPath.Regex.NET.Internal.Matcher;
using XPath.Regex.NET.Internal.Nfa;

namespace XPath.Regex.NET.Internal.Operations;

/// <summary>
/// Thin wrapper over <see cref="PikeVm.Matches"/> so Matches, Replace, Tokenize,
/// and AnalyzeString share identical non-overlapping leftmost-first enumeration
/// semantics. Match and IsMatch intentionally use the lower-level single-result
/// <see cref="PikeVm.Match"/> API instead of setting up a multi-result iterator.
/// </summary>
internal static class MatchIterator
{
    public static IEnumerable<MatchContext> EnumerateAll(
        NfaProgram program,
        RegexFlags flags,
        RegexDialect dialect,
        string input)
        => PikeVm.Matches(program, flags, dialect, input);
}
