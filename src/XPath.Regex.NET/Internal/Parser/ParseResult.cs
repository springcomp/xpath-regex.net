using XPath.Regex.NET.Internal.Ast;

namespace XPath.Regex.NET.Internal.Parser;

internal sealed class ParseResult
{
    public ParseResult(AstNode root, int captureCount)
    {
        Root = root;
        CaptureCount = captureCount;
    }

    public AstNode Root { get; }

    public int CaptureCount { get; }
}
