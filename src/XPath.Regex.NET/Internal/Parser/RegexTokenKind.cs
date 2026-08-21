namespace XPath.Regex.NET.Internal.Parser;

internal enum RegexTokenKind
{
    End,

    Pipe,
    LParen,
    RParen,
    NonCapturingPrefix,
    LBracket,
    RBracket,
    Caret,
    Dollar,
    Hyphen,
    Dot,

    Question,
    Star,
    Plus,
    LBrace,
    RBrace,
    Comma,
    Number,

    LiteralChar,
    SingleCharEscape,
    MultiCharEscape,
    CategoryEscape,
    ComplementEscape,
    BackReference,
}
