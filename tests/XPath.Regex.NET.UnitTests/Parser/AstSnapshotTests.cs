using System.Globalization;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Parser;

public sealed class AstSnapshotTests
{
    [Fact]
    public void Parse_ClassSubtraction_StableSnapshot()
    {
        ParseResult result = Parse("[a-z-[aeiou]]", RegexDialect.XPath30);

        string snapshot = Format(result.Root);

        Assert.Equal("Class(98-100,102-104,106-110,112-116,118-122;neg=False)", snapshot);
    }

    [Fact]
    public void Parse_NestedStructure_StableSnapshot()
    {
        ParseResult result = Parse("(ab|c)+", RegexDialect.XPath30);

        string snapshot = Format(result.Root);

        Assert.Equal("Repeat(min=1,max=inf,greedy=True,child=Capture(1,Alt(Concat(Lit(97),Lit(98))|Lit(99))))", snapshot);
    }

    private static ParseResult Parse(string pattern, RegexDialect dialect)
    {
        PreprocessedPattern preprocessed = Preprocessor.Process(pattern, RegexFlags.None);
        IReadOnlyList<RegexToken> tokens = RegexLexer.Tokenize(preprocessed, dialect);
        return RegexParser.Parse(tokens, dialect, RegexFlags.None, PermissiveUnicodeNameValidator.Instance);
    }

    private static string Format(AstNode node)
    {
        return node switch
        {
            LiteralNode literal => $"Lit({literal.CodePoint})",
            WildcardNode => "Dot",
            AnchorStartNode => "AnchorStart",
            AnchorEndNode => "AnchorEnd",
            BackReferenceNode backReference => $"BackRef({backReference.GroupNumber})",
            CaptureNode capture => $"Capture({capture.GroupNumber},{Format(capture.Child)})",
            NonCaptureNode nonCapture => $"NonCapture({Format(nonCapture.Child)})",
            RepeatNode repeat => $"Repeat(min={repeat.Min},max={(repeat.Max.HasValue ? repeat.Max.Value.ToString(CultureInfo.InvariantCulture) : "inf")},greedy={repeat.Greedy},child={Format(repeat.Child)})",
            ConcatNode concat => $"Concat({string.Join(",", concat.Children.Select(Format))})",
            AlternationNode alternation => $"Alt({string.Join("|", alternation.Alternatives.Select(Format))})",
            CharClassNode charClass => $"Class({string.Join(",", charClass.Ranges.Select(static range => $"{range.Lo}-{range.Hi}"))};neg={charClass.Negated})",
            _ => throw new InvalidOperationException($"Unsupported node {node.GetType().Name}"),
        };
    }
}
