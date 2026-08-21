using XPath.Regex.NET.Internal.Ast;

namespace XPath.Regex.NET.Internal.Parser;

internal static class SemanticValidator
{
    public static void Validate(
        AstNode root,
        int captureCount,
        RegexDialect dialect,
        IUnicodeNameValidator unicodeValidator)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(unicodeValidator);

        ValidateNode(root, captureCount, dialect);
    }

    private static void ValidateNode(AstNode node, int captureCount, RegexDialect dialect)
    {
        switch (node)
        {
            case LiteralNode:
            case WildcardNode:
            case CharClassNode:
            case AnchorStartNode:
            case AnchorEndNode:
                return;

            case BackReferenceNode backReference:
                if (dialect == RegexDialect.Xsd || backReference.GroupNumber <= 0 || backReference.GroupNumber > captureCount)
                    throw ParserExceptionFactory.BackReferenceInvalid(backReference.GroupNumber, offset: -1);

                return;

            case AlternationNode alternation:
                foreach (AstNode alternative in alternation.Alternatives)
                    ValidateNode(alternative, captureCount, dialect);

                return;

            case ConcatNode concat:
                foreach (AstNode child in concat.Children)
                    ValidateNode(child, captureCount, dialect);

                return;

            case RepeatNode repeat:
                if (repeat.Min < 0 || (repeat.Max.HasValue && repeat.Max.Value < repeat.Min))
                    throw ParserExceptionFactory.InvalidQuantifierBounds(offset: -1);

                if (!repeat.Greedy && dialect == RegexDialect.Xsd)
                    throw ParserExceptionFactory.ReluctantQuantifierNotSupported(dialect, offset: -1);

                ValidateNode(repeat.Child, captureCount, dialect);
                return;

            case CaptureNode capture:
                ValidateNode(capture.Child, captureCount, dialect);
                return;

            case NonCaptureNode nonCapture:
                if (dialect == RegexDialect.Xsd)
                    throw ParserExceptionFactory.FeatureNotSupported("(?:)", dialect, offset: -1);

                ValidateNode(nonCapture.Child, captureCount, dialect);
                return;

            default:
                throw new InvalidOperationException($"Unknown AST node type: {node.GetType().FullName}");
        }
    }
}
