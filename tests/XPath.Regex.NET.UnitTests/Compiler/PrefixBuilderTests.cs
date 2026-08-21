using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Compiler;

namespace XPath.Regex.NET.UnitTests.Compiler;

public sealed class PrefixBuilderTests
{
    [Fact]
    public void ExtractsLiteralConcatenationCaptureGroupAndMandatoryRepeat()
    {
        AstNode root = new ConcatNode([
          new CaptureNode(1, new LiteralNode('f')),
      new NonCaptureNode(new RepeatNode(new LiteralNode('o'), 2, 2, true)),
      new LiteralNode('x'),
    ]);

        Assert.Equal("foox", PrefixBuilder.TryExtractPrefix(root));
    }

    [Fact]
    public void StopsAtAlternationOptionalRepeatWildcardAndClass()
    {
        Assert.Null(PrefixBuilder.TryExtractPrefix(new AlternationNode([new LiteralNode('a'), new LiteralNode('b')])));
        Assert.Null(PrefixBuilder.TryExtractPrefix(new RepeatNode(new LiteralNode('a'), 0, 1, true)));
        Assert.Null(PrefixBuilder.TryExtractPrefix(new ConcatNode([new WildcardNode(), new LiteralNode('a')])));
        Assert.Null(PrefixBuilder.TryExtractPrefix(new ConcatNode([new CharClassNode([((int)'a', (int)'z')], false), new LiteralNode('b')])));
    }

    [Fact]
    public void ShortCircuitsAtFirstNonLiteralNode()
    {
        AstNode root = new ConcatNode([new LiteralNode('a'), new WildcardNode(), new LiteralNode('b')]);

        Assert.Equal("a", PrefixBuilder.TryExtractPrefix(root));
    }
}
