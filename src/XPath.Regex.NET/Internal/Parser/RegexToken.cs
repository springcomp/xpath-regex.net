namespace XPath.Regex.NET.Internal.Parser;

internal readonly record struct RegexToken(
    RegexTokenKind Kind,
    int OriginalOffset,
    int Length,
    ReadOnlyMemory<char> Lexeme,
    int? IntValue = null,
    char? CharValue = null,
    string? TextValue = null);
