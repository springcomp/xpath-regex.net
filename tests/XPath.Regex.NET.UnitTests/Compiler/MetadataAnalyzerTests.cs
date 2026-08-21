using System.Collections.Immutable;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Compiler;
using XPath.Regex.NET.Internal.Nfa;
using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Compiler;

public sealed class MetadataAnalyzerTests
{
    private static readonly ImmutableArray<(int Lo, int Hi)> AnyRange = ImmutableArray.Create((0, 0x10FFFF));

    [Fact]
    public void ComputeGroupMinLengths_AtomsHaveExpectedLengths()
    {
        Assert.Equal(1, Min(new LiteralNode('a')));
        Assert.Equal(1, Min(new WildcardNode()));
        Assert.Equal(1, Min(new CharClassNode(ImmutableArray.Create<(int Lo, int Hi)>(((int)'a', (int)'z')), false)));
        Assert.Equal(0, Min(new AnchorStartNode()));
        Assert.Equal(0, Min(new AnchorEndNode()));
    }

    [Fact]
    public void ComputeGroupMinLengths_CompositeNodesHaveExpectedLengths()
    {
        AstNode root = new CaptureNode(1, new ConcatNode(ImmutableArray.Create<AstNode>(
            new LiteralNode('a'),
            new AlternationNode(ImmutableArray.Create<AstNode>(new LiteralNode('b'), new AnchorEndNode())),
            new RepeatNode(new LiteralNode('c'), 2, 4, true))));

        Assert.Equal(3, MetadataAnalyzer.ComputeGroupMinLengths(root)[1]);
    }

    [Fact]
    public void ComputeGroupMinLengths_BackReferencesUseResolvedLengthAndZeroWhenUnresolved()
    {
        AstNode resolved = new ConcatNode(ImmutableArray.Create<AstNode>(
            new CaptureNode(1, new ConcatNode(ImmutableArray.Create<AstNode>(new LiteralNode('a'), new LiteralNode('b')))),
            new BackReferenceNode(1)));
        AstNode unresolved = new CaptureNode(1, new BackReferenceNode(9));

        Assert.Equal(4, MetadataAnalyzer.ComputeGroupMinLengths(resolved)[1] + 2);
        Assert.Equal(0, MetadataAnalyzer.ComputeGroupMinLengths(unresolved)[1]);
    }

    [Theory]
    [InlineData(0, null, 0)]
    [InlineData(3, null, 3)]
    [InlineData(3, 7, 3)]
    [InlineData(0, 0, 0)]
    public void ComputeMinRepeat_UsesLowerBoundOnly(int min, int? max, int expected)
    {
        var root = new CaptureNode(1, new RepeatNode(new LiteralNode('a'), min, max, true));
        Assert.Equal(expected, MetadataAnalyzer.ComputeGroupMinLengths(root)[1]);
    }

    [Fact]
    public void ComputeMinRepeat_ClampsOverflowToIntMaxValue()
    {
        var root = new CaptureNode(1,
            new RepeatNode(new RepeatNode(new LiteralNode('a'), 100_000, 100_000, true), 100_000, null, true));

        Assert.Equal(int.MaxValue, MetadataAnalyzer.ComputeGroupMinLengths(root)[1]);
    }

    [Theory]
    [InlineData("a", 1)]
    [InlineData("^a$", 1)]
    [InlineData("a|", 0)]
    [InlineData("(ab)\\1", 4)]
    public void Compile_ComputesMinMatchLength(string pattern, int expected)
    {
        NfaProgram program = RegexCompiler.Compile(Parse(pattern).Root, Parse(pattern).CaptureCount,
            RegexDialect.XPath30, RegexFlags.None);
        Assert.Equal(expected, program.MinMatchLength);
    }

    [Fact]
    public void ContainsBackReferences_ReportsPresence()
    {
        Assert.False(MetadataAnalyzer.ContainsBackReferences(new LiteralNode('a')));
        Assert.True(MetadataAnalyzer.ContainsBackReferences(new BackReferenceNode(1)));
        Assert.True(MetadataAnalyzer.ContainsBackReferences(new RepeatNode(
            new NonCaptureNode(new BackReferenceNode(2)), 0, null, true)));
    }

    [Fact]
    public void CanMatchEmpty_HandlesEpsilonAndZeroLengthBackReference()
    {
        var match = ImmutableArray.Create<NfaInstruction>(new MatchInstruction());
        var charThenMatch = ImmutableArray.Create<NfaInstruction>(
            new CharInstruction(AnyRange), new MatchInstruction());
        var backrefThenMatch = ImmutableArray.Create<NfaInstruction>(
            new BackRefInstruction(1, false), new MatchInstruction());

        Assert.True(MetadataAnalyzer.CanMatchEmpty(match, new Dictionary<int, int>()));
        Assert.False(MetadataAnalyzer.CanMatchEmpty(charThenMatch, new Dictionary<int, int>()));
        Assert.True(MetadataAnalyzer.CanMatchEmpty(backrefThenMatch, new Dictionary<int, int> { [1] = 0 }));
        Assert.False(MetadataAnalyzer.CanMatchEmpty(backrefThenMatch, new Dictionary<int, int>()));
    }

    private static int Min(AstNode node) => MetadataAnalyzer.ComputeGroupMinLengths(new CaptureNode(1, node))[1];

    private static ParseResult Parse(string pattern)
    {
        PreprocessedPattern preprocessed = Preprocessor.Process(pattern, RegexFlags.None);
        IReadOnlyList<RegexToken> tokens = RegexLexer.Tokenize(preprocessed, RegexDialect.XPath30);
        return RegexParser.Parse(tokens, RegexDialect.XPath30, RegexFlags.None,
            PermissiveUnicodeNameValidator.Instance);
    }
}
