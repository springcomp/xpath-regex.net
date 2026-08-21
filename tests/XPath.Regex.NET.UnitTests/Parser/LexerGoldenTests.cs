using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Parser;

public sealed class LexerGoldenTests
{
    [Fact]
    public void Tokenize_SimpleAlternation()
    {
        var tokens = Lex("ab|cd");

        AssertTokens(tokens,
            (RegexTokenKind.LiteralChar, "a", 0),
            (RegexTokenKind.LiteralChar, "b", 1),
            (RegexTokenKind.Pipe, "|", 2),
            (RegexTokenKind.LiteralChar, "c", 3),
            (RegexTokenKind.LiteralChar, "d", 4),
            (RegexTokenKind.End, string.Empty, 5));
    }

    [Fact]
    public void Tokenize_QuantifiersAndReluctantSuffix()
    {
        var tokens = Lex("a{2,4}?b*+");

        AssertTokens(tokens,
            (RegexTokenKind.LiteralChar, "a", 0),
            (RegexTokenKind.LBrace, "{", 1),
            (RegexTokenKind.Number, "2", 2),
            (RegexTokenKind.Comma, ",", 3),
            (RegexTokenKind.Number, "4", 4),
            (RegexTokenKind.RBrace, "}", 5),
            (RegexTokenKind.Question, "?", 6),
            (RegexTokenKind.LiteralChar, "b", 7),
            (RegexTokenKind.Star, "*", 8),
            (RegexTokenKind.Plus, "+", 9),
            (RegexTokenKind.End, string.Empty, 10));
    }

    [Fact]
    public void Tokenize_ClassSubtraction()
    {
        var tokens = Lex("[a-z-[aeiou]]");

        AssertTokens(tokens,
            (RegexTokenKind.LBracket, "[", 0),
            (RegexTokenKind.LiteralChar, "a", 1),
            (RegexTokenKind.Hyphen, "-", 2),
            (RegexTokenKind.LiteralChar, "z", 3),
            (RegexTokenKind.Hyphen, "-", 4),
            (RegexTokenKind.LBracket, "[", 5),
            (RegexTokenKind.LiteralChar, "a", 6),
            (RegexTokenKind.LiteralChar, "e", 7),
            (RegexTokenKind.LiteralChar, "i", 8),
            (RegexTokenKind.LiteralChar, "o", 9),
            (RegexTokenKind.LiteralChar, "u", 10),
            (RegexTokenKind.RBracket, "]", 11),
            (RegexTokenKind.RBracket, "]", 12),
            (RegexTokenKind.End, string.Empty, 13));
    }

    [Fact]
    public void Tokenize_Escapes()
    {
        var tokens = Lex("\\n\\t\\p{Lu}\\P{IsGreek}\\d\\W");

        AssertTokens(tokens,
            (RegexTokenKind.SingleCharEscape, "\\n", 0),
            (RegexTokenKind.SingleCharEscape, "\\t", 2),
            (RegexTokenKind.CategoryEscape, "\\p{Lu}", 4),
            (RegexTokenKind.ComplementEscape, "\\P{IsGreek}", 10),
            (RegexTokenKind.MultiCharEscape, "\\d", 21),
            (RegexTokenKind.MultiCharEscape, "\\W", 23),
            (RegexTokenKind.End, string.Empty, 25));
    }

    [Fact]
    public void Tokenize_BackReference()
    {
        var tokens = Lex("(a)\\1");

        AssertTokens(tokens,
            (RegexTokenKind.LParen, "(", 0),
            (RegexTokenKind.LiteralChar, "a", 1),
            (RegexTokenKind.RParen, ")", 2),
            (RegexTokenKind.BackReference, "\\1", 3),
            (RegexTokenKind.End, string.Empty, 5));
    }



    [Fact]
    public void Tokenize_AnchorsAndDot()
    {
        var tokens = Lex("^a.b$");

        AssertTokens(tokens,
            (RegexTokenKind.Caret, "^", 0),
            (RegexTokenKind.LiteralChar, "a", 1),
            (RegexTokenKind.Dot, ".", 2),
            (RegexTokenKind.LiteralChar, "b", 3),
            (RegexTokenKind.Dollar, "$", 4),
            (RegexTokenKind.End, string.Empty, 5));
    }

    private static IReadOnlyList<RegexToken> Lex(string pattern)
    {
        PreprocessedPattern preprocessed = Preprocessor.Process(pattern, RegexFlags.None);
        return RegexLexer.Tokenize(preprocessed, RegexDialect.XPath30);
    }

    private static void AssertTokens(
        IReadOnlyList<RegexToken> actual,
        params (RegexTokenKind Kind, string Lexeme, int OriginalOffset)[] expected)
    {
        Assert.Equal(expected.Length, actual.Count);

        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Kind, actual[i].Kind);
            Assert.Equal(expected[i].Lexeme, actual[i].Lexeme.ToString());
            Assert.Equal(expected[i].OriginalOffset, actual[i].OriginalOffset);
        }
    }
}
