using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Parser;

public sealed class ParserTests
{
    [Fact]
    public void Parse_SimpleAlternation_BuildsExpectedShape()
    {
        ParseResult result = Parse("ab|cd", RegexDialect.XPath30);

        Assert.Equal(0, result.CaptureCount);

        AlternationNode alternation = Assert.IsType<AlternationNode>(result.Root);
        Assert.Equal(2, alternation.Alternatives.Length);

        ConcatNode left = Assert.IsType<ConcatNode>(alternation.Alternatives[0]);
        ConcatNode right = Assert.IsType<ConcatNode>(alternation.Alternatives[1]);

        Assert.Collection(left.Children,
            node => AssertLiteral(node, 'a'),
            node => AssertLiteral(node, 'b'));

        Assert.Collection(right.Children,
            node => AssertLiteral(node, 'c'),
            node => AssertLiteral(node, 'd'));
    }

    [Fact]
    public void Parse_GroupCaptureNumbering_InXPath30Plus_SkipsNonCapturing()
    {
        ParseResult result = Parse("(a)(?:b)(c)", RegexDialect.XPath30);

        Assert.Equal(2, result.CaptureCount);

        ConcatNode concat = Assert.IsType<ConcatNode>(result.Root);
        Assert.Equal(3, concat.Children.Length);

        CaptureNode first = Assert.IsType<CaptureNode>(concat.Children[0]);
        Assert.Equal(1, first.GroupNumber);

        _ = Assert.IsType<NonCaptureNode>(concat.Children[1]);

        CaptureNode third = Assert.IsType<CaptureNode>(concat.Children[2]);
        Assert.Equal(2, third.GroupNumber);
    }

    [Fact]
    public void Parse_ReluctantQuantifier_InXPath30Plus_BuildsRepeatGreedyFalse()
    {
        ParseResult result = Parse("a{2,4}?", RegexDialect.XPath30);

        RepeatNode repeat = Assert.IsType<RepeatNode>(result.Root);
        Assert.Equal(2, repeat.Min);
        Assert.Equal(4, repeat.Max);
        Assert.False(repeat.Greedy);
    }

    [Fact]
    public void Parse_AnchorDotSequence_InXPath30Plus_BuildsExpectedAtoms()
    {
        ParseResult result = Parse("^a.b$", RegexDialect.XPath30);

        ConcatNode concat = Assert.IsType<ConcatNode>(result.Root);
        Assert.Collection(concat.Children,
            node => Assert.IsType<AnchorStartNode>(node),
            node => AssertLiteral(node, 'a'),
            node => Assert.IsType<WildcardNode>(node),
            node => AssertLiteral(node, 'b'),
            node => Assert.IsType<AnchorEndNode>(node));
    }

    [Fact]
    public void Parse_AlternationEmptyBranch_UsesEmptyConcatEpsilon()
    {
        ParseResult result = Parse("a|", RegexDialect.XPath30);

        AlternationNode alternation = Assert.IsType<AlternationNode>(result.Root);
        Assert.Equal(2, alternation.Alternatives.Length);
        _ = Assert.IsType<LiteralNode>(alternation.Alternatives[0]);

        ConcatNode epsilon = Assert.IsType<ConcatNode>(alternation.Alternatives[1]);
        Assert.Empty(epsilon.Children);
    }

    [Fact]
    public void Parse_UnknownUnicodeProperty_ThrowsForx0002()
    {
        var validator = new RejectingUnicodeNameValidator();

        Forx0002Exception ex = Assert.Throws<Forx0002Exception>(() => Parse("\\p{Nope}", RegexDialect.XPath30, RegexFlags.None, validator));

        Assert.Equal(0, ex.PatternOffset);
        Assert.Contains("Unknown Unicode property or block", ex.Message, StringComparison.Ordinal);
    }

    private static ParseResult Parse(
        string pattern,
        RegexDialect dialect,
        RegexFlags flags = default,
        IUnicodeNameValidator? validator = null)
    {
        RegexFlags effectiveFlags = flags == default ? RegexFlags.None : flags;
        PreprocessedPattern preprocessed = Preprocessor.Process(pattern, effectiveFlags);
        IReadOnlyList<RegexToken> tokens = RegexLexer.Tokenize(preprocessed, dialect);
        return RegexParser.Parse(tokens, dialect, effectiveFlags, validator ?? PermissiveUnicodeNameValidator.Instance);
    }

    private static void AssertLiteral(AstNode node, char expected)
    {
        LiteralNode literal = Assert.IsType<LiteralNode>(node);
        Assert.Equal(expected, literal.CodePoint);
    }

    private sealed class RejectingUnicodeNameValidator : IUnicodeNameValidator
    {
        public bool IsValidCategory(string name) => false;

        public bool IsValidBlock(string name) => false;
    }
}
