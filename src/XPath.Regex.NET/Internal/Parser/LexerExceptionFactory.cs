using XPath.Regex.NET.Errors;

namespace XPath.Regex.NET.Internal.Parser;

internal static class LexerExceptionFactory
{
    public static Forx0002Exception InvalidEscape(int originalOffset) =>
        new($"Invalid escape sequence at offset {originalOffset}.", originalOffset);

    public static Forx0002Exception DanglingBackslash(int originalOffset) =>
        new($"Dangling backslash at offset {originalOffset}.", originalOffset);

    public static Forx0002Exception UnterminatedUnicodePropertyEscape(int originalOffset) =>
        new($"Unterminated Unicode property escape at offset {originalOffset}.", originalOffset);

    public static Forx0002Exception EmptyUnicodePropertyEscape(int originalOffset) =>
        new($"Empty Unicode property escape at offset {originalOffset}.", originalOffset);

}
