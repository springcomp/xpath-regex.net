using System.Text;
using XPath.Regex.NET.Internal.Ast;
using XPath.Regex.NET.Internal.Unicode;

namespace XPath.Regex.NET.Internal.Compiler;

internal static class PrefixBuilder
{
    public static string? TryExtractPrefix(AstNode root)
    {
        (string prefix, _) = Extract(root);
        return prefix.Length == 0 ? null : prefix;
    }

    private static (string Prefix, bool Nullable) Extract(AstNode node) => node switch
    {
        LiteralNode literal when IsValidScalar(literal.CodePoint) => (char.ConvertFromUtf32(literal.CodePoint), false),
        CharClassNode cls when IsSingleton(cls) && IsValidScalar(cls.Ranges[0].Lo) => (char.ConvertFromUtf32(cls.Ranges[0].Lo), false),
        LiteralNode or CharClassNode or WildcardNode or AlternationNode or AnchorStartNode or AnchorEndNode or BackReferenceNode => (string.Empty, false),
        CaptureNode capture => Extract(capture.Child),
        NonCaptureNode nonCapture => Extract(nonCapture.Child),
        RepeatNode repeat when repeat.Min == 0 => (string.Empty, true),
        RepeatNode repeat => ExtractMandatoryRepeat(repeat),
        ConcatNode concat => ExtractConcat(concat),
        _ => (string.Empty, false),
    };

    private static (string Prefix, bool Nullable) ExtractMandatoryRepeat(RepeatNode repeat)
    {
        (string prefix, bool nullable) = Extract(repeat.Child);
        if (prefix.Length == 0)
            return (string.Empty, false);

        var builder = new StringBuilder(prefix.Length * repeat.Min);
        for (int i = 0; i < repeat.Min; i++)
            builder.Append(prefix);

        // A repeat is only exact-length (safe to keep concatenating subsequent
        // literals directly onto it) when Min == Max. Any variable-length repeat
        // (unbounded, or Min < Max) can consume more of the child pattern than
        // its mandatory Min copies, so treat it as "nullable" here to stop
        // further prefix extraction after this segment.
        bool variableLength = repeat.Max is null || repeat.Max != repeat.Min;
        return (builder.ToString(), nullable || variableLength);
    }

    private static (string Prefix, bool Nullable) ExtractConcat(ConcatNode concat)
    {
        var builder = new StringBuilder();
        foreach (AstNode child in concat.Children)
        {
            (string prefix, bool nullable) = Extract(child);
            if (prefix.Length == 0)
                return (builder.ToString(), concat.Children.All(IsNullable));
            builder.Append(prefix);
            if (nullable)
                return (builder.ToString(), concat.Children.All(IsNullable));
        }
        return (builder.ToString(), concat.Children.All(IsNullable));
    }

    private static bool IsNullable(AstNode node) => Extract(node).Nullable;

    private static bool IsSingleton(CharClassNode cls)
    {
        if (cls.Negated || cls.Ranges.Length != 1)
            return false;
        (int lo, int hi) = cls.Ranges[0];
        return lo == hi;
    }

    private static bool IsValidScalar(int codePoint) => codePoint is >= 0 and <= UnicodeConstants.MaxCodePoint and not (>= 0xD800 and <= 0xDFFF);
}
