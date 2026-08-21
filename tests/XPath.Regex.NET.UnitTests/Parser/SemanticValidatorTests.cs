using System.Collections.Immutable;
using XPath.Regex.NET.Errors;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Parser;

namespace XPath.Regex.NET.UnitTests.Parser;

public sealed class SemanticValidatorTests
{
    [Fact]
    public void Validate_BackReferencePastCaptureCount_ThrowsForx0002()
    {
        AstNode ast = new BackReferenceNode(2);

        _ = Assert.Throws<Forx0002Exception>(() => SemanticValidator.Validate(
            ast,
            captureCount: 1,
            RegexDialect.XPath30,
            PermissiveUnicodeNameValidator.Instance));
    }

    [Fact]
    public void Validate_NonCapturingGroupInXsd_ThrowsForx0002()
    {
        AstNode ast = new NonCaptureNode(new LiteralNode('a'));

        _ = Assert.Throws<Forx0002Exception>(() => SemanticValidator.Validate(
            ast,
            captureCount: 0,
            RegexDialect.Xsd,
            PermissiveUnicodeNameValidator.Instance));
    }

    [Fact]
    public void Validate_ReluctantRepeatInXsd_ThrowsForx0002()
    {
        AstNode ast = new RepeatNode(new LiteralNode('a'), 0, 1, Greedy: false);

        _ = Assert.Throws<Forx0002Exception>(() => SemanticValidator.Validate(
            ast,
            captureCount: 0,
            RegexDialect.Xsd,
            PermissiveUnicodeNameValidator.Instance));
    }

    [Fact]
    public void Validate_ValidTree_Passes()
    {
        AstNode ast = new ConcatNode(
            ImmutableArray.Create<AstNode>(
                new CaptureNode(1, new LiteralNode('a')),
                new BackReferenceNode(1)));

        SemanticValidator.Validate(ast, 1, RegexDialect.XPath30, PermissiveUnicodeNameValidator.Instance);
    }
}
