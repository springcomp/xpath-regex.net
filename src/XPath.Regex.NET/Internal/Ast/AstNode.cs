using System.Collections.Immutable;

namespace XPath.Regex.NET.Internal.Ast;

internal abstract record AstNode;

/// <summary>Literal Unicode scalar value.</summary>
internal sealed record LiteralNode(int CodePoint) : AstNode;

/// <summary>
/// Wildcard <c>.</c>. Excludes LF/CR unless DotAll flag is set (resolved at compile time).
/// </summary>
internal sealed record WildcardNode : AstNode;

/// <summary>
/// Character class expression <c>[…]</c>, pre-computed as a sorted list of
/// Unicode code-point ranges after all escapes, subtractions, and case-folding
/// have been resolved.
/// </summary>
internal sealed record CharClassNode(ImmutableArray<(int Lo, int Hi)> Ranges, bool Negated) : AstNode;

/// <summary>Anchor at start of input (or line when multiline).</summary>
internal sealed record AnchorStartNode : AstNode;

/// <summary>Anchor at end of input (or line when multiline).</summary>
internal sealed record AnchorEndNode : AstNode;

/// <summary>Back-reference to capturing group <see cref="GroupNumber"/> (1-indexed).</summary>
internal sealed record BackReferenceNode(int GroupNumber) : AstNode;

/// <summary>Alternation: matches any of the alternatives (left to right preference).</summary>
internal sealed record AlternationNode(ImmutableArray<AstNode> Alternatives) : AstNode;

/// <summary>Concatenation: matches the sequence of children left to right.</summary>
internal sealed record ConcatNode(ImmutableArray<AstNode> Children) : AstNode;

/// <summary>Repetition with greedy or reluctant semantics.</summary>
internal sealed record RepeatNode(
    AstNode Child,
    int Min,
    int? Max,       // null = unbounded
    bool Greedy
) : AstNode;

/// <summary>Capturing group.</summary>
internal sealed record CaptureNode(int GroupNumber, AstNode Child) : AstNode;

/// <summary>Non-capturing group (XPath 3.1 only). No save semantics.</summary>
internal sealed record NonCaptureNode(AstNode Child) : AstNode;
